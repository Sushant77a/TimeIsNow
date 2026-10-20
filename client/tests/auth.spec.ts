import { test, expect, makeUser } from './fixtures'

test.describe('logged out', () => {
  test('visiting the app redirects to login', async ({ page }) => {
    await page.goto('/')
    await expect(page).toHaveURL(/\/login$/)
    await expect(page.getByRole('button', { name: 'Sign in' })).toBeVisible()
  })

  test('register via UI creates account and lands on dashboard', async ({ page }) => {
    const user = makeUser()

    await page.goto('/register')
    await page.getByLabel('Name').fill(user.displayName)
    await page.getByLabel('Email').fill(user.email)
    await page.getByLabel('Password').fill(user.password)
    await page.getByRole('button', { name: 'Create account' }).click()

    await expect(page).toHaveURL('http://localhost:5173/')
    await expect(page.getByTestId('user-email')).toBeVisible()
  })

  test('login with an existing account works', async ({ page, user }) => {
    await page.goto('/login')
    await page.getByLabel('Email').fill(user.email)
    await page.getByLabel('Password').fill(user.password)
    await page.getByRole('button', { name: 'Sign in' }).click()

    await expect(page).toHaveURL('http://localhost:5173/')
    await expect(page.getByTestId('user-email')).toBeVisible()
  })

  test('login with a wrong password shows an error', async ({ page, user }) => {
    await page.goto('/login')
    await page.getByLabel('Email').fill(user.email)
    await page.getByLabel('Password').fill('wrong-password')
    await page.getByRole('button', { name: 'Sign in' }).click()

    await expect(page.getByRole('alert')).toBeVisible()
  })
})

test('signing out clears the session', async ({ app }) => {
  await app.getByRole('button', { name: 'Sign out' }).click()
  await expect(app).toHaveURL(/\/login$/)
})
