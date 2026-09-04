const STORAGE_KEY = 'jarvis.accessToken'

export function getAccessToken(): string | null {
  if (typeof localStorage === 'undefined') return null
  return localStorage.getItem(STORAGE_KEY)
}

export function setAccessToken(token: string) {
  localStorage.setItem(STORAGE_KEY, token)
}

export function clearAccessToken() {
  localStorage.removeItem(STORAGE_KEY)
}

export function isAuthenticated(): boolean {
  return !!getAccessToken()
}
