import { getRefreshToken, setAccessToken, setRefreshToken } from './accessToken'
import { getLoginScopeHeaders } from './loginScope'

type TokenPayload = {
  accessToken?: string
  refreshToken?: string
}

type TokenBody = TokenPayload & {
  data?: TokenPayload
}

let refreshing: Promise<string | null> | null = null

/** Đổi access token bằng refresh token. Các request 401 dùng chung một lượt gọi. */
export function refreshAccessToken(): Promise<string | null> {
  if (!refreshing) {
    refreshing = requestRefresh().finally(() => {
      refreshing = null
    })
  }
  return refreshing
}

async function requestRefresh(): Promise<string | null> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) return null

  const response = await fetch('/api/v1/app/account/refresh', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...getLoginScopeHeaders(),
    },
    body: JSON.stringify({ refreshToken }),
  })
  if (!response.ok) return null

  const body = (await response.json().catch(() => null)) as TokenBody | null
  const payload = body?.data ?? body
  const accessToken = payload?.accessToken
  const nextRefreshToken = payload?.refreshToken
  if (!accessToken || !nextRefreshToken) return null

  setAccessToken(accessToken)
  setRefreshToken(nextRefreshToken)
  return accessToken
}
