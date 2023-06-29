import React, { useEffect, useState } from "react"
import { EMPTY_GUID, IngredientReplacementRequest } from "src/shared/CookTime";

export default function IngredientNormalizerRow(props: IngredientReplacementRequest) {
  const {
    replacedId,
    name,
    usage,
    keptId
  } = props;
  const [replacement, setReplacement] = useState(props);
  return (
    <tr>
      <th scope="row">{replacedId}</th>
      <th>{name}</th>
      <th>{usage}</th>
      <th>
        <input
          type="text"
          name="replacementId"
          placeholder="Replace for (ID)"
          onChange={e => setReplacement({ ...replacement, keptId: e.target.value })}
          value={replacement.keptId === EMPTY_GUID ? "" : replacement.keptId} />
      </th>
      <th>
        <input type="hidden" name="ingredientId" value="@ingredient.Id" />
        <button
          className="btn btn-outline-success width-100"
          onClick={async (e) => {
            const response = await fetch("/api/ingredient/replace", {
              method: "post",
              body: JSON.stringify(replacement),
              headers: {
                "Content-Type": "application/json"
              }
            })
            if (response.ok) {
              // eslint-disable-next-line no-restricted-globals
              location.reload();
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
              const response = await fetch(`/api/ingredient/${replacedId}`, {
                method: "delete",
              })
              if (response.ok) {
                // eslint-disable-next-line no-restricted-globals
                location.reload();
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