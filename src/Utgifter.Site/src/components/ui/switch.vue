<script setup lang="ts">
defineProps<{ label: string }>()
const model = defineModel<boolean>()

function toggle() {
  console.log('toggle')
  model.value = !model.value
}
</script>

<template>
  <div class="flex gap-1">
    <span> {{ label }} </span>
    <div
      class="border rounded-xl h-6 w-10"
      :class="{ 'bg-green-100': model, 'bg-red-100': !model }"
      @click="toggle"
    >
      <div class="grid items-center h-full pl-1 pr-1">
        <div
          class="w-4 h-4 rounded-full bg-blue-950 shadow-md transform duration-300"
          :class="{
            'justify-self-start': model,
            'justify-self-end': !model
          }"
        ></div>
      </div>
    </div>
  </div>
</template>
