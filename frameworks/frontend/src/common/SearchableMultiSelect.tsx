import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type KeyboardEvent,
  type ReactNode,
} from 'react'
import { Check, Filter, X } from 'lucide-react'
import {
  DEFAULT_SELECT_PAGE_SIZE,
  useSearchableAsyncOptions,
} from '../hooks/useSearchableAsyncOptions'
import type {
  SearchableSelectLoadOptions,
  SearchableSelectOption,
} from './SearchableSelect'

export type SearchableMultiSelectOption = SearchableSelectOption & {
  group?: string
}

export type SearchableMultiSelectLoadOptions = SearchableSelectLoadOptions

export type SearchableMultiSelectProps = {
  value: string[]
  /** Call API — search/filter/phân trang do BE xử lý. */
  loadOptions: SearchableMultiSelectLoadOptions
  context?: Record<string, unknown>
  debounceMs?: number
  /** Số item mỗi lần load khi cuộn (mặc định 10). */
  pageSize?: number
  placeholder?: string
  emptyMessage?: string
  loadingMessage?: string
  loadingMoreMessage?: string
  disabled?: boolean
  triggerClassName?: string
  inputClassName?: string
  popupClassName?: string
  optionClassName?: string
  onValueChange: (values: string[]) => void
  renderIndicator?: () => ReactNode
}

const DEFAULT_DEBOUNCE_MS = 300

const defaultTriggerClass =
  'kit-searchable-multi-select relative flex min-h-[2.25rem] w-full min-w-0 items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-2.5 py-1 text-sm text-slate-800 outline-none transition hover:border-slate-300 focus-within:border-teal-500 focus-within:ring-2 focus-within:ring-teal-500/15'

const defaultInputClass =
  'min-w-[4rem] flex-1 border-0 bg-transparent p-0 text-sm text-inherit outline-none placeholder:text-slate-400'

const defaultPopupClass = 'kit-searchable-multi-select__popup'

const defaultOptionClass = 'kit-searchable-multi-select__option'

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

function useDebouncedValue<T>(value: T, delayMs: number) {
  const [debounced, setDebounced] = useState(value)
  useEffect(() => {
    const timer = window.setTimeout(() => setDebounced(value), delayMs)
    return () => window.clearTimeout(timer)
  }, [value, delayMs])
  return debounced
}

export function SearchableMultiSelect({
  value,
  loadOptions,
  context,
  debounceMs = DEFAULT_DEBOUNCE_MS,
  pageSize = DEFAULT_SELECT_PAGE_SIZE,
  placeholder = 'Filter by',
  emptyMessage = 'No results',
  loadingMessage = 'Loading…',
  loadingMoreMessage = 'Loading more…',
  disabled = false,
  triggerClassName = defaultTriggerClass,
  inputClassName = defaultInputClass,
  popupClassName = defaultPopupClass,
  optionClassName = defaultOptionClass,
  onValueChange,
  renderIndicator,
}: SearchableMultiSelectProps) {
  const rootRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const selectedCacheRef = useRef<Map<string, SearchableMultiSelectOption>>(
    new Map(),
  )
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const [activeIndex, setActiveIndex] = useState(0)

  const selectedSet = useMemo(() => new Set(value.map(String)), [value])
  const debouncedSearch = useDebouncedValue(query, debounceMs)

  const {
    options: remoteBase,
    loading,
    loadingMore,
    onListScroll,
  } = useSearchableAsyncOptions({
    enabled: true,
    open,
    loadOptions,
    search: debouncedSearch.trim(),
    context,
    pageSize,
  })

  const displayed = remoteBase as SearchableMultiSelectOption[]

  useEffect(() => {
    for (const option of displayed) {
      selectedCacheRef.current.set(String(option.value), option)
    }
  }, [displayed])

  const selectedOptions = useMemo(() => {
    for (const option of displayed) {
      if (selectedSet.has(String(option.value))) {
        selectedCacheRef.current.set(String(option.value), option)
      }
    }
    return value
      .map((id) => selectedCacheRef.current.get(String(id)))
      .filter((option): option is SearchableMultiSelectOption => Boolean(option))
  }, [value, selectedSet, displayed])

  const close = useCallback(() => {
    setOpen(false)
    setQuery('')
    setActiveIndex(0)
  }, [])

  useClickOutside(rootRef, close, open)

  useEffect(() => {
    if (activeIndex >= displayed.length) {
      setActiveIndex(Math.max(0, displayed.length - 1))
    }
  }, [activeIndex, displayed.length])

  const openDropdown = useCallback(() => {
    if (disabled) return
    setOpen(true)
  }, [disabled])

  const toggleOption = (option: SearchableMultiSelectOption) => {
    const id = String(option.value)
    selectedCacheRef.current.set(id, option)
    if (selectedSet.has(id)) {
      onValueChange(value.filter((entry) => String(entry) !== id))
      return
    }
    onValueChange([...value, id])
  }

  const removeOption = (id: string) => {
    onValueChange(value.filter((entry) => String(entry) !== id))
  }

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'ArrowDown') {
      event.preventDefault()
      if (!open) openDropdown()
      setActiveIndex((index) =>
        Math.min(index + 1, Math.max(0, displayed.length - 1)),
      )
      return
    }
    if (event.key === 'ArrowUp') {
      event.preventDefault()
      if (!open) openDropdown()
      setActiveIndex((index) => Math.max(index - 1, 0))
      return
    }
    if (event.key === 'Enter') {
      event.preventDefault()
      const option = displayed[activeIndex]
      if (option) toggleOption(option)
      return
    }
    if (event.key === 'Escape') {
      event.preventDefault()
      close()
      inputRef.current?.blur()
    }
    if (event.key === 'Backspace' && !query && value.length > 0) {
      onValueChange(value.slice(0, -1))
    }
  }

  const grouped = useMemo(() => {
    const groups = new Map<string, SearchableMultiSelectOption[]>()
    for (const option of displayed) {
      const key = option.group ?? ''
      const bucket = groups.get(key) ?? []
      bucket.push(option)
      groups.set(key, bucket)
    }
    return [...groups.entries()]
  }, [displayed])

  let optionIndex = -1

  return (
    <div
      ref={rootRef}
      className={[triggerClassName, open ? 'is-open' : '']
        .filter(Boolean)
        .join(' ')}
    >
      <div className="kit-searchable-multi-select__values">
        {selectedOptions.map((option) => (
          <span
            key={String(option.value)}
            className="kit-searchable-multi-select__chip"
          >
            <span className="kit-searchable-multi-select__chip-label">
              {option.label}
            </span>
            <button
              type="button"
              className="kit-searchable-multi-select__chip-remove"
              aria-label={`Remove ${option.label}`}
              onMouseDown={(event) => event.preventDefault()}
              onClick={() => removeOption(String(option.value))}
            >
              <X className="h-3 w-3" />
            </button>
          </span>
        ))}
        <input
          ref={inputRef}
          type="text"
          role="combobox"
          aria-expanded={open}
          aria-autocomplete="list"
          aria-busy={loading || loadingMore}
          disabled={disabled}
          value={query}
          placeholder={selectedOptions.length === 0 ? placeholder : ''}
          className={inputClassName}
          onFocus={openDropdown}
          onChange={(event) => {
            setQuery(event.target.value)
            setOpen(true)
            setActiveIndex(0)
          }}
          onKeyDown={handleKeyDown}
        />
      </div>
      <span className="kit-searchable-multi-select__indicator" aria-hidden>
        {renderIndicator ? (
          renderIndicator()
        ) : (
          <Filter className="h-4 w-4 text-slate-400" />
        )}
      </span>

      {open ? (
        <div
          className={[defaultPopupClass, popupClassName]
            .filter(Boolean)
            .join(' ')}
        >
          {loading ? (
            <p className="kit-searchable-multi-select__loading">
              {loadingMessage}
            </p>
          ) : displayed.length > 0 ? (
            <ul
              role="listbox"
              className="kit-searchable-multi-select__list"
              onScroll={onListScroll}
            >
              {grouped.map(([groupName, groupOptions]) => (
                <li
                  key={groupName || '__default'}
                  className="kit-searchable-multi-select__group"
                >
                  {groupName ? (
                    <div className="kit-searchable-multi-select__group-label">
                      {groupName}
                    </div>
                  ) : null}
                  <ul className="kit-searchable-multi-select__group-list">
                    {groupOptions.map((option) => {
                      optionIndex += 1
                      const index = optionIndex
                      const selected = selectedSet.has(String(option.value))
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
                          onClick={() => toggleOption(option)}
                        >
                          <span
                            className="kit-searchable-multi-select__check"
                            aria-hidden
                          >
                            {selected ? (
                              <Check className="h-3.5 w-3.5" />
                            ) : null}
                          </span>
                          <span>{option.label}</span>
                        </li>
                      )
                    })}
                  </ul>
                </li>
              ))}
              {loadingMore ? (
                <li
                  className="kit-searchable-multi-select__loading-more"
                  aria-hidden
                >
                  {loadingMoreMessage}
                </li>
              ) : null}
            </ul>
          ) : (
            <p className="kit-searchable-multi-select__empty">{emptyMessage}</p>
          )}
        </div>
      ) : null}
    </div>
  )
}
