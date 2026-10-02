import type {
  FormContextType,
  RJSFSchema,
  RegistryWidgetsType,
  StrictRJSFSchema,
  WidgetProps,
} from '@rjsf/utils'
import { InputText } from 'primereact/inputtext'
import { Textarea } from 'primereact/textarea'
import {
  fieldInputClass,
  fieldInputInvalidClass,
  fieldTextareaClass,
  fieldTextareaInvalidClass,
} from '../components/fieldStyles'

function BaseInput<
  T = unknown,
  S extends StrictRJSFSchema = RJSFSchema,
  F extends FormContextType = any,
>(props: WidgetProps<T, S, F> & { type?: string; as?: 'input' | 'textarea' }) {
  const {
    id,
    value,
    required,
    disabled,
    readonly,
    autofocus,
    onChange,
    onBlur,
    onFocus,
    placeholder,
    rawErrors,
    options,
    type = 'text',
    as = 'input',
  } = props
  const invalid = Boolean(rawErrors?.length)
  const common = {
    id,
    name: props.name,
    required,
    disabled: disabled || readonly,
    autoFocus: autofocus,
    placeholder: placeholder ?? undefined,
    value: value ?? '',
    spellCheck: false as const,
    unstyled: true as const,
    onBlur: () => onBlur(id, value),
    onFocus: () => onFocus(id, value),
  }

  if (as === 'textarea') {
    return (
      <Textarea
        {...common}
        rows={Number(options?.rows) || 4}
        className={invalid ? fieldTextareaInvalidClass : fieldTextareaClass}
        onChange={(e: { target: { value: string } }) =>
          onChange(e.target.value === '' ? options.emptyValue : e.target.value)
        }
      />
    )
  }

  return (
    <InputText
      {...common}
      type={type}
      className={invalid ? fieldInputInvalidClass : fieldInputClass}
      onChange={(e: { target: { value: string } }) =>
        onChange(e.target.value === '' ? options.emptyValue : e.target.value)
      }
    />
  )
}

function TextWidget(props: WidgetProps) {
  return <BaseInput {...props} type="text" />
}
function PasswordWidget(props: WidgetProps) {
  return <BaseInput {...props} type="password" />
}
function EmailWidget(props: WidgetProps) {
  return <BaseInput {...props} type="email" />
}
function URLWidget(props: WidgetProps) {
  return <BaseInput {...props} type="url" />
}
function DateWidget(props: WidgetProps) {
  return <BaseInput {...props} type="date" />
}
function DateTimeWidget(props: WidgetProps) {
  return <BaseInput {...props} type="datetime-local" />
}
function TimeWidget(props: WidgetProps) {
  return <BaseInput {...props} type="time" />
}
function TextareaWidget(props: WidgetProps) {
  return <BaseInput {...props} as="textarea" />
}

function UpDownWidget(props: WidgetProps) {
  const { id, value, disabled, readonly, autofocus, onChange, onBlur, onFocus, rawErrors } =
    props
  const invalid = Boolean(rawErrors?.length)
  return (
    <InputText
      unstyled
      id={id}
      type="number"
      disabled={disabled || readonly}
      autoFocus={autofocus}
      value={value ?? ''}
      className={invalid ? fieldInputInvalidClass : fieldInputClass}
      onChange={(e: { target: { value: string } }) => {
        const v = e.target.value
        onChange(v === '' ? undefined : Number(v))
      }}
      onBlur={() => onBlur(id, value)}
      onFocus={() => onFocus(id, value)}
    />
  )
}

function RangeWidget(props: WidgetProps) {
  const { schema, value, onChange, disabled, readonly, id } = props
  const min = typeof schema.minimum === 'number' ? schema.minimum : 0
  const max = typeof schema.maximum === 'number' ? schema.maximum : 100
  const step = typeof schema.multipleOf === 'number' ? schema.multipleOf : 1
  return (
    <div className="kit-df-rjsf-range">
      <input
        id={id}
        type="range"
        min={min}
        max={max}
        step={step}
        disabled={disabled || readonly}
        value={value ?? min}
        onChange={(e) => onChange(Number(e.target.value))}
      />
      <span className="kit-df-rjsf-range__value">{value ?? min}</span>
    </div>
  )
}

function SelectWidget(props: WidgetProps) {
  const {
    id,
    options,
    value,
    disabled,
    readonly,
    onChange,
    onBlur,
    onFocus,
    placeholder,
    rawErrors,
    multiple,
  } = props
  const { enumOptions = [], enumDisabled = [] } = options
  const invalid = Boolean(rawErrors?.length)
  const disabledSet = new Set(enumDisabled as unknown[])

  if (multiple) {
    return (
      <select
        id={id}
        multiple
        disabled={disabled || readonly}
        className={invalid ? fieldInputInvalidClass : fieldInputClass}
        value={(value as string[]) ?? []}
        onChange={(e) => {
          const selected = Array.from(e.target.selectedOptions).map((o) => o.value)
          onChange(selected)
        }}
        onBlur={() => onBlur(id, value)}
        onFocus={() => onFocus(id, value)}
      >
        {enumOptions.map((opt: { value: unknown; label: string }) => (
          <option
            key={String(opt.value)}
            value={String(opt.value)}
            disabled={disabledSet.has(opt.value)}
          >
            {opt.label}
          </option>
        ))}
      </select>
    )
  }

  return (
    <select
      id={id}
      disabled={disabled || readonly}
      className={invalid ? fieldInputInvalidClass : fieldInputClass}
      value={value == null ? '' : String(value)}
      onChange={(e) => {
        const next = e.target.value
        const matched = enumOptions.find(
          (opt: { value: unknown }) => String(opt.value) === next,
        )
        onChange(matched ? matched.value : next === '' ? undefined : next)
      }}
      onBlur={() => onBlur(id, value)}
      onFocus={() => onFocus(id, value)}
    >
      <option value="">{placeholder || 'Chọn…'}</option>
      {enumOptions.map((opt: { value: unknown; label: string }) => (
        <option
          key={String(opt.value)}
          value={String(opt.value)}
          disabled={disabledSet.has(opt.value)}
        >
          {opt.label}
        </option>
      ))}
    </select>
  )
}

function CheckboxWidget(props: WidgetProps) {
  const { id, value, disabled, readonly, onChange, label, schema } = props
  return (
    <label
      className="kit-df-rjsf-checkbox"
      htmlFor={id}
      onClick={(e) => e.stopPropagation()}
    >
      <input
        id={id}
        type="checkbox"
        className="h-4 w-4 rounded border-slate-300 text-teal-700 focus:ring-teal-600/30"
        checked={Boolean(value)}
        disabled={disabled || readonly}
        onClick={(e) => e.stopPropagation()}
        onKeyDown={(e) => {
          // Enter on a focused checkbox would implicitly submit the parent <form>.
          if (e.key === 'Enter') {
            e.preventDefault()
            e.stopPropagation()
          }
        }}
        onChange={(e) => {
          e.stopPropagation()
          onChange(e.target.checked)
        }}
      />
      <span>{label || schema.title}</span>
    </label>
  )
}

function CheckboxesWidget(props: WidgetProps) {
  const { id, options, value, disabled, readonly, onChange } = props
  const { enumOptions = [], enumDisabled = [] } = options
  const selected: unknown[] = Array.isArray(value) ? value : []
  const disabledSet = new Set(enumDisabled as unknown[])

  return (
    <div className="kit-df-rjsf-checkboxes" id={id}>
      {enumOptions.map((opt: { value: unknown; label: string }, index: number) => {
        const checked = selected.includes(opt.value)
        const itemId = `${id}_${index}`
        return (
          <label key={itemId} className="kit-df-rjsf-checkbox" htmlFor={itemId}>
            <input
              id={itemId}
              type="checkbox"
              className="h-4 w-4 rounded border-slate-300 text-teal-700 focus:ring-teal-600/30"
              checked={checked}
              disabled={disabled || readonly || disabledSet.has(opt.value)}
              onChange={(e) => {
                if (e.target.checked) onChange([...selected, opt.value])
                else onChange(selected.filter((v) => v !== opt.value))
              }}
            />
            <span>{opt.label}</span>
          </label>
        )
      })}
    </div>
  )
}

function HiddenWidget(props: WidgetProps) {
  const { id, value } = props
  return (
    <input type="hidden" id={id} value={value == null ? '' : String(value)} />
  )
}

function RadioWidget(props: WidgetProps) {
  const { id, options, value, disabled, readonly, onChange, required } = props
  const { enumOptions = [], enumDisabled = [] } = options
  const disabledSet = new Set(enumDisabled as unknown[])

  return (
    <div className="kit-df-rjsf-radios" id={id} role="radiogroup">
      {enumOptions.map((opt: { value: unknown; label: string }, index: number) => {
        const itemId = `${id}_${index}`
        return (
          <label key={itemId} className="kit-df-rjsf-radio" htmlFor={itemId}>
            <input
              id={itemId}
              type="radio"
              name={id}
              required={required && index === 0}
              className="h-4 w-4 border-slate-300 text-teal-700 focus:ring-teal-600/30"
              checked={value === opt.value}
              disabled={disabled || readonly || disabledSet.has(opt.value)}
              onClick={(e) => e.stopPropagation()}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.preventDefault()
                  e.stopPropagation()
                }
              }}
              onChange={() => onChange(opt.value)}
            />
            <span>{opt.label}</span>
          </label>
        )
      })}
    </div>
  )
}

function ColorWidget(props: WidgetProps) {
  return <BaseInput {...props} type="color" />
}

function FileWidget(props: WidgetProps) {
  const { id, disabled, readonly, onChange, options } = props
  return (
    <input
      id={id}
      type="file"
      disabled={disabled || readonly}
      multiple={Boolean(options.multiple)}
      className={fieldInputClass}
      onChange={(e) => {
        const files = e.target.files
        if (!files?.length) {
          onChange(options.emptyValue)
          return
        }
        const reader = new FileReader()
        reader.onload = () => onChange(reader.result)
        reader.readAsDataURL(files[0])
      }}
    />
  )
}

/** Widgets RJSF styled theo kit (PrimeReact Input + native select/checkbox). */
export const dynamicFormWidgets: RegistryWidgetsType = {
  TextWidget,
  PasswordWidget,
  EmailWidget,
  URLWidget,
  DateWidget,
  DateTimeWidget,
  TimeWidget,
  TextareaWidget,
  UpDownWidget,
  RangeWidget,
  SelectWidget,
  CheckboxWidget,
  CheckboxesWidget,
  RadioWidget,
  HiddenWidget,
  ColorWidget,
  FileWidget,
  text: TextWidget,
  password: PasswordWidget,
  email: EmailWidget,
  url: URLWidget,
  date: DateWidget,
  'datetime-local': DateTimeWidget,
  time: TimeWidget,
  textarea: TextareaWidget,
  updown: UpDownWidget,
  range: RangeWidget,
  select: SelectWidget,
  checkbox: CheckboxWidget,
  checkboxes: CheckboxesWidget,
  radio: RadioWidget,
  hidden: HiddenWidget,
  color: ColorWidget,
  file: FileWidget,
}
