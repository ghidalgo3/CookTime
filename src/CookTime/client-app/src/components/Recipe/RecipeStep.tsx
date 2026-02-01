import { OverlayTrigger, Tooltip } from "react-bootstrap";
import { IngredientRequirement, MultiPartRecipe, Recipe, RecipeComponent } from "src/shared/CookTime";

type Segment = {
    ingredient: IngredientRequirement | null,
    text: string
}

interface StepProps {
    recipe: Recipe | MultiPartRecipe;
    stepText: string;  // Steps are now plain strings
    multipart: boolean;
    component?: RecipeComponent;
    newServings: number;
}

function trifurcate(s: string, position: number, length: number) {
    return {
        before: s.substring(0, position),
        inside: s.substring(position, position + length),
        after: s.substring(position + length, s.length)
    };
}

export function Step({ recipe, stepText, multipart, component, newServings }: StepProps) {
    let segments: Segment[] = [{ ingredient: null, text: stepText }];

    const ingredientRequirements: IngredientRequirement[] = multipart
        ? component!.ingredients!
        : (recipe as Recipe).ingredients ?? [];

    // Sort by length descending to match longer ingredient names first
    const sortedIngredients = [...ingredientRequirements].sort((a, b) => {
        const aLength = a.text ?? a.ingredient.name.length;
        const bLength = b.text ?? b.ingredient.name.length;
        if (typeof aLength === 'string' && typeof bLength === 'string') {
            return bLength.length - aLength.length;
        }
        return (b.ingredient.name.length) - (a.ingredient.name.length);
    });

    for (const element of sortedIngredients) {
        const ingredientName = element.ingredient.name.split(";").map(s => `(${s.trim()})`).join("|");
        const ingredientRegex = new RegExp(`${ingredientName}`, 'i');

        let j = 0;
        while (j < segments.length) {
            const currentSegment = segments[j];
            const matches = currentSegment.text.match(ingredientRegex);

            if (matches != null) {
                const { before, inside, after } = trifurcate(currentSegment.text, matches.index!, matches[0].length);

                if (matches.index === 0) {
                    segments.splice(
                        j,
                        1,
                        { text: inside, ingredient: element },
                        { text: after, ingredient: null }
                    );
                } else {
                    segments.splice(
                        j,
                        1,
                        { text: before, ingredient: null },
                        { text: inside, ingredient: element },
                        { text: after, ingredient: null }
                    );
                    j++;
                }
            }
            j++;
        }
    }

    return (
        <div>
            {segments.map((segment, i) => {
                if (segment.ingredient == null) {
                    return <span key={i}>{segment.text}</span>;
                }

                const newQuantity = segment.ingredient.quantity * newServings / recipe.servingsProduced;
                const tooltipTitle = `${newQuantity} ${segment.ingredient.unit}`;

                return (
                    <OverlayTrigger
                        key={i}
                        overlay={<Tooltip id={`tooltip-${i}`}>{tooltipTitle}</Tooltip>}
                    >
                        <a className="tooltip-style" href="#">{segment.text}</a>
                    </OverlayTrigger>
                );
            })}
        </div>
    );
}
