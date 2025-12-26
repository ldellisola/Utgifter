<script setup lang="ts">
interface ListItemProps {
  highlighted?: boolean
}

defineProps<ListItemProps>()
</script>

<template>
  <li
    :class="[
      'p-2 text-sm list-item text-left first:border-t-0 border-t border border-b-0 border-r-0 border-l-0 border-black cursor-pointer transition-colors',
      highlighted ? 'bg-indigo-100' : 'hover:bg-gray-300'
    ]"
  >
    <slot />
  </li>
</template>
