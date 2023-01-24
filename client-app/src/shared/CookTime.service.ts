import { Category } from "./CookTime";

export async function getCategories() : Promise<string[]> {
  const response = await fetch("/api/category/list")
  return await response.json()
}
