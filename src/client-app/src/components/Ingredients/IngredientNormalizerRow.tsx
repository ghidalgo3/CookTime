import React, { useEffect, useState } from "react"
import { EMPTY_GUID, IngredientReplacementRequest } from "src/shared/CookTime";

interface IngredientNormalizerRowProps extends IngredientReplacementRequest {
  onError: (error: string) => void;
}

export default function IngredientNormalizerRow(props: IngredientNormalizerRowProps) {
  const {
    replacedId,
    name,
    usage,
    hasNutrition,
    keptId,
    onError
  } = props;
  const [replacement, setReplacement] = useState(props);
  return (
    <tr>
      <th scope="row">{replacedId}</th>
      <th>{name}</th>
      <th>{usage}</th>
      <th style={{ textAlign: 'center', fontSize: '18px' }}>
        {hasNutrition ? '✓' : '✗'}
      </th>
      <th>
        <input
          type="text"
          name="replacementId"
          placeholder="Replace with (ID)"
          onChange={e => setReplacement({ ...replacement, keptId: e.target.value })}
          value={replacement.keptId === EMPTY_GUID ? "" : replacement.keptId} />
      </th>
      <th>
        <input type="hidden" name="ingredientId" value="@ingredient.Id" />
        <button
          className="btn btn-outline-success width-100"
          onClick={async (e) => {
            try {
              const requestBody = {
                fromIngredientId: replacedId,
                toIngredientId: replacement.keptId
              };
              const response = await fetch("/api/ingredient/replace", {
                method: "post",
                body: JSON.stringify(requestBody),
                headers: {
                  "Content-Type": "application/json"
                }
              })
              if (!response.ok) {
                const errorText = await response.text();
                onError(`Failed to replace ingredient: ${errorText}`);
                return;
              }
              // eslint-disable-next-line no-restricted-globals
              location.reload();
            } catch (err) {
              onError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
            }
          }}
          type="submit">
          Replace
        </button>
        {
          usage === 0 &&
          <button
            className="btn btn-outline-success width-100"
            type="submit"
            onClick={async (e) => {
              try {
                const response = await fetch(`/api/ingredient/${replacedId}`, {
                  method: "delete",
                })
                if (!response.ok) {
                  const errorText = await response.text();
                  onError(`Failed to delete ingredient: ${errorText}`);
                  return;
                }
                // eslint-disable-next-line no-restricted-globals
                location.reload();
              } catch (err) {
                onError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
              }
            }}
          >
            Delete
          </button>
        }
      </th>
    </tr>
  );
}