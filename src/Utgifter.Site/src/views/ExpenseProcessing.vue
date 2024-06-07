<script setup lang="ts">
import {
  deleteExpense,
  processExpenses,
  updateExpense,
  uploadExpenses,
  type Expense,
  type ExpenseReport
} from '@/api/server'
import ExpensesTable from '@/components/ExpensesTable.vue'
import Button from '@/components/ui/button.vue'
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const report = ref<ExpenseReport | null>(null)
const file = ref<File | undefined>(undefined)

const onDragOver = () => (isHovered.value = true)
const isHovered = ref(false)
const onDragLeave = () => (isHovered.value = false)
const onDrop = async (e: DragEvent) => {
  e.preventDefault()
  isHovered.value = false
  file.value = e.dataTransfer?.files?.[0]

  report.value = await processExpenses(file.value!)
}

async function handleFileChange(e: Event) {
  const fileInput = e.target as HTMLInputElement
  file.value = fileInput.files?.[0]
  report.value = await processExpenses(file.value!)
}

function triggerFileInput() {
  const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
  fileInput.click()
}

const newExpenses = ref<Expense[]>([])
const existingExpenses = ref<Expense[]>([])

function removeNewExpense(expense: Expense, page: number, size: number) {
  report.value!.newExpenses = report.value?.newExpenses.filter((e) => e.id !== expense.id) ?? []
  reloadNewExpenses(page, size)
}

function reloadNewExpenses(page: number, size: number) {
  newExpenses.value = report.value?.newExpenses.slice(page * size, (page + 1) * size) ?? []
}

async function removeFromServer(expense: Expense, page: number, size: number) {
  await deleteExpense(expense)
  report.value!.existingExpenses = report.value!.existingExpenses.filter((e) => e.id !== expense.id)
  reloadExistingExpenses(page, size)
}
async function editFromServer(expense: Expense, page: number, size: number) {
  await updateExpense(expense)
}

function reloadExistingExpenses(page: number, size: number) {
  existingExpenses.value =
    report?.value?.existingExpenses.slice(page * size, (page + 1) * size) ?? []
}

async function saveNewExpenses() {
  await uploadExpenses(report.value!.newExpenses)
  router.push('/expenses')
}
</script>
<template>
  <div
    v-if="report === null"
    @dragover.prevent="onDragOver"
    @dragleave.prevent="onDragLeave"
    @drop.prevent="onDrop"
    :class="[
      'transition-colors',
      'duration-300',
      'ease-in-out',
      'hover:bg-black/10',
      'hover:border-black/50'
    ]"
    class="flex items-center m-2 justify-center h-screen border-4 rounded-2xl border-dashed border-gray-400"
  >
    <div class="text-center">
      <h1 class="text-2xl font-bold mb-4">Upload Expenses</h1>
      <form id="upload-form" class="flex flex-col items-center">
        <div>
          <input required type="file" @change="handleFileChange" class="hidden" />
          <button @click="triggerFileInput" class="px-4 py-2 bg-blue-500 text-white rounded">
            Select File
          </button>

          <p class="mt-4">or drag and drop a file here</p>
        </div>
      </form>
    </div>
  </div>
  <div v-else class="flex justify-center gap-3 flex-col mx-11 mt-5">
    <div class="inline-flex flex-row justify-between">
      <Button variant="danger">
        <RouterLink to="/expenses">Cancel</RouterLink>
      </Button>
      <Button @click="saveNewExpenses">Upload New Expenses</Button>
    </div>
    <span class="font-bold text-2xl">New Expenses</span>

    <ExpensesTable
      :expenses="newExpenses"
      @remove-expense="removeNewExpense"
      @load-expenses="reloadNewExpenses"
    />

    <span class="font-bold text-2xl">Existing Expenses</span>

    <ExpensesTable
      :expenses="existingExpenses"
      @edit-expense="editFromServer"
      @remove-expense="removeFromServer"
      @load-expenses="reloadExistingExpenses"
    />
  </div>
</template>
