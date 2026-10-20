import { expect, test } from './fixtures'

test('briefing endpoint returns stats and a narrative', async ({ playwright, user }) => {
  const api = await playwright.request.newContext({ baseURL: 'http://localhost:5099' })

  const login = await api.post('/api/auth/login', {
    data: { email: user.email, password: user.password },
  })
  expect(login.ok()).toBeTruthy()
  const { accessToken } = (await login.json()).data

  const response = await api.get('/api/dashboard/briefing', {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  expect(response.status()).toBe(200)

  const body = await response.json()
  expect(['template', 'ai']).toContain(body.data.narrativeSource)
  if (body.data.narrativeSource === 'ai') {
    expect(body.data.narrativeProvider).toBeTruthy()
  } else {
    expect(body.data.narrativeProvider).toBeNull()
  }
  expect(body.data.narrative.length).toBeGreaterThan(20)
  expect(body.data.weekStart).toMatch(/^\d{4}-\d{2}-\d{2}$/)
  expect(body.data.weekEnd).toMatch(/^\d{4}-\d{2}-\d{2}$/)
  expect(body.data.stats.todosCompleted).toBe(0)
  expect(body.data.stats.habitOpportunities).toBe(0)

  const previous = await api.get('/api/dashboard/briefing?weekStart=2026-09-21', {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  expect(previous.status()).toBe(200)
  const previousBody = await previous.json()
  expect(previousBody.data.weekStart).toBe('2026-09-21')
  expect(previousBody.data.weekEnd).toBe('2026-09-27')

  await api.dispose()
})

test('dashboard shows the weekly briefing card', async ({ app }) => {
  const card = app.getByTestId('weekly-briefing')

  await expect(card).toBeVisible({ timeout: 20000 })
  await expect(card).toContainText('Weekly briefing')
  await expect(card.getByText(/^(AI|Template)$/)).toBeVisible()
  await expect(card).toContainText(/\w{3} \d{1,2}, \d{4} to \w{3} \d{1,2}, \d{4}/)
  await expect(card.getByText('Habit check-ins', { exact: true })).toBeVisible()
  await expect(card.getByText('Todos closed', { exact: true })).toBeVisible()
})
