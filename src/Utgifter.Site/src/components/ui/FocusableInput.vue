<script setup lang="ts">
import { useParentElement } from '@vueuse/core'
import { nextTick, ref, onMounted } from 'vue'

type Props = {
  type?: 'text' | 'password' | 'email' | 'number' | 'tel' | 'url'
  textSuffix?: string
}

defineProps<Props>()

const model = defineModel()

const showInput = ref(false)

const input = ref<HTMLInputElement | null>()
const displayDiv = ref<HTMLDivElement | null>()
const inputWidth = ref<string>('auto')

function enableInput() {
  if (displayDiv.value) {
    inputWidth.value = `${displayDiv.value.offsetWidth}px`
  }
  showInput.value = true
  nextTick(() => input.value?.focus())
}

const parent = useParentElement()
onMounted(() => {
  parent.value?.addEventListener('click', enableInput)
})
</script>

<template>
  <input
    ref="input"
    :type="type"
    v-if="showInput"
    v-model="model"
    class="block rounded-md border-0 bg-white pl-3 pr-3 text-gray-900 ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-indigo-600 text-base h-8 overflow-hidden whitespace-nowrap [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
    :style="{ width: inputWidth }"
    @blur="showInput = false"
  />
  <div ref="displayDiv" v-else class="inline-block rounded-md border-0 bg-white pl-3 pr-3 text-gray-900 text-base h-8 overflow-hidden whitespace-nowrap">{{ model }} {{ textSuffix }}</div>
</template>
