import React from "react";
import { Row } from "react-bootstrap";
import { IngredientRequirement, MeasureUnit } from "src/shared/CookTime";
import { UnitPreference } from "src/shared/units";
import { IngredientDisplay } from "../Ingredients/IngredientDisplay";

interface IngredientRequirementListProps {
  ingredientRequirements: IngredientRequirement[];
  units: MeasureUnit[];
  multiplier: number;
  unitPreference: UnitPreference;
}

export function IngredientRequirementList({
  ingredientRequirements,
  units,
  multiplier,
  unitPreference,
}: IngredientRequirementListProps) {
  return (
    <>
      {ingredientRequirements.map((ingredient, idx) => {
        const scaledQuantity = ingredient.quantity * multiplier;
        return (
          <Row key={idx} className="ingredient-item">
            <IngredientDisplay
              showAlternatUnit={true}
              units={units}
              unitPreference={unitPreference}
              ingredientRequirement={{ ...ingredient, quantity: scaledQuantity }}
            />
          </Row>
        );
      })}
    </>
  );
}
