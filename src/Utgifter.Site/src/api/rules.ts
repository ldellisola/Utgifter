import type { Error } from './error'
import type { Expense } from './server'

export type Rule = {
  id?: string
  expectedStore: string
  newStore?: string
  newCategory?: string
  trip?: boolean
  shared?: boolean
}
export async function createRule(rule: Rule): Promise<[Error | null, Rule | null]> {
  const result = await fetch('/api/rules/', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(rule)
  })

  const json = await result.json()
  if (result.ok) return [null, json]
  return [await json, null]
}

export async function getRules(pageNumber?: number, pageSize?: number): Promise<Rule[]> {
  return await fetch(`/api/rules?pageSize=${pageSize ?? 10}&pageNumber=${pageNumber ?? 0}`)
    .then((response) => response.json())
    .then((data) => data.rules)
}

export async function deleteRule(rule: Rule): Promise<void> {
  await fetch(`/api/rules/${rule.id}`, { method: 'DELETE' })
}

export async function updateRule(rule: Rule): Promise<void> {
  await fetch(`/api/rules/${rule.id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(rule)
  })
}

export type RenamePreview = {
  expenses: Expense[]
  expectedStore: string
  newStore?: string
}

export async function getRenamePreview(ruleId: string): Promise<RenamePreview> {
  const response = await fetch(`/api/rules/${ruleId}/rename-preview`)
  return response.json()
}

export async function applyRename(ruleId: string): Promise<{ affectedRows: number }> {
  const response = await fetch(`/api/rules/${ruleId}/rename-apply`, { method: 'POST' })
  return response.json()
}
