const STORAGE_KEY = 'platform.accessToken'
const REFRESH_STORAGE_KEY = 'platform.refreshToken'

export function getAccessToken(): string | null {
  if (typeof localStorage === 'undefined') return null
  return localStorage.getItem(STORAGE_KEY)
}

export function setAccessToken(token: string) {
  localStorage.setItem(STORAGE_KEY, token)
}

export function getRefreshToken(): string | null {
  if (typeof localStorage === 'undefined') return null
  return localStorage.getItem(REFRESH_STORAGE_KEY)
}

export function setRefreshToken(token: string) {
  localStorage.setItem(REFRESH_STORAGE_KEY, token)
}

export function clearAccessToken() {
  localStorage.removeItem(STORAGE_KEY)
  localStorage.removeItem(REFRESH_STORAGE_KEY)
}

export function isAuthenticated(): boolean {
  return !!getAccessToken()
}

/** `exp` của JWT đã qua hoặc sắp tới trong 15 giây. Token không đọc được thì không coi là hết hạn. */
export function isAccessTokenExpired(token: string): boolean {
  const part = token.split('.')[1]
  if (!part) return false
  try {
    const padded = part.replace(/-/g, '+').replace(/_/g, '/')
    const payload = JSON.parse(atob(padded)) as { exp?: number }
    if (typeof payload.exp !== 'number') return false
    return payload.exp * 1000 <= Date.now() + 15_000
  } catch {
    return false
  }
}
