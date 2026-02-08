using System.Reflection;
using System.Text.Json;
using CookTime.Models.Contracts;
using ChatClient = OpenAI.Chat.ChatClient;
using ChatMessage = OpenAI.Chat.ChatMessage;
using SystemChatMessage = OpenAI.Chat.SystemChatMessage;
using UserChatMessage = OpenAI.Chat.UserChatMessage;
using ChatMessageContentPart = OpenAI.Chat.ChatMessageContentPart;
using ChatResponseFormat = OpenAI.Chat.ChatResponseFormat;
using ChatCompletionOptions = OpenAI.Chat.ChatCompletionOptions;

namespace CookTime.Services;

public class AIRecipeService
{
    private readonly ChatClient _chatClient;
    private readonly CookTimeDB _db;
    private readonly ILogger<AIRecipeService> _logger;
    private readonly string _systemPrompt;
    private readonly BinaryData _jsonSchema;

    private const double AutoMatchThreshold = 0.85;
    private const double SuggestThreshold = 0.5;
    private const int MaxCandidates = 5;

    public AIRecipeService(
        IConfiguration configuration,
        CookTimeDB db,
        ILogger<AIRecipeService> logger)
    {
        var apiKey = configuration["OpenAI:Key"]
            ?? throw new InvalidOperationException("OpenAI:Key configuration is required");

        var client = new OpenAI.OpenAIClient(apiKey);
        _chatClient = client.GetChatClient("gpt-4o");
        _db = db;
        _logger = logger;

        // Load embedded resources
        _systemPrompt = LoadEmbeddedResource("CookTime.Resources.RecipeGenerationPrompt.txt");
        var schemaJson = LoadEmbeddedResource("CookTime.Resources.RecipeGenerationSchema.json");
        _jsonSchema = BinaryData.FromString(schemaJson);
    }

    /// <summary>
    /// Generate a recipe from one or more images.
    /// </summary>
    /// <param name="images">Base64-encoded image data with MIME types</param>
    /// <param name="ownerId">The user ID who will own the recipe</param>
    /// <returns>Recipe generation result with match metadata</returns>
    public async Task<RecipeGenerationResultDto> GenerateFromImagesAsync(
        IEnumerable<(string Base64Data, string MimeType)> images,
        Guid ownerId)
    {
        var contentParts = new List<ChatMessageContentPart>
        {
            ChatMessageContentPart.CreateTextPart("Please extract the recipe from the following image(s):")
        };

        foreach (var (base64Data, mimeType) in images)
        {
            var imageData = BinaryData.FromBytes(Convert.FromBase64String(base64Data));
            contentParts.Add(ChatMessageContentPart.CreateImagePart(imageData, mimeType));
        }

        return await GenerateRecipeAsync(contentParts, ownerId);
    }

    /// <summary>
    /// Generate a recipe from text input.
    /// </summary>
    /// <param name="text">Recipe text to parse</param>
    /// <param name="ownerId">The user ID who will own the recipe</param>
    /// <returns>Recipe generation result with match metadata</returns>
    public async Task<RecipeGenerationResultDto> GenerateFromTextAsync(string text, Guid ownerId)
    {
        var contentParts = new List<ChatMessageContentPart>
        {
            ChatMessageContentPart.CreateTextPart($"Please extract the recipe from the following text:\n\n{text}")
        };

        return await GenerateRecipeAsync(contentParts, ownerId);
    }

    private async Task<RecipeGenerationResultDto> GenerateRecipeAsync(
        List<ChatMessageContentPart> contentParts,
        Guid ownerId)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(_systemPrompt),
            new UserChatMessage(contentParts)
        };

        var schemaFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: "recipe_extraction",
            jsonSchema: _jsonSchema,
            jsonSchemaIsStrict: true);

        var options = new ChatCompletionOptions
        {
            ResponseFormat = schemaFormat
        };

        _logger.LogInformation("Sending recipe generation request to OpenAI");

        var completion = await _chatClient.CompleteChatAsync(messages, options);
        var responseContent = completion.Value.Content[0].Text;

        _logger.LogDebug("Received AI response: {Response}", responseContent);

        var aiRecipe = JsonSerializer.Deserialize<AIRecipeResponse>(responseContent)
            ?? throw new InvalidOperationException("Failed to deserialize AI response");

        // Extract all ingredient names for batch matching
        var ingredientNames = aiRecipe.Components
            .SelectMany(c => c.Ingredients)
            .Select(i => i.Name)
            .ToList();

        // Batch search for ingredient matches
        var matchResults = await _db.SearchIngredientsBatchAsync(ingredientNames);

        // Build the result with reconciled ingredients
        return BuildResult(aiRecipe, matchResults, ownerId);
    }

    private RecipeGenerationResultDto BuildResult(
        AIRecipeResponse aiRecipe,
        Dictionary<string, List<IngredientMatchResultDto>> matchResults,
        Guid ownerId)
    {
        var ingredientMatches = new List<IngredientMatchDto>();
        var components = new List<ComponentCreateDto>();

        foreach (var aiComponent in aiRecipe.Components)
        {
            var componentIngredients = new List<IngredientRequirementCreateDto>();

            foreach (var aiIngredient in aiComponent.Ingredients)
            {
                var matches = matchResults.GetValueOrDefault(aiIngredient.Name) ?? [];
                var bestMatch = matches.FirstOrDefault();

                // Determine if we should auto-populate the ingredient ID
                Guid? matchedId = null;
                string? matchedName = null;
                double? confidence = null;

                if (bestMatch != null)
                {
                    confidence = bestMatch.Confidence;
                    if (bestMatch.Confidence >= AutoMatchThreshold)
                    {
                        matchedId = bestMatch.Id;
                        matchedName = bestMatch.Name;
                    }
                }

                // Build ingredient requirement
                componentIngredients.Add(new IngredientRequirementCreateDto
                {
                    Id = Guid.NewGuid(),
                    Ingredient = new IngredientRefDto
                    {
                        Id = matchedId ?? Guid.NewGuid(),
                        Name = matchedName ?? aiIngredient.Name,
                        IsNew = matchedId == null,
                        DensityKgPerL = 1.0
                    },
                    Quantity = aiIngredient.Quantity,
                    Unit = aiIngredient.Unit,
                    Position = aiIngredient.Position,
                    Text = matchedName ?? aiIngredient.Name
                });

                // Build match metadata
                var matchDto = new IngredientMatchDto
                {
                    OriginalText = aiIngredient.Name,
                    MatchedIngredientId = matchedId,
                    MatchedIngredientName = matchedName,
                    Confidence = confidence,
                    Candidates = matches
                        .Where(m => m.Confidence >= SuggestThreshold)
                        .Take(MaxCandidates)
                        .Select(m => new IngredientCandidateDto
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Confidence = m.Confidence
                        })
                        .ToList()
                };

                ingredientMatches.Add(matchDto);
            }

            components.Add(new ComponentCreateDto
            {
                Name = aiComponent.Name,
                Position = aiComponent.Position,
                Steps = aiComponent.Steps,
                Ingredients = componentIngredients
            });
        }

        var recipe = new RecipeCreateDto
        {
            Name = aiRecipe.Name,
            Description = aiRecipe.Description,
            Servings = aiRecipe.Servings,
            PrepMinutes = aiRecipe.PrepMinutes,
            CookingMinutes = aiRecipe.CookingMinutes,
            OwnerId = ownerId,
            Components = components,
            CategoryIds = []
        };

        return new RecipeGenerationResultDto
        {
            Recipe = recipe,
            IngredientMatches = ingredientMatches
        };
    }

    private static string LoadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
