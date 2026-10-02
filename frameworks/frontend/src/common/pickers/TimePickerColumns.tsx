import { memo, useCallback, useEffect, useRef } from 'react'

const ITEM_H = 32
const VISIBLE = 5
const PICKER_H = ITEM_H * VISIBLE

const HOURS = Array.from({ length: 24 }, (_, i) => i)
const MINUTES = Array.from({ length: 60 }, (_, i) => i)

function pad2(n: number): string {
  return String(n).padStart(2, '0')
}

function TimeScrollColumn({
  values,
  selected,
  onSelect,
  active,
}: {
  values: number[]
  selected: number
  onSelect: (v: number) => void
  active: boolean
}) {
  const listRef = useRef<HTMLDivElement>(null)
  const scrollEndRef = useRef<number | null>(null)

  const scrollToValue = useCallback(
    (value: number, smooth = false) => {
      const el = listRef.current
      if (!el) return
      const idx = values.indexOf(value)
      if (idx < 0) return
      el.scrollTo({ top: idx * ITEM_H, behavior: smooth ? 'smooth' : 'instant' })
    },
    [values],
  )

  useEffect(() => {
    if (!active) return
    scrollToValue(selected)
  }, [active, selected, scrollToValue])

  const handleScroll = () => {
    const el = listRef.current
    if (!el) return
    if (scrollEndRef.current != null) window.clearTimeout(scrollEndRef.current)
    scrollEndRef.current = window.setTimeout(() => {
      const idx = Math.round(el.scrollTop / ITEM_H)
      const clamped = Math.min(Math.max(idx, 0), values.length - 1)
      const next = values[clamped]!
      scrollToValue(next)
      if (next !== selected) onSelect(next)
    }, 80)
  }

  return (
    <div
      className="relative w-12 overflow-hidden rounded-xl border border-[#E2E8F0] bg-[#F8FAFC]"
      style={{ height: PICKER_H }}
    >
      <div
        aria-hidden
        className="pointer-events-none absolute inset-x-1 top-1/2 z-[1] h-8 -translate-y-1/2 rounded-lg bg-[#EFF6FF] ring-1 ring-[#2563EB]/20"
      />
      <div
        aria-hidden
        className="pointer-events-none absolute inset-x-0 top-0 z-[3] h-8 bg-gradient-to-b from-[#F8FAFC] to-transparent"
      />
      <div
        aria-hidden
        className="pointer-events-none absolute inset-x-0 bottom-0 z-[3] h-8 bg-gradient-to-t from-[#F8FAFC] to-transparent"
      />
      <div
        ref={listRef}
        className="h-full overflow-y-auto scroll-smooth [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
        style={{
          scrollSnapType: 'y mandatory',
          paddingTop: ITEM_H * 2,
          paddingBottom: ITEM_H * 2,
        }}
        onScroll={handleScroll}
      >
        {values.map((v) => {
          const isSelected = v === selected
          return (
            <button
              key={v}
              type="button"
              style={{ height: ITEM_H, scrollSnapAlign: 'center' }}
              className={[
                'relative z-[2] flex w-full appearance-none items-center justify-center border-0 bg-transparent text-sm tabular-nums outline-none transition-colors',
                isSelected
                  ? 'font-semibold text-[#1D4ED8]'
                  : 'font-medium text-slate-600',
              ].join(' ')}
              onClick={() => {
                onSelect(v)
                scrollToValue(v, true)
              }}
            >
              {pad2(v)}
            </button>
          )
        })}
      </div>
    </div>
  )
}

export type TimePickerColumnsProps = {
  hour: number
  minute: number
  onChange: (hour: number, minute: number) => void
  active?: boolean
}

export const TimePickerColumns = memo(function TimePickerColumns({
  hour,
  minute,
  onChange,
  active = true,
}: TimePickerColumnsProps) {
  const setHour = (h: number) => onChange(h, minute)
  const setMinute = (m: number) => onChange(hour, m)

  return (
    <div className="px-1 pt-2 pb-1">
      <div className="grid grid-cols-[auto_auto_auto] justify-center gap-x-3">
        <span className="col-start-1 row-start-1 justify-self-center pb-1 text-[10px] font-semibold uppercase tracking-wider text-slate-500">
          Giờ
        </span>
        <span className="col-start-3 row-start-1 justify-self-center pb-1 text-[10px] font-semibold uppercase tracking-wider text-slate-500">
          Phút
        </span>
        <div className="col-start-1 row-start-2">
          <TimeScrollColumn
            values={HOURS}
            selected={hour}
            onSelect={setHour}
            active={active}
          />
        </div>
        <span
          aria-hidden
          className="col-start-2 row-start-2 self-center text-lg font-semibold text-slate-400"
        >
          :
        </span>
        <div className="col-start-3 row-start-2">
          <TimeScrollColumn
            values={MINUTES}
            selected={minute}
            onSelect={setMinute}
            active={active}
          />
        </div>
      </div>
    </div>
  )
})
