<script setup lang="ts">
import Button from '@/components/ui/button.vue'
import NewRule from './NewRule.vue'
import RulesTable from './Table'
import RenameStoresModal from './RenameStoresModal.vue'
import { getRules, updateRule, deleteRule, type Rule } from '@/api/rules'
import { ref } from 'vue'

const rules = ref<Rule[]>([])
const renameRule = ref<Rule | null>(null)

async function loadRules(page?: number, size?: number) {
  rules.value = await getRules(page, size)
}
async function remove(rule: Rule, page: number, size: number) {
  await deleteRule(rule)
  await loadRules(page, size)
}
async function edit(rule: Rule, page: number, size: number) {
  await updateRule(rule)
  await loadRules(page, size)
}
function openRenameModal(rule: Rule) {
  renameRule.value = rule
}
function closeRenameModal() {
  renameRule.value = null
}
</script>

<template>
  <div class="flex justify-center gap-3 flex-col mx-11 mt-5">
    <div>
      <Button variant="primary">
        <RouterLink to="/">Go back</RouterLink>
      </Button>
    </div>
    <NewRule @created="loadRules" />
    <RulesTable
      :rules="rules"
      @load="loadRules"
      @remove="remove"
      @edit="edit"
      @rename="openRenameModal"
    />
    <RenameStoresModal :rule="renameRule" @close="closeRenameModal" />
  </div>
</template>
