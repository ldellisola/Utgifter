<script setup lang="ts" generic="T">
import { ref } from 'vue'

export type SelectProps<T> = {
  modelValue?: T
  options: { value: T; label: string }[]
}

const props = defineProps<SelectProps<T>>()
const emit = defineEmits(['update:modelValue', 'change'])

function handleChange(event: Event) {
  const target = event.target as HTMLSelectElement
  const selectedIndex = target.selectedIndex
  const selectedOption = props.options[selectedIndex]

  if (selectedOption) {
    emit('update:modelValue', selectedOption.value)
    emit('change', selectedOption.value)
  }
}

// This function helps Vue correctly select the initial option in the dropdown,
// even when the modelValue is null or undefined.
function valueComparator(a: T, b: T) {
  return String(a) === String(b)
}
</script>

<template>
  <select
    :value="modelValue"
    @change="handleChange"
    class="block w-full rounded-md border-0 bg-white pl-3 pr-10 text-gray-900 ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-indigo-600 sm:text-sm h-8"
  >
    <option v-for="option in options" :key="String(option.value)" :value="option.value" :selected="valueComparator(modelValue, option.value)">
      {{ option.label }}
    </option>
  </select>
</template>