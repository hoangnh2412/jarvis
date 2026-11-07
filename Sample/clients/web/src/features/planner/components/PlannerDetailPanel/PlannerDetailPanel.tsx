import { useEffect, useId, useRef, useState } from 'react'
import { Link2, X } from 'lucide-react'
import { Button } from 'primereact/button'
import { getPlannerMessages, type PlannerLocale } from '../../localization'
import type { PlannerColumn } from '../../types'
import { btnOutlinedClass, btnPrimaryClass } from '../fieldStyles'
import {
  PlannerItemForm,
  type PlannerItemFormState,
} from '../PlannerItemForm'

const PANEL_EXIT_MS = 280
const PANEL_WIDTH_KEY = 'kit-planner-detail-panel-width'
const DEFAULT_PANEL_WIDTH = 440
const MIN_PANEL_WIDTH = 280
const MAX_PANEL_RATIO = 0.92

function clampPanelWidth(width: number, viewportWidth = window.innerWidth) {
  const max = Math.max(MIN_PANEL_WIDTH, Math.floor(viewportWidth * MAX_PANEL_RATIO))
  return Math.min(max, Math.max(MIN_PANEL_WIDTH, Math.round(width)))
}

function readStoredPanelWidth() {
  if (typeof window === 'undefined') return DEFAULT_PANEL_WIDTH
  const raw = window.localStorage.getItem(PANEL_WIDTH_KEY)
  const parsed = raw ? Number(raw) : NaN
  if (!Number.isFinite(parsed)) return DEFAULT_PANEL_WIDTH
  return clampPanelWidth(parsed)
}

export type PlannerDetailPanelProps = {
  open: boolean
  mode: 'create' | 'edit'
  value: PlannerItemFormState
  columns: PlannerColumn[]
  locale?: PlannerLocale
  titleError?: string | null
  saving?: boolean
  onChange: (value: PlannerItemFormState) => void
  onSave: () => void
  onClose: () => void
  onDelete?: () => void
}

export function PlannerDetailPanel({
  open,
  mode,
  value,
  columns,
  locale = 'vi',
  titleError,
  saving = false,
  onChange,
  onSave,
  onClose,
  onDelete,
}: PlannerDetailPanelProps) {
  const messages = getPlannerMessages(locale)
  const titleId = useId()
  const [mounted, setMounted] = useState(open)
  const [entered, setEntered] = useState(false)
  const [width, setWidth] = useState(DEFAULT_PANEL_WIDTH)
  const [resizing, setResizing] = useState(false)
  const dragRef = useRef<{ startX: number; startWidth: number } | null>(null)

  useEffect(() => {
    setWidth(readStoredPanelWidth())
  }, [])

  useEffect(() => {
    if (open) {
      setMounted(true)
      const frame = window.requestAnimationFrame(() => {
        window.requestAnimationFrame(() => setEntered(true))
      })
      return () => window.cancelAnimationFrame(frame)
    }

    setEntered(false)
    const timer = window.setTimeout(() => setMounted(false), PANEL_EXIT_MS)
    return () => window.clearTimeout(timer)
  }, [open])

  useEffect(() => {
    if (!open) return
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && !saving && !resizing) onClose()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [open, saving, resizing, onClose])

  useEffect(() => {
    const onResize = () => {
      setWidth((current) => clampPanelWidth(current))
    }
    window.addEventListener('resize', onResize)
    return () => window.removeEventListener('resize', onResize)
  }, [])

  useEffect(() => {
    if (!resizing) return

    const onPointerMove = (event: PointerEvent) => {
      const drag = dragRef.current
      if (!drag) return
      const next = clampPanelWidth(drag.startWidth + (drag.startX - event.clientX))
      setWidth(next)
    }

    const onPointerUp = () => {
      const current = dragRef.current
      dragRef.current = null
      setResizing(false)
      document.body.classList.remove('kit-planner-detail-resizing')
      if (!current) return
      setWidth((value) => {
        const next = clampPanelWidth(value)
        window.localStorage.setItem(PANEL_WIDTH_KEY, String(next))
        return next
      })
    }

    window.addEventListener('pointermove', onPointerMove)
    window.addEventListener('pointerup', onPointerUp)
    window.addEventListener('pointercancel', onPointerUp)
    return () => {
      window.removeEventListener('pointermove', onPointerMove)
      window.removeEventListener('pointerup', onPointerUp)
      window.removeEventListener('pointercancel', onPointerUp)
      document.body.classList.remove('kit-planner-detail-resizing')
    }
  }, [resizing])

  if (!mounted) return null

  return (
    <div
      className={[
        'kit-planner-detail-layer',
        entered ? 'is-open' : '',
        resizing ? 'is-resizing' : '',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <button
        type="button"
        className="kit-planner-detail-backdrop"
        aria-label={messages.form.cancel}
        onClick={() => {
          if (!saving && !resizing) onClose()
        }}
      />
      <aside
        className="kit-planner-detail-panel"
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        style={{ width: `min(${width}px, 100%)` }}
      >
        <div
          className="kit-planner-detail-panel__resizer"
          role="separator"
          aria-orientation="vertical"
          aria-label={messages.form.resizePanel}
          aria-valuemin={MIN_PANEL_WIDTH}
          aria-valuemax={
            typeof window === 'undefined'
              ? 1200
              : Math.floor(window.innerWidth * MAX_PANEL_RATIO)
          }
          aria-valuenow={width}
          tabIndex={0}
          onPointerDown={(event) => {
            if (event.button !== 0) return
            event.preventDefault()
            event.stopPropagation()
            dragRef.current = {
              startX: event.clientX,
              startWidth: width,
            }
            setResizing(true)
            document.body.classList.add('kit-planner-detail-resizing')
            ;(event.currentTarget as HTMLElement).setPointerCapture?.(
              event.pointerId,
            )
          }}
          onKeyDown={(event) => {
            const step = event.shiftKey ? 48 : 24
            if (event.key === 'ArrowLeft') {
              event.preventDefault()
              setWidth((current) => {
                const next = clampPanelWidth(current + step)
                window.localStorage.setItem(PANEL_WIDTH_KEY, String(next))
                return next
              })
            }
            if (event.key === 'ArrowRight') {
              event.preventDefault()
              setWidth((current) => {
                const next = clampPanelWidth(current - step)
                window.localStorage.setItem(PANEL_WIDTH_KEY, String(next))
                return next
              })
            }
            if (event.key === 'Home') {
              event.preventDefault()
              const next = clampPanelWidth(MIN_PANEL_WIDTH)
              window.localStorage.setItem(PANEL_WIDTH_KEY, String(next))
              setWidth(next)
            }
            if (event.key === 'End') {
              event.preventDefault()
              const next = clampPanelWidth(window.innerWidth * MAX_PANEL_RATIO)
              window.localStorage.setItem(PANEL_WIDTH_KEY, String(next))
              setWidth(next)
            }
          }}
        />
        <div className="kit-planner-detail-panel__header">
          <div className="kit-planner-detail-panel__meta">
            <span className="kit-planner-detail-panel__type">
              <Link2 className="size-3.5" aria-hidden />
              {messages.form.taskType}
            </span>
            <span id={titleId} className="kit-planner-detail-panel__mode">
              {mode === 'edit'
                ? messages.form.editTitle
                : messages.form.createTitle}
            </span>
          </div>
          <Button
            type="button"
            unstyled
            className="kit-planner-detail-panel__close"
            aria-label={messages.form.cancel}
            disabled={saving}
            onClick={onClose}
          >
            <X className="size-4" />
          </Button>
        </div>

        <div className="kit-planner-detail-panel__body">
          <PlannerItemForm
            value={value}
            columns={columns}
            locale={locale}
            titleError={titleError}
            onChange={onChange}
          />
        </div>

        <div className="kit-planner-detail-panel__footer">
          {mode === 'edit' && onDelete ? (
            <Button
              type="button"
              unstyled
              className="kit-planner-detail-panel__delete"
              disabled={saving}
              onClick={onDelete}
            >
              {messages.form.delete}
            </Button>
          ) : (
            <span />
          )}
          <div className="kit-planner-detail-panel__actions">
            <Button
              type="button"
              unstyled
              className={btnOutlinedClass}
              disabled={saving}
              onClick={onClose}
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
      </aside>
    </div>
  )
}
