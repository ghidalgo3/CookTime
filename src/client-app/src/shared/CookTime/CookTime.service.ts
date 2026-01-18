import { IngredientInternalUpdate, PagedResult, RecipeList, RecipeListWithRecipes, RecipeView, Review } from "./CookTime.types";

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

export async function addToList(listName: string, recipeId: string): Promise<void> {
  await fetch(`/api/lists/${encodeURIComponent(listName)}/${recipeId}`, {
    method: "PUT"
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
  return (list.recipes ?? []) as RecipeView[];
}

export async function addToFavorites(recipeId: string): Promise<void> {
  return addToList("Favorites", recipeId);
}

export async function removeFromFavorites(recipeId: string): Promise<void> {
  return removeFromList("Favorites", recipeId);
}

// Convenience functions for groceries
export async function getGroceryList() {
  return getList("Groceries");
}

export async function addToGroceries(recipeId: string): Promise<void> {
  return addToList("Groceries", recipeId);
}

export async function removeFromGroceries(recipeId: string): Promise<void> {
  return removeFromList("Groceries", recipeId);
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

export function toTitleCase(str: string) {
  return str.replace(
    /\w\S*/g,
    function (txt) {
      return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    }
  );
}