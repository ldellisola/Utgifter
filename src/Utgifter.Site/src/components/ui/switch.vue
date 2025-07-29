<script setup lang="ts">
const model = defineModel<boolean>()

function toggle() {
  model.value = !model.value
}
</script>

<template>
  <div
    class="relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-slate-900 focus:ring-offset-2"
    :class="model ? 'bg-slate-900' : 'bg-slate-200'"
    @click="toggle"
  >
    <span
      aria-hidden="true"
      :class="model ? 'translate-x-5' : 'translate-x-0'"
      class="pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out"
    ></span>
  </div>
</template>
