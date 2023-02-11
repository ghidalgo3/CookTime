import React, {useEffect, useState} from "react"
import RecipeList from "src/components/RecipeList/RecipeList";

export default function Favorites() {
  return (
    <>
      <RecipeList title="Favorite Recipes" type="Favorites" />
    </>
  );
}