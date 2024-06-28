export async function getCategories(): Promise<string[]> {
  return await fetch('/api/categories')
    .then((response) => response.json())
    .then((data) => data.categories)
}
