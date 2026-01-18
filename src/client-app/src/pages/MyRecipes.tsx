import React from "react"
import { useSearchParams } from "react-router";
import RecipeList from "src/components/RecipeList/RecipeList";
import { useTitle } from "src/shared/useTitle";

export default function MyRecipes() {
  useTitle("My Recipes")
  const [searchParams] = useSearchParams();
  return (
    <>
      <RecipeList title="My Recipes" type="Mine" query={searchParams} />
    </>);
}