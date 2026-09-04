import { useState } from 'react'
import { ChevronDown } from 'lucide-react'
import { Select } from 'primereact/select'
import type { VersatileSelectorProps } from 'react-querybuilder'
import {
  optionClass,
  selectCombinatorClass,
  selectFieldClass,
  selectOperatorClass,
  selectTriggerClass,
} from './styles'
import { flattenOptions } from './utils'

export function PrimeSelector({
  options,
  value,
  handleOnChange,
  title,
  disabled,
  className,
  testID,
}: VersatileSelectorProps) {
  const [open, setOpen] = useState(false)
  const normalized = flattenOptions(options)
  const isCombinator = testID === 'combinators'

  const sizeClass =
    testID === 'fields'
      ? selectFieldClass
      : testID === 'operators'
        ? selectOperatorClass
        : isCombinator
          ? selectCombinatorClass
          : selectTriggerClass

  // PrimeReact sizes the popup from Select.Root (not Trigger). Keep Root
  // shrink-wrapped so between-rules AND/OR menus don't span the full row.
  const rootClass = 'inline-flex !w-fit max-w-full shrink-0'
  const popupClass = isCombinator
    ? 'w-max min-w-[4.75rem] max-h-64 overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-xl shadow-slate-900/10'
    : 'max-h-64 min-w-[var(--px-positioner-anchor-width)] overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-xl shadow-slate-900/10'
  const optionCls = isCombinator
    ? 'cursor-pointer px-3 py-2 text-xs font-semibold uppercase tracking-wide text-slate-800 outline-none transition-colors data-[focused]:bg-slate-50 data-[selected]:bg-teal-50 data-[selected]:font-semibold data-[selected]:text-teal-800'
    : optionClass

  return (
    <Select.Root
      value={value ? String(value) : undefined}
      open={open}
      options={normalized}
      optionLabel="label"
      optionValue="value"
      disabled={disabled}
      className={rootClass}
      onOpenChange={(e: { value: boolean }) => setOpen(e.value)}
      onValueChange={(e: { value?: unknown }) => {
        if (e.value != null) handleOnChange(String(e.value))
      }}
    >
      <Select.Trigger
        type="button"
        title={title}
        data-testid={testID}
        className={[sizeClass, className].filter(Boolean).join(' ')}
      >
        <Select.Value placeholder="Chọn…" />
        <Select.Indicator
          className={[
            'inline-flex shrink-0 text-slate-400 transition-transform duration-200',
            open ? 'rotate-180' : '',
          ].join(' ')}
        >
          <ChevronDown className="size-3.5" aria-hidden />
        </Select.Indicator>
      </Select.Trigger>
      <Select.Portal>
        <Select.Positioner className="z-[140] w-max" align="start" sideOffset={4}>
          <Select.Popup className={popupClass}>
            <Select.List className="m-0 max-h-64 list-none overflow-y-auto p-0 outline-none">
              {normalized.map((option, index) => (
                <Select.Option
                  key={option.value}
                  index={index}
                  className={optionCls}
                >
                  {option.label}
                </Select.Option>
              ))}
            </Select.List>
          </Select.Popup>
        </Select.Positioner>
      </Select.Portal>
    </Select.Root>
  )
}
