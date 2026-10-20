import { test, expect } from './fixtures'

test.describe('todos', () => {
  async function createTodo(page: import('@playwright/test').Page, title: string) {
    await page.getByRole('link', { name: 'Todos' }).click()
    await expect(page).toHaveURL(/\/todos$/)
    await page.getByRole('button', { name: 'New todo' }).click()
    await page.getByLabel('Title').fill(title)
    await page.getByLabel('Category').selectOption({ label: 'Personal' })
    await page.getByRole('button', { name: 'Save' }).click()
    await expect(page.getByText(title)).toBeVisible()
  }

  test('create, complete and delete a todo', async ({ app: page }) => {
    const title = `Playwright todo ${Date.now()}`

    await createTodo(page, title)

    const row = page.locator('div').filter({ hasText: title }).first()
    await row.locator('input[type="checkbox"]').click()
    await expect(page.getByText(title)).toHaveCount(0)

    await page.getByRole('combobox').selectOption('done')
    await expect(page.getByText(title)).toBeVisible()

    page.once('dialog', (dialog) => void dialog.accept())
    await page.getByRole('button', { name: 'Delete todo' }).first().click()
    await expect(page.getByText(title)).toHaveCount(0)
  })

  test('todo persists after reload', async ({ app: page }) => {
    const title = `Persistent todo ${Date.now()}`

    await createTodo(page, title)

    await page.reload()
    await page.getByRole('link', { name: 'Todos' }).click()
    await expect(page.getByText(title)).toBeVisible()
  })
})
