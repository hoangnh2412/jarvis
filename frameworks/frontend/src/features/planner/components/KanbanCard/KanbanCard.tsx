import {
  useEffect,
  useRef,
  useState,
  type CSSProperties,
  type KeyboardEvent,
} from 'react'
import { draggable } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { CalendarDays, Flag, TriangleAlert } from 'lucide-react'
import type { PlannerItem, PlannerPriority } from '../../types'
import { getPlannerMessages, type PlannerLocale } from '../../localization'
import {
  formatPlannerItemDate,
  getAssigneeInitials,
  isPlannerItemOverdue,
} from '../../utils'

export type KanbanCardProps = {
  item: PlannerItem
  locale?: PlannerLocale
  /** Tắt nhãn quá hạn cho cột kết thúc (ví dụ Done). */
  showOverdue?: boolean
  onOpen?: (item: PlannerItem) => void
}

const priorityChipClass: Record<PlannerPriority, string> = {
  low: 'kit-planner-chip--low',
  medium: 'kit-planner-chip--medium',
  high: 'kit-planner-chip--high',
}

const priorityAccent: Record<PlannerPriority, string> = {
  low: '#22c55e',
  medium: '#f59e0b',
  high: '#e11d48',
}

export function KanbanCard({
  item,
  locale = 'vi',
  showOverdue = true,
  onOpen,
}: KanbanCardProps) {
  const ref = useRef<HTMLDivElement>(null)
  const [dragging, setDragging] = useState(false)
  const messages = getPlannerMessages(locale)
  const priority = item.priority ?? 'medium'

  useEffect(() => {
    const element = ref.current
    if (!element) return

    return draggable({
      element,
      getInitialData: () => ({
        type: 'planner-card',
        itemId: item.id,
        statusId: item.statusId,
      }),
      onDragStart: () => setDragging(true),
      onDrop: () => setDragging(false),
    })
  }, [item.id, item.statusId])

  const priorityLabel =
    priority === 'high'
      ? messages.form.priorityHigh
      : priority === 'low'
        ? messages.form.priorityLow
        : messages.form.priorityMedium

  const overdue = showOverdue && isPlannerItemOverdue(item)
  const dateLabel = formatPlannerItemDate(item, locale)
  const initials = getAssigneeInitials(item.assignee)

  const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key !== 'Enter' && event.key !== ' ') return
    event.preventDefault()
    onOpen?.(item)
  }

  return (
    <div
      ref={ref}
      role="button"
      tabIndex={0}
      style={
        { '--kit-card-accent': priorityAccent[priority] } as CSSProperties
      }
      className={['kit-planner-card', dragging ? 'is-dragging' : '']
        .filter(Boolean)
        .join(' ')}
      onClick={() => onOpen?.(item)}
      onKeyDown={handleKeyDown}
    >
      <p className="kit-planner-card__title">{item.title}</p>
      {item.description ? (
        <p className="kit-planner-card__desc">{item.description}</p>
      ) : null}

      <div className="kit-planner-card__footer">
        <div className="kit-planner-card__meta">
          <span className={`kit-planner-chip ${priorityChipClass[priority]}`}>
            <Flag aria-hidden />
            {priorityLabel}
          </span>
          {dateLabel ? (
            <span
              className={[
                'kit-planner-chip',
                overdue ? 'kit-planner-chip--overdue' : '',
              ]
                .filter(Boolean)
                .join(' ')}
            >
              {overdue ? <TriangleAlert aria-hidden /> : <CalendarDays aria-hidden />}
              {overdue ? `${messages.page.overdue}: ${dateLabel}` : dateLabel}
            </span>
          ) : null}
        </div>

        {initials ? (
          <span className="kit-planner-avatar" title={item.assignee ?? undefined}>
            {initials}
          </span>
        ) : null}
      </div>
    </div>
  )
}
