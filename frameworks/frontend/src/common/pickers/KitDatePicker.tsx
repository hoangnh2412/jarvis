import {
  memo,
  useEffect,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import { createPortal } from 'react-dom'
import { CalendarDays, ChevronLeft, ChevronRight, X } from 'lucide-react'
import { InputText } from 'primereact/inputtext'
import {
  pickerFooterLinkClass,
  pickerFooterPrimaryClass,
  pickerInputClass,
  pickerNavBtnClass,
  pickerPanelClass,
} from './pickerStyles'
import {
  PICKER_MONTH_LABELS,
  PICKER_WEEKDAYS,
  buildMonthCells,
  formatDateDisplay,
  parseIsoDate,
  startOfMonth,
  toDateWire,
  toIsoDate,
} from './pickerUtils'

export type KitDatePickerProps = {
  value: unknown
  onChange: (isoDate: string) => void
  title?: string
  disabled?: boolean
  className?: string
  inputClassName?: string
  id?: string
  placeholder?: string
  'aria-label'?: string
  hideClearButton?: boolean
}

export const KitDatePicker = memo(function KitDatePicker({
  value,
  onChange,
  title,
  disabled,
  className,
  inputClassName,
  id,
  placeholder = 'Chọn ngày…',
  'aria-label': ariaLabel,
  hideClearButton = false,
}: KitDatePickerProps) {
  const propWire = toDateWire(value)
  const [draftWire, setDraftWire] = useState(propWire)
  const [open, setOpen] = useState(false)
  const [viewMonth, setViewMonth] = useState(() =>
    startOfMonth(parseIsoDate(propWire) ?? new Date()),
  )
  const [panelPos, setPanelPos] = useState<{ top: number; left: number } | null>(
    null,
  )
  const anchorRef = useRef<HTMLDivElement>(null)
  const panelRef = useRef<HTMLDivElement>(null)
  const onChangeRef = useRef(onChange)
  onChangeRef.current = onChange

  useEffect(() => {
    if (!open) setDraftWire(propWire)
  }, [propWire, open])

  useLayoutEffect(() => {
    if (!open) {
      setPanelPos(null)
      return
    }
    const update = () => {
      const el = anchorRef.current
      if (!el) return
      const rect = el.getBoundingClientRect()
      const width = 312
      const left = Math.min(Math.max(8, rect.left), window.innerWidth - width - 8)
      setPanelPos({ top: rect.bottom + 6, left })
    }
    update()
    window.addEventListener('resize', update)
    window.addEventListener('scroll', update, true)
    return () => {
      window.removeEventListener('resize', update)
      window.removeEventListener('scroll', update, true)
    }
  }, [open])

  useEffect(() => {
    if (!open) return
    const onPointerDown = (e: MouseEvent) => {
      const t = e.target as Node
      if (anchorRef.current?.contains(t)) return
      if (panelRef.current?.contains(t)) return
      setDraftWire(propWire)
      setOpen(false)
    }
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setDraftWire(propWire)
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
  const display = formatDateDisplay(open ? draftWire : propWire)
  const cells = useMemo(() => buildMonthCells(viewMonth), [viewMonth])

  const openPicker = () => {
    if (disabled || open) return
    setDraftWire(propWire)
    setViewMonth(startOfMonth(parseIsoDate(propWire) ?? new Date()))
    setOpen(true)
  }

  const handleConfirm = () => {
    const next = draftWire
    setOpen(false)
    if (next !== propWire) onChangeRef.current(next)
  }

  const handleToday = () => {
    const today = toIsoDate(new Date())
    setDraftWire(today)
    setViewMonth(startOfMonth(new Date()))
  }

  const handleClear = () => {
    setDraftWire('')
    setOpen(false)
    if (propWire !== '') onChangeRef.current('')
  }

  const panel =
    open && panelPos
      ? createPortal(
          <div
            ref={panelRef}
            role="dialog"
            aria-label={title ?? 'Chọn ngày'}
            className={[pickerPanelClass, 'w-[19.5rem]'].join(' ')}
            style={{ top: panelPos.top, left: panelPos.left }}
          >
            <div className="px-3.5 pt-3.5">
              <div className="mb-3 flex items-center justify-between gap-2">
                <button
                  type="button"
                  className={pickerNavBtnClass}
                  aria-label="Tháng trước"
                  onClick={() =>
                    setViewMonth(
                      new Date(viewMonth.getFullYear(), viewMonth.getMonth() - 1, 1),
                    )
                  }
                >
                  <ChevronLeft className="size-4" aria-hidden />
                </button>
                <p className="m-0 text-[13px] font-semibold tracking-tight text-slate-900">
                  {PICKER_MONTH_LABELS[viewMonth.getMonth()]}{' '}
                  <span className="text-slate-500">{viewMonth.getFullYear()}</span>
                </p>
                <button
                  type="button"
                  className={pickerNavBtnClass}
                  aria-label="Tháng sau"
                  onClick={() =>
                    setViewMonth(
                      new Date(viewMonth.getFullYear(), viewMonth.getMonth() + 1, 1),
                    )
                  }
                >
                  <ChevronRight className="size-4" aria-hidden />
                </button>
              </div>

              <div className="mb-1.5 grid grid-cols-7 gap-1 text-center">
                {PICKER_WEEKDAYS.map((d) => (
                  <span
                    key={d}
                    className="flex h-7 items-center justify-center text-[11px] font-semibold uppercase tracking-wide text-slate-400"
                  >
                    {d}
                  </span>
                ))}
              </div>

              <div className="grid grid-cols-7 gap-1 pb-1 text-center">
                {cells.map((cell) => {
                  const selected = cell.iso === draftWire
                  return (
                    <button
                      key={cell.iso + String(cell.inMonth)}
                      type="button"
                      onClick={() => {
                        setDraftWire(cell.iso)
                        if (!cell.inMonth) {
                          setViewMonth(startOfMonth(parseIsoDate(cell.iso)!))
                        }
                      }}
                      className={[
                        'inline-flex size-9 appearance-none items-center justify-center rounded-full border-0 text-[13px] font-medium outline-none transition',
                        selected
                          ? 'bg-[#2563EB] font-semibold text-white shadow-[0_4px_10px_-2px_rgba(37,99,235,0.35)]'
                          : cell.inMonth
                            ? 'bg-transparent text-slate-700 hover:bg-slate-100'
                            : 'bg-transparent text-slate-300 hover:bg-slate-50 hover:text-slate-400',
                        !selected && cell.isToday
                          ? 'font-semibold text-[#2563EB] ring-1 ring-inset ring-[#2563EB]/50'
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
            </div>

            <div className="mt-2 flex items-center justify-between gap-2 border-t border-slate-100 bg-slate-50/60 px-3.5 py-2.5">
              <div className="flex items-center gap-1">
                <button type="button" className={pickerFooterLinkClass} onClick={handleClear}>
                  Xóa
                </button>
                <button type="button" className={pickerFooterLinkClass} onClick={handleToday}>
                  Hôm nay
                </button>
              </div>
              <button
                type="button"
                className={pickerFooterPrimaryClass}
                disabled={!dirty}
                onClick={handleConfirm}
              >
                Xác nhận
              </button>
            </div>
          </div>,
          document.body,
        )
      : null

  const resolvedInputClass = [inputClassName ?? pickerInputClass, 'pr-9']
    .filter(Boolean)
    .join(' ')
  const showInlineClear = !hideClearButton && display && !disabled

  return (
    <div
      ref={anchorRef}
      className={['relative block w-full min-w-0', className].filter(Boolean).join(' ')}
    >
      <div className="relative">
        <InputText
          unstyled
          readOnly
          id={id}
          disabled={disabled}
          title={title}
          placeholder={placeholder}
          aria-label={ariaLabel}
          className={resolvedInputClass}
          value={display}
          onClick={openPicker}
        />
        {showInlineClear ? (
          <button
            type="button"
            title="Xóa ngày"
            aria-label="Xóa ngày"
            className="absolute right-2 top-1/2 inline-flex size-6 -translate-y-1/2 appearance-none items-center justify-center rounded-md border-0 bg-transparent text-slate-400 outline-none transition hover:bg-slate-100 hover:text-slate-600"
            onClick={(e) => {
              e.stopPropagation()
              handleClear()
            }}
          >
            <X className="size-3.5" aria-hidden />
          </button>
        ) : (
          <CalendarDays
            className="pointer-events-none absolute right-2.5 top-1/2 size-4 -translate-y-1/2 text-slate-400"
            aria-hidden
          />
        )}
      </div>
      {panel}
    </div>
  )
})
