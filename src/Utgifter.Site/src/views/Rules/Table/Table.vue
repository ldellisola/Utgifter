<script setup lang="ts">
import { ref, watch } from 'vue'
import Button from '@/components/ui/button.vue'
import Header from './Header.vue'
import Cell from './Cell.vue'
import type { Rule } from '@/api/rules'
import FocusableInput from '@/components/ui/FocusableInput.vue'
import FocusableAutoComplete from './FocusableAutoComplete.vue'
import Select from '@/components/ui/select.vue'
import { getCategories } from '@/api/categories'

type Props = { rules: Rule[] }

defineProps<Props>()

const emit = defineEmits<{
  load: [page: number, size: number]
  remove: [rule: Rule, page: number, size: number]
  edit: [rule: Rule, page: number, size: number]
}>()

const pageNumber = ref(0)
const pageSize = ref(10)

async function nextPage() {
  pageNumber.value++
  emit('load', pageNumber.value, pageSize.value)
}

async function previousPage() {
  pageNumber.value--
  emit('load', pageNumber.value, pageSize.value)
}

watch(
  pageSize,
  async () => {
    pageNumber.value = 0
    emit('load', pageNumber.value, pageSize.value)
  },
  { immediate: true }
)

async function remove(rule: Rule) {
  emit('remove', rule, pageNumber.value, pageSize.value)
}
async function edit(rule: Rule) {
  emit('edit', rule, pageNumber.value, pageSize.value)
}

const categories = ref<string[]>([])
categories.value = await getCategories()
</script>

<template>
  <div class="min-w-full mx-auto bg-white border border-black rounded-xl p-2 shadow-xl">
    <table class="min-w-full table-fixed">
      <thead>
        <tr>
          <Header class="w-1/5"> Expected Store </Header>
          <Header class="w-1/5">New Store</Header>
          <Header class="w-1/5">New Category</Header>
          <Header class="w-1/10">Is Trip</Header>
          <Header class="w-1/10">Is Shared</Header>
          <Header class="w-1/5">Actions</Header>
        </tr>
      </thead>
      <tbody>
        <tr v-for="rule in rules" :key="rule.id" class="hover:bg-gray-100">
          <Cell class="px-4"> {{ rule.expectedStore }}</Cell>
          <Cell> <FocusableInput v-model="rule.newStore" @change="edit(rule)" class="px-4" /> </Cell>
          <Cell>
            <FocusableAutoComplete
              class="inline-flex px-4"
              :values="categories"
              v-model="rule.newCategory"
              @change="edit(rule)"
            />
          </Cell>
          <Cell>
            <Select
              :options="[
                { value: null, label: '' },
                { value: true, label: 'True' },
                { value: false, label: 'False' }
              ]"
              v-model="rule.trip"
              @change="edit(rule)"
              class="px-4"
            />
          </Cell>
          <Cell>
            <Select
              :options="[
                { value: null, label: '' },
                { value: true, label: 'True' },
                { value: false, label: 'False' }
              ]"
              v-model="rule.shared"
              @change="edit(rule)"
              class="px-4"
            />
          </Cell>
          <Cell>
            <Button variant="danger" @click="remove(rule)"> Delete </Button>
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
          :disabled="rules.length < pageSize"
          @click="nextPage"
          class="px-4 py-2 mx-1 bg-gray-200 text-gray-800 rounded disabled:opacity-50"
        >
          Next
        </button>
      </div>
    </div>
  </div>
</template>
