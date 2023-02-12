import React, {useEffect, useState} from "react"
import RecipeList from "src/components/RecipeList/RecipeList";

export default function MyRecipes() {
  return (
  <>
    <RecipeList title="My Recipes" type="Mine" />
  </>);
}