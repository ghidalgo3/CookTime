import React, {useEffect, useState} from "react"

export default function IngredientNormalizer() {
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
        <tbody></tbody>
      </table>
    </>
  );
}