import moment from "moment";
import React from "react";

type RecipeStructuredDataProps = {
    recipe : MultiPartRecipe,
    images : Image[]
}

export class RecipeStructuredData extends React.Component<RecipeStructuredDataProps, {}>
{
    constructor(props: RecipeStructuredDataProps) {
        super(props);
    }

    render() {
        let {recipe, images} = this.props;
        let metadata = {
            "@context": "https://schema.org/",
            "@type": "Recipe",
            "name": recipe.name,
            "author": {
                "@type": "Person",
                "name": recipe.owner?.userName
            },
            "image": images.map(image => {
                return `${window.location.origin}/image/${image.id}`
            }),
            "cookTime": `${moment.duration(recipe.cooktimeMinutes, 'minutes').toISOString()}`,
            "recipeIngredient": recipe.recipeComponents.flatMap(component => {
                return component.ingredients?.map(ir => {
                    return `${ir.quantity} ${ir.unit} ${ir.ingredient.name}`
                })
            }),
            // TODO support HowToSection properly
            "recipeInstructions": recipe.recipeComponents.flatMap(component => {
                return component.steps?.map(step => {
                    return {
                        "@type": "HowToStep",
                        "text": step.text
                    }
                })
            }),
            "aggregateRating": {
                "@type": "AggregateRating",
                "ratingValue": recipe.averageReviews,
                "ratingCount": recipe.reviewCount
            },

        }
        return (
            <script type="application/ld+json">
                {JSON.stringify(metadata)}
            </script>
        )
    }
}