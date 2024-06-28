<script setup lang="ts">
import { ref } from 'vue'
import { deleteExpense, getExpenses, updateExpense } from '@/api/server'
import type { Expense } from '@/api/server'
import ExpensesTable from '@/components/ExpensesTable'
import Button from '@/components/ui/button.vue'
import { RouterLink } from 'vue-router'

const expenses = ref<Expense[]>([])
async function updateExpenses(page: number, size: number) {
  expenses.value = await getExpenses(page, size)
}
async function remove(expense: Expense, page: number, size: number) {
  await deleteExpense(expense)
  await updateExpenses(page, size)
}
async function edit(expense: Expense, page: number, size: number) {
  await updateExpense(expense)
  await updateExpenses(page, size)
}
</script>

<template>
  <div class="flex justify-center gap-3 flex-col mx-11 mt-5">
    <div class="flex flex-row gap-3">
      <Button variant="primary">
        <RouterLink to="/expenses/upload"> Upload Expenses</RouterLink>
      </Button>
      <Button variant="primary">
        <RouterLink to="/rules"> Rules </RouterLink>
      </Button>
    </div>
    <ExpensesTable
      :expenses="expenses"
      @load-expenses="updateExpenses"
      @edit-expense="edit"
      @remove-expense="remove"
    />
  </div>
</template>
