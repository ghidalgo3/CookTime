import React, { useEffect, useState } from "react"
import IngredientInternalUpdateRow from "src/components/Ingredients/IngredientInternalUpdateRow";
import { getInternalIngredientUpdates, IngredientInternalUpdate } from "src/shared/CookTime";
import { useTitle } from "src/shared/useTitle";

export default function IngredientsView() {
  const [ingredients, setIngredients] = useState<IngredientInternalUpdate[]>([]);
  useEffect(() => {
    async function loadIngredients() {
      const result = await getInternalIngredientUpdates();
      setIngredients(result)
    }
    loadIngredients();
  }, [])
  useTitle("Ingredients View")
  return (
    <>
      <h1>Ingredients</h1>
      <table className="table table-sm">
        <thead>
          <tr>
            <th scope="col">ID</th>
            <th scope="col">Ingredient names</th>
            <th scope="col">Nutrition description</th>
            <th scope="col">NDB/GTIN/UPC Number</th>
            <th scope="col">Count RegEx</th>
            <th scope="col">Unit mass (kg)</th>
            <th scope="col">Save/Delete</th>
          </tr>
        </thead>
        <tbody>{ingredients.map((i, idx) =>
          <IngredientInternalUpdateRow
            key={idx}
            {...i} />)}
        </tbody>
      </table>
    </>
  )
}