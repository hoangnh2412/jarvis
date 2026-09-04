/** Normalize to always end with `/` so relative paths join correctly. */
export function normalizeApiBaseUrl(url: string): string {
  const trimmed = url.trim()
  if (!trimmed) return '/api/'
  return trimmed.endsWith('/') ? trimmed : `${trimmed}/`
}

/**
 * Shared API base — host app sets `VITE_API_URL` (e.g. `/api/`).
 * Dev: same-origin + Vite `VITE_API_PROXY_TARGET` proxy.
 */
export const BASE_URL = normalizeApiBaseUrl(
  (typeof import.meta !== 'undefined'
    ? import.meta.env?.VITE_API_URL
    : undefined) || '/api/',
)

export const API_KEY =
  (typeof import.meta !== 'undefined'
    ? import.meta.env?.VITE_API_KEY
    : undefined) || ''

export const API_KEY_HEADER =
  (typeof import.meta !== 'undefined'
    ? import.meta.env?.VITE_API_KEY_NAME
    : undefined) || 'X-API-KEY'
