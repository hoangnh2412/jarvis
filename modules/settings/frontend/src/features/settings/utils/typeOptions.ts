/** Keep in sync with Jarvis.Setting.Validation.SettingTypeOptions. */

export const DEFAULT_EMAIL_REGEX =
  "^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$"

/** Token ngắn trong Options.regex — resolve về DEFAULT_EMAIL_REGEX. */
export const REGEX_TOKEN_DEFAULT = 'default'

export const DEFAULT_IMAGE_MAX_BYTES = 2 * 1024 * 1024

export const DEFAULT_IMAGE_MIME_TYPES = [
  'image/png',
  'image/jpeg',
  'image/gif',
  'image/webp',
] as const

export const DEFAULT_TEXTAREA_ROWS = 3

export type SettingTypeOptionMap = Record<string, string>

function decodeOptionPart(value: string) {
  try {
    return decodeURIComponent(value)
  } catch {
    return value
  }
}

/**
 * Parse Options dạng constraint: `key:value|key:value`
 * (Email / Image / Number / Textarea / Text — không dùng cho Combobox choice list).
 */
export function parseTypeOptions(raw?: string | null): SettingTypeOptionMap {
  if (!raw) return {}

  const map: SettingTypeOptionMap = {}
  for (const entry of raw.split('|').filter(Boolean)) {
    const separator = entry.indexOf(':')
    if (separator <= 0) continue
    const key = decodeOptionPart(entry.slice(0, separator)).trim()
    const value = decodeOptionPart(entry.slice(separator + 1))
    if (!key) continue
    map[key] = value
  }
  return map
}

export function getEmailRegex(options?: string | null): string {
  const map = parseTypeOptions(options)
  const regex = map.regex?.trim()
  if (!regex || regex.toLowerCase() === REGEX_TOKEN_DEFAULT) {
    return DEFAULT_EMAIL_REGEX
  }
  return regex
}

export function getImageMaxBytes(options?: string | null): number {
  const map = parseTypeOptions(options)
  const parsed = Number.parseInt(map.maxBytes ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : DEFAULT_IMAGE_MAX_BYTES
}

export function getImageMimeTypes(options?: string | null): string[] {
  const map = parseTypeOptions(options)
  const raw = map.mimeTypes?.trim()
  if (!raw) return [...DEFAULT_IMAGE_MIME_TYPES]

  const mimes = raw
    .split(',')
    .map((part) => part.trim())
    .filter(Boolean)
    .map((part) => (part.includes('/') ? part : `image/${part}`))

  return mimes.length > 0 ? mimes : [...DEFAULT_IMAGE_MIME_TYPES]
}

/** Số chữ số sau dấu thập phân; `null` = không giới hạn. */
export function getNumberDecimals(options?: string | null): number | null {
  const map = parseTypeOptions(options)
  const parsed = Number.parseInt(map.decimals ?? '', 10)
  return Number.isFinite(parsed) && parsed >= 0 ? parsed : null
}

/** Chiều cao UI; thiếu `rows` → DEFAULT_TEXTAREA_ROWS. */
export function getTextareaRows(options?: string | null): number {
  return getMaxTextareaRows(options) ?? DEFAULT_TEXTAREA_ROWS
}

/**
 * Giới hạn số dòng khi Options có `rows`; `null` = không giới hạn.
 * Đồng bộ với SettingTypeOptions.GetMaxTextareaRows.
 */
export function getMaxTextareaRows(options?: string | null): number | null {
  const map = parseTypeOptions(options)
  const parsed = Number.parseInt(map.rows ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : null
}

/** Đếm dòng text (rỗng = 1); xử lý `\n` / `\r\n` / `\r`. */
export function countTextLines(value: string): number {
  if (value.length === 0) return 1
  return value.replace(/\r\n/g, '\n').replace(/\r/g, '\n').split('\n').length
}

/** Cắt value còn tối đa `maxRows` dòng. */
export function clampTextLines(value: string, maxRows: number): string {
  if (maxRows <= 0 || countTextLines(value) <= maxRows) return value
  const normalized = value.replace(/\r\n/g, '\n').replace(/\r/g, '\n')
  const parts = normalized.split('\n')
  return parts.slice(0, maxRows).join('\n')
}

/** Độ dài tối đa; `null` = không giới hạn. */
export function getMaxLength(options?: string | null): number | null {
  const map = parseTypeOptions(options)
  const parsed = Number.parseInt(map.maxLength ?? '', 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : null
}
