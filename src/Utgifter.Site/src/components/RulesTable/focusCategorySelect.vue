<script setup lang="ts">
import { useParentElement } from '@vueuse/core'
import { ref, onMounted } from 'vue'
import AutoComplete from '@/components/ui/autocomplete'

const model = defineModel<string | undefined>()

const showInput = ref<boolean>(false)

function enableInput() {
  showInput.value = true
}

const parent = useParentElement()
onMounted(() => {
  parent.value?.addEventListener('click', enableInput)
})
</script>

<template>
  <AutoComplete :focus="showInput" v-if="showInput" v-model="model" @blur="showInput = false" />
  <div v-else>{{ model }}</div>
</template>
@/components/ui/autocomplete
