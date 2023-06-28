using System.Text.Json.Nodes;
using GustavoTech.Implementation;
using Microsoft.AspNetCore.WebUtilities;
using OpenAI;
using OpenAI.Chat;

namespace babe_algorithms;

/// <summary>
/// LEARNINGS:
/// 1. Don't use refs
/// </summary>
public class ChatGPT : IRecipeArtificialIntelligence
{
    private const string INGREDIENT_REQUIREMENTS = "ingredientRequirements";
    private const string RECIPE_NAME = "recipeName";
    private const string STEPS = "steps";

    public ChatGPT(OpenAIOptions configuration, ILogger<ChatGPT> logger)
    {
        this.OpenAIClient = new OpenAIClient(configuration.Key);
    }

    private OpenAIClient OpenAIClient { get; }

    public async Task<MultiPartRecipe> ConvertToRecipeAsync(string text, CancellationToken ct)
    {
        // TODO change to all when JSONSchema is modified to describe an array of ingredients.
        var prompt = "Convert the following to a recipe: ";
        var messages = new List<Message>
        {
            new Message(Role.System, "You convert unstructured recipe text into structured recipe objects."),
            new Message(Role.User, prompt + text),
        };
        var functions = AvailableChatFunctions();
        var chatRequest = new ChatRequest(
            messages,
            functions: functions,
            functionCall: "auto",
            model: "gpt-3.5-turbo-0613",
            temperature: 0.0);
        // model: "gpt-4-0613"); // not available via API call yet, only on the GUI.
        var result = await this.OpenAIClient.ChatEndpoint.GetCompletionAsync(chatRequest, ct);
        if (result.FirstChoice.Message.Function == null)
        {
            return null;
        }

        JsonNode arguments = JsonNode.Parse(result.FirstChoice.Message.Function.Arguments.ToString());
        var ingredientRequirements = arguments[INGREDIENT_REQUIREMENTS]?.AsArray().Select(ir =>
            new
            {
                Name = ir["name"].ToString(),
                Unit = GetUnit(ir),
                Quantity = ParseQuantity(ir["quantity"].ToString()),
                // Preparation = ir["preparation"].ToString(),
                // quantity = ir["quantity"],
            })
            .ToList() ?? new ();
        var steps = arguments[STEPS]?.AsArray().Select(ir =>
        {
            return new MultiPartRecipeStep()
            {
                Text = ir.ToString(),
            };
        }).ToList() ?? new();
        string recipeName = arguments[RECIPE_NAME].ToString().ToTitleCase();
        var recipe = new MultiPartRecipe()
        {
            Name = recipeName,
            ServingsProduced = ParseQuantity(arguments[nameof(MultiPartRecipe.ServingsProduced)]?.ToString() ?? "0.0"),
            CooktimeMinutes = (int)ParseQuantity(arguments[nameof(MultiPartRecipe.CooktimeMinutes)]?.ToString() ?? "0.0"),
            RecipeComponents = new List<RecipeComponent>()
            {
                new RecipeComponent()
                {
                    Name = recipeName,
                    Steps = steps,
                    Ingredients = ingredientRequirements.Select(ir => new MultiPartIngredientRequirement()
                    {
                        Ingredient = new Ingredient()
                        {
                            Name = ir.Name
                        },
                        Quantity = ir.Quantity,
                        Unit = ir.Unit,
                    }).ToList(),
                }
            }
        };
        return recipe;

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
    }

    /// <summary>
    /// Parse a double out of string that may contain a number or a fraction.
    /// </summary>
    private static double ParseQuantity(string quantity)
    {
        if (double.TryParse(quantity, out double val))
        {
            return val;
        }
        else if (quantity.Contains('/'))
        {
            var tokens = quantity.Split('/');
            var (p, q) = (tokens[0], tokens[1]);
            if (double.TryParse(p, out double pn) && double.TryParse(q, out double qn))
            {
                return pn / qn;
            }
        }

        return 0;
    }

    private static Unit GetUnit(JsonNode ir)
    {
        if(Enum.TryParse<Unit>(ir["unit"].ToString(), ignoreCase: true, out Unit value))
        {
            return value;
        }
        else
        {
            return Unit.Count;
        }
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
                name: "CreateRecipe",
                description: "Creates a recipe",
                parameters: new JsonObject
                {
                    // ["$defs"] = new JsonObject
                    // {

                    //     ["ingredient"] = new JsonObject
                    //     {
                    //         ["properties"] = new JsonObject
                    //         {
                    //             ["name"] = new JsonObject
                    //             {
                    //                 ["type"] = "string",
                    //                 ["description"] = "The name of the ingredient"
                    //             },
                    //         }
                    //     },
                    // },
                    // Top level argument MUST be an object for ChatGPT
                    ["type"] = "object", 
                    ["properties"] = new JsonObject
                    {
                        [RECIPE_NAME] = new JsonObject
                        {
                            ["type"] = "string", 
                            ["description"] = "The name of the recipe",
                        },
                        [nameof(MultiPartRecipe.CooktimeMinutes)] = new JsonObject
                        {
                            ["type"] = "string", 
                            ["description"] = "The cooking time of the recipe in minutes.",
                        },
                        [nameof(MultiPartRecipe.ServingsProduced)] = new JsonObject
                        {
                            ["type"] = "string", 
                            ["description"] = "The number of servings the recipe produces",
                        },
                        [STEPS] = new JsonObject
                        {

                            ["type"] = "array", 
                            ["items"] = new JsonObject
                            {
                                ["type"] = "string",
                                ["description"] = "Recipe steps.",
                            }
                        },
                        [INGREDIENT_REQUIREMENTS] = new JsonObject
                        {
                            ["type"] = "array", 
                            ["items"] = new JsonObject
                            {
                                // ["ingredientRequirement"] = new JsonObject
                                // {
                                    ["type"] = "object", 
                                    ["properties"] = new JsonObject
                                    {
                                        // TODO change "name" to "ingredient" object when GPT-4 is available
                                        // ["ingredient"] = new JsonObject
                                        // {
                                        //     ["type"] = "object",
                                        //     ["properties"] = new JsonObject
                                        //     {
                                        //         ["name"] = new JsonObject
                                        //         {
                                        //             ["type"] = "string",
                                        //             ["description"] = "The name of the ingredient"
                                        //         },
                                        //     }
                                        // },
                                        ["name"] = new JsonObject
                                        {
                                            ["type"] = "string",
                                            // ["enum"] = unitsArray,
                                            ["description"] = "Just the simple ingredient name without cooking preparation steps, quantities, units, or abbreviations.",
                                        },
                                        ["preparation"] = new JsonObject
                                        {
                                            ["type"] = "string",
                                            ["description"] = "Ingredient preparation, if present, like chopping or heating.",
                                        },
                                        ["unit"] = new JsonObject
                                        {
                                            ["type"] = "string",
                                            ["enum"] = unitsArray,
                                            ["description"] = "The unit the ingredient is described in.",
                                        },
                                        ["quantity"] = new JsonObject
                                        {
                                            ["type"] = "string",
                                            ["description"] = "The decimal numeric amount of the unit requested.",
                                        }
                                    },
                                    ["required"] = new JsonArray { "ingredient", "unit", "quantity" }
                                // }
                            }
                        }
                    }
                })
        };

        return functions;
    }
}