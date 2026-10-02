import { useMemo } from 'react'
import { SearchableSelect } from '../../../../common/SearchableSelect'
import {
  fieldSearchableSelectTriggerClass,
  fieldSearchableSelectTriggerInvalidClass,
} from '../fieldStyles'

export type CraftDocSelectOption<T extends string = string> = {
  label: string
  value: T
}

export type CraftDocSelectProps<T extends string = string> = {
  id?: string
  value: T | undefined
  options: readonly CraftDocSelectOption<T>[]
  onChange: (value: T) => void
  placeholder?: string
  searchPlaceholder?: string
  emptyMessage?: string
  invalid?: boolean
  disabled?: boolean
  searchable?: boolean
  className?: string
}

export function CraftDocSelect<T extends string = string>({
  value,
  options,
  onChange,
  placeholder = 'Chọn…',
  searchPlaceholder,
  emptyMessage = 'Không có kết quả',
  invalid = false,
  disabled = false,
  searchable = true,
  className = '',
}: CraftDocSelectProps<T>) {
  const list = useMemo(
    () => options.map((option) => ({ label: option.label, value: option.value })),
    [options],
  )

  const triggerClassName = [
    invalid ? fieldSearchableSelectTriggerInvalidClass : fieldSearchableSelectTriggerClass,
    className,
  ]
    .filter(Boolean)
    .join(' ')

  const handleValueChange = (next: string | number | null) => {
    if (next == null || next === '') {
      onChange('' as T)
      return
    }
    onChange(String(next) as T)
  }

  return (
    <SearchableSelect
      searchable={searchable}
      value={value ?? ''}
      options={list}
      placeholder={placeholder}
      searchPlaceholder={searchPlaceholder ?? placeholder}
      emptyMessage={emptyMessage}
      disabled={disabled}
      triggerClassName={triggerClassName}
      onValueChange={handleValueChange}
    />
  )
}
