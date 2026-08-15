import { DebouncedInputText } from '../DebouncedInputText'
import { DateOnlyPicker } from '../DateOnlyPicker'
import { DateTimePicker } from '../DateTimePicker'
import { parseBetweenValue, toDateWire } from '../utils'

export function BetweenValueEditor({
  value,
  handleOnChange,
  title,
  disabled,
  className,
  isNumber,
  isDate,
  isDateTime,
}: {
  value: unknown
  handleOnChange: (v: unknown) => void
  title?: string
  disabled?: boolean
  className?: string
  isNumber: boolean
  isDate: boolean
  isDateTime?: boolean
}) {
  const [fromWire, toWire] = parseBetweenValue(value)

  if (isDate) {
    const from = isDateTime ? fromWire : toDateWire(fromWire)
    const to = isDateTime ? toWire : toDateWire(toWire)
    const Picker = isDateTime ? DateTimePicker : DateOnlyPicker
    const commitBetween = (index: 0 | 1, iso: string) => {
      const parts: [string, string] = [from, to]
      parts[index] = iso
      handleOnChange(parts)
    }
    return (
      <div
        title={title}
        className={['inline-flex flex-wrap items-center gap-1.5', className]
          .filter(Boolean)
          .join(' ')}
      >
        <Picker
          value={from}
          disabled={disabled}
          placeholder={isDateTime ? 'Từ ngày…' : 'Từ ngày…'}
          onChange={(iso) => commitBetween(0, iso)}
        />
        <span className="shrink-0 text-xs font-medium text-slate-400">–</span>
        <Picker
          value={to}
          disabled={disabled}
          placeholder={isDateTime ? 'Đến ngày…' : 'Đến ngày…'}
          onChange={(iso) => commitBetween(1, iso)}
        />
      </div>
    )
  }

  const setPart = (index: 0 | 1, raw: string) => {
    const next: [string, string] = [fromWire, toWire]
    next[index] = raw
    if (isNumber) {
      handleOnChange(
        next.map((part) => {
          if (part === '') return ''
          const n = Number(part)
          return Number.isFinite(n) ? n : part
        }),
      )
      return
    }
    handleOnChange(next)
  }

  return (
    <div
      title={title}
      className={[
        'inline-flex h-10 min-w-[17rem] w-[17rem] items-center gap-1.5',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <DebouncedInputText
        type={isNumber ? 'number' : 'text'}
        value={fromWire}
        disabled={disabled}
        placeholder="Từ…"
        className="box-border h-10 min-w-0 flex-1 rounded-xl border border-solid !border-[#e2e8f0] bg-white px-2.5 text-sm text-slate-800 outline-none transition hover:!border-[#cbd5e1] focus:!border-[#0d9488] focus:ring-2 focus:ring-teal-600/20 disabled:opacity-50"
        onCommit={(raw) => setPart(0, raw)}
      />
      <span className="shrink-0 text-xs font-medium text-slate-400">–</span>
      <DebouncedInputText
        type={isNumber ? 'number' : 'text'}
        value={toWire}
        disabled={disabled}
        placeholder="Đến…"
        className="box-border h-10 min-w-0 flex-1 rounded-xl border border-solid !border-[#e2e8f0] bg-white px-2.5 text-sm text-slate-800 outline-none transition hover:!border-[#cbd5e1] focus:!border-[#0d9488] focus:ring-2 focus:ring-teal-600/20 disabled:opacity-50"
        onCommit={(raw) => setPart(1, raw)}
      />
    </div>
  )
}
