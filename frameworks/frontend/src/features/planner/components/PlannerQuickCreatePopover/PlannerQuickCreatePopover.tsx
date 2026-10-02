import { useEffect, useLayoutEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { CalendarDays, Flag, X } from 'lucide-react'
import { Button } from 'primereact/button'
import { InputText } from 'primereact/inputtext'
import { getPlannerMessages, type PlannerLocale } from '../../localization'
import type { PlannerColumn, PlannerPriority } from '../../types'
import { formatPlannerDateRange } from '../../utils'
import { btnOutlinedClass, btnPrimaryClass, fieldInputClass } from '../fieldStyles'
import type { PlannerItemFormState } from '../PlannerItemForm'

export type PlannerQuickCreateAnchor = {
  top: number
  left: number
  width: number
  height: number
}

export type PlannerQuickCreatePopoverProps = {
  open: boolean
  anchor: PlannerQuickCreateAnchor | null
  value: PlannerItemFormState
  columns: PlannerColumn[]
  locale?: PlannerLocale
  titleError?: string | null
  saving?: boolean
  onChange: (value: PlannerItemFormState) => void
  onSave: () => void
  onCancel: () => void
  onOpenFull?: () => void
}

function clamp(value: number, min: number, max: number) {
  return Math.min(Math.max(value, min), max)
}

export function PlannerQuickCreatePopover({
  open,
  anchor,
  value,
  columns,
  locale = 'vi',
  titleError,
  saving = false,
  onChange,
  onSave,
  onCancel,
  onOpenFull,
}: PlannerQuickCreatePopoverProps) {
  const messages = getPlannerMessages(locale)
  const panelRef = useRef<HTMLDivElement>(null)
  const titleRef = useRef<HTMLInputElement>(null)
  const [position, setPosition] = useState({ top: 0, left: 0, placement: 'below' as 'above' | 'below' })

  useLayoutEffect(() => {
    if (!open || !anchor || !panelRef.current) return
    const panel = panelRef.current.getBoundingClientRect()
    const margin = 12
    const gap = 10
    const placeBelow =
      anchor.top + anchor.height + gap + panel.height <= window.innerHeight - margin ||
      anchor.top < panel.height + gap + margin
    const top = placeBelow
      ? clamp(anchor.top + anchor.height + gap, margin, window.innerHeight - panel.height - margin)
      : clamp(anchor.top - panel.height - gap, margin, window.innerHeight - panel.height - margin)
    const left = clamp(
      anchor.left + anchor.width / 2 - panel.width / 2,
      margin,
      Math.max(margin, window.innerWidth - panel.width - margin),
    )
    setPosition({ top, left, placement: placeBelow ? 'below' : 'above' })
  }, [anchor, open, value.priority, value.statusId])

  useEffect(() => {
    if (!open) return
    const frame = requestAnimationFrame(() => titleRef.current?.focus())
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.preventDefault()
        onCancel()
      }
      if (event.key === 'Enter' && (event.metaKey || event.ctrlKey)) {
        event.preventDefault()
        onSave()
      }
    }
    window.addEventListener('keydown', onKeyDown)
    return () => {
      cancelAnimationFrame(frame)
      window.removeEventListener('keydown', onKeyDown)
    }
  }, [onCancel, onSave, open])

  if (!open || !anchor) return null

  const priorities: { value: PlannerPriority; label: string }[] = [
    { value: 'low', label: messages.form.priorityLow },
    { value: 'medium', label: messages.form.priorityMedium },
    { value: 'high', label: messages.form.priorityHigh },
  ]
  const range = formatPlannerDateRange(value.start, value.end, locale)
  const columnName =
    columns.find((column) => column.id === value.statusId)?.name ?? value.statusId

  return createPortal(
    <>
      <button
        type="button"
        className="kit-planner-quick-create__backdrop"
        aria-label={messages.form.cancel}
        onClick={onCancel}
      />
      <div
        ref={panelRef}
        className={`kit-planner-quick-create kit-planner-quick-create--${position.placement}`}
        style={{ top: position.top, left: position.left }}
        role="dialog"
        aria-modal="true"
        aria-label={messages.form.createTitle}
        onClick={(event) => event.stopPropagation()}
      >
        <span className="kit-planner-quick-create__arrow" aria-hidden />
        <div className="kit-planner-quick-create__card">
          <div className="kit-planner-quick-create__header">
            <strong>{messages.form.createTitle}</strong>
            <Button
              type="button"
              unstyled
              className="kit-planner-quick-create__close"
              aria-label={messages.form.cancel}
              onClick={onCancel}
            >
              <X className="size-4" />
            </Button>
          </div>

          <input
            ref={titleRef}
            value={value.title}
            placeholder={messages.form.titlePlaceholder}
            className={`${fieldInputClass} kit-planner-quick-create__title${titleError ? ' kit-planner-quick-create__title--invalid' : ''}`}
            onChange={(event) => onChange({ ...value, title: event.target.value })}
            onKeyDown={(event) => {
              if (event.key === 'Enter') {
                event.preventDefault()
                onSave()
              }
            }}
          />
          {titleError ? (
            <p className="kit-planner-quick-create__error">{titleError}</p>
          ) : null}

          <div className="kit-planner-quick-create__meta">
            <span className="kit-planner-quick-create__meta-item">
              <CalendarDays className="size-3.5" aria-hidden />
              {range || messages.form.scheduleSection}
            </span>
            <span className="kit-planner-quick-create__meta-item">{columnName}</span>
          </div>

          <div className="kit-planner-quick-create__priorities">
            {priorities.map((priority) => {
              const active = (value.priority ?? 'medium') === priority.value
              return (
                <button
                  key={priority.value}
                  type="button"
                  className={[
                    'kit-planner-quick-create__priority',
                    `kit-planner-quick-create__priority--${priority.value}`,
                    active ? 'is-active' : '',
                  ]
                    .filter(Boolean)
                    .join(' ')}
                  onClick={() => onChange({ ...value, priority: priority.value })}
                >
                  <Flag className="size-3" aria-hidden />
                  {priority.label}
                </button>
              )
            })}
          </div>

          <label className="kit-planner-quick-create__field">
            <span>{messages.form.column}</span>
            <select
              className={fieldInputClass}
              value={value.statusId}
              onChange={(event) =>
                onChange({ ...value, statusId: event.target.value })
              }
            >
              {columns.map((column) => (
                <option key={column.id} value={column.id}>
                  {column.name}
                </option>
              ))}
            </select>
          </label>

          <label className="kit-planner-quick-create__field">
            <span>{messages.form.assignee}</span>
            <InputText
              value={value.assignee ?? ''}
              placeholder={messages.form.assigneePlaceholder}
              className={fieldInputClass}
              onChange={(event: { target: { value: string } }) =>
                onChange({ ...value, assignee: event.target.value })
              }
            />
          </label>

          <div className="kit-planner-quick-create__footer">
            {onOpenFull ? (
              <Button
                type="button"
                unstyled
                className={btnOutlinedClass}
                disabled={saving}
                onClick={onOpenFull}
              >
                {messages.page.moreDetails}
              </Button>
            ) : (
              <span />
            )}
            <div className="kit-planner-quick-create__actions">
              <Button
                type="button"
                unstyled
                className={btnOutlinedClass}
                disabled={saving}
                onClick={onCancel}
              >
                {messages.form.cancel}
              </Button>
              <Button
                type="button"
                unstyled
                className={btnPrimaryClass}
                disabled={saving}
                onClick={onSave}
              >
                {messages.form.save}
              </Button>
            </div>
          </div>
        </div>
      </div>
    </>,
    document.body,
  )
}
