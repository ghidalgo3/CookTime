import React, {useEffect, useState} from "react"
import { useLocation, useSearchParams } from "react-router-dom";
import { RecipeEdit } from "src/components/RecipeEdit/RecipeEdit";
import { Helmet } from 'react-helmet-async';

export const RECIPE_PAGE_PATH = "Recipes/Details";

export function Path(id: string) {
  return `/${RECIPE_PAGE_PATH}?id=${id}`;
}

export default function Recipe() {
  const [searchParams, setSearchParams] = useSearchParams();
  const origin = window.location.origin;
  const recipeId = searchParams.get("id");
  return (
    recipeId ?
    <>
      <Helmet>
        <link rel="canonical" href={`${origin}/${RECIPE_PAGE_PATH}?id=${recipeId}`} />
      </Helmet>
      <RecipeEdit recipeId={recipeId} multipart />
    </>
    :
    <>No</>
  );
}