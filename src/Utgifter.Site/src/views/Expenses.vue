<script setup lang="ts">
import { ref } from 'vue'
import { deleteExpense, getExpenses, updateExpense } from '@/api/server'
import type { Expense } from '@/api/server'
import ExpensesTable from '@/components/ExpensesTable'
import Button from '@/components/ui/button.vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const fileInput = ref<HTMLInputElement | null>(null)
const isDragging = ref(false)

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

function triggerFileInput() {
  fileInput.value?.click()
}

function handleFileChange(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    navigateWithFile(file)
  }
}

function onDragOver(e: DragEvent) {
  e.preventDefault()
  isDragging.value = true
}

function onDragLeave(e: DragEvent) {
  e.preventDefault()
  isDragging.value = false
}

function onDrop(e: DragEvent) {
  e.preventDefault()
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) {
    navigateWithFile(file)
  }
}

function navigateWithFile(file: File) {
  router.push({ name: 'upload', state: { file } })
}
</script>

<template>
  <div
    class="min-h-screen"
    @dragover="onDragOver"
    @dragleave="onDragLeave"
    @drop="onDrop"
  >
    <div
      v-if="isDragging"
      class="fixed inset-0 bg-blue-50/80 border-4 border-dashed border-blue-400 z-50 flex items-center justify-center pointer-events-none"
    >
      <span class="text-2xl font-bold text-blue-600">Drop file to upload</span>
    </div>
    <div class="flex gap-3 flex-col mx-11 mt-5">
      <input
        ref="fileInput"
        type="file"
        accept=".xlsx,.xls"
        class="hidden"
        @change="handleFileChange"
      />
      <div class="flex flex-row gap-3">
        <Button variant="primary" @click="triggerFileInput">Upload Expenses</Button>
        <Button variant="primary" to="/rules">Rules</Button>
      </div>
      <ExpensesTable
        :expenses="expenses"
        @load-expenses="updateExpenses"
        @edit-expense="edit"
        @remove-expense="remove"
      />
    </div>
  </div>
</template>
