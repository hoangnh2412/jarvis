import type { LoginResult } from '@jarvis/core'

type ApiResponse = { data: unknown }

export function extractApiData<T>(response: ApiResponse): T {
  const payload = response.data as { data?: T } | T
  if (payload && typeof payload === 'object' && 'data' in payload) {
    return (payload as { data?: T }).data as T
  }
  return payload as T
}

export function extractLoginResult(response: ApiResponse): LoginResult {
  return extractApiData<LoginResult>(response)
}
