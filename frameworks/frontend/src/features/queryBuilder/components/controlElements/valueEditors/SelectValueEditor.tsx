import { useState } from 'react'
import { ChevronDown } from 'lucide-react'
import { Select } from 'primereact/select'
import { optionClass } from '../styles'
import { flattenOptions } from '../utils'

export function SelectValueEditor({
  value,
  handleOnChange,
  title,
  disabled,
  className,
  values,
}: {
  value: unknown
  handleOnChange: (v: unknown) => void
  title?: string
  disabled?: boolean
  className?: string
  values: unknown
}) {
  const [open, setOpen] = useState(false)
  const list = flattenOptions(values)
  return (
    <Select.Root
      value={value == null ? undefined : String(value)}
      open={open}
      options={list}
      optionLabel="label"
      optionValue="value"
      disabled={disabled}
      className="inline-flex !w-fit max-w-full shrink-0"
      onOpenChange={(e: { value: boolean }) => setOpen(e.value)}
      onValueChange={(e: { value?: unknown }) => {
        if (e.value != null) handleOnChange(String(e.value))
      }}
    >
      <Select.Trigger
        type="button"
        title={title}
        className={[
          'inline-flex h-10 min-w-[17rem] w-[17rem] items-center justify-between gap-2 rounded-xl border border-solid !border-[#e2e8f0] bg-white px-3 text-sm text-slate-800 shadow-[0_1px_2px_rgba(15,23,42,0.04)] outline-none transition hover:!border-[#cbd5e1] focus-visible:!border-[#0d9488] focus-visible:ring-2 focus-visible:ring-teal-600/20 disabled:cursor-not-allowed disabled:opacity-50',
          className,
        ]
          .filter(Boolean)
          .join(' ')}
      >
        <Select.Value placeholder="Giá trị…" />
        <Select.Indicator className="inline-flex shrink-0 text-slate-400">
          <ChevronDown className="size-3.5" aria-hidden />
        </Select.Indicator>
      </Select.Trigger>
      <Select.Portal>
        <Select.Positioner className="z-[140] w-max" align="start" sideOffset={4}>
          <Select.Popup className="max-h-64 min-w-[var(--px-positioner-anchor-width)] overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-xl shadow-slate-900/10">
            <Select.List className="m-0 max-h-64 list-none overflow-y-auto p-0 outline-none">
              {list.map((option, index) => (
                <Select.Option
                  key={option.value}
                  index={index}
                  className={optionClass}
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
