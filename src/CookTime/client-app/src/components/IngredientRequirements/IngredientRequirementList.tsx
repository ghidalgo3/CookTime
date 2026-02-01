import React from "react";
import { Row } from "react-bootstrap";
import { IngredientRequirement, MeasureUnit } from "src/shared/CookTime";
import { IngredientDisplay } from "../Ingredients/IngredientDisplay";

interface IngredientRequirementListProps {
  ingredientRequirements: IngredientRequirement[];
  units: MeasureUnit[];
  multiplier: number;
}

export function IngredientRequirementList({
  ingredientRequirements,
  units,
  multiplier,
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
              ingredientRequirement={{ ...ingredient, quantity: scaledQuantity }}
            />
          </Row>
        );
      })}
    </>
  );
}
