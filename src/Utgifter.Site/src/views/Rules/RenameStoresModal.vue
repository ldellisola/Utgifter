<script setup lang="ts">
import { ref, watch } from 'vue'
import Modal from '@/components/ui/modal.vue'
import Button from '@/components/ui/button.vue'
import { getRenamePreview, applyRename, type RenamePreview, type Rule } from '@/api/rules'

const props = defineProps<{
  rule: Rule | null
}>()

const emit = defineEmits<{
  close: []
  applied: []
}>()

const preview = ref<RenamePreview | null>(null)
const loading = ref(false)
const applying = ref(false)

watch(
  () => props.rule,
  async (newRule) => {
    if (newRule?.id) {
      loading.value = true
      preview.value = await getRenamePreview(newRule.id)
      loading.value = false
    } else {
      preview.value = null
    }
  },
  { immediate: true }
)

async function apply() {
  if (!props.rule?.id) return
  applying.value = true
  await applyRename(props.rule.id)
  applying.value = false
  emit('applied')
  emit('close')
}
</script>

<template>
  <Modal :open="!!rule" title="Rename Stores Preview" @close="emit('close')">
    <div v-if="loading" class="text-center py-8">Loading...</div>
    <div v-else-if="preview">
      <div class="mb-4 p-3 bg-gray-100 rounded-lg">
        <p>
          <span class="font-semibold">From:</span> {{ preview.expectedStore }}
        </p>
        <p>
          <span class="font-semibold">To:</span>
          {{ preview.newStore || '(no new store defined)' }}
        </p>
        <p class="text-sm text-gray-600 mt-1">
          {{ preview.expenses.length }} expense(s) will be renamed
        </p>
      </div>

      <div v-if="preview.expenses.length === 0" class="text-center py-4 text-gray-500">
        No expenses found matching this store name.
      </div>

      <div v-else class="overflow-x-auto">
        <table class="min-w-full table-auto border-collapse">
          <thead>
            <tr class="bg-gray-50">
              <th class="px-3 py-2 text-left text-sm font-semibold border-b">Date</th>
              <th class="px-3 py-2 text-left text-sm font-semibold border-b">Person</th>
              <th class="px-3 py-2 text-left text-sm font-semibold border-b">Store</th>
              <th class="px-3 py-2 text-left text-sm font-semibold border-b">Amount</th>
              <th class="px-3 py-2 text-left text-sm font-semibold border-b">Category</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="expense in preview.expenses" :key="expense.id" class="hover:bg-gray-50">
              <td class="px-3 py-2 text-sm border-b">{{ expense.date }}</td>
              <td class="px-3 py-2 text-sm border-b">{{ expense.person }}</td>
              <td class="px-3 py-2 text-sm border-b">{{ expense.store }}</td>
              <td class="px-3 py-2 text-sm border-b">
                {{ expense.amount }} {{ expense.originalCurrency }}
              </td>
              <td class="px-3 py-2 text-sm border-b">{{ expense.category || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="mt-4 flex justify-end gap-2">
        <Button variant="primary" @click="emit('close')">Cancel</Button>
        <Button
          v-if="preview.newStore && preview.expenses.length > 0"
          variant="primary"
          :disabled="applying"
          @click="apply"
        >
          {{ applying ? 'Applying...' : 'Apply Rename' }}
        </Button>
      </div>
    </div>
  </Modal>
</template>
