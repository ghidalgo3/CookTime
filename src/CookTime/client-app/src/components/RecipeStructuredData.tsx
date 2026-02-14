import moment from "moment";
import React from "react";
import { MultiPartRecipe, Image } from "src/shared/CookTime";

type RecipeStructuredDataProps = {
  recipe: MultiPartRecipe,
  images: Image[]
}

export function RecipeStructuredData({ recipe, images }: RecipeStructuredDataProps) {
  const metadata: any = {
    "@context": "https://schema.org/",
    "@type": "Recipe",
    "name": recipe.name,
    "author": {
      "@type": "Person",
      "name": recipe.owner?.userName
    },
    "image": images.map(image => image.url),
    "recipeYield": recipe.servingsProduced,
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
          "text": step
        }
      })
    }),
  }

  if (recipe.reviewCount > 0) {
    metadata["aggregateRating"] = {
      "@type": "AggregateRating",
      "ratingValue": recipe.averageReviews,
      "ratingCount": recipe.reviewCount,
      "bestRating": 5,
      "worstRating": 1
    }
  }

  return (
    <script type="application/ld+json">
      {JSON.stringify(metadata)}
    </script>
  )
}