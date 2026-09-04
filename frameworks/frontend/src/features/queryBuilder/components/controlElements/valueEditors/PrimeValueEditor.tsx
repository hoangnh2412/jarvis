import { ToggleSwitch } from 'primereact/toggleswitch'
import type { ValueEditorProps } from 'react-querybuilder'
import { DebouncedInputText } from '../DebouncedInputText'
import { DateOnlyPicker } from '../DateOnlyPicker'
import { DateTimePicker } from '../DateTimePicker'
import { InTagsValueEditor } from './InTagsValueEditor'
import { inputClass } from '../styles'
import { isDateField, isDateOnlyField, isDateTimeField } from '../utils'
import { BetweenValueEditor } from './BetweenValueEditor'
import { SelectValueEditor } from './SelectValueEditor'

export function PrimeValueEditor(props: ValueEditorProps) {
  const {
    fieldData,
    operator,
    value,
    handleOnChange,
    title,
    disabled,
    className,
    type,
    values,
    inputType,
  } = props

  if (operator === 'null' || operator === 'notNull') return null

  const isNumber =
    inputType === 'number' ||
    fieldData?.inputType === 'number' ||
    fieldData?.datatype === 'number'

  const dateField = isDateField(inputType, fieldData)
  const dateTimeField = isDateTimeField(inputType, fieldData)
  const dateOnlyField = isDateOnlyField(inputType, fieldData)

  if (operator === 'between') {
    return (
      <BetweenValueEditor
        value={value}
        handleOnChange={handleOnChange}
        title={title}
        disabled={disabled}
        className={className}
        isNumber={isNumber}
        isDate={dateField}
        isDateTime={dateTimeField}
      />
    )
  }

  if (operator === 'in') {
    return (
      <InTagsValueEditor
        value={value}
        handleOnChange={handleOnChange}
        title={title}
        disabled={disabled}
        className={className}
      />
    )
  }

  const editorType = type ?? fieldData?.valueEditorType ?? 'text'

  if (editorType === 'checkbox') {
    return (
      <div className="inline-flex h-10 min-w-[17rem] w-[17rem] items-center gap-2.5 rounded-xl border border-solid !border-[#e2e8f0] bg-white px-3 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
        <ToggleSwitch.Root
          checked={value === true || value === 'true'}
          disabled={disabled}
          onCheckedChange={(e: { checked: boolean }) => handleOnChange(e.checked)}
          className="relative inline-flex h-5 w-9 shrink-0"
          inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0"
          ariaLabel={typeof title === 'string' ? title : 'Value'}
        >
          <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-5 w-9 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
            <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-4 w-4 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-4" />
          </ToggleSwitch.Control>
        </ToggleSwitch.Root>
        <span className="text-sm text-slate-600">
          {value === true || value === 'true' ? 'True' : 'False'}
        </span>
      </div>
    )
  }

  if (editorType === 'select') {
    return (
      <SelectValueEditor
        value={value}
        handleOnChange={handleOnChange}
        title={title}
        disabled={disabled}
        className={className}
        values={values}
      />
    )
  }

  if (dateTimeField) {
    return (
      <DateTimePicker
        value={value}
        title={title}
        disabled={disabled}
        className={className}
        onChange={handleOnChange}
      />
    )
  }

  if (dateOnlyField) {
    return (
      <DateOnlyPicker
        value={value}
        title={title}
        disabled={disabled}
        className={className}
        onChange={handleOnChange}
      />
    )
  }

  return (
    <DebouncedInputText
      type={isNumber ? 'number' : inputType ?? 'text'}
      value={value == null ? '' : String(value)}
      title={title}
      disabled={disabled}
      placeholder="Nhập giá trị…"
      className={[inputClass, className].filter(Boolean).join(' ')}
      onCommit={(raw) => {
        if (isNumber && raw !== '') {
          const n = Number(raw)
          handleOnChange(Number.isFinite(n) ? n : raw)
          return
        }
        handleOnChange(raw)
      }}
    />
  )
}
