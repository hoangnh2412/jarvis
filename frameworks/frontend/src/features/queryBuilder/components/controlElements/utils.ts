import type { OptionList } from 'react-querybuilder'

export type FlatOption = { label: string; value: string }

export function flattenOptions(options: OptionList | unknown): FlatOption[] {
  if (!Array.isArray(options)) return []
  const out: FlatOption[] = []
  for (const item of options as unknown[]) {
    if (item && typeof item === 'object' && 'options' in item) {
      const group = item as {
        options?: Array<{ name?: string; value?: string; label?: string }>
      }
      for (const o of group.options ?? []) {
        out.push({
          label: o.label ?? String(o.name ?? o.value ?? ''),
          value: String(o.name ?? o.value ?? ''),
        })
      }
      continue
    }
    const o = item as { name?: string; value?: string; label?: string }
    out.push({
      label: o.label ?? String(o.name ?? o.value ?? ''),
      value: String(o.name ?? o.value ?? ''),
    })
  }
  return out
}

export function parseBetweenValue(value: unknown): [string, string] {
  if (Array.isArray(value) && value.length >= 2) {
    return [String(value[0] ?? ''), String(value[1] ?? '')]
  }
  if (typeof value === 'string' && value.includes(',')) {
    const [a = '', b = ''] = value.split(',').map((s) => s.trim())
    return [a, b]
  }
  return ['', '']
}

export function isDateTimeField(
  inputType: string | null | undefined,
  fieldData: { inputType?: string | null; datatype?: string | null } | undefined,
): boolean {
  const raw =
    inputType || fieldData?.inputType || fieldData?.datatype || undefined
  return raw === 'datetime' || raw === 'datetime-local'
}

export function isDateOnlyField(
  inputType: string | null | undefined,
  fieldData: { inputType?: string | null; datatype?: string | null } | undefined,
): boolean {
  const raw =
    inputType || fieldData?.inputType || fieldData?.datatype || undefined
  return raw === 'date'
}

export function isDateField(
  inputType: string | null | undefined,
  fieldData: { inputType?: string | null; datatype?: string | null } | undefined,
): boolean {
  return isDateOnlyField(inputType, fieldData) || isDateTimeField(inputType, fieldData)
}

/** Wire format for FilterParser date (and DateTime day-level): `yyyy-MM-dd`. */
export function toDateWire(value: unknown): string {
  if (value == null || value === '') return ''
  const s = String(value)
  const match = s.match(/^(\d{4}-\d{2}-\d{2})/)
  return match?.[1] ?? ''
}

/** ISO wire for DateTime fields — BE accepts `yyyy-MM-ddTHH:mm:ss.fffZ`. */
export function toDateTimeWire(value: unknown): string {
  if (value == null || value === '') return ''
  const s = String(value).trim()
  const parsed = new Date(s)
  if (!Number.isNaN(parsed.getTime())) return parsed.toISOString()
  const dateOnly = s.match(/^(\d{4}-\d{2}-\d{2})$/)
  if (dateOnly) {
    const dt = new Date(`${dateOnly[1]}T00:00:00`)
    if (!Number.isNaN(dt.getTime())) return dt.toISOString()
  }
  return ''
}

export function isDateOnlyWire(value: unknown): boolean {
  const s = String(value ?? '').trim()
  return /^\d{4}-\d{2}-\d{2}$/.test(s)
}

/** Preserve date-only vs ISO — do not normalize to ISO for storage compare. */
export function normalizeDateTimeWire(value: unknown): string {
  if (value == null || value === '') return ''
  return String(value).trim()
}

export function parseIsoDateTime(wire: string): Date | null {
  if (!wire) return null
  const dt = new Date(wire)
  return Number.isNaN(dt.getTime()) ? null : dt
}

export function formatDateOnlyDisplay(isoDate: string): string {
  const dt = parseIsoDate(isoDate)
  if (!dt) return ''
  const d = String(dt.getDate()).padStart(2, '0')
  const m = String(dt.getMonth() + 1).padStart(2, '0')
  const y = dt.getFullYear()
  return `${d}/${m}/${y}`
}

export function formatDateTimeDisplay(wire: string): string {
  const raw = String(wire ?? '').trim()
  if (!raw) return ''
  if (isDateOnlyWire(raw)) return formatDateOnlyDisplay(raw)
  const dt = parseIsoDateTime(raw)
  if (!dt) return ''
  const d = String(dt.getDate()).padStart(2, '0')
  const m = String(dt.getMonth() + 1).padStart(2, '0')
  const y = dt.getFullYear()
  const hh = String(dt.getHours()).padStart(2, '0')
  const mm = String(dt.getMinutes()).padStart(2, '0')
  return `${d}/${m}/${y} ${hh}:${mm}`
}

export function localPartsFromDateTimeWire(wire: string): {
  dateIso: string
  hour: number
  minute: number
  hasTime: boolean
} {
  const raw = String(wire ?? '').trim()
  if (!raw) {
    const now = new Date()
    return { dateIso: toIsoDate(now), hour: 0, minute: 0, hasTime: false }
  }
  if (isDateOnlyWire(raw)) {
    return { dateIso: raw, hour: 0, minute: 0, hasTime: false }
  }
  const dt = parseIsoDateTime(raw)
  if (!dt) {
    const now = new Date()
    return { dateIso: toIsoDate(now), hour: 0, minute: 0, hasTime: false }
  }
  return {
    dateIso: toIsoDate(dt),
    hour: dt.getHours(),
    minute: dt.getMinutes(),
    hasTime: true,
  }
}

export function wireFromDateTimeParts(parts: {
  dateIso: string
  hour: number
  minute: number
  hasTime: boolean
}): string {
  if (!parts.dateIso) return ''
  if (!parts.hasTime) return parts.dateIso
  return combineLocalDateTime(parts.dateIso, parts.hour, parts.minute)
}

export function combineLocalDateTime(
  dateIso: string,
  hour: number,
  minute: number,
): string {
  if (!dateIso) return ''
  const dt = parseIsoDate(dateIso)
  if (!dt) return ''
  dt.setHours(hour, minute, 0, 0)
  return dt.toISOString()
}

export function parseIsoDate(isoDate: string): Date | null {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(isoDate)) return null
  const [y, m, d] = isoDate.split('-').map(Number)
  const dt = new Date(y!, m! - 1, d!)
  return Number.isNaN(dt.getTime()) ? null : dt
}

export function toIsoDate(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}
