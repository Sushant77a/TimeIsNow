import type { ApiEnvelope, AuthResponse } from '../types'

export class ApiError extends Error {
  readonly statusCode: number
  readonly errors?: Record<string, string[]>

  constructor(message: string, statusCode: number, errors?: Record<string, string[]>) {
    super(message)
    this.name = 'ApiError'
    this.statusCode = statusCode
    this.errors = errors
  }
}

let accessToken: string | null = null
let refreshPromise: Promise<string | null> | null = null
let sessionExpiredHandler: (() => void) | null = null

export function setAccessToken(token: string | null) {
  accessToken = token
}

export function setSessionExpiredHandler(handler: (() => void) | null) {
  sessionExpiredHandler = handler
}

async function refreshAccessToken(): Promise<string | null> {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      try {
        const response = await fetch('/api/auth/refresh', {
          method: 'POST',
          credentials: 'same-origin',
        })
        if (!response.ok) return null
        const envelope = (await response.json()) as ApiEnvelope<AuthResponse>
        accessToken = envelope.data?.accessToken ?? null
        return accessToken
      } catch {
        return null
      } finally {
        refreshPromise = null
      }
    })()
  }
  return refreshPromise
}

// Endpoints that must not trigger a refresh retry: refreshing the refresh
// token would loop, and the others are terminal (a 401 there means bad
// credentials, not an expired session).
function isRefreshExempt(path: string) {
  return path.startsWith('/api/auth/refresh') || path.startsWith('/api/auth/logout')
}

async function request<T>(path: string, init?: RequestInit, retry = true): Promise<T> {
  const headers: Record<string, string> = {}
  if (init?.body) headers['Content-Type'] = 'application/json'
  if (accessToken) headers.Authorization = `Bearer ${accessToken}`

  const response = await fetch(path, {
    ...init,
    credentials: 'same-origin',
    headers: { ...headers, ...(init?.headers as Record<string, string> | undefined) },
  })

  if (response.status === 401 && retry && !isRefreshExempt(path)) {
    const token = await refreshAccessToken()
    if (token) return request<T>(path, init, false)
    sessionExpiredHandler?.()
  }

  let envelope: ApiEnvelope<T> | null = null
  try {
    envelope = (await response.json()) as ApiEnvelope<T>
  } catch {
    // non-JSON body (e.g. empty 204) — fall through to status handling
  }

  if (!response.ok) {
    throw new ApiError(
      envelope?.message ?? `Request failed (${response.status})`,
      envelope?.statusCode ?? response.status,
      typeof envelope?.data === 'object' && envelope.data !== null
        ? (envelope.data as Record<string, string[]>)
        : undefined,
    )
  }

  return envelope!.data
}

function queryString(params: Record<string, unknown>): string {
  const query = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue
    query.set(key, String(value))
  }
  const result = query.toString()
  return result ? `?${result}` : ''
}

export const api = {
  get: <T>(path: string, params?: Record<string, unknown>) =>
    request<T>(`${path}${params ? queryString(params) : ''}`),

  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'POST', body: body ? JSON.stringify(body) : undefined }),

  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),

  delete: (path: string) => request<void>(path, { method: 'DELETE' }),
}
