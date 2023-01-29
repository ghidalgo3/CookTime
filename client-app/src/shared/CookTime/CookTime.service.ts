import { PagedResult, RecipeView } from "./CookTime.types";

export async function getCategories() : Promise<string[]> {
  const response = await fetch("/api/category/list")
  return await response.json()
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