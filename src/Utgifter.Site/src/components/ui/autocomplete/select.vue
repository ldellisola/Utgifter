<script setup lang="ts">
import Fuse from 'fuse.js'
import { nextTick, ref, watch, onMounted, onUnmounted } from 'vue'
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
const dropdownStyle = ref({})

function updateDropdownPosition() {
  if (input.value) {
    const rect = input.value.getBoundingClientRect()
    dropdownStyle.value = {
      position: 'absolute',
      top: `${rect.bottom + window.scrollY}px`,
      left: `${rect.left + window.scrollX}px`,
      width: `${rect.width}px`
    }
  }
}

watch(
  () => props.focus,
  (focus) => {
    if (focus) {
      nextTick(() => {
        input.value!.focus()
        filterCategories(model.value ?? '')
        updateDropdownPosition()
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
  setTimeout(() => {
    filteredCategories.value = undefined
    emit('blur')
  }, 150)
}

function onFocus() {
  filterCategories(model.value ?? '')
  updateDropdownPosition()
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
}

function clearInput() {
  model.value = undefined
  filterCategories('')
  emit('change', false)
}

onMounted(() => {
  window.addEventListener('resize', updateDropdownPosition)
  window.addEventListener('scroll', updateDropdownPosition, true)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateDropdownPosition)
  window.removeEventListener('scroll', updateDropdownPosition, true)
})
</script>

<template>
  <div class="relative w-max">
    <input
      ref="input"
      class="block w-full rounded-md border-0 bg-white pl-3 pr-3 text-gray-900 ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-indigo-600 sm:text-sm h-8"
      type="text"
      v-model="model"
      @focus="onFocus"
      @input="(e: any) => filterCategories(e.target.value)"
      @paste="(e: any) => filterCategories(e.target.value)"
      @blur="onBlur"
    />
    <Teleport to="body">
      <div
        :style="dropdownStyle"
        class="absolute z-50 rounded border border-black bg-white mt-1"
        v-if="filteredCategories !== undefined"
      >
        <ul class="overflow-auto list-none max-h-60">
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
    </Teleport>
  </div>
</template>