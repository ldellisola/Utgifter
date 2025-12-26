<script setup lang="ts">
import { RouterLink } from 'vue-router'

export type ButtonProps = {
  variant?: 'primary' | 'danger'
  disabled?: boolean
  to?: string
}
withDefaults(defineProps<ButtonProps>(), {
  variant: 'primary',
  disabled: false,
  to: undefined
})
const emit = defineEmits<{
  click: [event: MouseEvent]
}>()
</script>

<template>
  <component
    :is="to ? RouterLink : 'button'"
    :to="to"
    @click="(e: MouseEvent) => emit('click', e)"
    :disabled="disabled"
    class="inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-slate-400 focus:ring-offset-2 disabled:pointer-events-none disabled:opacity-50"
    :class="{
      'bg-slate-900 text-white hover:bg-slate-700': variant === 'primary',
      'bg-red-500 text-white hover:bg-red-600': variant === 'danger'
    }"
  >
    <slot />
  </component>
</template>
