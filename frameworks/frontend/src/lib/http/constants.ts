/** Normalize to always end with `/` so relative paths join correctly. */
export function normalizeApiBaseUrl(url: string): string {
  const trimmed = url.trim()
  if (!trimmed) return '/api/'
  return trimmed.endsWith('/') ? trimmed : `${trimmed}/`
}

/**
 * Shared API base — host app sets `VITE_API_URL` (e.g. `/api/`).
 * Dev: same-origin; the host Vite config proxies `/api`.
 */
export const BASE_URL = normalizeApiBaseUrl(
  (typeof import.meta !== 'undefined'
    ? import.meta.env?.VITE_API_URL
    : undefined) || '/api/',
)

/** Fallback khi host không truyền `apiKey` vào `configurePlatformHttp`. */
export const API_KEY = ''

/** Fallback khi host không truyền `apiKeyHeader` vào `configurePlatformHttp`. */
export const API_KEY_HEADER = 'X-API-KEY'
