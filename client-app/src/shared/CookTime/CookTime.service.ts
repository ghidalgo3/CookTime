import { RecipeView } from "./CookTime.types";

export async function getCategories() : Promise<string[]> {
  const response = await fetch("/api/category/list")
  return await response.json()
}

export async function getRecipeViews(args?: {search?: string, page?: number}) : Promise<RecipeView[]>{
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
  return (await response.json()) as RecipeView[];
}