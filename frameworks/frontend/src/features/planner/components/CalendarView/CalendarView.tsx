import { useMemo, useState } from 'react'
import FullCalendar from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import interactionPlugin from '@fullcalendar/interaction'
import type {
  DateSelectArg,
  EventApi,
  EventClickArg,
  EventContentArg,
  EventDropArg,
} from '@fullcalendar/core'
import viLocale from '@fullcalendar/core/locales/vi'
import type { EventResizeDoneArg } from '@fullcalendar/interaction'
import { Flag } from 'lucide-react'
import type {
  PlannerColumn,
  PlannerItem,
  PlannerPriority,
  ReschedulePlannerItemPayload,
} from '../../types'
import type { PlannerLocale } from '../../localization'
import { getAssigneeInitials, toCalendarEvents } from '../../utils'
import {
  PlannerEventTooltip,
  hoverInfoFromEvent,
} from '../PlannerEventTooltip'

function calendarItemId(event: EventApi) {
  const itemId = event.extendedProps.itemId
  return typeof itemId === 'string' ? itemId : event.id
}

function shiftIso(iso: string | null | undefined, ms: number) {
  if (!iso) return null
  return new Date(new Date(iso).getTime() + ms).toISOString()
}

function renderEventContent(arg: EventContentArg) {
  const assignee = arg.event.extendedProps.assignee
  const priority =
    (arg.event.extendedProps.priority as PlannerPriority | undefined) ?? 'medium'
  const initials =
    typeof assignee === 'string' ? getAssigneeInitials(assignee) : ''

  return (
    <div className="kit-planner-cal-event">
      <div className="kit-planner-cal-event__row">
        <span
          className={`kit-planner-cal-event__priority kit-planner-cal-event__priority--${priority}`}
          aria-hidden
        >
          <Flag className="size-2.5" />
        </span>
        <span className="kit-planner-cal-event__title">{arg.event.title}</span>
        {initials ? (
          <span className="kit-planner-cal-event__avatar" aria-hidden>
            {initials}
          </span>
        ) : null}
      </div>
    </div>
  )
}

export type CalendarViewProps = {
  items: PlannerItem[]
  columns?: PlannerColumn[]
  locale?: PlannerLocale
  onSelectSlot?: (start: string, end: string, allDay: boolean) => void
  onOpenItem?: (itemId: string) => void
  onReschedule?: (payload: ReschedulePlannerItemPayload) => void
  className?: string
}

export function CalendarView({
  items,
  columns = [],
  locale = 'vi',
  onSelectSlot,
  onOpenItem,
  onReschedule,
  className = '',
}: CalendarViewProps) {
  const events = useMemo(() => toCalendarEvents(items), [items])
  const [hoverInfo, setHoverInfo] = useState<ReturnType<
    typeof hoverInfoFromEvent
  > | null>(null)

  const hideTooltip = () => setHoverInfo(null)

  const showTooltip = (event: EventApi, el: HTMLElement) => {
    const statusId = event.extendedProps.statusId
    const columnName =
      typeof statusId === 'string'
        ? columns.find((column) => column.id === statusId)?.name
        : null
    setHoverInfo(hoverInfoFromEvent(event, el, columnName))
  }

  const handleEventDrop = (arg: EventDropArg) => {
    hideTooltip()
    const ms =
      (arg.event.start?.getTime() ?? 0) - (arg.oldEvent.start?.getTime() ?? 0)
    const originalStart = arg.event.extendedProps.originalStart as
      | string
      | undefined
    const originalEnd = arg.event.extendedProps.originalEnd as
      | string
      | null
      | undefined
    onReschedule?.({
      id: calendarItemId(arg.event),
      start:
        shiftIso(originalStart, ms) ??
        arg.event.start?.toISOString() ??
        new Date().toISOString(),
      end: originalEnd == null ? null : shiftIso(originalEnd, ms),
      allDay: Boolean(arg.event.extendedProps.originalAllDay),
    })
  }

  const handleEventResize = (arg: EventResizeDoneArg) => {
    hideTooltip()
    onReschedule?.({
      id: calendarItemId(arg.event),
      start: arg.event.start?.toISOString() ?? new Date().toISOString(),
      end: arg.event.end?.toISOString() ?? null,
      allDay: Boolean(arg.event.extendedProps.originalAllDay),
    })
  }

  const handleSelect = (arg: DateSelectArg) => {
    hideTooltip()
    onSelectSlot?.(arg.start.toISOString(), arg.end.toISOString(), arg.allDay)
    arg.view.calendar.unselect()
  }

  const handleEventClick = (arg: EventClickArg) => {
    hideTooltip()
    onOpenItem?.(calendarItemId(arg.event))
  }

  return (
    <div className={['kit-planner-calendar', className].filter(Boolean).join(' ')}>
      <FullCalendar
        plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
        initialView="dayGridMonth"
        headerToolbar={{
          left: 'prev,next today',
          center: 'title',
          right: 'dayGridMonth,timeGridWeek,timeGridDay',
        }}
        locales={locale === 'vi' ? [viLocale] : []}
        locale={locale === 'vi' ? 'vi' : 'en'}
        height="100%"
        editable
        selectable
        selectMirror={false}
        unselectAuto
        eventDurationEditable={false}
        fixedWeekCount={false}
        dayMaxEvents={5}
        moreLinkClick="popover"
        displayEventTime={false}
        events={events}
        eventContent={renderEventContent}
        eventMouseEnter={(arg) => showTooltip(arg.event, arg.el)}
        eventMouseLeave={hideTooltip}
        eventDragStart={hideTooltip}
        eventResizeStart={hideTooltip}
        eventDrop={handleEventDrop}
        eventResize={handleEventResize}
        select={handleSelect}
        eventClick={handleEventClick}
      />
      <PlannerEventTooltip info={hoverInfo} locale={locale} />
    </div>
  )
}
