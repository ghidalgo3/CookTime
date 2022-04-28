
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Models;

#nullable enable

[Owned]
public class MultiPartRecipeStep : IRecipeStep
{
    public MultiPartRecipeStep(){}
    public MultiPartRecipeStep(RecipeStep step)
    {
        this.Text = step.Text;
    }
    public string Text { get; set; }
}