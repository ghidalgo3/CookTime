import React, {useEffect, useState} from "react"
import { Spinner } from "react-bootstrap";
import { IngredientInternalUpdate } from "src/shared/CookTime";

export default function IngredientInternalUpdateRow(props : IngredientInternalUpdate) {
  const {
    ingredientId,
    ingredientNames,
    gtinUpc,
    ndbNumber,
    countRegex,
    expectedUnitMass } = props;
  const [update, setUpdate] = useState(props);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [failed, setFailed] = useState(false);
  return (
    <tr>
      <th scope="row">{ingredientId}</th>
      <th>
        <input
          type="text"
          name="ingredientNames"
          placeholder="Ingredient names (semi-colon separated)"
          onChange={e => setUpdate({...update, ingredientNames: e.target.value})}
          value={update.ingredientNames} />
      </th>
      <th>
        <input
          type="number"
          name="ndbNumber"
          placeholder="NDB Number"
          onChange={e => setUpdate({...update, ndbNumber: Number.parseInt(e.target.value)})}
          value={update.ndbNumber === 0 ? "" : update.ndbNumber} />
        <input
          type="text"
          name="gtinUpc"
          placeholder="GTIN/UPC Number"
          onChange={e => setUpdate({...update, gtinUpc: e.target.value})}
          value={update.gtinUpc} />
      </th>
      <th>
        <input
          type="text"
          name="countRegex"
          onChange={e => setUpdate({...update, countRegex: e.target.value})}
          placeholder="Count RegEx"
          value={update.countRegex} />
      </th>
      <th>
        <input
          type="number"
          name="expectedUnitMass"
          step="0.001"
          onChange={e => setUpdate({...update, expectedUnitMass: Number.parseInt(e.target.value)})}
          value={update.expectedUnitMass} />
      </th>
      <th>
        <input type="hidden" name="ingredientId" value={ingredientId} />
        {isSubmitting ? <Spinner /> :
          <button className="btn btn-outline-success width-100" type="submit" onClick={async () => {
            setIsSubmitting(true);
            var response = await fetch("/api/ingredient/internalupdate", {
              method: "post",
              body: JSON.stringify(update),
              headers: {
                "Content-Type": "application/json"
              }
            })
            setIsSubmitting(false);
            if (response.ok) {
              setUpdate(await response.json());
              setFailed(false);
            } else {
              setFailed(true);
            }
          }}>{failed ? "Failed" : "Save"}</button>
        }
      </th>
    </tr>
  );
}