import React, {useEffect, useState} from "react"
import { LoaderFunctionArgs, useLoaderData, useSearchParams } from "react-router-dom";
import RecipeDisplay from "src/components/Recipe/RecipeDisplay";
import { RecipeEdit } from "src/components/RecipeEdit/RecipeEdit";
import { getMultiPartRecipe, MultiPartRecipe } from "src/shared/CookTime";
import { URLSearchParams } from "url";

export const RECIPE_PAGE_PATH = "/Recipes/Details";
export async function loader({params, request} : LoaderFunctionArgs) {
  const recipeId = new URL(request.url).searchParams.get("id") ?? "";
  return await getMultiPartRecipe(recipeId);
}
export function Path(id: string) {
  return `${RECIPE_PAGE_PATH}?id=${id}`;
}
export default function Recipe() {
  const [searchParams, setSearchParams] = useSearchParams();
  let recipe = useLoaderData() as MultiPartRecipe
  const recipeId = searchParams.get("id");
  return (
    recipeId ?
    <RecipeDisplay recipe={recipe} servings={Number.parseInt(searchParams.get("servings") ?? "0")} />
    // <RecipeEdit recipeId={recipeId} multipart />
    :
    <>No</>
  );
}