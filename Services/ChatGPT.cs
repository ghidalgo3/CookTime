using System.Text.Json.Nodes;
using OpenAI;
using OpenAI.Chat;

namespace babe_algorithms;

public class ChatGPT : IRecipeArtificialIntelligence
{

    public ChatGPT(OpenAIOptions configuration, ILogger<ChatGPT> logger)
    {
        this.OpenAIClient = new OpenAIClient(configuration.Key);
    }

    private OpenAIClient OpenAIClient { get; }

    public async Task<MultiPartRecipe> ConvertToRecipeAsync(string text, CancellationToken ct)
    {
        // TODO change to all when JSONSchema is modified to describe an array of ingredients.
        var prompt = "What is the first ingredients in ";
        var messages = new List<Message>
        {
            new Message(Role.System, "You convert unstructured recipe text into structured recipe objects."),
            new Message(Role.User, prompt + text),
        };
        var functions = AvailableChatFunctions();
        var chatRequest = new ChatRequest(messages, functions: functions, functionCall: "auto", model: "gpt-3.5-turbo-0613");
        var result = await this.OpenAIClient.ChatEndpoint.GetCompletionAsync(chatRequest, ct);
        Console.WriteLine(result.FirstChoice.Message);
        // chat.AppendSystemMessage(
        // """
        //     Transform free form recipe into JSON with ingredient quantity, units, and name separates by semicolons.
        //     Give the ingredients as a shopping list, without preparation details.
        // """);

        // chat.AppendExampleChatbotOutput(
        // """
        //     {"name": "Recipe Name", "cookTime": "30 minutes", "ingredients": ["1;cup;flour", "2;grams;salt"], "steps": ["First step", "Second step"]}
        // """);

        // chat.AppendUserInput(text);
        // string response = await chat.GetResponseFromChatbotAsync();
        return null;
    }

    private static List<Function> AvailableChatFunctions()
    {
        // Define the functions that the assistant is able to use:
        var units = Enum.GetValues<Unit>().Select(unit => unit.ToString()).ToArray();
        var unitsArray = new JsonArray();
        foreach (var unit in units)
        {
            unitsArray.Add(unit);
        }

        var functions = new List<Function>
        {
            // TODO this function needs to accept an array of ingredients
            // Follow this guide to make this object be an array
            // https://json-schema.org/learn/file-system.html
            new Function(
                name: "ExtractIngredients",
                description: "Gets the ingredient list of a recipe",
                parameters: new JsonObject
                {
                    ["type"] = "object", // Required by JSON schema
                    ["properties"] = new JsonObject
                    {
                        ["name"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The name of the ingredient"
                        },
                        ["unit"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["enum"] = unitsArray,
                            ["description"] = "The unit the ingredient is described in.",
                        },
                        ["quantity"] = new JsonObject
                        {
                            ["type"] = "number",
                            ["description"] = "The amount of the ingredient.",
                        }
                    },
                    ["required"] = new JsonArray { "ingredient", "unit", "quantity" }
                })
        };

        return functions;
    }
}