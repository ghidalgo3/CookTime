import React, {useEffect, useState} from "react"
import RecipeList from "src/components/RecipeList/RecipeList";
import { useTitle } from "src/shared/useTitle";

export default function MyRecipes() {
  useTitle("My Recipes")
  return (
  <>
    <RecipeList title="My Recipes" type="Mine" />
  </>);
}