import type { APIRequestContext } from '@playwright/test'
import { expect, test } from './fixtures'

async function tokenFor(api: APIRequestContext, email: string, password: string): Promise<string> {
  const login = await api.post('/api/auth/login', { data: { email, password } })
  expect(login.ok()).toBeTruthy()
  return (await login.json()).data.accessToken
}

test('practice plan endpoint returns a usable draft', async ({ playwright, user }) => {
  const api = await playwright.request.newContext({ baseURL: 'http://localhost:5099' })
  const accessToken = await tokenFor(api, user.email, user.password)

  const response = await api.post('/api/practices/plan', {
    headers: { Authorization: `Bearer ${accessToken}` },
    data: { prompt: 'chakra balancing', sessionsPerDay: 3, days: 21 },
  })
  expect(response.status()).toBe(200)

  const { data } = await response.json()
  expect(['ai', 'template']).toContain(data.source)
  expect(data.title.length).toBeGreaterThan(0)
  expect(data.durationDays).toBeGreaterThan(0)
  expect(data.sessions.length).toBeGreaterThanOrEqual(1)
  for (const session of data.sessions) {
    expect(session.name.length).toBeGreaterThan(0)
    expect(session.instructions.length).toBeGreaterThan(0)
  }

  await api.dispose()
})

test('listing practices requires authentication', async ({ playwright }) => {
  const api = await playwright.request.newContext({ baseURL: 'http://localhost:5099' })
  const response = await api.get('/api/practices')
  expect(response.status()).toBe(401)
  await api.dispose()
})

test('logs sessions, computes streaks, and surfaces the dashboard card', async ({ playwright, user }) => {
  const api = await playwright.request.newContext({ baseURL: 'http://localhost:5099' })
  const accessToken = await tokenFor(api, user.email, user.password)
  const headers = { Authorization: `Bearer ${accessToken}` }

  const categories = await api.get('/api/categories', { headers })
  const categoryId = (await categories.json()).data[0].id

  const start = new Date()
  start.setDate(start.getDate() - 2)

  const created = await api.post('/api/practices', {
    headers,
    data: {
      categoryId,
      title: 'Chakra Balancing',
      overview: 'Three sessions a day.',
      startDate: start.toISOString().slice(0, 10),
      durationDays: 21,
      status: 'Active',
      sessions: [
        { name: 'Morning', timeOfDay: 'Morning', durationMinutes: 10, instructions: 'Work root to crown.' },
        { name: 'Midday', timeOfDay: 'Midday', durationMinutes: 8, instructions: 'Check in with each chakra.' },
      ],
    },
  })
  expect(created.status()).toBe(201)
  const practice = (await created.json()).data
  expect(practice.sessionsPerDay).toBe(2)

  for (let day = 2; day >= 0; day--) {
    const date = new Date()
    date.setDate(date.getDate() - day)
    for (let order = 1; order <= 2; order++) {
      const log = await api.post(`/api/practices/${practice.id}/logs`, {
        headers,
        data: { date: date.toISOString().slice(0, 10), sessionOrder: order, isCompleted: true },
      })
      expect(log.ok()).toBeTruthy()
    }
  }

  const detail = await api.get(`/api/practices/${practice.id}`, { headers })
  const stats = (await detail.json()).data.stats
  expect(stats.totalCompleted).toBe(6)
  expect(stats.todayCompleted).toBe(2)
  expect(stats.currentStreak).toBe(3)
  expect(stats.bestStreak).toBe(3)
  expect(stats.completionRate).toBe(1)

  const rejected = await api.post(`/api/practices/${practice.id}/logs`, {
    headers,
    data: { date: new Date().toISOString().slice(0, 10), sessionOrder: 9, isCompleted: true },
  })
  expect(rejected.status()).toBe(400)

  const dashboard = await api.get('/api/dashboard/today', { headers })
  const practicesToday = (await dashboard.json()).data.practicesToday
  expect(practicesToday).toHaveLength(1)
  expect(practicesToday[0].completedCount).toBe(2)
  expect(practicesToday[0].sessionsPerDay).toBe(2)

  await api.dispose()
})

test('creates a practice through the UI and logs a session', async ({ app: page }) => {
  const navLink = page.getByRole('link', { name: 'Practices' })
  await expect(navLink).toBeVisible()
  await page.waitForTimeout(500)
  await navLink.click()
  await expect(page).toHaveURL(/\/practices$/)

  await page.getByRole('button', { name: 'New practice' }).click()
  await page.getByLabel('What do you want to practice?').fill('Chakra Balancing')
  await page.getByRole('button', { name: 'Create manually' }).click()

  await expect(page.getByRole('heading', { name: 'Review plan' })).toBeVisible()
  await page.getByLabel('Title').fill('UI Chakra Practice')
  await page.getByPlaceholder('What to do in this session').fill('Root to crown, slowly.')
  await page.getByRole('button', { name: 'Start practice' }).click()

  const card = page.getByTestId('practice-card').filter({ hasText: 'UI Chakra Practice' })
  await expect(card).toBeVisible()

  const session = card.getByTestId('practice-session-toggle').first()
  await session.click()
  await expect(card).toContainText('1/1 today')

  await page.reload()
  const reloaded = page.getByTestId('practice-card').filter({ hasText: 'UI Chakra Practice' })
  await expect(reloaded).toContainText('1/1 today')

  await reloaded.getByTestId('practice-expand').click()
  await expect(page.getByTestId('practice-chart')).toBeVisible()
})
