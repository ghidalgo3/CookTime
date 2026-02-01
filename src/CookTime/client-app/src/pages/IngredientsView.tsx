import React, { useEffect, useState } from "react"
import { Accordion, Form } from "react-bootstrap";
import IngredientUnifiedRow from "src/components/Ingredients/IngredientUnifiedRow";
import { IngredientUnified } from "src/shared/CookTime";
import { useTitle } from "src/shared/useTitle";

type SortField = 'name' | 'usage' | 'hasNutrition';
type SortDirection = 'asc' | 'desc';

export default function IngredientsView() {
  const [ingredients, setIngredients] = useState<IngredientUnified[]>([]);
  const [sortField, setSortField] = useState<SortField>('name');
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  const [error, setError] = useState<string | null>(null);

  const loadIngredients = async () => {
    try {
      const response = await fetch("/api/ingredient/unified");
      if (!response.ok) {
        const errorText = await response.text();
        setError(`Failed to load ingredients: ${errorText}`);
        return;
      }
      const result = await response.json() as IngredientUnified[];
      setIngredients(result);
      setError(null);
    } catch (err) {
      setError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
    }
  };

  useEffect(() => {
    loadIngredients();
  }, []);

  const sortedIngredients = [...ingredients].sort((a, b) => {
    let aValue: string | number | boolean;
    let bValue: string | number | boolean;

    if (sortField === 'name') {
      aValue = a.ingredientNames.toLowerCase();
      bValue = b.ingredientNames.toLowerCase();
    } else if (sortField === 'usage') {
      aValue = a.usage;
      bValue = b.usage;
    } else {
      aValue = a.hasNutrition ? 1 : 0;
      bValue = b.hasNutrition ? 1 : 0;
    }

    if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
    if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
    return 0;
  });

  const handleError = (errorMessage: string) => {
    setError(errorMessage);
  };

  const handleDeleted = () => {
    loadIngredients();
  };

  useTitle("Ingredients");

  return (
    <>
      <h1>Ingredients</h1>
      <p className="text-muted mb-3">
        Manage ingredient properties, nutrition data, and merge duplicate ingredients.
      </p>
      {error && (
        <div className="alert alert-danger alert-dismissible" role="alert">
          {error}
          <button
            type="button"
            className="btn-close"
            aria-label="Close"
            onClick={() => setError(null)}
          ></button>
        </div>
      )}
      <div className="d-flex gap-3 mb-3 align-items-center">
        <span className="fw-bold">Sort by:</span>
        <Form.Select
          style={{ width: 'auto' }}
          value={`${sortField}-${sortDirection}`}
          onChange={(e) => {
            const [field, direction] = e.target.value.split('-') as [SortField, SortDirection];
            setSortField(field);
            setSortDirection(direction);
          }}
        >
          <option value="name-asc">Name (A-Z)</option>
          <option value="name-desc">Name (Z-A)</option>
          <option value="usage-asc">Usage (Low to High)</option>
          <option value="usage-desc">Usage (High to Low)</option>
          <option value="hasNutrition-asc">Nutrition (Missing first)</option>
          <option value="hasNutrition-desc">Nutrition (Has first)</option>
        </Form.Select>
        <span className="text-muted">
          {sortedIngredients.length} ingredients
        </span>
      </div>
      <Accordion>
        {sortedIngredients.map((ingredient, idx) =>
          <IngredientUnifiedRow
            key={ingredient.ingredientId}
            eventKey={idx.toString()}
            onError={handleError}
            onDeleted={handleDeleted}
            {...ingredient}
          />
        )}
      </Accordion>
    </>
  );
}