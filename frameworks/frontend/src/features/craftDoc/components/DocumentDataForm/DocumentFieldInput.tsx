import { InputText } from 'primereact/inputtext'
import { Textarea } from 'primereact/textarea'
import { ToggleSwitch } from 'primereact/toggleswitch'
import { FieldType, type DocumentField } from '../../types'
import { getFieldInputPlaceholder } from '../../utils'
import { CraftDocSelect } from '../CraftDocSelect'
import {
  fieldInputClass,
  fieldInputInvalidClass,
  fieldTextareaClass,
  fieldTextareaInvalidClass,
} from '../fieldStyles'

export type DocumentFieldInputProps = {
  field: DocumentField
  value: string
  onChange: (value: string) => void
  highlighted?: boolean
}

export function DocumentFieldInput({
  field,
  value,
  onChange,
  highlighted,
}: DocumentFieldInputProps) {
  const invalid = Boolean(highlighted)
  const inputPlaceholder = getFieldInputPlaceholder(field)

  if (field.type === FieldType.Select && field.options?.length) {
    return (
      <CraftDocSelect
        value={value || undefined}
        options={field.options}
        placeholder="-- Chọn --"
        invalid={invalid}
        onChange={onChange}
      />
    )
  }

  if (field.type === FieldType.Textarea) {
    return (
      <Textarea
        value={value}
        rows={3}
        unstyled
        placeholder={inputPlaceholder}
        className={invalid ? fieldTextareaInvalidClass : fieldTextareaClass}
        onChange={(event: { target: { value: string } }) => onChange(event.target.value)}
      />
    )
  }

  if (field.type === FieldType.Date) {
    return (
      <InputText
        type="date"
        value={toInputDate(value)}
        unstyled
        className={invalid ? fieldInputInvalidClass : fieldInputClass}
        onChange={(event: { target: { value: string } }) =>
          onChange(fromInputDate(event.target.value))
        }
      />
    )
  }

  if (field.type === FieldType.Checkbox) {
    const checked = value === 'true' || value === '1' || value.toLowerCase() === 'yes'
    return (
      <ToggleSwitch.Root
        checked={checked}
        onCheckedChange={(event: { checked: boolean }) =>
          onChange(event.checked ? 'true' : '')
        }
      >
        <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-5 w-9 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-blue-600">
          <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-4 w-4 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-4" />
        </ToggleSwitch.Control>
      </ToggleSwitch.Root>
    )
  }

  return (
    <InputText
      type="text"
      value={value}
      unstyled
      placeholder={inputPlaceholder}
      className={invalid ? fieldInputInvalidClass : fieldInputClass}
      onChange={(event: { target: { value: string } }) => onChange(event.target.value)}
    />
  )
}

function toInputDate(value: string): string {
  if (!value) return ''
  const match = value.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/)
  if (!match) return value
  const [, day, month, year] = match
  return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`
}

function fromInputDate(value: string): string {
  if (!value) return ''
  const [year, month, day] = value.split('-')
  if (!year || !month || !day) return value
  return `${day.padStart(2, '0')}/${month.padStart(2, '0')}/${year}`
}
