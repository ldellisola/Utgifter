import ExpenseProcessing from '@/views/ExpenseProcessing.vue'
import Expenses from '@/views/Expenses.vue'
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: Expenses
    },
    {
      path: '/expenses',
      name: 'expenses',
      component: Expenses
    },
    {
      path: '/expenses/upload',
      name: 'upload',
      component: ExpenseProcessing
    }
  ]
})

export default router
