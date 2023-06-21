using OpenAI_API;

namespace babe_algorithms;

public class ChatGPT : IRecipeArtificialIntelligence
{

    public ChatGPT(OpenAIOptions configuration, ILogger<ChatGPT> logger)
    {
        this.OpenAIClient = new OpenAIAPI(configuration.Key);
    }

    private OpenAIAPI OpenAIClient { get; }

    public async Task<MultiPartRecipe> ConvertToRecipeAsync(string text, CancellationToken ct)
    {
        var chat = this.OpenAIClient.Chat.CreateConversation();
        chat.AppendSystemMessage(
        """
            Transform free form recipe into JSON with ingredient quantity, units, and name separates by semicolons.
            Give the ingredients as a shopping list, without preparation details.
        """);

        chat.AppendExampleChatbotOutput(
        """
            {"name": "Recipe Name", "cookTime": "30 minutes", "ingredients": ["1;cup;flour", "2;grams;salt"], "steps": ["First step", "Second step"]}
        """);

        chat.AppendUserInput(text);
        string response = await chat.GetResponseFromChatbotAsync();
        return JsonConvert.DeserializeObject<MultiPartRecipe>(response);
    }
}