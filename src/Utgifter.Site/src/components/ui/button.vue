<script setup lang="ts">
export type ButtonProps = {
  variant?: 'primary' | 'danger'
}
withDefaults(defineProps<ButtonProps>(), {
  variant: 'primary'
})
const emit = defineEmits<{
  click: [event: MouseEvent]
}>()
</script>

<template>
  <button
    @click="(e) => emit('click', e)"
    class="rounded-xl p-2 hover:text-white hover:shadow-xl font-bold"
    :class="{
      'bg-indigo-300 hover:bg-indigo-400': variant === 'primary',
      'bg-red-300 hover:bg-red-400': variant === 'danger'
    }"
  >
    <slot />
  </button>
</template>
