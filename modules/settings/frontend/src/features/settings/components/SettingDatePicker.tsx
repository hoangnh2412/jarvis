import { useEffect, useRef } from 'react'
import flatpickr from 'flatpickr'
import 'flatpickr/dist/flatpickr.min.css'
import {
  parseStoredDate,
  resolveHourFormat,
  serializeDate,
  serializeDateTime,
  shouldShowSeconds,
  toFlatpickrFormat,
} from '../utils/dateFormat'

const inputClass =
  'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70'
const inputInvalidClass =
  '!border-red-500 focus:!border-red-500 focus:!ring-red-500/20'

type SettingDatePickerProps = {
  value: string
  disabled?: boolean
  invalid?: boolean
  showTime?: boolean
  /** App format from Localization (DateFormat or DateFormat + TimeFormat). */
  displayFormat: string
  onChange: (value: string) => void
}

export function SettingDatePicker({
  value,
  disabled = false,
  invalid = false,
  showTime = false,
  displayFormat,
  onChange,
}: SettingDatePickerProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const pickerRef = useRef<flatpickr.Instance | null>(null)
  const onChangeRef = useRef(onChange)
  onChangeRef.current = onChange

  const flatpickrFormat = toFlatpickrFormat(displayFormat, showTime)
  const withSeconds = shouldShowSeconds(displayFormat)
  const hour12 = resolveHourFormat(displayFormat) === '12'
  const placeholder = displayFormat || (showTime ? 'Chọn ngày giờ' : 'Chọn ngày')

  useEffect(() => {
    if (!inputRef.current) return

    pickerRef.current?.destroy()
    pickerRef.current = flatpickr(inputRef.current, {
      allowInput: false,
      clickOpens: !disabled,
      disableMobile: true,
      enableTime: showTime,
      enableSeconds: showTime && withSeconds,
      time_24hr: showTime ? !hour12 : true,
      dateFormat: flatpickrFormat,
      defaultDate: parseStoredDate(value) ?? undefined,
      onChange: (selectedDates: Date[]) => {
        const next = selectedDates[0]
        if (!next) {
          onChangeRef.current('')
          return
        }
        onChangeRef.current(
          showTime
            ? serializeDateTime(next, withSeconds)
            : serializeDate(next),
        )
      },
    })

    return () => {
      pickerRef.current?.destroy()
      pickerRef.current = null
    }
    // Re-create when format/mode changes; value sync is handled below.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [disabled, showTime, flatpickrFormat, withSeconds, hour12])

  useEffect(() => {
    const picker = pickerRef.current
    if (!picker) return
    const next = parseStoredDate(value)
    const current = picker.selectedDates[0]
    if (!next) {
      if (current) picker.clear()
      return
    }
    if (!current || current.getTime() !== next.getTime()) {
      picker.setDate(next, false)
    }
  }, [value])

  useEffect(() => {
    const picker = pickerRef.current
    if (!picker) return
    if (disabled) picker.close()
    picker.set('clickOpens', !disabled)
  }, [disabled])

  return (
    <input
      ref={inputRef}
      type="text"
      readOnly
      disabled={disabled}
      placeholder={placeholder}
      className={[inputClass, invalid ? inputInvalidClass : '']
        .filter(Boolean)
        .join(' ')}
    />
  )
}
