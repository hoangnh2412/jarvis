export type LocalizationFormats = {
  dateFormat: string
  timeFormat: string
}

export const DEFAULT_DATE_FORMAT = 'dd/MM/yyyy'
export const DEFAULT_TIME_FORMAT = 'HH:mm'

/** Combine Localization.DateFormat + TimeFormat for DateTime picker. */
export function buildDateTimeDisplayFormat(
  dateFormat = DEFAULT_DATE_FORMAT,
  timeFormat = DEFAULT_TIME_FORMAT,
): string {
  const date = dateFormat.trim() || DEFAULT_DATE_FORMAT
  const time = timeFormat.trim() || DEFAULT_TIME_FORMAT
  return `${date} ${time}`
}

/**
 * Convert app tokens (dd/MM/yyyy, HH:mm, tt) → flatpickr dateFormat
 * (d/m/Y, H:i, K …).
 */
export function toFlatpickrFormat(appFormat: string, withTime: boolean): string {
  const format = (appFormat || DEFAULT_DATE_FORMAT).trim()
  const source = withTime
    ? format
    : format.replace(/\s+(HH|hh|H|h).*$/i, '').trim() || DEFAULT_DATE_FORMAT

  // Replace longer tokens first to avoid partial overlaps.
  return source
    .replace(/yyyy/g, 'Y')
    .replace(/yy/g, 'y')
    .replace(/dd/g, 'd')
    .replace(/MM/g, 'm')
    .replace(/HH/g, 'H')
    .replace(/hh/g, 'h')
    .replace(/mm/g, 'i')
    .replace(/ss/g, 'S')
    .replace(/\btt\b/gi, 'K')
}

export function resolveHourFormat(appFormat: string): '12' | '24' {
  return /\bhh\b/i.test(appFormat) || /\btt\b/i.test(appFormat) ? '12' : '24'
}

export function shouldShowSeconds(appFormat: string): boolean {
  return /:ss\b/i.test(appFormat) || /\bss\b/i.test(appFormat)
}

export function parseStoredDate(value: string): Date | null {
  if (!value?.trim()) return null
  const trimmed = value.trim()

  if (/^\d{4}-\d{2}-\d{2}$/.test(trimmed)) {
    const [y, m, d] = trimmed.split('-').map(Number)
    const date = new Date(y, m - 1, d)
    return Number.isNaN(date.getTime()) ? null : date
  }

  if (/^\d{4}-\d{2}-\d{2}T/.test(trimmed)) {
    const date = new Date(trimmed)
    return Number.isNaN(date.getTime()) ? null : date
  }

  const parsed = Date.parse(trimmed)
  if (Number.isNaN(parsed)) return null
  return new Date(parsed)
}

export function serializeDate(date: Date | null | undefined): string {
  if (!date || Number.isNaN(date.getTime())) return ''
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

export function serializeDateTime(
  date: Date | null | undefined,
  withSeconds: boolean,
): string {
  if (!date || Number.isNaN(date.getTime())) return ''
  const pad = (n: number) => String(n).padStart(2, '0')
  const base = `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
  return withSeconds ? `${base}:${pad(date.getSeconds())}` : base
}

export function splitMultiSelectValue(value: string): string[] {
  if (!value?.trim()) return []
  return value
    .split(',')
    .map((part) => part.trim())
    .filter(Boolean)
}

export function joinMultiSelectValue(values: string[]): string {
  return values.join(',')
}
