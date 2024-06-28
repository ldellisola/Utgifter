<script setup lang="ts">
import { ref, watch } from 'vue'
import Button from '@/components/ui/button.vue'
import Header from './Header.vue'
import Cell from './Cell.vue'
import type { Rule } from '@/api/rules'
import FocusableInput from './FocusableInput.vue'
import FocusableAutoComplete from './FocusableAutoComplete.vue'
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
    <table class="min-w-full">
      <thead>
        <tr>
          <Header> Expected Store </Header>
          <Header>New Store</Header>
          <Header>New Category</Header>
          <Header>Is Trip</Header>
          <Header>Is Shared</Header>
          <Header>Actions</Header>
        </tr>
      </thead>
      <tbody>
        <tr v-for="rule in rules" :key="rule.id" class="hover:bg-gray-100">
          <Cell> {{ rule.expectedStore }}</Cell>
          <Cell> <FocusableInput v-model="rule.newStore" @change="edit(rule)" /> </Cell>
          <Cell>
            <FocusableAutoComplete
              class="inline-flex"
              :values="categories"
              v-model="rule.newCategory"
              @change="edit(rule)"
            />
          </Cell>
          <Cell>
            <select
              v-model="rule.trip"
              @change="edit(rule)"
              class="border border-black rounded bg-white px-2"
            >
              <option :value="undefined" selected></option>
              <option :value="true">True</option>
              <option :value="false">False</option>
            </select>
          </Cell>
          <Cell>
            <select
              v-model="rule.shared"
              @change="edit(rule)"
              class="border border-black rounded bg-white px-2"
            >
              <option :value="undefined" selected></option>
              <option :value="true">True</option>
              <option :value="false">False</option>
            </select>
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
