import React, { useEffect, useState } from "react"
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
      <h1>Ingredients</h1>
      <h2>
        How to use
      </h2>
      <p>
        When you find an ingredient that should be replace with another one, copy the ID of the ingredient "to remove" and paste it in the text field of the ingredient "to keep".
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
      <table className="table table-sm">
        <thead>
          <tr>
            <th scope="col">ID</th>
            <th
              scope="col"
              style={{ cursor: 'pointer' }}
              onClick={() => handleSort('name')}
            >
              Ingredient names {getSortIcon('name')}
            </th>
            <th
              scope="col"
              style={{ cursor: 'pointer' }}
              onClick={() => handleSort('usage')}
            >
              Recipes using {getSortIcon('usage')}
            </th>
            <th
              scope="col"
              style={{ cursor: 'pointer' }}
              onClick={() => handleSort('hasNutrition')}
            >
              Has Nutrition {getSortIcon('hasNutrition')}
            </th>
            <th scope="col">Replace with</th>
            <th scope="col">Save/Delete</th>
          </tr>
        </thead>
        <tbody>
          {sortedReplacements?.map((r, i) => <IngredientNormalizerRow key={i} {...r} onError={handleError} />)}
        </tbody>
      </table>
    </>
  );
}