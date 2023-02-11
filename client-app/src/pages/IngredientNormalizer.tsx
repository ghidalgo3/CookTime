import React, {useEffect, useState} from "react"
import IngredientNormalizerRow from "src/components/Ingredients/IngredientNormalizerRow";
import { IngredientReplacementRequest } from "src/shared/CookTime";

export default function IngredientNormalizer() {
  const [replacements, setReplacements] = useState<IngredientReplacementRequest[]>([])
  useEffect(() => {
    async function loadReplacements() {
      const request = await fetch("/api/ingredient/normalized");
      const result = await request.json() as IngredientReplacementRequest[];
      setReplacements(result);
    }
    loadReplacements();
  }, [])
  return (
    <>
      <h1>Ingredients</h1>

      <table className="table table-sm">
        <thead>
          <tr>
            <th scope="col">ID</th>
            <th scope="col">Ingredient names</th>
            <th scope="col">Recipes using</th>
            <th scope="col">Replace with</th>
            <th scope="col">Save/Delete</th>
          </tr>
        </thead>
        <tbody>
          {replacements?.map((r, i) => <IngredientNormalizerRow key={i} {...r} />)}
        </tbody>
      </table>
    </>
  );
}