export async function getTrips(): Promise<string[]> {
  return await fetch('/api/trips')
    .then((response) => response.json())
    .then((data) => data.trips)
}
