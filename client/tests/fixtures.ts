import { test as base, type Page } from '@playwright/test'
import { expect } from '@playwright/test'

export interface TestUser {
  email: string
  password: string
  displayName: string
}

let userCounter = 0

export function makeUser(): TestUser {
  userCounter += 1
  const stamp = `${Date.now()}-${userCounter}`
  return {
    email: `pw-${stamp}@test.local`,
    password: 'Test1234!',
    displayName: `PW User ${userCounter}`,
  }
}

export const test = base.extend<{ user: TestUser; app: Page }>({
  user: async ({ playwright }, use) => {
    const api = await playwright.request.newContext({ baseURL: 'http://localhost:5099' })
    const user = makeUser()
    const response = await api.post('/api/auth/register', {
      data: { email: user.email, password: user.password, displayName: user.displayName },
    })
    if (!response.ok()) {
      throw new Error(`register failed: ${response.status()} ${await response.text()}`)
    }
    await api.dispose()
    await use(user)
  },

  app: async ({ page, user }, use) => {
    await page.goto('/login')
    await page.getByLabel('Email').fill(user.email)
    await page.getByLabel('Password').fill(user.password)
    await page.getByRole('button', { name: 'Sign in' }).click()
    await expect(page.getByTestId('user-email')).toHaveText(user.email)
    await expect(page.getByTestId('sidebar-nav')).toBeVisible()
    await use(page)
  },
})

export { expect }
