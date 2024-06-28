<script setup lang="ts">
import { ref } from 'vue'
import Button from '@/components/ui/button.vue'
import AutoComplete from '@/components/ui/autocomplete'
import { createRule } from '@/api/rules'
import { getCategories } from '@/api/categories'

const emit = defineEmits<{
  created: []
}>()

const isClosed = ref(true)
const toggleCard = () => (isClosed.value = !isClosed.value)

type Form = {
  expectedStore: string
  newCategory: string
  trip?: boolean
  newStore: string
  shared?: boolean
}
const form = ref<Form>({
  expectedStore: '',
  newCategory: '',
  trip: undefined,
  newStore: '',
  shared: undefined
})

function prepareString(text: string) {
  const trim = text.trim()
  if (trim.length === 0) return undefined
  return trim
}

const expectedStoreError = ref<string | null>(null)
const transformError = ref<string | null>(null)

async function submitForm() {
  const [error, data] = await createRule({
    expectedStore: form.value.expectedStore.trim(),
    newCategory: prepareString(form.value.newCategory),
    trip: form.value.trip,
    newStore: prepareString(form.value.newStore),
    shared: form.value.shared
  })

  if (error) {
    expectedStoreError.value = error.errors!['expectedStore']?.join(', ')
    transformError.value = error.errors!['']?.join(', ')
  }

  if (data) {
    emit('created')
    expectedStoreError.value = null
    transformError.value = null
    form.value = {
      expectedStore: '',
      newCategory: '',
      trip: undefined,
      newStore: '',
      shared: undefined
    }
  }
}

const categories = await getCategories()
</script>
<template>
  <div class="border-2 rounded-lg border-black">
    <div
      class="bg-white rounded-t-lg font-bold p-1"
      :class="{ 'rounded-b-lg': isClosed }"
      @click="toggleCard"
    >
      New Rule
    </div>
    <div class="flex flex-col p-2" :class="{ hidden: isClosed }">
      <div class="flex flex-row gap-1">
        <label for="expecteStore"> When the <b>store</b> is:</label>
        <input
          v-model="form.expectedStore"
          class="flex-grow border rounded border-black"
          id="expecteStore"
        />
        <span class="text-red-500" v-if="expectedStoreError">{{ expectedStoreError }}</span>
      </div>
      <div>
        <b>Transform</b> the record to:
        <span class="text-red-500" v-if="transformError">{{ transformError }}</span>
      </div>
      <div class="flex flex-wrap m-2">
        <div class="w-1/2 h-1/2 flex gap-2 px-2 my-2">
          <label for="newCategory"> Category: </label>
          <AutoComplete
            :values="categories"
            v-model="form.newCategory"
            class="flex-grow"
            id="newCategory"
          />
        </div>

        <div class="w-1/2 h-1/2 flex gap-2 px-2 my-2">
          <label for="isTrip"> Trip: </label>
          <select
            v-model="form.trip"
            class="border border-black rounded bg-white flex-grow px-2"
            id="isTrip"
          >
            <option :value="undefined" selected></option>
            <option :value="true">True</option>
            <option :value="false">False</option>
          </select>
        </div>

        <div class="w-1/2 h-1/2 flex gap-2 px-2 my-2">
          <label for="newStore"> Store: </label>
          <input
            v-model="form.newStore"
            class="border border-black flex-grow rounded"
            id="newStore"
          />
        </div>

        <div class="w-1/2 h-1/2 flex gap-2 px-2 my-2">
          <label for="isShared"> Shared: </label>
          <select
            v-model="form.shared"
            class="border border-black rounded bg-white flex-grow px-2"
            id="isShared"
          >
            <option :value="undefined" selected></option>
            <option :value="true">True</option>
            <option :value="false">False</option>
          </select>
        </div>
      </div>
      <Button @click="submitForm" variant="primary">Add Rule</Button>
    </div>
  </div>
</template>
