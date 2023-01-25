export async function getCategories() : Promise<string[]> {
  const response = await fetch("/api/category/list")
  return await response.json()
}

export async function getRecipeCards({search, page}: {search: string, page: number}) {
  const response = await fetch("/api/multipartrecipe")
  return await response.json()
}