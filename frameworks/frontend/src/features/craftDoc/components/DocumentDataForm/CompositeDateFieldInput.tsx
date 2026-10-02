import { InputText } from 'primereact/inputtext'
import type { DatePartCompositeView } from '../../utils/compositeDateFields'
import { fieldInputClass, fieldInputInvalidClass } from '../fieldStyles'

export type CompositeDateFieldInputProps = {
  composite: DatePartCompositeView
  onChange: (formDateValue: string) => void
  highlighted?: boolean
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

export function CompositeDateFieldInput({
  composite,
  onChange,
  highlighted,
}: CompositeDateFieldInputProps) {
  const invalid = Boolean(highlighted)
  const fieldId = `craft-doc-composite-date-${composite.id}`

  return (
    <InputText
      id={fieldId}
      type="date"
      value={toInputDate(composite.value)}
      unstyled
      className={invalid ? fieldInputInvalidClass : fieldInputClass}
      onChange={(event: { target: { value: string } }) =>
        onChange(fromInputDate(event.target.value))
      }
    />
  )
}
