import React, {useEffect, useState} from "react"
import { useSearchParams } from "react-router-dom";
import { RecipeEdit } from "src/components/RecipeEdit/RecipeEdit";

export default function Recipe() {
  const [searchParams, setSearchParams] = useSearchParams();
  const recipeId = searchParams.get("id");
  return (
    recipeId ?
    <RecipeEdit recipeId={recipeId} multipart />
    :
    <>No</>
  );
}