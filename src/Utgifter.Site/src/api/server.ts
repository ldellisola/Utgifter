export type Expense = {
  id: string
  amount: number
  date: Date
  person: string
  store: string
  city: string
  originalCurrency: string
  hash: string
  category?: string
  shared: boolean
  trip?: string
}

export async function getExpenses(pageNumber?: number, pageSize?: number): Promise<Expense[]> {
  return await fetch(`/api/expenses?pageSize=${pageSize ?? 10}&pageNumber=${pageNumber ?? 0}`)
    .then((response) => response.json())
    .then((data) => data.expenses)
}

export type ExpenseReport = {
  existingExpenses: Expense[]
  newExpenses: Expense[]
}

export type ApiError = {
  status: string
  code: number
  reason: string
  note: string
}

export async function processExpenses(file: File): Promise<ExpenseReport> {
  const formData = new FormData()
  formData.append('ExpenseFile', file)

  const response = await fetch('/api/expenses/upload', { method: 'POST', body: formData })

  if (!response.ok) {
    const error: ApiError = await response.json()
    throw new Error(error.reason)
  }

  return response.json()
}

export async function deleteExpense(expense: Expense): Promise<void> {
  await fetch(`/api/expenses/${expense.id}`, { method: 'DELETE' })
}

export async function updateExpense(expense: Expense): Promise<void> {
  await fetch(`/api/expenses/`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ expenses: [expense] })
  })
}

export async function uploadExpenses(expenses: Expense[]): Promise<void> {
  await fetch('/api/expenses/', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ expenses })
  })
}
