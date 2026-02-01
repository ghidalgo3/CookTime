import React, { useEffect, useState } from "react"
import { Accordion, Form } from "react-bootstrap";
import IngredientNormalizerRow from "src/components/Ingredients/IngredientNormalizerRow";
import { IngredientReplacementRequest } from "src/shared/CookTime";
import { useTitle } from "src/shared/useTitle";

type SortField = 'name' | 'usage' | 'hasNutrition';
type SortDirection = 'asc' | 'desc';

export default function IngredientNormalizer() {
  const [replacements, setReplacements] = useState<IngredientReplacementRequest[]>([])
  const [sortField, setSortField] = useState<SortField>('name')
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc')
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function loadReplacements() {
      try {
        const request = await fetch("/api/ingredient/normalized");
        if (!request.ok) {
          const errorText = await request.text();
          setError(`Failed to load ingredients: ${errorText}`);
          return;
        }
        const result = await request.json() as IngredientReplacementRequest[];
        setReplacements(result);
        setError(null); // Clear any previous errors
      } catch (err) {
        setError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
      }
    }
    loadReplacements();
  }, [])

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc')
    } else {
      setSortField(field)
      setSortDirection('asc')
    }
  }

  const sortedReplacements = [...replacements].sort((a, b) => {
    let aValue: any, bValue: any;

    if (sortField === 'name') {
      aValue = a.name.toLowerCase();
      bValue = b.name.toLowerCase();
    } else if (sortField === 'usage') {
      aValue = a.usage;
      bValue = b.usage;
    } else if (sortField === 'hasNutrition') {
      aValue = a.hasNutrition ? 1 : 0;
      bValue = b.hasNutrition ? 1 : 0;
    }

    if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
    if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
    return 0;
  })

  const getSortIcon = (field: SortField) => {
    if (sortField !== field) return '↕️';
    return sortDirection === 'asc' ? '↑' : '↓';
  }

  const handleError = (errorMessage: string) => {
    setError(errorMessage);
  }

  useTitle("Ingredient Normalizer")
  return (
    <>
      <h1>Ingredient Normalizer</h1>
      <p className="text-muted mb-3">
        When you find an ingredient that should be replaced with another one, copy the ID of the ingredient "to remove" and paste it in the text field of the ingredient "to keep".
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
          {sortedReplacements.length} ingredients
        </span>
      </div>
      <Accordion>
        {sortedReplacements?.map((r, i) =>
          <IngredientNormalizerRow
            key={r.replacedId}
            eventKey={i.toString()}
            {...r}
            onError={handleError}
          />
        )}
      </Accordion>
    </>
  );
}