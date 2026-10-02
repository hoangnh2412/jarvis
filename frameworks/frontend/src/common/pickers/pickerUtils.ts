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

export function toDateWire(value: unknown): string {
  if (value == null || value === '') return ''
  const s = String(value)
  const match = s.match(/^(\d{4}-\d{2}-\d{2})/)
  return match?.[1] ?? ''
}

export function formatDateDisplay(isoDate: string): string {
  const dt = parseIsoDate(isoDate)
  if (!dt) return ''
  const d = String(dt.getDate()).padStart(2, '0')
  const m = String(dt.getMonth() + 1).padStart(2, '0')
  const y = dt.getFullYear()
  return `${d}/${m}/${y}`
}

export function formatTimeDisplay(hour: number, minute: number): string {
  return `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
}

export function isDateOnlyWire(value: unknown): boolean {
  const s = String(value ?? '').trim()
  return /^\d{4}-\d{2}-\d{2}$/.test(s)
}

export function parseIsoDateTime(wire: string): Date | null {
  if (!wire) return null
  const dt = new Date(wire)
  return Number.isNaN(dt.getTime()) ? null : dt
}

export function normalizeDateTimeWire(value: unknown): string {
  if (value == null || value === '') return ''
  return String(value).trim()
}

export function formatDateTimeDisplay(wire: string): string {
  const raw = String(wire ?? '').trim()
  if (!raw) return ''
  if (isDateOnlyWire(raw)) return formatDateDisplay(raw)
  const dt = parseIsoDateTime(raw)
  if (!dt) return ''
  return `${formatDateDisplay(toIsoDate(dt))} ${formatTimeDisplay(dt.getHours(), dt.getMinutes())}`
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

export function localPartsFromTimeWire(wire: string): { hour: number; minute: number } {
  const raw = String(wire ?? '').trim()
  const match = raw.match(/^(\d{1,2}):(\d{2})/)
  if (!match) return { hour: 0, minute: 0 }
  const hour = Math.min(23, Math.max(0, Number(match[1])))
  const minute = Math.min(59, Math.max(0, Number(match[2])))
  return { hour, minute }
}

export function wireFromTimeParts(hour: number, minute: number): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${pad(hour)}:${pad(minute)}`
}

export function startOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1)
}

export function buildMonthCells(viewMonth: Date): Array<{
  iso: string
  day: number
  inMonth: boolean
  isToday: boolean
}> {
  const year = viewMonth.getFullYear()
  const month = viewMonth.getMonth()
  const firstDow = new Date(year, month, 1).getDay()
  const daysInMonth = new Date(year, month + 1, 0).getDate()
  const daysInPrev = new Date(year, month, 0).getDate()
  const todayIso = toIsoDate(new Date())

  const cells: Array<{
    iso: string
    day: number
    inMonth: boolean
    isToday: boolean
  }> = []

  for (let i = 0; i < firstDow; i++) {
    const day = daysInPrev - firstDow + 1 + i
    const dt = new Date(year, month - 1, day)
    cells.push({
      iso: toIsoDate(dt),
      day,
      inMonth: false,
      isToday: toIsoDate(dt) === todayIso,
    })
  }
  for (let day = 1; day <= daysInMonth; day++) {
    const dt = new Date(year, month, day)
    cells.push({
      iso: toIsoDate(dt),
      day,
      inMonth: true,
      isToday: toIsoDate(dt) === todayIso,
    })
  }
  while (cells.length % 7 !== 0) {
    const day = cells.length - firstDow - daysInMonth + 1
    const dt = new Date(year, month + 1, day)
    cells.push({
      iso: toIsoDate(dt),
      day,
      inMonth: false,
      isToday: toIsoDate(dt) === todayIso,
    })
  }
  return cells
}

export const PICKER_WEEKDAYS = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'] as const

export const PICKER_MONTH_LABELS = [
  'Tháng 1',
  'Tháng 2',
  'Tháng 3',
  'Tháng 4',
  'Tháng 5',
  'Tháng 6',
  'Tháng 7',
  'Tháng 8',
  'Tháng 9',
  'Tháng 10',
  'Tháng 11',
  'Tháng 12',
] as const
