<script setup lang="ts">
import { useParentElement } from '@vueuse/core'
import { nextTick, ref, getCurrentInstance, onMounted } from 'vue'

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
    v-if="showInput"
    v-model="model"
    class="border rounded border-black"
    @blur="showInput = false"
  />
  <div v-else>{{ model }}</div>
</template>
