import {
  useEffect,
  useId,
  useRef,
  useState,
  type ChangeEvent,
  type ReactNode,
} from 'react'
import {
  CalendarDays,
  Check,
  ChevronDown,
  CircleDot,
  Flag,
  Sun,
  UserRound,
} from 'lucide-react'
import { InputText } from 'primereact/inputtext'
import { Textarea } from 'primereact/textarea'
import { ToggleSwitch } from 'primereact/toggleswitch'
import type {
  CreatePlannerItemPayload,
  PlannerColumn,
  PlannerItem,
  PlannerPriority,
} from '../../types'
import { getPlannerMessages, type PlannerLocale } from '../../localization'
import { getAssigneeInitials } from '../../utils'

export type PlannerItemFormState = CreatePlannerItemPayload

export type PlannerItemFormProps = {
  value: PlannerItemFormState
  columns: PlannerColumn[]
  locale?: PlannerLocale
  titleError?: string | null
  onChange: (value: PlannerItemFormState) => void
}

const priorityTone: Record<PlannerPriority, string> = {
  low: 'kit-planner-prop__priority--low',
  medium: 'kit-planner-prop__priority--medium',
  high: 'kit-planner-prop__priority--high',
}

const STATUS_FALLBACK = ['#64748b', '#2563eb', '#d97706', '#16a34a'] as const

function columnTone(column: PlannerColumn, index: number) {
  return column.color?.trim() || STATUS_FALLBACK[index % STATUS_FALLBACK.length]
}

function toLocalInputValue(iso?: string | null) {
  if (!iso) return ''
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return ''
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

function fromLocalInputValue(value: string) {
  if (!value) return null
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return null
  return date.toISOString()
}

function PropRow({
  icon,
  label,
  children,
}: {
  icon: ReactNode
  label: string
  children: ReactNode
}) {
  return (
    <div className="kit-planner-prop">
      <div className="kit-planner-prop__label">
        <span className="kit-planner-prop__icon" aria-hidden>
          {icon}
        </span>
        <span>{label}</span>
      </div>
      <div className="kit-planner-prop__value">{children}</div>
    </div>
  )
}

function StatusPicker({
  columns,
  value,
  label,
  onChange,
}: {
  columns: PlannerColumn[]
  value: string
  label: string
  onChange: (statusId: string) => void
}) {
  const [open, setOpen] = useState(false)
  const rootRef = useRef<HTMLDivElement>(null)
  const listId = useId()
  const activeIndex = Math.max(
    0,
    columns.findIndex((column) => column.id === value),
  )
  const active = columns[activeIndex] ?? columns[0]
  const tone = active ? columnTone(active, activeIndex) : STATUS_FALLBACK[0]

  useEffect(() => {
    if (!open) return
    const onPointerDown = (event: MouseEvent) => {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false)
    }
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false)
    }
    window.addEventListener('mousedown', onPointerDown)
    window.addEventListener('keydown', onKeyDown)
    return () => {
      window.removeEventListener('mousedown', onPointerDown)
      window.removeEventListener('keydown', onKeyDown)
    }
  }, [open])

  if (!active) return null

  return (
    <div className="kit-planner-status" ref={rootRef}>
      <button
        type="button"
        className="kit-planner-status__trigger"
        style={{
          ['--kit-status-tone' as string]: tone,
          ['--kit-status-soft' as string]: `${tone}22`,
        }}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-controls={listId}
        aria-label={label}
        onClick={() => setOpen((prev) => !prev)}
      >
        <span className="kit-planner-status__dot" aria-hidden />
        <span className="kit-planner-status__text">{active.name}</span>
        <ChevronDown className="kit-planner-status__chevron size-3.5" aria-hidden />
      </button>

      {open ? (
        <ul
          id={listId}
          className="kit-planner-status__menu"
          role="listbox"
          aria-label={label}
        >
          {columns.map((column, index) => {
            const selected = column.id === value
            const optionTone = columnTone(column, index)
            return (
              <li key={column.id} role="presentation">
                <button
                  type="button"
                  role="option"
                  aria-selected={selected}
                  className={`kit-planner-status__option${selected ? ' is-selected' : ''}`}
                  style={{ ['--kit-status-tone' as string]: optionTone }}
                  onClick={() => {
                    onChange(column.id)
                    setOpen(false)
                  }}
                >
                  <span className="kit-planner-status__dot" aria-hidden />
                  <span className="kit-planner-status__option-label">
                    {column.name}
                  </span>
                  {selected ? (
                    <Check className="size-3.5 shrink-0" aria-hidden />
                  ) : (
                    <span className="kit-planner-status__check-spacer" aria-hidden />
                  )}
                </button>
              </li>
            )
          })}
        </ul>
      ) : null}
    </div>
  )
}

export function itemToFormState(item: PlannerItem): PlannerItemFormState {
  return {
    title: item.title,
    description: item.description ?? '',
    statusId: item.statusId,
    start: item.start,
    end: item.end ?? null,
    allDay: Boolean(item.allDay),
    priority: item.priority ?? 'medium',
    assignee: item.assignee ?? '',
  }
}

export function PlannerItemForm({
  value,
  columns,
  locale = 'vi',
  titleError,
  onChange,
}: PlannerItemFormProps) {
  const messages = getPlannerMessages(locale)
  const priorities: { value: PlannerPriority; label: string }[] = [
    { value: 'low', label: messages.form.priorityLow },
    { value: 'medium', label: messages.form.priorityMedium },
    { value: 'high', label: messages.form.priorityHigh },
  ]
  const activePriority = value.priority ?? 'medium'
  const initials = getAssigneeInitials(value.assignee)
  const hasAssignee = Boolean(value.assignee?.trim())

  return (
    <div className="kit-planner-task-form">
      <div className="kit-planner-task-form__title-block">
        <InputText
          unstyled
          value={value.title}
          placeholder={messages.form.titlePlaceholder}
          className={`kit-planner-task-form__title${titleError ? ' is-invalid' : ''}`}
          aria-label={messages.form.title}
          aria-invalid={Boolean(titleError)}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            onChange({ ...value, title: e.target.value })
          }
        />
        {titleError ? (
          <p className="kit-planner-task-form__error">{titleError}</p>
        ) : null}
      </div>

      <div
        className="kit-planner-props"
        role="group"
        aria-label={messages.form.detailSection}
      >
        <PropRow
          icon={<CircleDot className="size-3.5" />}
          label={messages.form.column}
        >
          <StatusPicker
            columns={columns}
            value={value.statusId}
            label={messages.form.column}
            onChange={(statusId) => onChange({ ...value, statusId })}
          />
        </PropRow>

        <PropRow
          icon={<UserRound className="size-3.5" />}
          label={messages.form.assignee}
        >
          <label
            className={`kit-planner-assignee${hasAssignee ? '' : ' is-empty'}`}
          >
            <span
              className={`kit-planner-assignee__avatar${hasAssignee ? '' : ' is-empty'}`}
              aria-hidden
            >
              {hasAssignee ? initials : <UserRound className="size-3.5" />}
            </span>
            <InputText
              unstyled
              value={value.assignee ?? ''}
              placeholder={messages.form.assigneePlaceholder}
              className="kit-planner-assignee__input"
              aria-label={messages.form.assignee}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                onChange({ ...value, assignee: e.target.value })
              }
            />
          </label>
        </PropRow>

        <PropRow
          icon={<CalendarDays className="size-3.5" />}
          label={messages.form.dates}
        >
          <div className="kit-planner-prop__dates">
            <InputText
              unstyled
              type={value.allDay ? 'date' : 'datetime-local'}
              value={
                value.allDay
                  ? toLocalInputValue(value.start).slice(0, 10)
                  : toLocalInputValue(value.start)
              }
              className="kit-planner-prop__date-input"
              aria-label={messages.form.start}
              onChange={(e: ChangeEvent<HTMLInputElement>) => {
                const raw = value.allDay
                  ? `${e.target.value}T00:00`
                  : e.target.value
                const start = fromLocalInputValue(raw)
                if (!start) return
                onChange({ ...value, start })
              }}
            />
            <span className="kit-planner-prop__dates-sep" aria-hidden>
              →
            </span>
            <InputText
              unstyled
              type={value.allDay ? 'date' : 'datetime-local'}
              value={
                value.allDay
                  ? toLocalInputValue(value.end).slice(0, 10)
                  : toLocalInputValue(value.end)
              }
              className="kit-planner-prop__date-input"
              aria-label={messages.form.end}
              onChange={(e: ChangeEvent<HTMLInputElement>) => {
                const raw = value.allDay
                  ? `${e.target.value}T23:59`
                  : e.target.value
                onChange({
                  ...value,
                  end: fromLocalInputValue(raw),
                })
              }}
            />
          </div>
        </PropRow>

        <PropRow
          icon={<Flag className="size-3.5" />}
          label={messages.form.priority}
        >
          <div className="kit-planner-prop__priority-group">
            {priorities.map((option) => {
              const active = activePriority === option.value
              return (
                <button
                  key={option.value}
                  type="button"
                  aria-pressed={active}
                  className={[
                    'kit-planner-prop__priority',
                    priorityTone[option.value],
                    active ? 'is-active' : '',
                  ]
                    .filter(Boolean)
                    .join(' ')}
                  onClick={() =>
                    onChange({ ...value, priority: option.value })
                  }
                >
                  <Flag className="size-3" aria-hidden />
                  {option.label}
                </button>
              )
            })}
          </div>
        </PropRow>

        <PropRow
          icon={<Sun className="size-3.5" />}
          label={messages.form.allDay}
        >
          <div className="kit-planner-prop__toggle">
            <ToggleSwitch.Root
              checked={Boolean(value.allDay)}
              onCheckedChange={(e: { checked: boolean }) =>
                onChange({ ...value, allDay: e.checked })
              }
              className="relative inline-flex h-5 w-9 shrink-0"
              inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0 disabled:cursor-not-allowed"
              ariaLabel={messages.form.allDay}
            >
              <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-5 w-9 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
                <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-4 w-4 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-4" />
              </ToggleSwitch.Control>
            </ToggleSwitch.Root>
            <span className="kit-planner-prop__hint">
              {messages.form.allDayHint}
            </span>
          </div>
        </PropRow>
      </div>

      <div className="kit-planner-task-form__description">
        <p className="kit-planner-task-form__description-label">
          {messages.form.description}
        </p>
        <Textarea
          value={value.description ?? ''}
          rows={6}
          placeholder={messages.form.descriptionPlaceholder}
          className="kit-planner-task-form__description-input"
          aria-label={messages.form.description}
          onChange={(e: ChangeEvent<HTMLTextAreaElement>) =>
            onChange({ ...value, description: e.target.value })
          }
        />
      </div>
    </div>
  )
}
