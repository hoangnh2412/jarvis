import type { ChangeEvent } from 'react'
import { CalendarDays, CalendarRange, LayoutGrid, Search, X } from 'lucide-react'
import type { PlannerPriority, PlannerViewMode } from '../../types'
import { getPlannerMessages, type PlannerLocale } from '../../localization'

export type PlannerPriorityFilter = PlannerPriority | 'all'

export type PlannerToolbarProps = {
  view: PlannerViewMode
  onViewChange: (view: PlannerViewMode) => void
  locale?: PlannerLocale
  search?: string
  onSearchChange?: (value: string) => void
  priority?: PlannerPriorityFilter
  onPriorityChange?: (value: PlannerPriorityFilter) => void
  /** Số mục sau khi lọc, hiển thị cạnh bộ lọc. */
  total?: number
}

const segmentBase =
  'inline-flex items-center gap-1.5 rounded-lg border-0 px-3 py-1.5 text-sm font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30'

export function PlannerToolbar({
  view,
  onViewChange,
  locale = 'vi',
  search,
  onSearchChange,
  priority = 'all',
  onPriorityChange,
  total,
}: PlannerToolbarProps) {
  const messages = getPlannerMessages(locale)
  const options: {
    value: PlannerViewMode
    label: string
    Icon: typeof LayoutGrid
  }[] = [
    { value: 'kanban', label: messages.page.kanban, Icon: LayoutGrid },
    { value: 'calendar', label: messages.page.calendar, Icon: CalendarDays },
    { value: 'timeline', label: messages.page.timeline, Icon: CalendarRange },
  ]

  const showSearch = typeof onSearchChange === 'function'

  return (
    <div className="mb-3 flex shrink-0 flex-wrap items-center gap-2">
      <div
        role="tablist"
        aria-label={messages.page.title}
        className="inline-flex items-center gap-1 rounded-xl border border-slate-200 bg-slate-50 p-1"
      >
        {options.map(({ value, label, Icon }) => {
          const active = view === value
          return (
            <button
              key={value}
              type="button"
              role="tab"
              aria-selected={active}
              className={[
                segmentBase,
                active
                  ? 'bg-white text-teal-700 shadow-sm'
                  : 'bg-transparent text-slate-600 hover:bg-white/70 hover:text-slate-900',
              ].join(' ')}
              onClick={() => onViewChange(value)}
            >
              <Icon className="h-4 w-4" />
              {label}
            </button>
          )
        })}
      </div>

      {showSearch ? (
        <div className="relative min-w-0 flex-1 sm:max-w-xs">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            value={search ?? ''}
            placeholder={messages.page.search}
            aria-label={messages.page.search}
            className="box-border h-10 w-full rounded-xl border border-slate-200 bg-white pl-9 pr-9 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20"
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              onSearchChange?.(e.target.value)
            }
          />
          {search ? (
            <button
              type="button"
              aria-label={messages.page.clearFilters}
              className="absolute right-2 top-1/2 -translate-y-1/2 rounded-md border-0 bg-transparent p-1 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-700"
              onClick={() => onSearchChange?.('')}
            >
              <X className="h-3.5 w-3.5" />
            </button>
          ) : null}
        </div>
      ) : null}

      {typeof total === 'number' ? (
        <span className="ml-auto text-sm tabular-nums text-slate-500">
          {messages.page.itemsCount(total)}
        </span>
      ) : null}
    </div>
  )
}
