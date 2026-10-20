import { test, expect } from './fixtures'

test.describe('navigation', () => {
  test('All scope shows All-scope modules but no page modules', async ({ app: page }) => {
    await expect(page.getByRole('link', { name: 'Dashboard' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Todos' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Habits' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Finance' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Settings' })).toBeVisible()

    await expect(page.getByRole('link', { name: 'Journal' })).toHaveCount(0)
    await expect(page.getByRole('link', { name: 'Schedule' })).toHaveCount(0)
    await expect(page.getByRole('link', { name: 'Goals' })).toHaveCount(0)
    await expect(page.getByRole('link', { name: 'Notes' })).toHaveCount(0)
    await expect(page.getByRole('link', { name: 'Daily Entries' })).toHaveCount(0)
  })

  test('Personal scope shows its modules but not the All-scope ones', async ({ app: page }) => {
    await page.getByTestId('scope-chip').filter({ hasText: 'Personal' }).click()

    await expect(page.getByRole('link', { name: 'Journal' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Schedule' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Goals' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Notes' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Settings' })).toBeVisible()

    await expect(page.getByRole('link', { name: 'Todos' })).toHaveCount(0)
    await expect(page.getByRole('link', { name: 'Habits' })).toHaveCount(0)
    await expect(page.getByRole('link', { name: 'Finance' })).toHaveCount(0)
  })

  test('switching back to All restores the All-scope nav', async ({ app: page }) => {
    await page.getByTestId('scope-chip').filter({ hasText: 'Personal' }).click()
    await expect(page.getByRole('link', { name: 'Journal' })).toBeVisible()

    await page.getByTestId('scope-chip').filter({ hasText: 'All' }).click()
    await expect(page.getByRole('link', { name: 'Todos' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Journal' })).toHaveCount(0)
  })
})
