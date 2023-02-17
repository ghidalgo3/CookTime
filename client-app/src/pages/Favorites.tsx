import React, {useEffect, useState} from "react"
import RecipeList from "src/components/RecipeList/RecipeList";
import { useTitle } from "src/shared/useTitle";

export default function Favorites() {
  useTitle("Favorites")
  return (
    <>
      <RecipeList title="Favorite Recipes" type="Favorites" />
    </>
  );
}