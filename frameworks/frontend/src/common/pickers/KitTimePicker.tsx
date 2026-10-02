import {
  memo,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from 'react'
import { createPortal } from 'react-dom'
import { Clock, X } from 'lucide-react'
import { InputText } from 'primereact/inputtext'
import {
  pickerFooterLinkClass,
  pickerFooterPrimaryClass,
  pickerInputClass,
  pickerPanelClass,
} from './pickerStyles'
import { TimePickerColumns } from './TimePickerColumns'
import {
  formatTimeDisplay,
  localPartsFromTimeWire,
  wireFromTimeParts,
} from './pickerUtils'

export type KitTimePickerProps = {
  value: unknown
  onChange: (timeWire: string) => void
  title?: string
  disabled?: boolean
  className?: string
  inputClassName?: string
  id?: string
  placeholder?: string
  'aria-label'?: string
  hideClearButton?: boolean
}

export const KitTimePicker = memo(function KitTimePicker({
  value,
  onChange,
  title,
  disabled,
  className,
  inputClassName,
  id,
  placeholder = 'Chọn giờ…',
  'aria-label': ariaLabel,
  hideClearButton = false,
}: KitTimePickerProps) {
  const raw = String(value ?? '').trim()
  const hasValue = Boolean(raw)
  const committedParts = hasValue
    ? localPartsFromTimeWire(raw)
    : { hour: 0, minute: 0 }
  const propWire = hasValue
    ? wireFromTimeParts(committedParts.hour, committedParts.minute)
    : ''
  const [draftHour, setDraftHour] = useState(() => committedParts.hour)
  const [draftMinute, setDraftMinute] = useState(() => committedParts.minute)
  const [open, setOpen] = useState(false)
  const [panelPos, setPanelPos] = useState<{ top: number; left: number } | null>(
    null,
  )
  const anchorRef = useRef<HTMLDivElement>(null)
  const panelRef = useRef<HTMLDivElement>(null)
  const onChangeRef = useRef(onChange)
  onChangeRef.current = onChange

  const draftWire = wireFromTimeParts(draftHour, draftMinute)

  useEffect(() => {
    if (!open) {
      const parts = hasValue
        ? localPartsFromTimeWire(raw)
        : { hour: 0, minute: 0 }
      setDraftHour(parts.hour)
      setDraftMinute(parts.minute)
    }
  }, [value, open, hasValue, raw])

  useLayoutEffect(() => {
    if (!open) {
      setPanelPos(null)
      return
    }
    const update = () => {
      const el = anchorRef.current
      if (!el) return
      const rect = el.getBoundingClientRect()
      const width = 220
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
      setOpen(false)
    }
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false)
    }
    document.addEventListener('mousedown', onPointerDown)
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('mousedown', onPointerDown)
      document.removeEventListener('keydown', onKey)
    }
  }, [open])

  const display = hasValue
    ? formatTimeDisplay(committedParts.hour, committedParts.minute)
    : open
      ? formatTimeDisplay(draftHour, draftMinute)
      : ''

  const dirty = draftWire !== propWire

  const openPicker = () => {
    if (disabled || open) return
    setDraftHour(committedParts.hour)
    setDraftMinute(committedParts.minute)
    setOpen(true)
  }

  const handleConfirm = () => {
    setOpen(false)
    if (draftWire !== propWire) onChangeRef.current(draftWire)
  }

  const handleClear = () => {
    setOpen(false)
    if (hasValue) onChangeRef.current('')
  }

  const panel =
    open && panelPos
      ? createPortal(
          <div
            ref={panelRef}
            role="dialog"
            aria-label={title ?? 'Chọn giờ'}
            className={[pickerPanelClass, 'w-[13.75rem]'].join(' ')}
            style={{ top: panelPos.top, left: panelPos.left }}
          >
            <TimePickerColumns
              active={open}
              hour={draftHour}
              minute={draftMinute}
              onChange={(hour, minute) => {
                setDraftHour(hour)
                setDraftMinute(minute)
              }}
            />
            <div className="flex items-center justify-between gap-2 border-t border-slate-100 bg-slate-50/60 px-3 py-2.5">
              <button type="button" className={pickerFooterLinkClass} onClick={handleClear}>
                Xóa
              </button>
              <button
                type="button"
                className={pickerFooterPrimaryClass}
                disabled={!dirty && !hasValue}
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
  const showInlineClear = !hideClearButton && hasValue && !disabled

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
            title="Xóa giờ"
            aria-label="Xóa giờ"
            className="absolute right-2 top-1/2 inline-flex size-6 -translate-y-1/2 appearance-none items-center justify-center rounded-md border-0 bg-transparent text-slate-400 outline-none transition hover:bg-slate-100 hover:text-slate-600"
            onClick={(e) => {
              e.stopPropagation()
              handleClear()
            }}
          >
            <X className="size-3.5" aria-hidden />
          </button>
        ) : (
          <Clock
            className="pointer-events-none absolute right-2.5 top-1/2 size-4 -translate-y-1/2 text-slate-400"
            aria-hidden
          />
        )}
      </div>
      {panel}
    </div>
  )
})
