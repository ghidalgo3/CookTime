import { IngredientInternalUpdate, PagedResult, RecipeView, Review } from "./CookTime.types";

export const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

export const DietDetails = {
  TODAYS_TEN: "TodaysTen"
}

export async function getMultiPartRecipe(id : string) {
  const response = await fetch(`/api/multipartrecipe/${id}`);
  return await response.json();
}

export async function createRecipeWithName(recipeCreationArgs : {name: string, body?: string}) {
  const form = new FormData();
  form.set("name", recipeCreationArgs.name);
  if (recipeCreationArgs.body) {
    form.set("body", recipeCreationArgs.body);
  }
  const response = await fetch("/api/multipartrecipe/create", {
    method: "post",
    body: form
  });
  return response;
}

export async function importRecipeFromImage(recipeCreationArgs : FormData) {
  const response = await fetch("/api/multipartrecipe/importFromImage", {
    method: "POST",
    body: recipeCreationArgs
  });
  return response;
}

export async function getCategories() : Promise<string[]> {
  const response = await fetch("/api/category/list")
  return await response.json()
}

export async function getNutritionData(id : string) {
  const response = await fetch(`/api/MultiPartRecipe/${id}/nutritionData`);
  return await response.json();
}

export async function getRecipeViews(args?: {search?: string, page?: number}) : Promise<PagedResult<RecipeView>>{
  let query = "?"
  if (args) {
    const {search, page} = args;
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

export async function getFavoriteRecipeViews() {
  const response = await fetch("/api/multipartrecipe/favorites")
  return (await response.json()) as RecipeView[];
}

export async function getMyRecipes() {
  const response = await fetch("/api/multipartrecipe/mine")
  return (await response.json()) as RecipeView[];
}

export async function addToFavorites(recipeId: string) : Promise<void> {
  const response = await fetch(`/api/multipartrecipe/${recipeId}/favorite`,
    {
      method: "put"
    }
  );
}

export async function removeFromFavorites(recipeId: string) : Promise<void> {
  const response = await fetch(`/api/multipartrecipe/${recipeId}/favorite`,
    {
      method: "delete"
    }
  );
}

export function toPagedResult<T>(values : T[]) : PagedResult<T> {
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

export async function getReviews(recipeId : string) {
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