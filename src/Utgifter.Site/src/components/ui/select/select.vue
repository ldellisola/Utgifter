<script setup lang="ts">
import type { Expense } from '@/api/server'
import Fuse from 'fuse.js'
import { ref, watch } from 'vue'
import listItem from './listItem.vue'

interface CategorySelectProps {
  expense: Expense
  categories: string[]
}

const props = defineProps<CategorySelectProps>()
const emit = defineEmits<{
  change: [newCategory: boolean]
}>()

const filteredCategories = ref<string[] | undefined>(undefined)
const fuse = new Fuse(props.categories, {
  isCaseSensitive: false,
  threshold: 0.3,
  ignoreLocation: true
})

watch(
  () => props.categories,
  (newCategories) => {
    fuse.setCollection(newCategories)
  }
)

function onBlur() {
  filteredCategories.value = undefined
}

function filterCategories(input: string) {
  if (input?.length > 0) {
    filteredCategories.value = fuse.search(input).map((t) => t.item)
  } else {
    filteredCategories.value = props.categories
  }
}

function selectCategory(category?: string) {
  console.log('selectCategory', category)
  props.expense.category = category
  emit('change', category !== undefined && !props.categories.includes(category))
  onBlur()
}

function clearInput() {
  console.log('clearInput')
  props.expense.category = undefined
  filterCategories('')
  emit('change', false)
}
</script>

<template>
  <div class="w-max relative">
    <input
      class="pl-2 overflow-hidden w-max relative box-border border-2 border-black rounded outline-none"
      type="text"
      v-model="expense.category"
      @focus="(e: any) => filterCategories(e.target.value)"
      @input="(e: any) => filterCategories(e.target.value)"
      @paste="(e: any) => filterCategories(e.target.value)"
      @blur="onBlur"
    />
    <div
      class="absolute z-50 top-full left-0 rounded right-0 border border-black bg-white mt-1"
      v-if="filteredCategories !== undefined"
    >
      <ul class="overflow-auto list-none">
        <listItem
          v-for="category in filteredCategories"
          :key="category"
          @mousedown="selectCategory(category)"
        >
          {{ category }}
        </listItem>
        <listItem
          v-if="
            expense.category !== '' &&
            expense.category !== null &&
            !filteredCategories.includes(expense.category!)
          "
          @mousedown="selectCategory(expense.category)"
        >
          <i>Add</i> '{{ expense.category }}'
        </listItem>
        <listItem @mousedown="clearInput()">
          <b>Remove</b>
        </listItem>
      </ul>
    </div>
  </div>
</template>
