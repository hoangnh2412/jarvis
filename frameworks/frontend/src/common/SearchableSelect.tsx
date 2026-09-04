import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type KeyboardEvent,
  type ReactNode,
} from 'react'
import { ChevronDown } from 'lucide-react'
import { Select } from 'primereact/select'

export type SearchableSelectOption = {
  label: string
  value: string | number
}

export type SearchableSelectProps = {
  value?: string | number | ''
  options: readonly SearchableSelectOption[]
  placeholder?: string
  searchPlaceholder?: string
  emptyMessage?: string
  disabled?: boolean
  searchable?: boolean
  triggerClassName?: string
  inputClassName?: string
  popupClassName?: string
  optionClassName?: string
  indicatorClassName?: string
  onValueChange: (value: string | number | null) => void
  renderValue?: (option: SearchableSelectOption | null) => ReactNode
}

const defaultTriggerClass =
  'relative flex h-full w-full min-w-0 items-center justify-between gap-1 rounded-md border border-slate-200 bg-white px-2.5 text-sm text-slate-800 outline-none transition hover:border-slate-300 focus-within:border-teal-500 focus-within:ring-2 focus-within:ring-teal-500/15'

const defaultInputClass =
  'min-w-0 flex-1 border-0 bg-transparent p-0 text-inherit outline-none placeholder:text-slate-400'

const defaultPopupClass = 'kit-searchable-select__popup'

const defaultOptionClass = 'kit-searchable-select__option'

function filterOptions(list: readonly SearchableSelectOption[], query: string) {
  const q = query.trim().toLowerCase()
  if (!q) return list as SearchableSelectOption[]
  return list.filter((option) => option.label.toLowerCase().includes(q))
}

function useClickOutside(
  ref: React.RefObject<HTMLElement | null>,
  onOutside: () => void,
  enabled: boolean,
) {
  useEffect(() => {
    if (!enabled) return
    const onMouseDown = (event: MouseEvent) => {
      if (!ref.current?.contains(event.target as Node)) onOutside()
    }
    document.addEventListener('mousedown', onMouseDown)
    return () => document.removeEventListener('mousedown', onMouseDown)
  }, [enabled, onOutside, ref])
}

/** Combobox: click mở full list, gõ trên input để lọc, popup căn theo wrapper. */
function SearchableSelectCombobox({
  value,
  options,
  placeholder = 'Chọn…',
  emptyMessage = 'Không có kết quả',
  disabled = false,
  triggerClassName = defaultTriggerClass,
  inputClassName = defaultInputClass,
  popupClassName = defaultPopupClass,
  optionClassName = defaultOptionClass,
  indicatorClassName = 'text-slate-400',
  onValueChange,
}: Omit<SearchableSelectProps, 'searchable' | 'searchPlaceholder' | 'renderValue'>) {
  const rootRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const [isEditing, setIsEditing] = useState(false)
  const [activeIndex, setActiveIndex] = useState(0)

  const list = useMemo(() => options as SearchableSelectOption[], [options])

  const selectedOption = useMemo(
    () => list.find((option) => String(option.value) === String(value ?? '')) ?? null,
    [list, value],
  )

  const filtered = useMemo(() => {
    if (!isEditing) return list
    return filterOptions(list, query)
  }, [isEditing, list, query])

  const close = useCallback(() => {
    setOpen(false)
    setIsEditing(false)
    setQuery('')
    setActiveIndex(0)
  }, [])

  useClickOutside(rootRef, close, open)

  useEffect(() => {
    if (activeIndex >= filtered.length) {
      setActiveIndex(Math.max(0, filtered.length - 1))
    }
  }, [activeIndex, filtered.length])

  const openAll = useCallback(() => {
    if (disabled) return
    setIsEditing(false)
    setQuery(selectedOption?.label ?? '')
    setActiveIndex(
      Math.max(
        0,
        list.findIndex((option) => String(option.value) === String(value ?? '')),
      ),
    )
    setOpen(true)
  }, [disabled, list, selectedOption?.label, value])

  const selectOption = (option: SearchableSelectOption) => {
    onValueChange(option.value)
    close()
    inputRef.current?.blur()
  }

  const handleInputChange = (next: string) => {
    setIsEditing(true)
    setQuery(next)
    setOpen(true)
    setActiveIndex(0)
  }

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'ArrowDown') {
      event.preventDefault()
      if (!open) openAll()
      setActiveIndex((index) => Math.min(index + 1, Math.max(0, filtered.length - 1)))
      return
    }
    if (event.key === 'ArrowUp') {
      event.preventDefault()
      if (!open) openAll()
      setActiveIndex((index) => Math.max(index - 1, 0))
      return
    }
    if (event.key === 'Enter') {
      event.preventDefault()
      const option = filtered[activeIndex]
      if (option) selectOption(option)
      return
    }
    if (event.key === 'Escape') {
      event.preventDefault()
      close()
      inputRef.current?.blur()
    }
  }

  const displayValue = open || isEditing ? query : (selectedOption?.label ?? '')

  return (
    <div
      ref={rootRef}
      className={['kit-searchable-select', open ? 'is-open' : '', triggerClassName]
        .filter(Boolean)
        .join(' ')}
    >
      <input
        ref={inputRef}
        type="text"
        role="combobox"
        aria-expanded={open}
        aria-autocomplete="list"
        aria-controls="kit-searchable-select-listbox"
        disabled={disabled}
        value={displayValue}
        placeholder={placeholder}
        className={inputClassName}
        onFocus={openAll}
        onChange={(event) => handleInputChange(event.target.value)}
        onKeyDown={handleKeyDown}
      />
      <button
        type="button"
        tabIndex={-1}
        disabled={disabled}
        aria-label="Mở danh sách"
        className="kit-searchable-select__toggle"
        onMouseDown={(event) => event.preventDefault()}
        onClick={() => {
          if (disabled) return
          if (open) {
            close()
            return
          }
          openAll()
          inputRef.current?.focus()
        }}
      >
        <ChevronDown
          className={['kit-searchable-select__chevron', indicatorClassName]
            .filter(Boolean)
            .join(' ')}
          aria-hidden
        />
      </button>

      {open ? (
        <div className={[defaultPopupClass, popupClassName].filter(Boolean).join(' ')}>
          {filtered.length > 0 ? (
            <ul
              id="kit-searchable-select-listbox"
              role="listbox"
              className="kit-searchable-select__list"
            >
              {filtered.map((option, index) => {
                const selected = String(option.value) === String(value ?? '')
                const active = index === activeIndex
                return (
                  <li
                    key={String(option.value)}
                    role="option"
                    aria-selected={selected}
                    className={[
                      defaultOptionClass,
                      optionClassName,
                      selected ? 'is-selected' : '',
                      active ? 'is-active' : '',
                    ]
                      .filter(Boolean)
                      .join(' ')}
                    onMouseDown={(event) => event.preventDefault()}
                    onMouseEnter={() => setActiveIndex(index)}
                    onClick={() => selectOption(option)}
                  >
                    {option.label}
                  </li>
                )
              })}
            </ul>
          ) : (
            <p className="kit-searchable-select__empty">{emptyMessage}</p>
          )}
        </div>
      ) : null}
    </div>
  )
}

function SearchableSelectBasic({
  value,
  options,
  placeholder = 'Chọn…',
  disabled = false,
  triggerClassName = defaultTriggerClass,
  popupClassName = 'min-w-[var(--px-positioner-anchor-width)] overflow-hidden rounded-lg border border-slate-200 bg-white shadow-lg',
  optionClassName = defaultOptionClass,
  indicatorClassName = 'text-slate-400',
  onValueChange,
  renderValue,
}: Omit<SearchableSelectProps, 'searchable' | 'searchPlaceholder' | 'emptyMessage' | 'inputClassName'>) {
  const [open, setOpen] = useState(false)
  const list = options as SearchableSelectOption[]

  const selectedOption = useMemo(
    () => list.find((option) => String(option.value) === String(value ?? '')) ?? null,
    [list, value],
  )

  return (
    <Select.Root
      value={value ?? ''}
      open={open}
      options={list}
      optionLabel="label"
      optionValue="value"
      disabled={disabled}
      onOpenChange={(e: { value: boolean }) => setOpen(e.value)}
      onValueChange={(e: { value?: unknown }) => {
        const next = e.value
        if (next === '' || next == null) {
          onValueChange(null)
          return
        }
        onValueChange(typeof next === 'number' ? next : String(next))
      }}
    >
      <Select.Trigger type="button" className={triggerClassName}>
        {renderValue ? renderValue(selectedOption) : <Select.Value placeholder={placeholder} />}
        <Select.Indicator className={indicatorClassName}>
          <ChevronDown className="size-3.5" aria-hidden />
        </Select.Indicator>
      </Select.Trigger>
      <Select.Portal>
        <Select.Positioner className="z-[200]">
          <Select.Popup className={popupClassName}>
            <Select.List className="m-0 max-h-56 list-none overflow-y-auto p-1 outline-none">
              {list.map((option, index) => (
                <Select.Option key={String(option.value)} index={index} className={optionClassName}>
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

export function SearchableSelect({
  searchable = true,
  ...props
}: SearchableSelectProps) {
  if (searchable) {
    return <SearchableSelectCombobox {...props} />
  }
  return <SearchableSelectBasic {...props} />
}
