<script setup lang="ts">
import { ref } from 'vue'
import Button from '@/components/ui/button.vue'
import AutoComplete from '@/components/ui/autocomplete'
import Select from '@/components/ui/select.vue'
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
  trip: null,
  newStore: '',
  shared: null
})

function prepareString(text: string) {
  const trim = text.trim()
  if (trim.length === 0) return null
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
      trip: null,
      newStore: '',
      shared: null
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
      <div class="flex flex-row gap-1 items-center">
        <label for="expecteStore"> When the <b>store</b> is:</label>
        <input
          v-model="form.expectedStore"
          class="block flex-grow rounded-md border-0 bg-white pl-3 pr-10 text-gray-900 ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-indigo-600 sm:text-sm h-8"
          id="expecteStore"
        />
        <span class="text-red-500" v-if="expectedStoreError">{{ expectedStoreError }}</span>
      </div>
      <div>
        <b>Transform</b> the record to:
        <span class="text-red-500" v-if="transformError">{{ transformError }}</span>
      </div>
      <div class="flex flex-wrap m-2">
        <div class="w-1/2 flex gap-2 px-2 my-2 items-center">
          <label for="newCategory"> Category: </label>
          <AutoComplete
            :values="categories"
            v-model="form.newCategory"
            class="flex-grow"
            id="newCategory"
          />
        </div>

        <div class="w-1/2 flex gap-2 px-2 my-2 items-center">
          <label for="isTrip"> Trip: </label>
          <Select
            :options="[
              { value: null, label: '' },
              { value: true, label: 'True' },
              { value: false, label: 'False' }
            ]"
            v-model="form.trip"
            class="flex-grow"
            id="isTrip"
          />
        </div>

        <div class="w-1/2 flex gap-2 px-2 my-2 items-center">
          <label for="newStore"> Store: </label>
          <input
            v-model="form.newStore"
            class="block w-full rounded-md border-0 bg-white pl-3 pr-10 text-gray-900 ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-indigo-600 sm:text-sm h-8"
            id="newStore"
          />
        </div>

        <div class="w-1/2 flex gap-2 px-2 my-2 items-center">
          <label for="isShared"> Shared: </label>
          <Select
            :options="[
              { value: null, label: '' },
              { value: true, label: 'True' },
              { value: false, label: 'False' }
            ]"
            v-model="form.shared"
            class="flex-grow"
            id="isShared"
          />
        </div>
      </div>
      <Button @click="submitForm" variant="primary">Add Rule</Button>
    </div>
  </div>
</template>
