import React from "react";
import { OverlayTrigger, Tooltip } from "react-bootstrap";
import { IngredientRequirement, MultiPartRecipe, Recipe, RecipeComponent, RecipeStep } from "src/shared/CookTime";
type Segment = {
    ingredient: IngredientRequirement | null,
    text: string
}
export class Step extends React.Component<{
    recipe: Recipe | MultiPartRecipe,
    recipeStep: RecipeStep,
    multipart: boolean,
    component?: RecipeComponent,
    newServings: number
}, {}> {
    constructor(props: any) {
        super(props);
    }

    trifurcate(s: string, position: number, length: number) {
        let before = s.substring(0, position)
        let inside = s.substring(position, position + length)
        let after = s.substring(position + length, s.length)
        return {
            before: before,
            inside: inside,
            after: after
        }
    }

    render() {
        // list ingredient strings array
        // foreach ingredient find matches of this string in the text
        // insert tooltip in each match
        let originalText = this.props.recipeStep.text
        let segments: Segment[] = [{ ingredient: null, text: originalText }]
        let ingredientRequirements: IngredientRequirement[] = []
        if (this.props.multipart) {
            ingredientRequirements = this.props.component!.ingredients!
        } else {
            ingredientRequirements = (this.props.recipe as Recipe).ingredients ?? []
        }
        let copyIr = Array.from(ingredientRequirements);
        copyIr.sort((a, b) => {
            if (a.text != null && b.text != null) {
                return b.text.length - a.text.length;
            } else {
                return b.ingredient.name.length - a.ingredient.name.length
            }
        })
        for (let i = 0; i < copyIr.length; i++) {
            // console.log(segments);
            const element = copyIr![i];
            let ingredientName = element.ingredient.name.split(";").map(s => `(${s.trim()})`).join("|");
            // let ingredientName = element.ingredient.name.split(";").map(s => s.trim()).join("|");
            // console.log(`Matching ingredient ${ingredientName}`);
            let ingredientRegex = new RegExp(`${ingredientName}`, 'i')
            let j = 0;
            while (j < segments.length) {
                let currentSegment = segments[j];
                // console.log(`Current segment ${j} is '${currentSegment.text}' Segments length ${segments.length}`)
                let matches = currentSegment.text.match(ingredientRegex)
                if (matches != null) {
                    // console.log(matches);
                    // console.log(`Match for regex ${ingredientName} found at index ${matches.index!} for current segment '${currentSegment.text} and match length ${matches}`)
                    let trifurcation = this.trifurcate(currentSegment.text, matches.index!, matches[0].length);
                    if (matches.index! == 0) {
                        // array size grows by 1
                        segments.splice(
                            j,
                            1,
                            { text: trifurcation.inside, ingredient: element },
                            { text: trifurcation.after, ingredient: null })
                        // console.log(segments);
                        j++;
                    } else {
                        // array size grows by 2
                        segments.splice(
                            j,
                            1,
                            { text: trifurcation.before, ingredient: null },
                            { text: trifurcation.inside, ingredient: element },
                            { text: trifurcation.after, ingredient: null })
                        // console.log(segments);
                        j++;
                    }
                }
                j++;
            }
        }
        const Link = ({ id, children, title }: { id: any, children: any, title: any }) => (
            <OverlayTrigger overlay={<Tooltip id={id}>{title}</Tooltip>}>
                <a className="tooltip-style" href="javascript:void(0);">{children}</a>
            </OverlayTrigger>
        );
        return (
            <div>
                {segments.map((segment, i) => {
                    if (segment.ingredient == null) {
                        return segment.text
                    } else {
                        let newQuantity = segment.ingredient.quantity * this.props.newServings / this.props.recipe.servingsProduced;
                        let tooltipTitle = `${newQuantity} ${segment.ingredient.unit}`;
                        return (
                            <Link title={tooltipTitle} id={i}>
                                {segment.text}
                            </Link>
                        )
                    }
                })}
            </div>)
    }
}