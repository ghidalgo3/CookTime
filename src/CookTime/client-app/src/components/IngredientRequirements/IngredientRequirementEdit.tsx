import React from "react";
import { Button, Col, Form, Row } from "react-bootstrap";
import { IngredientRequirement, MeasureUnit } from "src/shared/CookTime";
import { v4 as uuidv4 } from "uuid";
import { IngredientInput } from "../Ingredients/IngredientInput";

interface IngredientRequirementEditProps {
  ingredientRequirements: IngredientRequirement[];
  units: MeasureUnit[];
  onDelete: (ir: IngredientRequirement) => void;
  onNewIngredientRequirement: () => void;
  updateIngredientRequirement: (
    ir: IngredientRequirement,
    update: (ir: IngredientRequirement) => IngredientRequirement
  ) => void;
}

function getUnitDisplayName(unitName: string): string {
  switch (unitName.toLowerCase()) {
    // Weight
    case "ounce":
      return "oz";
    case "pound":
      return "lb";
    case "milligram":
      return "mg";
    case "gram":
      return "g";
    case "kilogram":
      return "kg";
    // Volume
    case "tablespoon":
      return "Tbsp";
    case "teaspoon":
      return "tsp";
    case "milliliter":
      return "mL";
    case "cup":
      return "cup";
    case "fluid_ounce":
      return "fl oz";
    case "pint":
      return "pint";
    case "quart":
      return "quart";
    case "gallon":
      return "gallon";
    case "liter":
      return "L";
    // Count
    case "count":
      return "unit";
    default:
      return unitName;
  }
}

function buildUnitOptions(units: MeasureUnit[]) {
  const countOptions = units
    .filter((u) => u.siType === "count")
    .map((unit) => (
      <option key={unit.name} value={unit.name}>
        {getUnitDisplayName(unit.name)}
      </option>
    ));

  const massOptions = units
    .filter((u) => u.siType === "weight")
    .map((unit) => (
      <option key={unit.name} value={unit.name}>
        {getUnitDisplayName(unit.name)}
      </option>
    ));

  const volumeOptions = units
    .filter((u) => u.siType === "volume")
    .map((unit) => (
      <option key={unit.name} value={unit.name}>
        {getUnitDisplayName(unit.name)}
      </option>
    ));

  return [
    { group: "count", options: countOptions },
    { group: "weight", options: massOptions },
    { group: "volume", options: volumeOptions },
  ].map((x, idx) => (
    <optgroup key={idx} label={x.group}>
      {x.options}
    </optgroup>
  ));
}

interface IngredientEditRowProps {
  ir: IngredientRequirement;
  idx: number;
  units: MeasureUnit[];
  ingredientRequirements: IngredientRequirement[];
  onDelete: (ir: IngredientRequirement) => void;
  updateIngredientRequirement: (
    ir: IngredientRequirement,
    update: (ir: IngredientRequirement) => IngredientRequirement
  ) => void;
}

function IngredientEditRow({
  ir,
  idx,
  units,
  ingredientRequirements,
  onDelete,
  updateIngredientRequirement,
}: IngredientEditRowProps) {
  const id =
    ir.ingredient.id === "" ||
    ir.ingredient.id === "00000000-0000-0000-0000-000000000000"
      ? idx.toString()
      : ir.ingredient.id;

  const unitOptions = buildUnitOptions(units);

  const handleQuantityChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    let newValue = parseFloat(e.target.value);
    if (Number.isNaN(newValue)) {
      newValue = 0.0;
    }
    updateIngredientRequirement(ir, (ir) => {
      ir.quantity = newValue;
      return ir;
    });
  };

  const handleUnitChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    updateIngredientRequirement(ir, (ir) => {
      ir.unit = e.currentTarget.value;
      return ir;
    });
  };

  const handleIngredientSelect = (
    text: string,
    ingredient: IngredientRequirement["ingredient"],
    isNew: boolean
  ) => {
    updateIngredientRequirement(ir, (ir) => {
      ir.ingredient = ingredient;
      ir.ingredient.isNew = isNew;
      ir.text = text;
      if (isNew) {
        ir.id = uuidv4();
      }
      return ir;
    });
  };

  return (
    <Row key={id} className="margin-bottom-8">
      <Col xs={2} className="ingredient-col-left">
        <Form.Control
          type="number"
          min="0"
          inputMode="decimal"
          onChange={handleQuantityChange}
          placeholder="0"
          value={ir.quantity === 0.0 ? "" : ir.quantity}
        />
      </Col>
      <Col xs={3} className="ingredient-col-middle">
        <Form.Select
          className="border-0"
          onChange={handleUnitChange}
          value={ir.unit}
        >
          {unitOptions}
        </Form.Select>
      </Col>
      <Col className="ingredient-col-right get-smaller">
        <IngredientInput
          isNew={ir.ingredient.isNew}
          query={(text) => `/api/recipe/ingredients?name=${text}`}
          ingredient={ir.ingredient}
          text={ir.text}
          className=""
          currentRequirements={ingredientRequirements}
          onSelect={handleIngredientSelect}
        />
      </Col>
      <Col xs={1}>
        <Button
          className="float-end"
          variant="danger"
          onClick={() => onDelete(ir)}
        >
          <i className="bi bi-trash"></i>
        </Button>
      </Col>
    </Row>
  );
}

export function IngredientRequirementEdit({
  ingredientRequirements,
  units,
  onDelete,
  onNewIngredientRequirement,
  updateIngredientRequirement,
}: IngredientRequirementEditProps) {
  return (
    <Form>
      {ingredientRequirements?.map((ir, idx) => (
        <IngredientEditRow
          key={ir.id || idx}
          ir={ir}
          idx={idx}
          units={units}
          ingredientRequirements={ingredientRequirements}
          onDelete={onDelete}
          updateIngredientRequirement={updateIngredientRequirement}
        />
      ))}
      <Col xs={12}>
        <Button
          variant="outline-primary"
          className="width-100"
          onClick={onNewIngredientRequirement}
        >
          New ingredient
        </Button>
      </Col>
    </Form>
  );
}
