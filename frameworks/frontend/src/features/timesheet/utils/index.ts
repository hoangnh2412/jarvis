import type { ReactNode } from 'react'
import type {
  TimesheetDayCellContext,
  TimesheetDayColumn,
  TimesheetGrain,
  TimesheetGroupField,
  TimesheetGridRow,
  TimesheetLog,
  TimesheetWorkItem,
  TimesheetWorkLogEntry,
} from '../types'

export type TimesheetSlotContent<TContext> =
  | ReactNode
  | ((ctx: TContext) => ReactNode)

export function resolveTimesheetContent<TContext>(
  content: TimesheetSlotContent<TContext> | undefined,
  ctx: TContext,
  fallback: ReactNode,
): ReactNode {
  if (content == null) return fallback
  if (typeof content === 'function') {
    return (content as (ctx: TContext) => ReactNode)(ctx)
  }
  return content
}

const pad = (n: number) => String(n).padStart(2, '0')

export function formatYmd(date: Date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

export function parseYmd(value: string) {
  const [year, month, day] = value.split('-').map(Number)
  return new Date(year, (month ?? 1) - 1, day ?? 1)
}

export function startOfMonth(date = new Date()) {
  return new Date(date.getFullYear(), date.getMonth(), 1)
}

export function endOfMonth(date = new Date()) {
  return new Date(date.getFullYear(), date.getMonth() + 1, 0)
}

export function enumerateDays(from: Date, to: Date) {
  const days: Date[] = []
  const cursor = new Date(from.getFullYear(), from.getMonth(), from.getDate())
  const end = new Date(to.getFullYear(), to.getMonth(), to.getDate())
  while (cursor.getTime() <= end.getTime() && days.length < 366) {
    days.push(new Date(cursor))
    cursor.setDate(cursor.getDate() + 1)
  }
  return days
}

export function startOfWeek(date: Date) {
  const next = new Date(date.getFullYear(), date.getMonth(), date.getDate())
  const weekday = next.getDay()
  const offset = weekday === 0 ? -6 : 1 - weekday
  next.setDate(next.getDate() + offset)
  return next
}

export function enumerateWeeks(from: Date, to: Date) {
  const weeks: Date[] = []
  const cursor = startOfWeek(from)
  const end = startOfWeek(to)
  while (cursor.getTime() <= end.getTime() && weeks.length < 60) {
    weeks.push(new Date(cursor))
    cursor.setDate(cursor.getDate() + 7)
  }
  return weeks
}

export function isWeekend(date: Date) {
  const day = date.getDay()
  return day === 0 || day === 6
}

export function isSameDay(a: Date, b: Date) {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  )
}

export function formatHours(value?: number | null) {
  if (value == null || value === 0) return ''
  return Number.isInteger(value) ? String(value) : value.toFixed(1)
}

export function getInitials(name?: string | null) {
  if (!name) return ''
  const parts = name.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return ''
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase()
  return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase()
}

const AVATAR_TONES = ['teal', 'sky', 'violet', 'amber', 'rose', 'emerald'] as const

export function getAvatarTone(name?: string | null) {
  const value = name?.trim() ?? ''
  let hash = 0
  for (let i = 0; i < value.length; i += 1) {
    hash = (hash * 31 + value.charCodeAt(i)) % 2147483647
  }
  return AVATAR_TONES[Math.abs(hash) % AVATAR_TONES.length]
}

export function dayField(id: string) {
  return `h_${id.split('-').join('')}`
}

export function buildDayColumns(
  from: Date,
  to: Date,
  grain: TimesheetGrain,
  locale = 'vi',
  today = new Date(),
): TimesheetDayColumn[] {
  const tag = locale === 'en' ? 'en-US' : 'en-US'
  const points = grain === 'week' ? enumerateWeeks(from, to) : enumerateDays(from, to)

  return points.map((date) => {
    const id = formatYmd(date)
    const weekday = date
      .toLocaleDateString(tag, { weekday: 'short' })
      .slice(0, 2)
      .toUpperCase()
    return {
      id,
      date,
      field: dayField(id),
      dayNum:
        grain === 'week'
          ? `${pad(date.getDate())}`
          : pad(date.getDate()),
      weekday: grain === 'week' ? 'WK' : weekday,
      isWeekend: grain === 'day' && isWeekend(date),
      isToday:
        grain === 'day'
          ? isSameDay(date, today)
          : isSameDay(startOfWeek(today), date),
    }
  })
}

function emptyHours(columns: TimesheetDayColumn[]) {
  return Object.fromEntries(columns.map((column) => [column.id, 0]))
}

function addHours(
  target: Record<string, number>,
  date: string,
  hours: number,
  grain: TimesheetGrain,
) {
  if (grain === 'day') {
    target[date] = (target[date] ?? 0) + hours
    return
  }
  const weekId = formatYmd(startOfWeek(parseYmd(date)))
  target[weekId] = (target[weekId] ?? 0) + hours
}

function sumHours(hoursByDay: Record<string, number>) {
  return Object.values(hoursByDay).reduce((total, hours) => total + hours, 0)
}

export function shiftMonthRange(from: string, delta: number) {
  const anchor = parseYmd(from)
  anchor.setMonth(anchor.getMonth() + delta)
  return {
    from: formatYmd(startOfMonth(anchor)),
    to: formatYmd(endOfMonth(anchor)),
  }
}

export function normalizeGroupFields(
  fields?: TimesheetGroupField[],
): [TimesheetGroupField, TimesheetGroupField] {
  const first = fields?.[0] ?? 'project'
  let second = fields?.[1] ?? 'user'
  if (first === 'none') return ['project', 'user']
  if (second === first) second = 'none'
  return [first, second]
}

function getGroupFieldMeta(log: TimesheetLog, field: TimesheetGroupField) {
  if (field === 'key') {
    return {
      id: `key:${log.projectKey}`,
      label: log.projectKey,
      key: log.projectKey,
      projectId: log.projectId,
      projectName: log.projectName,
      userId: undefined,
      userName: undefined,
    }
  }
  if (field === 'user') {
    return {
      id: `user:${log.userId}`,
      label: log.userName,
      key: undefined,
      projectId: undefined,
      projectName: undefined,
      userId: log.userId,
      userName: log.userName,
    }
  }
  return {
    id: `project:${log.projectId}`,
    label: log.projectName,
    key: log.projectKey,
    projectId: log.projectId,
    projectName: log.projectName,
    userId: undefined,
    userName: undefined,
  }
}

function inRange(logs: TimesheetLog[], from: string, to: string) {
  return logs.filter((log) => log.date >= from && log.date <= to)
}

export function buildTimesheetRows(
  logs: TimesheetLog[],
  columns: TimesheetDayColumn[],
  options: {
    from: string
    to: string
    groupBy: TimesheetGroupField[]
    grain: TimesheetGrain
  },
): TimesheetGridRow[] {
  const scoped = inRange(logs, options.from, options.to)
  const grain = options.grain
  const [primaryField, secondaryField] = normalizeGroupFields(options.groupBy)

  if (secondaryField === 'none') {
    const rows = new Map<string, TimesheetGridRow>()
    for (const log of scoped) {
      const meta = getGroupFieldMeta(log, primaryField)
      let row = rows.get(meta.id)
      if (!row) {
        row = {
          id: meta.id,
          rowType: 'leaf',
          depth: 0,
          label: meta.label,
          key: meta.key,
          projectId: log.projectId,
          projectName: log.projectName,
          userId: log.userId,
          userName: log.userName,
          hoursByDay: emptyHours(columns),
          logged: 0,
        }
        rows.set(meta.id, row)
      }
      addHours(row.hoursByDay, log.date, log.hours, grain)
    }
    return [...rows.values()].map((row) => ({
      ...row,
      logged: sumHours(row.hoursByDay),
    }))
  }

  const groups = new Map<string, TimesheetGridRow>()
  const leaves = new Map<string, TimesheetGridRow>()

  for (const log of scoped) {
    const primaryMeta = getGroupFieldMeta(log, primaryField)
    const secondaryMeta = getGroupFieldMeta(log, secondaryField)
    const groupId = primaryMeta.id
    const leafId = `${primaryMeta.id}::${secondaryMeta.id}`

    let group = groups.get(groupId)
    if (!group) {
      group = {
        id: groupId,
        rowType: 'group',
        depth: 0,
        label: primaryMeta.label,
        key: primaryMeta.key,
        projectId: log.projectId,
        projectName: log.projectName,
        userId: log.userId,
        userName: log.userName,
        hoursByDay: emptyHours(columns),
        logged: 0,
        childIds: [],
      }
      groups.set(groupId, group)
    }

    let leaf = leaves.get(leafId)
    if (!leaf) {
      leaf = {
        id: leafId,
        rowType: 'leaf',
        depth: 1,
        label: secondaryMeta.label,
        key: secondaryMeta.key,
        projectId: log.projectId,
        projectName: log.projectName,
        userId: log.userId,
        userName: log.userName,
        hoursByDay: emptyHours(columns),
        logged: 0,
        parentId: groupId,
      }
      leaves.set(leafId, leaf)
      group.childIds = [...(group.childIds ?? []), leaf.id]
    }

    addHours(leaf.hoursByDay, log.date, log.hours, grain)
    addHours(group.hoursByDay, log.date, log.hours, grain)
  }

  const rows: TimesheetGridRow[] = []
  for (const group of groups.values()) {
    group.logged = sumHours(group.hoursByDay)
    rows.push(group)
    for (const childId of group.childIds ?? []) {
      const leaf = leaves.get(childId)
      if (!leaf) continue
      leaf.logged = sumHours(leaf.hoursByDay)
      rows.push(leaf)
    }
  }
  return rows
}

export function flattenTimesheetRows(
  rows: TimesheetGridRow[],
  expanded: Set<string>,
) {
  const visible: TimesheetGridRow[] = []
  for (const row of rows) {
    if (row.rowType === 'leaf' && row.parentId && !expanded.has(row.parentId)) {
      continue
    }
    visible.push(row)
  }
  return visible
}

export function buildTotalRow(
  rows: TimesheetGridRow[],
  columns: TimesheetDayColumn[],
  label: string,
): TimesheetGridRow {
  const hoursByDay = emptyHours(columns)
  const tops = rows.filter((row) => row.depth === 0)
  for (const row of tops) {
    for (const column of columns) {
      hoursByDay[column.id] += row.hoursByDay[column.id] ?? 0
    }
  }
  return {
    id: '__total__',
    rowType: 'group',
    depth: 0,
    label,
    hoursByDay,
    logged: sumHours(hoursByDay),
  }
}

export function rangeInputValue(from: string, to: string) {
  const start = parseYmd(from)
  const end = parseYmd(to)
  const fmt = (date: Date) =>
    `${pad(date.getDate())}/${date.toLocaleString('en-US', { month: 'short' })}/${String(date.getFullYear()).slice(2)}`
  return `${fmt(start)} - ${fmt(end)}`
}

export function formatDayLabel(value: string) {
  const date = parseYmd(value)
  return `${pad(date.getDate())}/${date.toLocaleString('en-US', { month: 'short' })}/${String(date.getFullYear()).slice(2)}`
}

function logMatchesRow(log: TimesheetLog, row: TimesheetGridRow) {
  const id = row.id
  if (id.includes('::')) {
    return (
      Boolean(row.projectId) &&
      Boolean(row.userId) &&
      log.projectId === row.projectId &&
      log.userId === row.userId
    )
  }
  if (id.startsWith('project:')) return log.projectId === row.projectId
  if (id.startsWith('user:')) return log.userId === row.userId
  if (id.startsWith('key:')) return log.projectKey === row.key
  return (
    (!row.projectId || log.projectId === row.projectId) &&
    (!row.userId || log.userId === row.userId)
  )
}

export function collectWorkItemsForCell(
  logs: TimesheetLog[],
  ctx: TimesheetDayCellContext,
): TimesheetWorkItem[] {
  return logs
    .filter((log) => {
      const inSlot =
        ctx.grain === 'week'
          ? formatYmd(startOfWeek(parseYmd(log.date))) === ctx.date
          : log.date === ctx.date
      return inSlot && logMatchesRow(log, ctx.row)
    })
    .flatMap((log) => log.workItems ?? [])
}

export function collectWorkLogEntriesForCell(
  logs: TimesheetLog[],
  ctx: TimesheetDayCellContext,
): TimesheetWorkLogEntry[] {
  return logs
    .filter((log) => {
      const inSlot =
        ctx.grain === 'week'
          ? formatYmd(startOfWeek(parseYmd(log.date))) === ctx.date
          : log.date === ctx.date
      return inSlot && logMatchesRow(log, ctx.row)
    })
    .flatMap((log) =>
      (log.workItems ?? []).map((item) => ({
        id: `${log.id}-${item.id}`,
        date: log.date,
        userId: log.userId,
        userName: log.userName,
        issueKey: item.issueKey,
        issueTitle: item.title,
        description: item.issueKey
          ? `Working on issue ${item.issueKey}`
          : item.title,
        hours: item.hours,
      })),
    )
}
