import React from "react"
import { useSearchParams, useLocation } from "react-router";
import { RecipePage } from "src/components/Recipe";
import { Helmet } from 'react-helmet-async';
import { RecipeGenerationResult } from "src/shared/CookTime";

export const RECIPE_PAGE_PATH = "Recipes/Details";

export function Path(id: string) {
  return `/${RECIPE_PAGE_PATH}?id=${id}`;
}

type LocationState = {
  generatedRecipe?: RecipeGenerationResult;
}

export default function Recipe() {
  const [searchParams] = useSearchParams();
  const location = useLocation();
  const origin = typeof window !== 'undefined' ? window.location.origin : '';
  const recipeId = searchParams.get("id");
  const generatedRecipe = (location.state as LocationState)?.generatedRecipe;

  return (
    recipeId ?
      <>
        <Helmet>
          <link rel="canonical" href={`${origin}/${RECIPE_PAGE_PATH}?id=${recipeId}`} />
        </Helmet>
        <RecipePage recipeId={recipeId} generatedRecipe={generatedRecipe} />
      </>
      :
      <>No</>
  );
}