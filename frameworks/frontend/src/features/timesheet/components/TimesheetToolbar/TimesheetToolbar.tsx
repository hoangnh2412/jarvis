import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import {
  CalendarDays,
  ChevronLeft,
  ChevronRight,
  Download,
  Menu,
  Plus,
  X,
} from 'lucide-react'
import { Button } from 'primereact/button'
import { SearchableMultiSelect } from '../../../../common/SearchableMultiSelect'
import type { SearchableSelectLoadParams } from '../../../../common/SearchableSelect'
import type {
  TimesheetGrain,
  TimesheetGroupField,
  TimesheetOptionsResult,
} from '../../types'
import { getTimesheetMessages, type TimesheetLocale } from '../../localization'
import { callGetTimesheetOptions } from '../../services'
import {
  normalizeGroupFields,
  rangeInputValue,
  shiftMonthRange,
} from '../../utils'
import { btnOutlinedClass, btnPrimaryClass } from '../fieldStyles'

type ActiveGroupField = Exclude<TimesheetGroupField, 'none'>

export type TimesheetToolbarProps = {
  locale?: TimesheetLocale
  filterIds: string[]
  onFilterChange: (value: string[]) => void
  groupBy: TimesheetGroupField[]
  onGroupByChange: (value: TimesheetGroupField[]) => void
  grain: TimesheetGrain
  onGrainChange: (value: TimesheetGrain) => void
  from: string
  to: string
  onRangeChange: (from: string, to: string) => void
  onExport: () => void
  onSave?: () => void
  saving?: boolean
  /** Override load options (mặc định gọi `callGetTimesheetOptions`). */
  loadFilterOptions?: (
    params: SearchableSelectLoadParams,
  ) => Promise<TimesheetOptionsResult>
}

async function defaultLoadFilterOptions(params: SearchableSelectLoadParams) {
  const res = await callGetTimesheetOptions(params)
  return res.data
}

export function TimesheetToolbar({
  locale = 'vi',
  filterIds,
  onFilterChange,
  groupBy,
  onGroupByChange,
  grain,
  onGrainChange,
  from,
  to,
  onRangeChange,
  onExport,
  onSave,
  saving,
  loadFilterOptions = defaultLoadFilterOptions,
}: TimesheetToolbarProps) {
  const messages = getTimesheetMessages(locale)
  const grainMenuRef = useRef<HTMLDivElement>(null)
  const groupMenuRef = useRef<HTMLDivElement>(null)
  const [grainOpen, setGrainOpen] = useState(false)
  const [groupOpen, setGroupOpen] = useState(false)
  const [primaryGroupBy, secondaryGroupBy] = useMemo(
    () => normalizeGroupFields(groupBy),
    [groupBy],
  )

  useEffect(() => {
    if (!grainOpen && !groupOpen) return
    const onMouseDown = (event: MouseEvent) => {
      const target = event.target as Node
      if (grainOpen && !grainMenuRef.current?.contains(target)) setGrainOpen(false)
      if (groupOpen && !groupMenuRef.current?.contains(target)) setGroupOpen(false)
    }
    document.addEventListener('mousedown', onMouseDown)
    return () => document.removeEventListener('mousedown', onMouseDown)
  }, [grainOpen, groupOpen])

  const groupFieldOptions = useMemo(
    () => [
      { value: 'project', label: messages.group.chipProject },
      { value: 'user', label: messages.group.chipUser },
      { value: 'key', label: messages.group.chipKey },
    ],
    [messages.group.chipKey, messages.group.chipProject, messages.group.chipUser],
  )

  const activeGroupFields = useMemo(
    () =>
      [primaryGroupBy, secondaryGroupBy].filter(
        (field): field is ActiveGroupField => field !== 'none',
      ),
    [primaryGroupBy, secondaryGroupBy],
  )

  const inactiveGroupFields = useMemo(
    () =>
      groupFieldOptions.filter(
        (option) => !activeGroupFields.includes(option.value as ActiveGroupField),
      ),
    [activeGroupFields, groupFieldOptions],
  )

  const getFieldLabel = useCallback(
    (field: ActiveGroupField | TimesheetGroupField) =>
      groupFieldOptions.find((option) => option.value === field)?.label ?? field,
    [groupFieldOptions],
  )

  const addGroupField = useCallback(
    (field: ActiveGroupField) => {
      if (activeGroupFields.includes(field)) return
      if (primaryGroupBy === 'none') {
        onGroupByChange([field, 'none'])
        return
      }
      if (secondaryGroupBy === 'none') {
        onGroupByChange([primaryGroupBy, field])
        return
      }
      onGroupByChange([primaryGroupBy, field])
    },
    [activeGroupFields, onGroupByChange, primaryGroupBy, secondaryGroupBy],
  )

  const removeGroupField = useCallback(
    (field: ActiveGroupField) => {
      if (field === primaryGroupBy) {
        onGroupByChange([secondaryGroupBy === 'none' ? 'project' : secondaryGroupBy, 'none'])
        return
      }
      if (field === secondaryGroupBy) {
        onGroupByChange([primaryGroupBy, 'none'])
      }
    },
    [onGroupByChange, primaryGroupBy, secondaryGroupBy],
  )

  const rangeLabel = useMemo(() => rangeInputValue(from, to), [from, to])

  const shiftMonth = useCallback(
    (delta: number) => {
      const next = shiftMonthRange(from, delta)
      onRangeChange(next.from, next.to)
    },
    [from, onRangeChange],
  )

  return (
    <div className="kit-timesheet-toolbar">
      <div className="kit-timesheet-toolbar__top">
        <nav className="kit-timesheet-crumb" aria-label="breadcrumb">
          <span>{messages.page.reports}</span>
          <ChevronRight className="h-3.5 w-3.5 text-slate-400" aria-hidden />
          <strong>{messages.page.loggedTime}</strong>
        </nav>
        <div className="kit-timesheet-toolbar__actions">
          <Button
            type="button"
            unstyled
            className={`${btnOutlinedClass} !h-9`}
            onClick={onExport}
          >
            <Download className="h-4 w-4" />
            {messages.page.export}
          </Button>
          {onSave ? (
            <Button
              type="button"
              unstyled
              className={`${btnPrimaryClass} !h-9 !w-auto !px-3.5`}
              disabled={saving}
              onClick={onSave}
            >
              {messages.page.save}
            </Button>
          ) : null}
        </div>
      </div>

      <div className="kit-timesheet-toolbar__controls">
        <div className="kit-timesheet-range-nav">
          <button
            type="button"
            className="kit-timesheet-range-nav__btn"
            aria-label="Previous month"
            onClick={() => shiftMonth(-1)}
          >
            <ChevronLeft className="h-4 w-4" />
          </button>
          <CalendarDays className="h-4 w-4 text-slate-500" aria-hidden />
          <span className="kit-timesheet-range-nav__label">{rangeLabel}</span>
          <button
            type="button"
            className="kit-timesheet-range-nav__btn"
            aria-label="Next month"
            onClick={() => shiftMonth(1)}
          >
            <ChevronRight className="h-4 w-4" />
          </button>
          <span className="kit-timesheet-range-nav__divider" aria-hidden />
          <div ref={grainMenuRef} className="kit-timesheet-range-nav__menu">
            <button
              type="button"
              className="kit-timesheet-range-nav__btn"
              aria-label={messages.page.grain}
              aria-expanded={grainOpen}
              onClick={() => setGrainOpen((open) => !open)}
            >
              <Menu className="h-4 w-4" />
            </button>
            {grainOpen ? (
              <div className="kit-timesheet-range-nav__popover" role="menu">
                <button
                  type="button"
                  role="menuitem"
                  className={
                    grain === 'day'
                      ? 'kit-timesheet-range-nav__menu-item is-active'
                      : 'kit-timesheet-range-nav__menu-item'
                  }
                  onClick={() => {
                    onGrainChange('day')
                    setGrainOpen(false)
                  }}
                >
                  {messages.page.grainDay}
                </button>
                <button
                  type="button"
                  role="menuitem"
                  className={
                    grain === 'week'
                      ? 'kit-timesheet-range-nav__menu-item is-active'
                      : 'kit-timesheet-range-nav__menu-item'
                  }
                  onClick={() => {
                    onGrainChange('week')
                    setGrainOpen(false)
                  }}
                >
                  {messages.page.grainWeek}
                </button>
              </div>
            ) : null}
          </div>
        </div>

        <SearchableMultiSelect
          value={filterIds}
          loadOptions={loadFilterOptions}
          placeholder={messages.page.filterBy}
          emptyMessage={messages.page.empty}
          triggerClassName="kit-timesheet-filter kit-searchable-multi-select"
          onValueChange={onFilterChange}
        />

        <div
          ref={groupMenuRef}
          className="kit-timesheet-groupby"
          onClick={() => setGroupOpen((open) => !open)}
        >
          <span className="kit-timesheet-groupby__label">{messages.page.groupBy}</span>
          <div className="kit-timesheet-groupby__selects">
            {activeGroupFields.map((field, index) => (
              <span key={field} className="kit-timesheet-groupby__tag">
                <span className="kit-timesheet-groupby__tag-index">{index + 1}.</span>
                <span className="kit-timesheet-groupby__value">{getFieldLabel(field)}</span>
              </span>
            ))}
          </div>
          {groupOpen ? (
            <div
              className="kit-timesheet-groupby__popover"
              onClick={(event) => event.stopPropagation()}
            >
              <div className="kit-timesheet-groupby__section">
                <div className="kit-timesheet-groupby__section-title">
                  {messages.group.active}
                </div>
                {activeGroupFields.map((field, index) => (
                  <button
                    key={field}
                    type="button"
                    className="kit-timesheet-groupby__row"
                    onClick={() => removeGroupField(field)}
                  >
                    <span className="kit-timesheet-groupby__row-label">
                      {index === 1 ? <span className="kit-timesheet-groupby__row-arrow">↳</span> : null}
                      {getFieldLabel(field)}
                    </span>
                    <X className="h-4 w-4" />
                  </button>
                ))}
              </div>
              <div className="kit-timesheet-groupby__section">
                <div className="kit-timesheet-groupby__section-title">
                  {messages.group.inactive}
                </div>
                {inactiveGroupFields.map((option) => (
                  <button
                    key={String(option.value)}
                    type="button"
                    className="kit-timesheet-groupby__row"
                    onClick={() => addGroupField(option.value as ActiveGroupField)}
                  >
                    <span className="kit-timesheet-groupby__row-label">{option.label}</span>
                    <Plus className="h-4 w-4" />
                  </button>
                ))}
              </div>
            </div>
          ) : null}
        </div>
      </div>
    </div>
  )
}
