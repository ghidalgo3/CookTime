import { AggregatedIngredient, IngredientInternalUpdate, PagedResult, RecipeGenerationResult, RecipeList, RecipeListWithRecipes, RecipeView, Review } from "./CookTime.types";

export const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

export const DietDetails = {
  TODAYS_TEN: "TodaysTen"
}

export async function getMultiPartRecipe(id: string) {
  const response = await fetch(`/api/multipartrecipe/${id}`);
  return await response.json();
}

export async function createRecipeWithName(recipeCreationArgs: { name: string }) {
  const response = await fetch("/api/multipartrecipe/create", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(recipeCreationArgs)
  });
  return response;
}

export async function importRecipeFromImage(recipeCreationArgs: FormData) {
  const response = await fetch("/api/multipartrecipe/importFromImage", {
    method: "POST",
    body: recipeCreationArgs
  });
  return response;
}

export async function generateRecipeFromImages(files: File[]): Promise<{ ok: boolean; data?: RecipeGenerationResult; error?: string }> {
  const formData = new FormData();
  files.forEach(file => formData.append("files", file));

  const response = await fetch("/api/recipe/generate-from-image", {
    method: "POST",
    body: formData
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ error: "Failed to generate recipe" }));
    return { ok: false, error: errorData.error || "Failed to generate recipe" };
  }

  const data = await response.json() as RecipeGenerationResult;
  return { ok: true, data };
}

export async function generateRecipeFromText(text: string): Promise<{ ok: boolean; data?: RecipeGenerationResult; error?: string }> {
  const response = await fetch("/api/recipe/generate-from-text", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ text })
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ error: "Failed to generate recipe" }));
    return { ok: false, error: errorData.error || "Failed to generate recipe" };
  }

  const data = await response.json() as RecipeGenerationResult;
  return { ok: true, data };
}

export async function generateRecipeImage(id: string) {
  const response = await fetch(`/api/multipartrecipe/${id}/generateImage`, {
    method: "POST",
  });
  return response;
}

export async function getCategories(): Promise<string[]> {
  const response = await fetch("/api/category/list")
  return await response.json()
}

export async function getNutritionData(id: string) {
  const response = await fetch(`/api/MultiPartRecipe/${id}/nutritionData`);
  return await response.json();
}

export async function getRecipeViews(args?: { search?: string, page?: number }): Promise<PagedResult<RecipeView>> {
  let query = "?"
  if (args) {
    const { search, page } = args;
    const queryParams = []
    if (search) {
      queryParams.push(`search=${encodeURIComponent(search)}`);
    }
    if (page) {
      queryParams.push(`page=${encodeURIComponent(page)}`);
    }
    query += queryParams.join("&")
  }
  const response = await fetch("/api/multipartrecipe" + query)
  return (await response.json()) as PagedResult<RecipeView>;
}

export async function getFeaturedRecipeViews() {
  const response = await fetch("/api/multipartrecipe/featured")
  return (await response.json()) as RecipeView[];
}

export async function getNewRecipeViews() {
  const response = await fetch("/api/multipartrecipe/new")
  return (await response.json()) as RecipeView[];
}

export async function getMyRecipes(args?: { page?: number }): Promise<PagedResult<RecipeView>> {
  let query = "";
  if (args?.page) {
    query = `?page=${encodeURIComponent(args.page)}`;
  }
  const response = await fetch("/api/multipartrecipe/mine" + query);
  return (await response.json()) as PagedResult<RecipeView>;
}

// Generic list API functions
export async function getLists(): Promise<RecipeList[]> {
  const response = await fetch("/api/lists");
  return await response.json();
}

export async function getList(listName: string): Promise<RecipeListWithRecipes | null> {
  const response = await fetch(`/api/lists/${encodeURIComponent(listName)}`);
  if (!response.ok) {
    return null;
  }
  return await response.json();
}

export async function addToList(listName: string, recipeId: string, quantity?: number): Promise<void> {
  const url = quantity !== undefined 
    ? `/api/lists/${encodeURIComponent(listName)}/${recipeId}?quantity=${quantity}`
    : `/api/lists/${encodeURIComponent(listName)}/${recipeId}`;
  await fetch(url, {
    method: "PUT"
  });
}

export async function updateRecipeQuantityInList(listName: string, recipeId: string, quantity: number): Promise<void> {
  await fetch(`/api/lists/${encodeURIComponent(listName)}/${recipeId}?quantity=${quantity}`, {
    method: "PATCH"
  });
}

export async function removeFromList(listName: string, recipeId: string): Promise<void> {
  await fetch(`/api/lists/${encodeURIComponent(listName)}/${recipeId}`, {
    method: "DELETE"
  });
}

// Convenience functions for favorites
export async function getFavoriteRecipeViews() {
  const list = await getList("Favorites");
  if (!list) {
    return [] as RecipeView[];
  }
  return (list.recipes ?? []).map(item => item.recipe) as RecipeView[];
}

export async function addToFavorites(recipeId: string): Promise<void> {
  return addToList("Favorites", recipeId);
}

export async function removeFromFavorites(recipeId: string): Promise<void> {
  return removeFromList("Favorites", recipeId);
}

// Convenience functions for groceries
export async function createGroceryList(): Promise<{ created: boolean }> {
  const response = await fetch("/api/lists/Groceries", {
    method: "POST"
  });
  return { created: response.status === 201 };
}

export async function getGroceryList() {
  return getList("Groceries");
}

export async function getGroceryListIngredients(): Promise<AggregatedIngredient[]> {
  const response = await fetch("/api/lists/Groceries/ingredients");
  if (!response.ok) {
    return [];
  }
  return await response.json();
}

export async function addToGroceries(recipeId: string, quantity?: number): Promise<void> {
  return addToList("Groceries", recipeId, quantity);
}

export async function removeFromGroceries(recipeId: string): Promise<void> {
  return removeFromList("Groceries", recipeId);
}

export async function updateGroceryRecipeQuantity(recipeId: string, quantity: number): Promise<void> {
  return updateRecipeQuantityInList("Groceries", recipeId, quantity);
}

export async function toggleGroceryIngredientSelected(ingredientId: string): Promise<boolean> {
  const response = await fetch(`/api/lists/Groceries/ingredients/${ingredientId}`, {
    method: "PUT"
  });
  const result = await response.json();
  return result.selected;
}

export async function clearGrocerySelectedIngredients(): Promise<number> {
  const response = await fetch("/api/lists/Groceries/ingredients", {
    method: "DELETE"
  });
  const result = await response.json();
  return result.clearedCount;
}

// List management functions
export async function createList(listName: string): Promise<{ created: boolean }> {
  const response = await fetch(`/api/lists/${encodeURIComponent(listName)}`, {
    method: "POST"
  });
  return { created: response.status === 201 };
}

export async function deleteList(listId: string): Promise<{ ok: boolean }> {
  const response = await fetch(`/api/lists/${listId}`, {
    method: "DELETE"
  });
  return { ok: response.ok };
}

export type RecipeListUpdateRequest = {
  name?: string;
  description?: string;
  isPublic?: boolean;
};

export async function updateListMetadata(listId: string, update: RecipeListUpdateRequest): Promise<RecipeList | null> {
  const response = await fetch(`/api/lists/${listId}`, {
    method: "PATCH",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(update)
  });
  if (!response.ok) {
    return null;
  }
  return await response.json();
}

export function toPagedResult<T>(values: T[]): PagedResult<T> {
  return {
    results: values,
    currentPage: 1,
    pageCount: 1,
    pageSize: values.length,
    rowCount: values.length,
    firstRowOnPage: 0,
    lastRowOnPage: values.length - 1
  }
}

export async function getReviews(recipeId: string) {
  const response = await fetch(`/api/multipartrecipe/${recipeId}/reviews`)
  return (await response.json()) as Review[];
}

export async function getInternalIngredientUpdates() {
  const response = await fetch(`/api/ingredient/internalUpdate`)
  return (await response.json()) as IngredientInternalUpdate[];
}

// Image management functions
export async function deleteRecipeImage(recipeId: string, imageId: string): Promise<{ ok: boolean; error?: string }> {
  const response = await fetch(`/api/multipartrecipe/${recipeId}/images/${imageId}`, {
    method: "DELETE"
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ error: "Failed to delete image" }));
    return { ok: false, error: errorData.error || "Failed to delete image" };
  }

  return { ok: true };
}

export async function reorderRecipeImages(recipeId: string, imageIds: string[]): Promise<{ ok: boolean; error?: string }> {
  const response = await fetch(`/api/multipartrecipe/${recipeId}/images/reorder`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ imageIds })
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ error: "Failed to reorder images" }));
    return { ok: false, error: errorData.error || "Failed to reorder images" };
  }

  return { ok: true };
}

export async function uploadRecipeImage(recipeId: string, file: File): Promise<{ ok: boolean; data?: { id: string; url: string }; error?: string }> {
  const formData = new FormData();
  formData.append("files", file);

  const response = await fetch(`/api/multipartrecipe/${recipeId}/image`, {
    method: "PUT",
    body: formData
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ error: "Failed to upload image" }));
    return { ok: false, error: errorData.error || "Failed to upload image" };
  }

  const data = await response.json();
  return { ok: true, data };
}

export function toTitleCase(str: string) {
  return str.replace(
    /\w\S*/g,
    function (txt) {
      return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    }
  );
}