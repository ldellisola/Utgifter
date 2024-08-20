<script setup lang="ts">
import Fuse from 'fuse.js'
import { nextTick, ref, watch } from 'vue'
import listItem from './listItem.vue'

interface CategorySelectProps {
  values: string[]
  focus?: boolean
}

const props = defineProps<CategorySelectProps>()
const emit = defineEmits<{
  change: [newCategory: boolean]
  blur: []
}>()

const input = ref<HTMLInputElement | null>(null)
watch(
  () => props.focus,
  (focus) => {
    if (focus) {
      nextTick(() => {
        input.value!.focus()
        filterCategories(model.value ?? '')
      })
    }
  },
  { immediate: true }
)

const model = defineModel<string | undefined>()

const filteredCategories = ref<string[] | undefined>(undefined)
const fuse = new Fuse(props.values, {
  isCaseSensitive: false,
  threshold: 0.3,
  ignoreLocation: true
})

watch(
  () => props.values,
  (newCategories) => {
    fuse.setCollection(newCategories)
    filteredCategories.value = undefined
  }
)

function onBlur() {
  filteredCategories.value = undefined
  emit('blur')
}

function filterCategories(input: string) {
  if (input?.length > 0) {
    filteredCategories.value = fuse.search(input).map((t) => t.item)
  } else {
    filteredCategories.value = props.values
  }
}

function selectCategory(category?: string) {
  model.value = category
  emit('change', category !== undefined && !props.values.includes(category))
  onBlur()
}

function clearInput() {
  model.value = undefined
  filterCategories('')
  emit('change', false)
}
</script>

<template>
  <div class="relative w-max">
    <input
      ref="input"
      class="pl-2 overflow-hidden w-full relative box-border border border-black rounded outline-none"
      type="text"
      v-model="model"
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
          v-if="model !== '' && model !== null && !filteredCategories.includes(model!)"
          @mousedown="selectCategory(model)"
        >
          <i>Add</i> '{{ model }}'
        </listItem>
        <listItem @mousedown="clearInput()">
          <b>Remove</b>
        </listItem>
      </ul>
    </div>
  </div>
</template>
