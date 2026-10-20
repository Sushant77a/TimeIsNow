import { test, expect } from './fixtures'

test('toggling a module off in All scope removes it from the sidebar', async ({ app: page }) => {
  await page.getByRole('link', { name: 'Settings' }).click()
  await expect(page).toHaveURL(/\/settings$/)

  const allScope = page.getByTestId('modules-all')
  const finance = allScope.getByTestId('module-toggle').filter({ hasText: 'Finance' })
  await finance.click()
  await expect(finance).toHaveAttribute('aria-pressed', 'false')
  await expect(allScope.getByText('Saved')).toBeVisible()

  await expect(page.getByRole('link', { name: 'Finance' })).toHaveCount(0)
})

test('toggling a module on in All scope adds it to the sidebar', async ({ app: page }) => {
  await page.getByRole('link', { name: 'Settings' }).click()

  const allScope = page.getByTestId('modules-all')
  const journal = allScope.getByTestId('module-toggle').filter({ hasText: 'Journal' })
  await journal.click()
  await expect(journal).toHaveAttribute('aria-pressed', 'true')
  await expect(allScope.getByText('Saved')).toBeVisible()

  await expect(page.getByRole('link', { name: 'Journal' })).toBeVisible()
})

test('module preferences persist after reload', async ({ app: page }) => {
  await page.getByRole('link', { name: 'Settings' }).click()

  const allScope = page.getByTestId('modules-all')
  await allScope.getByTestId('module-toggle').filter({ hasText: 'Habits' }).click()
  await expect(allScope.getByText('Saved')).toBeVisible()

  await page.reload()
  await expect(page.getByRole('link', { name: 'Habits' })).toHaveCount(0)

  await page.getByRole('link', { name: 'Settings' }).click()
  const reloadedScope = page.getByTestId('modules-all')
  await expect(
    reloadedScope.getByTestId('module-toggle').filter({ hasText: 'Habits' }),
  ).toHaveAttribute('aria-pressed', 'false')
})

test('category module toggle only affects that category scope', async ({ app: page }) => {
  await page.getByRole('link', { name: 'Settings' }).click()

  const personal = page.getByTestId('modules-category').filter({ hasText: 'Personal' })
  await personal.getByTestId('module-toggle').filter({ hasText: 'Journal' }).click()
  await expect(personal.getByText('Saved')).toBeVisible()

  await page.getByTestId('scope-chip').filter({ hasText: 'Personal' }).click()
  await expect(page.getByRole('link', { name: 'Journal' })).toHaveCount(0)
  await expect(page.getByRole('link', { name: 'Schedule' })).toBeVisible()

  await page.getByTestId('scope-chip').filter({ hasText: 'All' }).click()
  await expect(page.getByRole('link', { name: 'Todos' })).toBeVisible()
})
