import type { APIRequestContext } from '@playwright/test'
import { expect, test } from './fixtures'

const MCP_URL = 'http://localhost:5099/mcp'

type RpcResult = { status: number; body: string }

async function rpc(
  api: APIRequestContext,
  token: string | null,
  payload: unknown,
): Promise<RpcResult> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    Accept: 'application/json, text/event-stream',
  }
  if (token) headers.Authorization = `Bearer ${token}`

  const response = await api.post(MCP_URL, { headers, data: payload })
  return { status: response.status(), body: await response.text() }
}

function parseRpc(body: string): any[] {
  const text = body.trim()
  if (text.startsWith('{') || text.startsWith('[')) return [JSON.parse(text)]
  return text
    .split('\n')
    .filter((line) => line.startsWith('data:'))
    .map((line) => JSON.parse(line.slice(5).trim()))
}

async function mcpToken(api: APIRequestContext, email: string): Promise<string> {
  const login = await api.post('http://localhost:5099/api/auth/login', {
    data: { email, password: 'Test1234!' },
  })
  const accessToken = (await login.json()).data.accessToken
  const minted = await api.post('http://localhost:5099/api/auth/mcp-token', {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  expect(minted.ok()).toBeTruthy()
  return (await minted.json()).data.token
}

test('mcp endpoint requires authentication', async ({ playwright }) => {
  const api = await playwright.request.newContext()
  const { status } = await rpc(api, null, {
    jsonrpc: '2.0',
    id: 1,
    method: 'tools/list',
    params: {},
  })
  expect(status).toBe(401)
  await api.dispose()
})

test('mcp exposes tools and answers a call', async ({ playwright, user }) => {
  const api = await playwright.request.newContext()
  const token = await mcpToken(api, user.email)

  const init = await rpc(api, token, {
    jsonrpc: '2.0',
    id: 1,
    method: 'initialize',
    params: {
      protocolVersion: '2025-11-25',
      capabilities: {},
      clientInfo: { name: 'playwright', version: '1.0' },
    },
  })
  expect(init.status).toBe(200)
  const initPayload = parseRpc(init.body).find((m) => m.id === 1)
  expect(initPayload.result.serverInfo.name).toBe('TimeIsNow.API')

  const list = await rpc(api, token, {
    jsonrpc: '2.0',
    id: 2,
    method: 'tools/list',
    params: {},
  })
  const tools = parseRpc(list.body).find((m) => m.id === 2).result.tools
  const names = tools.map((t: { name: string }) => t.name)
  expect(names).toEqual(
    expect.arrayContaining([
      'get_weekly_briefing',
      'get_today',
      'list_categories',
      'list_todos',
      'create_todo',
      'list_habits',
      'log_habit',
      'list_journal_entries',
      'add_journal_entry',
    ]),
  )

  const created = await rpc(api, token, {
    jsonrpc: '2.0',
    id: 3,
    method: 'tools/call',
    params: { name: 'create_todo', arguments: { title: 'MCP test todo', priority: 'high' } },
  })
  const callResult = parseRpc(created.body).find((m) => m.id === 3).result
  expect(callResult.isError ?? false).toBe(false)
  const text = callResult.content.map((c: { text: string }) => c.text).join('')
  expect(text).toContain('MCP test todo')
  expect(text).toContain('"priority":"High"')

  await api.dispose()
})

test('mcp tool errors surface a useful message', async ({ playwright, user }) => {
  const api = await playwright.request.newContext()
  const token = await mcpToken(api, user.email)

  const { body } = await rpc(api, token, {
    jsonrpc: '2.0',
    id: 1,
    method: 'tools/call',
    params: { name: 'create_todo', arguments: { title: '   ' } },
  })
  const result = parseRpc(body).find((m) => m.id === 1).result
  expect(result.isError).toBe(true)
  const text = result.content.map((c: { text: string }) => c.text).join('')
  expect(text).toContain('title must not be empty')

  await api.dispose()
})
