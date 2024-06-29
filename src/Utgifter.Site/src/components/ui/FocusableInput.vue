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
    class="border rounded border-black"
    @blur="showInput = false"
  />
  <div v-else>{{ model }} {{ textSuffix }}</div>
</template>
