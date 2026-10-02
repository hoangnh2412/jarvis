import { useLayoutEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { CalendarDays, Flag, TriangleAlert } from 'lucide-react'
import type { EventApi } from '@fullcalendar/core'
import { getPlannerMessages, type PlannerLocale } from '../../localization'
import type { PlannerPriority } from '../../types'
import {
  formatPlannerDateRange,
  getAssigneeInitials,
  isPlannerItemOverdue,
} from '../../utils'

export type PlannerEventHoverInfo = {
  itemId?: string
  title: string
  description?: string | null
  assignee?: string | null
  priority?: PlannerPriority | null
  columnName?: string | null
  start: string
  end?: string | null
  overdue?: boolean
  rect: DOMRect
}

type Placement = 'above' | 'below'

export function hoverInfoFromEvent(
  event: EventApi,
  el: HTMLElement,
  columnName?: string | null,
): PlannerEventHoverInfo {
  const start =
    (event.extendedProps.originalStart as string | undefined) ??
    event.startStr ??
    event.start?.toISOString() ??
    ''
  const end =
    (event.extendedProps.originalEnd as string | null | undefined) ??
    event.endStr ??
    event.end?.toISOString() ??
    null
  const assignee = event.extendedProps.assignee
  const description = event.extendedProps.description
  const priority = event.extendedProps.priority as PlannerPriority | undefined
  const itemId = event.extendedProps.itemId
  const overdue = isPlannerItemOverdue({
    id: typeof itemId === 'string' ? itemId : event.id,
    title: event.title,
    statusId: String(event.extendedProps.statusId ?? ''),
    start,
    end,
    priority,
    assignee: typeof assignee === 'string' ? assignee : null,
  })

  return {
    itemId: typeof itemId === 'string' ? itemId : event.id,
    title: event.title,
    description: typeof description === 'string' ? description : null,
    assignee: typeof assignee === 'string' ? assignee : null,
    priority: priority ?? 'medium',
    columnName: columnName ?? null,
    start,
    end,
    overdue,
    rect: el.getBoundingClientRect(),
  }
}

export type PlannerEventTooltipProps = {
  info: PlannerEventHoverInfo | null
  locale?: PlannerLocale
}

export function PlannerEventTooltip({
  info,
  locale = 'vi',
}: PlannerEventTooltipProps) {
  const messages = getPlannerMessages(locale)
  const nodeRef = useRef<HTMLDivElement>(null)
  const [coords, setCoords] = useState<{
    left: number
    top: number
    placement: Placement
  } | null>(null)

  useLayoutEffect(() => {
    if (!info) {
      setCoords(null)
      return
    }

    const tip = nodeRef.current
    const width = tip?.offsetWidth ?? 320
    const height = tip?.offsetHeight ?? 180
    const gap = 10
    const margin = 8
    let left = info.rect.left + info.rect.width / 2
    left = Math.min(
      Math.max(left, margin + width / 2),
      window.innerWidth - margin - width / 2,
    )
    const placeBelow = info.rect.top < height + gap + margin
    setCoords({
      left,
      top: placeBelow ? info.rect.bottom + gap : info.rect.top - gap,
      placement: placeBelow ? 'below' : 'above',
    })
  }, [info])

  useLayoutEffect(() => {
    if (!coords || !nodeRef.current) return
    const node = nodeRef.current
    node.classList.remove('kit-planner-event-tooltip--visible')
    const frame = requestAnimationFrame(() => {
      node.classList.add('kit-planner-event-tooltip--visible')
    })
    return () => cancelAnimationFrame(frame)
  }, [
    coords,
    info?.title,
    info?.start,
    info?.end,
    info?.assignee,
    info?.description,
    info?.priority,
    info?.columnName,
  ])

  if (!info || typeof document === 'undefined') return null

  const initials = getAssigneeInitials(info.assignee) || '—'
  const assigneeName = info.assignee?.trim() || messages.page.unassigned
  const range = formatPlannerDateRange(info.start, info.end, locale)
  const priority = info.priority ?? 'medium'
  const priorityLabel =
    priority === 'high'
      ? messages.form.priorityHigh
      : priority === 'low'
        ? messages.form.priorityLow
        : messages.form.priorityMedium
  const placement = coords?.placement ?? 'above'
  const isPositioned = coords !== null

  return createPortal(
    <div
      ref={nodeRef}
      className={[
        'kit-planner-event-tooltip',
        `kit-planner-event-tooltip--${placement}`,
        isPositioned ? 'kit-planner-event-tooltip--positioned' : '',
      ]
        .filter(Boolean)
        .join(' ')}
      role="tooltip"
      style={
        coords
          ? {
              left: coords.left,
              top: coords.top,
            }
          : {
              left: info.rect.left + info.rect.width / 2,
              top: info.rect.top - 10,
              visibility: 'hidden',
            }
      }
    >
      {placement === 'below' ? (
        <div className="kit-planner-event-tooltip__arrow" aria-hidden />
      ) : null}
      <div className="kit-planner-event-tooltip__card">
        <div className="kit-planner-event-tooltip__top">
          <div className="kit-planner-event-tooltip__avatar" aria-hidden>
            {initials}
          </div>
          <div className="kit-planner-event-tooltip__heading">
            <p className="kit-planner-event-tooltip__title">{info.title}</p>
            {info.columnName ? (
              <p className="kit-planner-event-tooltip__status">{info.columnName}</p>
            ) : null}
          </div>
        </div>

        {info.description ? (
          <p className="kit-planner-event-tooltip__description">{info.description}</p>
        ) : null}

        <div className="kit-planner-event-tooltip__chips">
          <span
            className={`kit-planner-event-tooltip__chip kit-planner-event-tooltip__chip--${priority}`}
          >
            <Flag aria-hidden />
            {priorityLabel}
          </span>
          {info.overdue ? (
            <span className="kit-planner-event-tooltip__chip kit-planner-event-tooltip__chip--overdue">
              <TriangleAlert aria-hidden />
              {messages.page.overdue}
            </span>
          ) : null}
        </div>

        <div className="kit-planner-event-tooltip__rows">
          <p className="kit-planner-event-tooltip__meta">
            {messages.page.tooltipAssignee(assigneeName)}
          </p>
          {range ? (
            <p className="kit-planner-event-tooltip__meta">
              <CalendarDays className="kit-planner-event-tooltip__meta-icon" aria-hidden />
              {messages.page.tooltipDate(range)}
            </p>
          ) : null}
        </div>
      </div>
      {placement === 'above' ? (
        <div className="kit-planner-event-tooltip__arrow" aria-hidden />
      ) : null}
    </div>,
    document.body,
  )
}
