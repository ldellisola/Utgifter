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

function enableInput() {
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
    class="block w-full rounded-md border-0 bg-white pl-3 pr-10 text-gray-900 ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-indigo-600 text-base h-8"
    @blur="showInput = false"
  />
  <div v-else class="block w-full rounded-md border-0 bg-white pl-3 pr-10 text-gray-900 text-base h-8">{{ model }} {{ textSuffix }}</div>
</template>
