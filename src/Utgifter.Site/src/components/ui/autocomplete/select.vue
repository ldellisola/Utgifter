<script setup lang="ts">
import Fuse from 'fuse.js'
import { nextTick, ref, watch, onMounted, onUnmounted, computed } from 'vue'
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
const highlightedIndex = ref(-1)
const listContainer = ref<HTMLUListElement | null>(null)

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

// Computed list of all options including "Add" and "Remove"
const allOptions = computed(() => {
  if (!filteredCategories.value) return []
  
  const options = [...filteredCategories.value]
  
  // Add "Add new" option if input doesn't match existing
  if (model.value && model.value !== '' && !filteredCategories.value.includes(model.value)) {
    options.push(`__ADD__:${model.value}`)
  }
  
  // Always add "Remove" option
  options.push('__REMOVE__')
  
  return options
})

// Reset highlight when options change
watch(allOptions, () => {
  highlightedIndex.value = -1
})

function onBlur() {
  setTimeout(() => {
    filteredCategories.value = undefined
    highlightedIndex.value = -1
    emit('blur')
  }, 150)
}

function onFocus() {
  filterCategories(model.value ?? '')
  updateDropdownPosition()
  highlightedIndex.value = -1
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
  filteredCategories.value = undefined
  highlightedIndex.value = -1
}

function clearInput() {
  model.value = undefined
  filterCategories('')
  emit('change', false)
  filteredCategories.value = undefined
  highlightedIndex.value = -1
}

function scrollToHighlighted() {
  if (listContainer.value && highlightedIndex.value >= 0) {
    const items = listContainer.value.querySelectorAll('li')
    const highlightedItem = items[highlightedIndex.value]
    if (highlightedItem) {
      highlightedItem.scrollIntoView({ block: 'nearest', behavior: 'smooth' })
    }
  }
}

function handleKeyDown(event: KeyboardEvent) {
  if (!filteredCategories.value) return

  const optionsCount = allOptions.value.length

  switch (event.key) {
    case 'ArrowDown':
      event.preventDefault()
      highlightedIndex.value = (highlightedIndex.value + 1) % optionsCount
      nextTick(scrollToHighlighted)
      break

    case 'ArrowUp':
      event.preventDefault()
      highlightedIndex.value = highlightedIndex.value <= 0 ? optionsCount - 1 : highlightedIndex.value - 1
      nextTick(scrollToHighlighted)
      break

    case 'Enter':
      event.preventDefault()
      if (highlightedIndex.value >= 0 && highlightedIndex.value < optionsCount) {
        const selected = allOptions.value[highlightedIndex.value]
        if (selected === '__REMOVE__') {
          clearInput()
        } else if (selected.startsWith('__ADD__:')) {
          const newValue = selected.replace('__ADD__:', '')
          selectCategory(newValue)
        } else {
          selectCategory(selected)
        }
      }
      break

    case 'Escape':
      event.preventDefault()
      filteredCategories.value = undefined
      highlightedIndex.value = -1
      input.value?.blur()
      break
  }
}

function handleOptionClick(index: number) {
  const selected = allOptions.value[index]
  if (selected === '__REMOVE__') {
    clearInput()
  } else if (selected.startsWith('__ADD__:')) {
    const newValue = selected.replace('__ADD__:', '')
    selectCategory(newValue)
  } else {
    selectCategory(selected)
  }
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
      @keydown="handleKeyDown"
    />
    <Teleport to="body">
      <div
        :style="dropdownStyle"
        class="absolute z-50 rounded border border-black bg-white mt-1"
        v-if="filteredCategories !== undefined"
      >
        <ul ref="listContainer" class="overflow-auto list-none max-h-60">
          <listItem
            v-for="(option, index) in allOptions"
            :key="option"
            :highlighted="highlightedIndex === index"
            @mousedown="handleOptionClick(index)"
            @mouseenter="highlightedIndex = index"
          >
            <template v-if="option === '__REMOVE__'">
              <b>Remove</b>
            </template>
            <template v-else-if="option.startsWith('__ADD__:')">
              <i>Add</i> '{{ option.replace('__ADD__:', '') }}'
            </template>
            <template v-else>
              {{ option }}
            </template>
          </listItem>
        </ul>
      </div>
    </Teleport>
  </div>
</template>