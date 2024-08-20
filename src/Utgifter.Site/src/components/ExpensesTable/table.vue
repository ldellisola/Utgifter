<script setup lang="ts">
import { ref, watch } from 'vue'
import { getCategories } from '@/api/categories'
import { getTrips } from '@/api/trips'
import type { Expense } from '@/api/server'
import AutoComplete from '@/components/ui/autocomplete'
import Button from '@/components/ui/button.vue'
import Header from './header.vue'
import Cell from './cell.vue'
import FocusableInput from '../ui/FocusableInput.vue'
type Props = { expenses: Expense[] }

defineProps<Props>()

const emit = defineEmits<{
  loadExpenses: [page: number, size: number]
  removeExpense: [expense: Expense, page: number, size: number]
  editExpense: [expense: Expense, page: number, size: number]
}>()

const pageNumber = ref(0)
const pageSize = ref(10)

async function nextPage() {
  pageNumber.value++
  emit('loadExpenses', pageNumber.value, pageSize.value)
}

async function previousPage() {
  pageNumber.value--
  emit('loadExpenses', pageNumber.value, pageSize.value)
}

watch(
  pageSize,
  async () => {
    pageNumber.value = 0
    emit('loadExpenses', pageNumber.value, pageSize.value)
  },
  { immediate: true }
)

async function remove(expense: Expense) {
  emit('removeExpense', expense, pageNumber.value, pageSize.value)
}
async function edit(expense: Expense) {
  emit('editExpense', expense, pageNumber.value, pageSize.value)
}

const categories = ref<string[]>(await getCategories())
async function updateExpeseWithCategory(expense: Expense, newCategory: boolean) {
  await edit(expense)
  if (newCategory) {
    setTimeout(async () => {
      categories.value = await getCategories()
    }, 1000)
    // categories.value = await getCategories()
  }
}

const trips = ref<string[]>(await getTrips())
async function updateExpeseWithTrip(expense: Expense, newTrip: boolean) {
  await edit(expense)
  if (newTrip) {
    setTimeout(async () => {
      trips.value = await getTrips()
    }, 1000)
    // trips.value = await getTrips()
  }
}
</script>

<template>
  <div class="min-w-full mx-auto bg-white border border-black rounded-xl p-2 shadow-xl">
    <table class="min-w-full">
      <thead>
        <tr>
          <Header>Date</Header>
          <Header>Amount</Header>
          <Header>Store</Header>
          <Header>Category</Header>
          <Header>City</Header>
          <Header>Trip</Header>
          <Header>Shared</Header>
          <Header>Person</Header>
          <Header>Actions</Header>
        </tr>
      </thead>
      <tbody>
        <tr v-for="expense in expenses" :key="expense.id" class="hover:bg-gray-100">
          <Cell> {{ expense.date }}</Cell>
          <Cell>
            <FocusableInput
              v-model="expense.amount"
              textSuffix="kr"
              @change="edit(expense)"
              type="number"
            />
          </Cell>
          <Cell> {{ expense.store }}</Cell>
          <Cell>
            <AutoComplete
              v-model="expense.category"
              :values="categories"
              @change="(newCategory) => updateExpeseWithCategory(expense, newCategory)"
            />
          </Cell>
          <Cell>{{ expense.city }}</Cell>
          <Cell>
            <AutoComplete
              v-model="expense.trip"
              :values="trips"
              @change="(newTrip) => updateExpeseWithTrip(expense, newTrip)"
            />
          </Cell>
          <Cell>
            <input type="checkbox" v-model="expense.shared" @change="edit(expense)" />
          </Cell>
          <Cell>{{ expense.person }}</Cell>
          <Cell>
            <Button variant="danger" @click="remove(expense)"> Delete </Button>
          </Cell>
        </tr>
      </tbody>
    </table>
    <div class="flex justify-between items-center mt-4">
      <div>
        <label for="perPage" class="mr-2">Items per page:</label>
        <select id="perPage" v-model="pageSize" class="px-2 py-1 border rounded">
          <option :value="10">10</option>
          <option :value="20">20</option>
          <option :value="50">50</option>
        </select>
      </div>
      <div class="flex items-center">
        <button
          @click="previousPage"
          :disabled="pageNumber === 0"
          class="px-4 py-2 mx-1 bg-gray-200 text-gray-800 rounded disabled:opacity-50"
        >
          Previous
        </button>
        <span class="px-4 py-2 mx-1">Page {{ pageNumber }} </span>
        <button
          :disabled="expenses.length < pageSize"
          @click="nextPage"
          class="px-4 py-2 mx-1 bg-gray-200 text-gray-800 rounded disabled:opacity-50"
        >
          Next
        </button>
      </div>
    </div>
  </div>
</template>
@/components/ui/autocomplete
