import { useMemo, useRef, useState } from 'react'
import FullCalendar from '@fullcalendar/react'
import resourceTimelinePlugin from '@fullcalendar/resource-timeline'
import interactionPlugin from '@fullcalendar/interaction'
import { CalendarDays, ChevronLeft, ChevronRight } from 'lucide-react'
import type {
  DatesSetArg,
  DateSelectArg,
  EventApi,
  EventClickArg,
  EventContentArg,
  EventDropArg,
  SlotLabelContentArg,
} from '@fullcalendar/core'
import type { EventResizeDoneArg } from '@fullcalendar/interaction'
import type {
  PlannerColumn,
  PlannerItem,
  UpdatePlannerItemPayload,
} from '../../types'
import { getPlannerMessages, type PlannerLocale } from '../../localization'
import {
  COLUMN_RESOURCE_PREFIX,
  fromTimelineResourceId,
  toTimelineEvents,
  toTimelineResources,
} from '../../utils'
import {
  hoverInfoFromEvent,
  PlannerEventTooltip,
  type PlannerEventHoverInfo,
} from '../PlannerEventTooltip'

/** Key non-commercial của FullCalendar Premium (resource-timeline). */
const SCHEDULER_LICENSE_KEY = 'CC-Attribution-NonCommercial-NoDerivatives'

type ResourceSelectArg = DateSelectArg & {
  resource?: { id: string } | null
}

export type TimelineViewProps = {
  items: PlannerItem[]
  columns?: PlannerColumn[]
  locale?: PlannerLocale
  onSelectSlot?: (
    start: string,
    end: string,
    allDay: boolean,
    statusId?: string,
  ) => void
  onOpenItem?: (itemId: string) => void
  onUpdateItem?: (payload: UpdatePlannerItemPayload) => void
  className?: string
}

function renderEventContent(arg: EventContentArg) {
  return (
    <div className="kit-planner-gantt-event__label">{arg.event.title}</div>
  )
}

function renderSlotLabel(arg: SlotLabelContentArg) {
  const weekday = arg.date
    .toLocaleDateString('en-US', { weekday: 'short' })
    .toUpperCase()
  const day = String(arg.date.getDate()).padStart(2, '0')
  return (
    <div className="kit-planner-gantt-slot">
      <span className="kit-planner-gantt-slot__weekday">{weekday}</span>
      <span className="kit-planner-gantt-slot__day">{day}</span>
    </div>
  )
}

function formatMonthTitle(date: Date) {
  return new Intl.DateTimeFormat('en-US', {
    month: 'long',
    year: 'numeric',
  }).format(date)
}

function toMonthInputValue(date: Date) {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}`
}

function ganttResourceClassNames(arg: { resource: { id: string } }) {
  return arg.resource.id.startsWith(COLUMN_RESOURCE_PREFIX)
    ? ['kit-planner-gantt-group']
    : ['kit-planner-gantt-task']
}

export function TimelineView({
  items,
  columns = [],
  locale = 'vi',
  onSelectSlot,
  onOpenItem,
  onUpdateItem,
  className = '',
}: TimelineViewProps) {
  const messages = getPlannerMessages(locale)
  const calendarRef = useRef<FullCalendar>(null)
  const monthInputRef = useRef<HTMLInputElement>(null)
  const [cursor, setCursor] = useState(() => new Date())
  const [hoverInfo, setHoverInfo] = useState<PlannerEventHoverInfo | null>(null)

  const resources = useMemo(
    () => toTimelineResources(items, columns),
    [columns, items],
  )
  const events = useMemo(() => toTimelineEvents(items), [items])

  const applyDrop = (arg: EventDropArg) => {
    onUpdateItem?.({
      id: arg.event.id,
      start: arg.event.start?.toISOString() ?? new Date().toISOString(),
      end: arg.event.end?.toISOString() ?? null,
      allDay: true,
    })
  }

  const handleEventResize = (arg: EventResizeDoneArg) => {
    onUpdateItem?.({
      id: arg.event.id,
      start: arg.event.start?.toISOString() ?? new Date().toISOString(),
      end: arg.event.end?.toISOString() ?? null,
      allDay: true,
    })
  }

  const handleSelect = (arg: ResourceSelectArg) => {
    const statusId = arg.resource
      ? fromTimelineResourceId(arg.resource.id, items)
      : undefined
    onSelectSlot?.(
      arg.start.toISOString(),
      arg.end.toISOString(),
      true,
      statusId ?? undefined,
    )
  }

  const handleEventClick = (arg: EventClickArg) => {
    onOpenItem?.(arg.event.id)
  }

  const hideTooltip = () => {
    document
      .querySelectorAll('.kit-planner-timeline .is-hovered-lane')
      .forEach((node) => node.classList.remove('is-hovered-lane'))
    setHoverInfo(null)
  }

  const showTooltip = (event: EventApi, el: HTMLElement) => {
    document
      .querySelectorAll('.kit-planner-timeline .is-hovered-lane')
      .forEach((node) => node.classList.remove('is-hovered-lane'))
    el.closest('.fc-timeline-lane')?.classList.add('is-hovered-lane')
    setHoverInfo(hoverInfoFromEvent(event, el))
  }

  const handleDatesSet = (arg: DatesSetArg) => {
    setCursor(arg.view.currentStart)
  }

  const getApi = () => calendarRef.current?.getApi()

  const openMonthPicker = () => {
    const input = monthInputRef.current
    if (!input) return
    if (typeof input.showPicker === 'function') {
      input.showPicker()
      return
    }
    input.click()
  }

  return (
    <div
      className={['kit-planner-calendar kit-planner-timeline', className]
        .filter(Boolean)
        .join(' ')}
    >
      <div className="kit-planner-gantt-toolbar">
        <h2 className="kit-planner-gantt-toolbar__title">
          {messages.page.projectTimeline}
        </h2>
        <div className="kit-planner-gantt-toolbar__nav">
          <button
            type="button"
            className="kit-planner-gantt-toolbar__icon-btn"
            aria-label="Previous month"
            onClick={() => getApi()?.prev()}
          >
            <ChevronLeft className="h-4 w-4" />
          </button>
          <button
            type="button"
            className="kit-planner-gantt-toolbar__date"
            onClick={openMonthPicker}
          >
            <CalendarDays className="h-4 w-4" />
            <span>{formatMonthTitle(cursor)}</span>
          </button>
          <input
            ref={monthInputRef}
            type="month"
            className="kit-planner-gantt-toolbar__month-input"
            value={toMonthInputValue(cursor)}
            aria-hidden
            tabIndex={-1}
            onChange={(event) => {
              const [year, month] = event.target.value.split('-').map(Number)
              if (!year || !month) return
              getApi()?.gotoDate(new Date(year, month - 1, 1))
            }}
          />
          <button
            type="button"
            className="kit-planner-gantt-toolbar__icon-btn"
            aria-label="Next month"
            onClick={() => getApi()?.next()}
          >
            <ChevronRight className="h-4 w-4" />
          </button>
        </div>
      </div>

      <div className="kit-planner-gantt-body">
        <FullCalendar
          ref={calendarRef}
          schedulerLicenseKey={SCHEDULER_LICENSE_KEY}
          plugins={[resourceTimelinePlugin, interactionPlugin]}
          initialView="resourceTimelineMonth"
          headerToolbar={false}
          views={{
            resourceTimelineMonth: {
              type: 'resourceTimeline',
              duration: { months: 1 },
              slotDuration: { days: 1 },
              slotLabelInterval: { days: 1 },
            },
          }}
          locale="en"
          height="100%"
          editable
          selectable
          selectMirror
          eventResourceEditable={false}
          displayEventTime={false}
          resourceAreaHeaderContent={messages.page.taskName}
          resourceAreaWidth="17.5rem"
          slotMinWidth={68}
          resourcesInitiallyExpanded
          resources={resources}
          resourceLabelClassNames={ganttResourceClassNames}
          resourceLaneClassNames={ganttResourceClassNames}
          events={events}
          eventContent={renderEventContent}
          eventMouseEnter={(arg) => showTooltip(arg.event, arg.el)}
          eventMouseLeave={hideTooltip}
          eventDragStart={hideTooltip}
          eventResizeStart={hideTooltip}
          slotLabelContent={renderSlotLabel}
          datesSet={handleDatesSet}
          eventDrop={applyDrop}
          eventResize={handleEventResize}
          select={handleSelect}
          eventClick={handleEventClick}
        />
      </div>
      <PlannerEventTooltip info={hoverInfo} locale={locale} />
    </div>
  )
}
