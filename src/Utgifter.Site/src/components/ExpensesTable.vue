<script setup lang="ts">
import { ref, watch } from 'vue'
import { getExpenseCategories } from '@/api/server'
import type { Expense } from '@/api/server'
import CategorySimpleSelect from '@/components/ui/select'
import Button from '@/components/ui/button.vue'
type Props = { expenses: Expense[] }

defineProps<Props>()

const emit = defineEmits<{
  loadExpenses: [page: number, size: number]
  removeExpense: [expense: Expense, page: number, size: number]
  editExpense: [expense: Expense, page: number, size: number]
}>()

const pageNumber = ref(0)
const pageSize = ref(10)
const categories = ref<string[]>(await getExpenseCategories())

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

async function updateExpeseWithCategory(expense: Expense, newCategory: boolean) {
  await edit(expense)
  if (newCategory) {
    categories.value = await getExpenseCategories()
  }
}
</script>

<template>
  <div class="min-w-full mx-auto bg-white border border-black rounded-xl p-2 shadow-xl">
    <table class="min-w-full">
      <thead>
        <tr>
          <th class="py-2 px-4 border-b border-gray-200">Date</th>
          <th class="py-2 px-4 border-b border-gray-200">Amount</th>
          <th class="py-2 px-4 border-b border-gray-200">Store</th>
          <th class="py-2 px-4 border-b border-gray-200">Category</th>
          <th class="py-2 px-4 border-b border-gray-200">City</th>
          <th class="py-2 px-4 border-b border-gray-200">Trip</th>
          <th class="py-2 px-4 border-b border-gray-200">Shared</th>
          <th class="py-2 px-4 border-b border-gray-200">Person</th>
          <th class="py-2 px-4 border-b border-gray-200">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="expense in expenses" :key="expense.id" class="hover:bg-gray-100">
          <td class="py-2 px-4 border-b border-gray-200">{{ expense.date }}</td>
          <td class="py-2 px-4 border-b border-gray-200">{{ expense.amount }} kr</td>
          <td class="py-2 px-4 border-b border-gray-200">{{ expense.store }}</td>
          <td class="py-2 px-4 border-b border-gray-200">
            <CategorySimpleSelect
              :expense="expense"
              :categories="categories"
              @change="(newCategory) => updateExpeseWithCategory(expense, newCategory)"
            />
          </td>
          <td class="py-2 px-4 border-b border-gray-200">{{ expense.city }}</td>
          <td class="py-2 px-4 border-b border-gray-200">
            <input type="checkbox" v-model="expense.trip" @change="edit(expense)" />
          </td>
          <td class="py-2 px-4 border-b border-gray-200">
            <input type="checkbox" v-model="expense.shared" @change="edit(expense)" />
          </td>
          <td class="py-2 px-4 border-b border-gray-200">{{ expense.person }}</td>
          <td class="py-2 px-4 border-b border-gray-200">
            <Button type="danger" @click="remove(expense)"> Delete </Button>
          </td>
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
