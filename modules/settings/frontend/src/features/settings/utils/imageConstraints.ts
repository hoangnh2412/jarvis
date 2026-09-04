import {
  DEFAULT_IMAGE_MAX_BYTES,
  DEFAULT_IMAGE_MIME_TYPES,
  getImageMaxBytes,
  getImageMimeTypes,
} from './typeOptions'

/** @deprecated Prefer getImageMaxBytes(options) — giữ export cho tương thích. */
export const SETTING_IMAGE_MAX_BYTES = DEFAULT_IMAGE_MAX_BYTES

/** @deprecated Prefer getImageMimeTypes(options). */
export const SETTING_IMAGE_MIME_TYPES = DEFAULT_IMAGE_MIME_TYPES

export type SettingImageMimeType = (typeof DEFAULT_IMAGE_MIME_TYPES)[number]

export const SETTING_IMAGE_ACCEPT = DEFAULT_IMAGE_MIME_TYPES.join(',')

export function isAllowedImageMimeType(
  mime: string,
  options?: string | null,
): boolean {
  const allowed = new Set(
    getImageMimeTypes(options).map((entry) => entry.toLowerCase()),
  )
  return allowed.has(mime.toLowerCase())
}

export function getImageAccept(options?: string | null): string {
  return getImageMimeTypes(options).join(',')
}

/** Returns decoded payload size in bytes, or null if not a valid data URL for allowed MIME. */
export function getImageDataUrlDecodedBytes(
  value: string,
  options?: string | null,
): number | null {
  const trimmed = value.trim()
  const prefix = 'data:'
  const marker = ';base64,'
  if (!trimmed.toLowerCase().startsWith(prefix)) return null

  const markerIndex = trimmed.toLowerCase().indexOf(marker)
  if (markerIndex < 0) return null

  const mime = trimmed.slice(prefix.length, markerIndex).trim().toLowerCase()
  if (!isAllowedImageMimeType(mime, options)) return null

  const base64 = trimmed.slice(markerIndex + marker.length)
  if (!/^[A-Za-z0-9+/]+={0,2}$/.test(base64)) return null

  const padding = base64.endsWith('==') ? 2 : base64.endsWith('=') ? 1 : 0
  return Math.floor((base64.length * 3) / 4) - padding
}

export function validateImageDataUrl(
  value: string,
  options?: string | null,
): string | null {
  const trimmed = value.trim()
  if (!trimmed) return null

  const maxBytes = getImageMaxBytes(options)
  const bytes = getImageDataUrlDecodedBytes(trimmed, options)
  if (bytes === null) {
    const mimes = getImageMimeTypes(options).join(', ')
    return `Ảnh phải là data URL (${mimes}).`
  }
  if (bytes > maxBytes) {
    const mb = maxBytes / (1024 * 1024)
    return `Ảnh vượt quá ${mb % 1 === 0 ? mb : mb.toFixed(2)}MB.`
  }
  return null
}
