import { useMemo, useState } from 'react'
import { ChevronDown } from 'lucide-react'
import { Select } from 'primereact/select'
import type { SettingOption } from '../types'

const triggerClass =
  'flex h-11 w-full items-center justify-between gap-2 rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 data-[positioner-open]:border-teal-600 data-[positioner-open]:ring-2 data-[positioner-open]:ring-teal-600/20'

const triggerInvalidClass = `${triggerClass} !border-red-500 data-[positioner-open]:!border-red-500 data-[positioner-open]:!ring-red-500/20`

type SettingSelectProps = {
  value: string
  options: readonly SettingOption[]
  onChange: (value: string) => void
  placeholder?: string
  invalid?: boolean
  disabled?: boolean
  filter?: boolean
  filterPlaceholder?: string
}

/** Local select with optional search — keeps frameworks/frontend untouched. */
export function SettingSelect({
  value,
  options,
  onChange,
  placeholder = 'Chọn…',
  invalid = false,
  disabled = false,
  filter = false,
  filterPlaceholder = 'Tìm kiếm…',
}: SettingSelectProps) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const list = options as SettingOption[]
  const filtered = useMemo(() => {
    if (!filter || !query.trim()) return list
    const needle = query.trim().toLowerCase()
    return list.filter(
      (option) =>
        option.label.toLowerCase().includes(needle) ||
        option.value.toLowerCase().includes(needle),
    )
  }, [filter, list, query])

  return (
    <Select.Root
      value={value || undefined}
      open={open}
      options={filtered}
      optionLabel="label"
      optionValue="value"
      disabled={disabled}
      onOpenChange={(e: { value: boolean }) => {
        setOpen(e.value)
        if (!e.value) setQuery('')
      }}
      onValueChange={(e: { value?: unknown }) => {
        if (e.value != null) onChange(String(e.value))
      }}
    >
      <Select.Trigger
        type="button"
        className={invalid ? triggerInvalidClass : triggerClass}
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
            {filter && (
              <div className="border-b border-slate-100 px-2 pb-2 pt-1">
                <input
                  type="search"
                  value={query}
                  placeholder={filterPlaceholder}
                  className="box-border h-9 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20"
                  onChange={(event) => setQuery(event.target.value)}
                  onClick={(event) => event.stopPropagation()}
                  onKeyDown={(event) => event.stopPropagation()}
                />
              </div>
            )}
            <Select.List className="m-0 max-h-60 list-none overflow-y-auto p-0 outline-none">
              {filtered.length ? (
                filtered.map((option, index) => (
                  <Select.Option
                    key={option.value}
                    index={index}
                    className="cursor-pointer px-3.5 py-2.5 text-sm text-slate-800 outline-none transition-colors data-[focused]:bg-slate-50 data-[selected]:bg-teal-50 data-[selected]:font-medium data-[selected]:text-teal-800"
                  >
                    {option.label}
                  </Select.Option>
                ))
              ) : (
                <li className="px-3.5 py-2.5 text-sm text-slate-500">
                  Không tìm thấy kết quả.
                </li>
              )}
            </Select.List>
          </Select.Popup>
        </Select.Positioner>
      </Select.Portal>
    </Select.Root>
  )
}
