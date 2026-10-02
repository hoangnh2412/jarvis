import type { ReactNode } from 'react'
import type { EventInput } from '@fullcalendar/core'
import type { ResourceInput } from '@fullcalendar/resource'
import type { PlannerColumn, PlannerItem } from '../types'

export type PlannerSlotContent<TContext> =
  | ReactNode
  | ((ctx: TContext) => ReactNode)

export function resolvePlannerContent<TContext>(
  content: PlannerSlotContent<TContext> | undefined,
  ctx: TContext,
  fallback: ReactNode,
): ReactNode {
  if (content == null) return fallback
  if (typeof content === 'function') {
    return (content as (ctx: TContext) => ReactNode)(ctx)
  }
  return content
}

function plannerEventClassNames(item: PlannerItem, index: number) {
  return [
    'kit-planner-gantt-event',
    `kit-planner-gantt-event--${item.priority ?? 'medium'}`,
    `kit-planner-gantt-event--tone-${index % 4}`,
  ]
}

function startOfLocalDay(date: Date) {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate())
}

function formatYmd(date: Date) {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

function addDays(date: Date, days: number) {
  const next = new Date(date)
  next.setDate(next.getDate() + days)
  return next
}

function toAllDaySpan(startIso: string, endIso?: string | null) {
  const startDay = startOfLocalDay(new Date(startIso))
  let endExclusive = startOfLocalDay(new Date(endIso ?? startIso))
  endExclusive = addDays(endExclusive, 1)
  if (endExclusive.getTime() <= startDay.getTime()) {
    endExclusive = addDays(startDay, 1)
  }
  return { start: formatYmd(startDay), end: formatYmd(endExclusive) }
}

export function toCalendarEvents(items: PlannerItem[]): EventInput[] {
  return items.map((item, index) => {
    const due = startOfLocalDay(new Date(item.end || item.start))
    const day = formatYmd(due)
    return {
      id: item.id,
      title: item.title,
      start: day,
      end: formatYmd(addDays(due, 1)),
      allDay: true,
      display: 'block',
      classNames: plannerEventClassNames(item, index),
      extendedProps: {
        itemId: item.id,
        originalStart: item.start,
        originalEnd: item.end ?? null,
        originalAllDay: Boolean(item.allDay),
        statusId: item.statusId,
        priority: item.priority,
        assignee: item.assignee,
        description: item.description,
      },
    }
  })
}

export const UNASSIGNED_RESOURCE_ID = '__unassigned__'
export const COLUMN_RESOURCE_PREFIX = 'column:'

export function toAssigneeResourceId(assignee?: string | null) {
  const name = assignee?.trim()
  return name ? `assignee:${name}` : UNASSIGNED_RESOURCE_ID
}

export function fromAssigneeResourceId(resourceId: string) {
  if (!resourceId || resourceId === UNASSIGNED_RESOURCE_ID) return null
  return resourceId.replace(/^assignee:/, '') || null
}

export function toColumnResourceId(statusId: string) {
  return `${COLUMN_RESOURCE_PREFIX}${statusId}`
}

export function fromTimelineResourceId(
  resourceId: string,
  items: PlannerItem[],
) {
  if (resourceId.startsWith(COLUMN_RESOURCE_PREFIX)) {
    return resourceId.slice(COLUMN_RESOURCE_PREFIX.length)
  }
  return items.find((item) => item.id === resourceId)?.statusId ?? null
}

/** Gantt: mỗi hàng là một task, nhóm theo cột trạng thái. */
export function toTimelineResources(
  items: PlannerItem[],
  columns: PlannerColumn[],
): ResourceInput[] {
  const sorted = columns.slice().sort((a, b) => a.order - b.order)

  return sorted
    .map((column) => ({
      id: toColumnResourceId(column.id),
      title: column.name,
      children: items
        .filter((item) => item.statusId === column.id)
        .map((item) => ({
          id: item.id,
          title: item.title,
        })),
    }))
    .filter((group) => group.children.length > 0)
}

export function toTimelineEvents(items: PlannerItem[]): EventInput[] {
  return items.map((item, index) => {
    const span = toAllDaySpan(item.start, item.end)
    return {
      id: item.id,
      resourceId: item.id,
      title: item.title,
      start: span.start,
      end: span.end,
      allDay: true,
      display: 'block',
      classNames: plannerEventClassNames(item, index),
      extendedProps: {
        statusId: item.statusId,
        priority: item.priority,
        assignee: item.assignee,
        description: item.description,
        originalStart: item.start,
        originalEnd: item.end ?? null,
        originalAllDay: Boolean(item.allDay),
      },
    }
  })
}

export function defaultItemStart(date = new Date()) {
  const next = new Date(date)
  next.setMinutes(0, 0, 0)
  next.setHours(next.getHours() + 1)
  return next.toISOString()
}

export function defaultItemEnd(startIso: string) {
  const start = new Date(startIso)
  start.setHours(start.getHours() + 1)
  return start.toISOString()
}

const localeTag: Record<string, string> = { vi: 'vi-VN', en: 'en-US' }

/** Nhãn ngày ngắn cho card Kanban: "17 thg 8" hoặc "17 thg 8, 09:00". */
export function formatPlannerItemDate(item: PlannerItem, locale = 'vi') {
  const start = new Date(item.start)
  if (Number.isNaN(start.getTime())) return ''

  const tag = localeTag[locale] ?? localeTag.vi
  const datePart = new Intl.DateTimeFormat(tag, {
    day: 'numeric',
    month: 'short',
  }).format(start)

  if (item.allDay) return datePart

  const timePart = new Intl.DateTimeFormat(tag, {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  }).format(start)

  return `${datePart}, ${timePart}`
}

/** Khoảng ngày cho tooltip: "Jul 30 – Aug 6" hoặc "30 thg 7 – 6 thg 8". */
export function formatPlannerDateRange(
  startIso: string,
  endIso?: string | null,
  locale = 'vi',
) {
  const start = new Date(startIso)
  if (Number.isNaN(start.getTime())) return ''

  const tag = localeTag[locale] ?? localeTag.vi
  const fmt = new Intl.DateTimeFormat(tag, {
    month: 'short',
    day: 'numeric',
  })
  const startLabel = fmt.format(start)
  if (!endIso) return startLabel

  const end = new Date(endIso)
  if (Number.isNaN(end.getTime())) return startLabel

  const sameDay =
    start.getFullYear() === end.getFullYear() &&
    start.getMonth() === end.getMonth() &&
    start.getDate() === end.getDate()
  if (sameDay) return startLabel

  return `${startLabel} – ${fmt.format(end)}`
}

/** Mốc so sánh là `end` nếu có, không thì `start`. */
export function isPlannerItemOverdue(item: PlannerItem, now = new Date()) {
  const due = new Date(item.end ?? item.start)
  if (Number.isNaN(due.getTime())) return false
  return due.getTime() < now.getTime()
}

/** Chữ viết tắt cho avatar người phụ trách, tối đa 2 ký tự. */
export function getAssigneeInitials(assignee?: string | null) {
  if (!assignee) return ''
  const parts = assignee.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return ''
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase()
  return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase()
}
