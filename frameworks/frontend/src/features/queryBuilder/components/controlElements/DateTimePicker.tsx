import {
  memo,
  useEffect,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import { createPortal } from 'react-dom'
import { ChevronLeft, ChevronRight, Clock, X } from 'lucide-react'
import { InputText } from 'primereact/inputtext'
import { inputClass } from './styles'
import {
  formatDateTimeDisplay,
  localPartsFromDateTimeWire,
  normalizeDateTimeWire,
  parseIsoDate,
  toIsoDate,
  wireFromDateTimeParts,
} from './utils'
import { TimePickerColumns } from './TimePickerColumns'

const WEEKDAYS = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'] as const

const MONTH_LABELS = [
  'Tháng 1',
  'Tháng 2',
  'Tháng 3',
  'Tháng 4',
  'Tháng 5',
  'Tháng 6',
  'Tháng 7',
  'Tháng 8',
  'Tháng 9',
  'Tháng 10',
  'Tháng 11',
  'Tháng 12',
] as const

type DraftParts = {
  dateIso: string
  hour: number
  minute: number
  hasTime: boolean
}

function startOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1)
}

function buildMonthCells(viewMonth: Date): Array<{
  iso: string
  day: number
  inMonth: boolean
  isToday: boolean
}> {
  const year = viewMonth.getFullYear()
  const month = viewMonth.getMonth()
  const firstDow = new Date(year, month, 1).getDay()
  const daysInMonth = new Date(year, month + 1, 0).getDate()
  const daysInPrev = new Date(year, month, 0).getDate()
  const todayIso = toIsoDate(new Date())

  const cells: Array<{
    iso: string
    day: number
    inMonth: boolean
    isToday: boolean
  }> = []

  for (let i = 0; i < firstDow; i++) {
    const day = daysInPrev - firstDow + 1 + i
    const dt = new Date(year, month - 1, day)
    const iso = toIsoDate(dt)
    cells.push({ iso, day, inMonth: false, isToday: iso === todayIso })
  }
  for (let day = 1; day <= daysInMonth; day++) {
    const dt = new Date(year, month, day)
    const iso = toIsoDate(dt)
    cells.push({ iso, day, inMonth: true, isToday: iso === todayIso })
  }
  while (cells.length % 7 !== 0) {
    const day = cells.length - firstDow - daysInMonth + 1
    const dt = new Date(year, month + 1, day)
    const iso = toIsoDate(dt)
    cells.push({ iso, day, inMonth: false, isToday: iso === todayIso })
  }
  return cells
}

function partsFromWire(wire: string): DraftParts {
  return localPartsFromDateTimeWire(wire)
}

export const DateTimePicker = memo(function DateTimePicker({
  value,
  onChange,
  title,
  disabled,
  className,
  placeholder = 'Chọn ngày…',
}: {
  value: unknown
  onChange: (isoDateTime: string) => void
  title?: string
  disabled?: boolean
  className?: string
  placeholder?: string
}) {
  const propWire = normalizeDateTimeWire(value)
  const [draftParts, setDraftParts] = useState<DraftParts>(() =>
    partsFromWire(propWire),
  )
  const [open, setOpen] = useState(false)
  const [viewMonth, setViewMonth] = useState(() =>
    startOfMonth(parseIsoDate(partsFromWire(propWire).dateIso) ?? new Date()),
  )
  const [panelPos, setPanelPos] = useState<{ top: number; left: number } | null>(
    null,
  )
  const anchorRef = useRef<HTMLDivElement>(null)
  const panelRef = useRef<HTMLDivElement>(null)
  const onChangeRef = useRef(onChange)
  onChangeRef.current = onChange

  const draftWire = wireFromDateTimeParts(draftParts)

  useEffect(() => {
    if (!open) setDraftParts(partsFromWire(propWire))
  }, [propWire, open])

  useLayoutEffect(() => {
    if (!open) {
      setPanelPos(null)
      return
    }
    const update = () => {
      const el = anchorRef.current
      const panel = panelRef.current
      if (!el) return
      const rect = el.getBoundingClientRect()
      const width = 312
      const left = Math.min(
        Math.max(8, rect.left),
        window.innerWidth - width - 8,
      )
      const panelHeight = panel?.offsetHeight ?? 360
      const gap = 6
      const spaceBelow = window.innerHeight - rect.bottom - gap - 8
      const spaceAbove = rect.top - gap - 8
      const openUp = spaceBelow < panelHeight && spaceAbove > spaceBelow
      const top = openUp
        ? Math.max(8, rect.top - panelHeight - gap)
        : rect.bottom + gap
      setPanelPos({ top, left })
    }
    update()
    const raf = requestAnimationFrame(update)
    window.addEventListener('resize', update)
    window.addEventListener('scroll', update, true)
    return () => {
      cancelAnimationFrame(raf)
      window.removeEventListener('resize', update)
      window.removeEventListener('scroll', update, true)
    }
  }, [open, draftParts, viewMonth])

  useEffect(() => {
    if (!open) return
    const onPointerDown = (e: MouseEvent) => {
      const t = e.target as Node
      if (anchorRef.current?.contains(t)) return
      if (panelRef.current?.contains(t)) return
      setDraftParts(partsFromWire(propWire))
      setOpen(false)
    }
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setDraftParts(partsFromWire(propWire))
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', onPointerDown)
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('mousedown', onPointerDown)
      document.removeEventListener('keydown', onKey)
    }
  }, [open, propWire])

  const dirty = draftWire !== propWire
  const displayWire = open ? draftWire : propWire
  const display = formatDateTimeDisplay(displayWire)
  const cells = useMemo(() => buildMonthCells(viewMonth), [viewMonth])

  const openPicker = () => {
    if (disabled || open) return
    const parts = partsFromWire(propWire)
    setDraftParts(parts)
    setViewMonth(startOfMonth(parseIsoDate(parts.dateIso) ?? new Date()))
    setOpen(true)
  }

  const handleCancel = () => {
    setDraftParts(partsFromWire(propWire))
    setOpen(false)
  }

  const handleConfirm = () => {
    const next = draftWire
    setOpen(false)
    if (next !== propWire) onChangeRef.current(next)
  }

  const panel =
    open && panelPos
      ? createPortal(
          <div
            ref={panelRef}
            role="dialog"
            aria-label={title ?? 'Chọn ngày giờ'}
            className="fixed z-[500] flex max-h-[min(24rem,calc(100vh-1rem))] w-[21.5rem] flex-col overflow-hidden rounded-2xl border border-slate-200/90 bg-white text-slate-900 shadow-[0_16px_48px_-12px_rgba(15,23,42,0.28)]"
            style={{ top: panelPos.top, left: panelPos.left }}
          >
            <div className="min-h-0 flex-1 overflow-y-auto overscroll-contain px-3 pt-3">
              <div className="mb-2 flex items-center justify-between gap-2">
                <button
                  type="button"
                  className="inline-flex size-8 appearance-none items-center justify-center rounded-lg border-0 bg-transparent text-slate-500 outline-none transition hover:bg-slate-100 hover:text-teal-800"
                  aria-label="Tháng trước"
                  onClick={() =>
                    setViewMonth(
                      new Date(
                        viewMonth.getFullYear(),
                        viewMonth.getMonth() - 1,
                        1,
                      ),
                    )
                  }
                >
                  <ChevronLeft className="size-4" aria-hidden />
                </button>
                <p className="m-0 text-[13px] font-semibold tracking-tight text-slate-900">
                  {MONTH_LABELS[viewMonth.getMonth()]}{' '}
                  <span className="text-slate-500">{viewMonth.getFullYear()}</span>
                </p>
                <button
                  type="button"
                  className="inline-flex size-8 appearance-none items-center justify-center rounded-lg border-0 bg-transparent text-slate-500 outline-none transition hover:bg-slate-100 hover:text-teal-800"
                  aria-label="Tháng sau"
                  onClick={() =>
                    setViewMonth(
                      new Date(
                        viewMonth.getFullYear(),
                        viewMonth.getMonth() + 1,
                        1,
                      ),
                    )
                  }
                >
                  <ChevronRight className="size-4" aria-hidden />
                </button>
              </div>

              <div className="mb-1 grid grid-cols-7 gap-0.5 text-center">
                {WEEKDAYS.map((d) => (
                  <span
                    key={d}
                    className="flex h-6 items-center justify-center text-[10px] font-semibold uppercase tracking-wide text-slate-400"
                  >
                    {d}
                  </span>
                ))}
              </div>

              <div className="grid grid-cols-7 gap-0.5 text-center">
                {cells.map((cell) => {
                  const selected = cell.iso === draftParts.dateIso
                  return (
                    <button
                      key={cell.iso + String(cell.inMonth)}
                      type="button"
                      onClick={() => {
                        setDraftParts((prev) => ({
                          ...prev,
                          dateIso: cell.iso,
                          ...(!prev.hasTime ? { hour: 0, minute: 0 } : {}),
                        }))
                        if (!cell.inMonth) {
                          setViewMonth(startOfMonth(parseIsoDate(cell.iso)!))
                        }
                      }}
                      className={[
                        'inline-flex size-8 appearance-none items-center justify-center rounded-full border-0 text-xs font-medium outline-none transition',
                        selected
                          ? 'bg-teal-600 font-semibold text-white shadow-[0_4px_10px_-2px_rgba(13,148,136,0.4)]'
                          : cell.inMonth
                            ? 'bg-transparent text-slate-700 hover:bg-slate-100'
                            : 'bg-transparent text-slate-300 hover:bg-slate-50 hover:text-slate-400',
                        !selected && cell.isToday
                          ? 'font-semibold text-teal-700 ring-1 ring-inset ring-teal-600/80'
                          : '',
                      ]
                        .filter(Boolean)
                        .join(' ')}
                    >
                      {cell.day}
                    </button>
                  )
                })}
              </div>

              {draftParts.dateIso ? (
                <div className="border-t border-slate-100 pt-2.5">
                  {!draftParts.hasTime ? (
                    <button
                      type="button"
                      className="inline-flex h-8 w-full appearance-none items-center justify-center gap-1.5 rounded-lg border border-dashed border-slate-200 bg-slate-50/80 text-xs font-semibold text-slate-600 outline-none transition hover:border-teal-200 hover:bg-teal-50/60 hover:text-teal-800"
                      onClick={() =>
                        setDraftParts((prev) => ({
                          ...prev,
                          hasTime: true,
                          hour: prev.hour || 0,
                          minute: prev.minute || 0,
                        }))
                      }
                    >
                      <Clock className="size-3.5" aria-hidden />
                      Thêm giờ
                    </button>
                  ) : (
                    <>
                      <TimePickerColumns
                        active={open}
                        hour={draftParts.hour}
                        minute={draftParts.minute}
                        onChange={(hour, minute) =>
                          setDraftParts((prev) => ({
                            ...prev,
                            hour,
                            minute,
                            hasTime: true,
                          }))
                        }
                      />
                      <button
                        type="button"
                        className="inline-flex h-8 w-full appearance-none items-center justify-center gap-1.5 rounded-lg border border-dashed border-slate-200 bg-slate-50/80 text-xs font-semibold text-slate-600 outline-none transition hover:border-teal-200 hover:bg-teal-50/60 hover:text-teal-800"
                        onClick={() =>
                          setDraftParts((prev) => ({
                            ...prev,
                            hasTime: false,
                            hour: 0,
                            minute: 0,
                          }))
                        }
                      >
                        Bỏ giờ
                      </button>
                    </>
                  )}
                </div>
              ) : null}
            </div>

            <div className="flex shrink-0 items-center justify-end gap-2 border-t border-slate-100 bg-white px-3 py-2">
              <button
                type="button"
                className="inline-flex h-8 appearance-none items-center justify-center rounded-lg border border-solid border-slate-200 bg-white px-3.5 text-xs font-semibold text-slate-600 outline-none transition hover:border-slate-300 hover:bg-white hover:text-slate-800"
                onClick={handleCancel}
              >
                Cancel
              </button>
              <button
                type="button"
                className="inline-flex h-8 appearance-none items-center justify-center rounded-lg border-0 bg-teal-600 px-3.5 text-xs font-semibold text-white outline-none shadow-sm transition hover:bg-teal-700 disabled:cursor-not-allowed disabled:opacity-45"
                disabled={!dirty || !draftParts.dateIso}
                onClick={handleConfirm}
              >
                Confirm
              </button>
            </div>
          </div>,
          document.body,
        )
      : null

  return (
    <div
      ref={anchorRef}
      className={['relative inline-block min-w-[17rem] w-[17rem]', className]
        .filter(Boolean)
        .join(' ')}
    >
      <div className="relative">
        <InputText
          unstyled
          readOnly
          disabled={disabled}
          title={title}
          placeholder={placeholder}
          className={[inputClass, display ? 'pr-9' : ''].filter(Boolean).join(' ')}
          value={display}
          onClick={openPicker}
        />
        {display && !disabled ? (
          <button
            type="button"
            title="Xóa ngày"
            aria-label="Xóa ngày"
            className="absolute right-2 top-1/2 inline-flex size-6 -translate-y-1/2 appearance-none items-center justify-center rounded-md border-0 bg-transparent text-slate-400 outline-none transition hover:bg-slate-100 hover:text-slate-600"
            onClick={(e) => {
              e.stopPropagation()
              setDraftParts({
                dateIso: '',
                hour: 0,
                minute: 0,
                hasTime: false,
              })
              setOpen(false)
              if (propWire !== '') onChangeRef.current('')
            }}
          >
            <X className="size-3.5" aria-hidden />
          </button>
        ) : null}
      </div>
      {panel}
    </div>
  )
})
