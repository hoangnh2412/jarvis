import { useState, type ReactNode } from 'react'
import { ChevronDown } from 'lucide-react'
import { Select } from 'primereact/select'
import {
  fieldSelectTriggerClass,
  fieldSelectTriggerInvalidClass,
} from '../fieldStyles'

export type FieldSelectOption<T extends string | number> = {
  label: string
  value: T
}

export type FieldSelectProps<T extends string | number> = {
  id?: string
  value: T | null | undefined
  options: readonly FieldSelectOption<T>[]
  onChange: (value: T) => void
  placeholder?: string
  invalid?: boolean
  disabled?: boolean
  className?: string
}

export function FieldSelect<T extends string | number>({
  id,
  value,
  options,
  onChange,
  placeholder = 'Chọn…',
  invalid = false,
  disabled = false,
  className = '',
}: FieldSelectProps<T>) {
  const [open, setOpen] = useState(false)
  const list = options as FieldSelectOption<T>[]

  return (
    <Select.Root
      id={id}
      value={value ?? undefined}
      open={open}
      options={list}
      optionLabel="label"
      optionValue="value"
      disabled={disabled}
      onOpenChange={(e: { value: boolean }) => setOpen(e.value)}
      onValueChange={(e: { value?: unknown }) => {
        if (e.value != null) onChange(e.value as T)
      }}
    >
      <Select.Trigger
        type="button"
        className={[
          invalid ? fieldSelectTriggerInvalidClass : fieldSelectTriggerClass,
          className,
        ]
          .filter(Boolean)
          .join(' ')}
      >
        <Select.Value placeholder={placeholder} />
        <Select.Indicator
          className={[
            'inline-flex shrink-0 text-slate-500 transition-transform duration-200 ease-out',
            open ? 'rotate-180' : 'rotate-0',
          ].join(' ')}
        >
          <ChevronDown className="size-4" aria-hidden />
        </Select.Indicator>
      </Select.Trigger>
      <Select.Portal>
        <Select.Positioner className="z-[120]">
          <Select.Popup className="min-w-[var(--px-positioner-anchor-width)] overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-lg">
            <Select.List className="m-0 list-none p-0 outline-none">
              {list.map((option, index) => (
                <Select.Option
                  key={String(option.value)}
                  index={index}
                  className="cursor-pointer px-3.5 py-2.5 text-sm text-slate-800 outline-none transition-colors data-[focused]:bg-slate-50 data-[selected]:bg-teal-50 data-[selected]:font-medium data-[selected]:text-teal-800"
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

export type FieldSelectEmptyProps = {
  children?: ReactNode
}
