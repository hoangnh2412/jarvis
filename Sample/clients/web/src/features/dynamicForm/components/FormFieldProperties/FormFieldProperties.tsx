import { useEffect, useRef, useState } from 'react'
import {
  draggable,
  dropTargetForElements,
} from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import {
  ChevronDown,
  GripVertical,
  Info,
  Minus,
  Plus,
  X,
} from 'lucide-react'
import { Button } from 'primereact/button'
import { InputText } from 'primereact/inputtext'
import { Label } from 'primereact/label'
import type { DynamicFormField, DynamicFormSelectOption } from '../../types'
import {
  createSelectOption,
  normalizeSelectOptions,
} from '../../utils'
import { btnOutlinedClass, fieldInputClass } from '../fieldStyles'

export type FormFieldPropertiesLabels = {
  title: string
  fieldLabel: string
  fieldKey: string
  fieldRequired: string
  fieldPlaceholder: string
  fieldHelp: string
  fieldDefault: string
  fieldDefaultNone: string
  fieldOptions: string
  optionSetup: string
  optionValue: string
  addOption: string
  removeOption: string
  quickEdit: string
  quickEditHint: string
  sortAlpha: string
  sortAlphaHint: string
  remindEmpty: string
  remindEmptyHint: string
  removeField: string
  close?: string
}

export type FormFieldPropertiesProps = {
  title?: string
  field: DynamicFormField | null
  labels: FormFieldPropertiesLabels
  onChange: (next: DynamicFormField) => void
  onRemove: () => void
  onClose?: () => void
}

function PropCheck({
  id,
  checked,
  label,
  hint,
  danger,
  onChange,
}: {
  id: string
  checked: boolean
  label: string
  hint?: string
  danger?: boolean
  onChange: (next: boolean) => void
}) {
  return (
    <div className="kit-df-prop-check flex items-start gap-2">
      <input
        id={id}
        type="checkbox"
        className="mt-0.5 h-4 w-4 shrink-0 rounded border-slate-300 text-teal-700 focus:ring-teal-500/30"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
      />
      <div className="min-w-0 flex-1">
        <Label
          htmlFor={id}
          className={[
            'inline-flex items-center gap-1 text-sm font-medium',
            danger && checked ? 'text-red-600' : 'text-slate-700',
          ].join(' ')}
        >
          {label}
          {hint ? (
            <span title={hint} className="inline-flex text-slate-400">
              <Info className="h-3.5 w-3.5" aria-hidden />
            </span>
          ) : null}
        </Label>
      </div>
    </div>
  )
}

function OptionRow({
  fieldId,
  option,
  index,
  showValue,
  labels,
  onPatch,
  onRemove,
  onReorder,
}: {
  fieldId: string
  option: DynamicFormSelectOption
  index: number
  showValue: boolean
  labels: FormFieldPropertiesLabels
  onPatch: (patch: Partial<DynamicFormSelectOption>) => void
  onRemove: () => void
  onReorder: (from: number, to: number) => void
}) {
  const ref = useRef<HTMLDivElement>(null)
  const handleRef = useRef<HTMLButtonElement>(null)
  useEffect(() => {
    const el = ref.current
    const handle = handleRef.current
    if (!el || !handle) return
    return draggable({
      element: el,
      dragHandle: handle,
      getInitialData: () => ({
        source: 'select-option',
        fieldId,
        index,
      }),
    })
  }, [fieldId, index])
  useEffect(() => {
    const el = ref.current
    if (!el) return
    return dropTargetForElements({
      element: el,
      getData: () => ({ target: 'select-option', index }),
      onDrop: ({ source }) => {
        const data = source.data as {
          source?: string
          fieldId?: string
          index?: number
        }
        if (
          data.source === 'select-option' &&
          data.fieldId === fieldId &&
          typeof data.index === 'number' &&
          data.index !== index
        ) {
          onReorder(data.index, index)
        }
      },
    })
  }, [fieldId, index, onReorder])
  return (
    <div
      ref={ref}
      className="kit-df-option-row flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white p-1.5"
      data-option-index={index}
    >
      <input
        type="checkbox"
        className="h-4 w-4 shrink-0 rounded border-slate-300 text-teal-700"
        checked={option.enabled !== false}
        title="Hiện option"
        onChange={(e) => onPatch({ enabled: e.target.checked })}
      />
      <div className="min-w-0 flex-1 space-y-1">
        <InputText
          unstyled
          className="h-8 w-full rounded-md border border-slate-200 bg-slate-50 px-2 text-sm text-slate-800 outline-none transition hover:border-slate-300 focus:border-teal-500 focus:bg-white focus:ring-2 focus:ring-teal-500/15"
          value={option.label}
          placeholder="Nhãn"
          onChange={(e: { target: { value: string } }) => {
            const label = e.target.value
            onPatch({
              label,
              ...(showValue
                ? {}
                : {
                    value: label
                      .trim()
                      .toLowerCase()
                      .replace(/\s+/g, '_')
                      .replace(/[^\w\-]/g, ''),
                  }),
            })
          }}
        />
        {showValue ? (
          <InputText
            unstyled
            className="h-7 w-full rounded-md border border-dashed border-slate-200 bg-white px-2 text-xs text-slate-600 outline-none focus:border-teal-500"
            value={option.value}
            placeholder={labels.optionValue}
            onChange={(e: { target: { value: string } }) =>
              onPatch({ value: e.target.value.replace(/\s+/g, '_') })
            }
          />
        ) : null}
      </div>
      <button
        type="button"
        className="inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-md text-slate-400 transition hover:bg-red-50 hover:text-red-600"
        aria-label={labels.removeOption}
        title={labels.removeOption}
        onClick={onRemove}
      >
        <Minus className="h-4 w-4" />
      </button>
      <button
        ref={handleRef}
        type="button"
        className="inline-flex h-7 w-7 shrink-0 cursor-grab items-center justify-center rounded-md text-slate-400 transition hover:bg-slate-100 hover:text-slate-600 active:cursor-grabbing"
        aria-label="Kéo sắp xếp"
      >
        <GripVertical className="h-4 w-4" />
      </button>
    </div>
  )
}

function SelectOptionsEditor({
  field,
  labels,
  onChange,
}: {
  field: DynamicFormField
  labels: FormFieldPropertiesLabels
  onChange: (next: DynamicFormField) => void
}) {
  const [showSetup, setShowSetup] = useState(false)
  const options = normalizeSelectOptions(field.options)
  const setOptions = (next: DynamicFormSelectOption[]) => {
    onChange({ ...field, options: next })
  }
  const reorder = (from: number, to: number) => {
    const next = [...options]
    const [item] = next.splice(from, 1)
    next.splice(to, 0, item)
    setOptions(next)
  }
  return (
    <div className="kit-df-select-editor flex flex-col gap-2.5">
      <div className="flex items-center justify-between gap-2">
        <span className="text-xs font-semibold text-slate-700">
          {labels.fieldOptions}
        </span>
        <button
          type="button"
          className="text-xs font-medium text-teal-700 transition hover:text-teal-800 hover:underline"
          onClick={() => setShowSetup((v) => !v)}
        >
          {labels.optionSetup}
          {showSetup ? ' ▴' : ' ▾'}
        </button>
      </div>
      <div className="flex max-h-56 flex-col gap-1.5 overflow-y-auto pr-0.5">
        {options.map((option, index) => (
          <OptionRow
            key={option.id ?? `opt-${index}`}
            fieldId={field.id}
            option={option}
            index={index}
            showValue={showSetup}
            labels={labels}
            onPatch={(patch) => {
              const next = [...options]
              next[index] = { ...next[index], ...patch }
              setOptions(next)
            }}
            onRemove={() => setOptions(options.filter((_, i) => i !== index))}
            onReorder={reorder}
          />
        ))}
      </div>
      <button
        type="button"
        className="inline-flex h-8 items-center justify-center gap-1 rounded-lg border border-dashed border-slate-300 text-xs font-medium text-slate-600 transition hover:border-teal-400 hover:bg-teal-50/50 hover:text-teal-800"
        onClick={() =>
          setOptions([
            ...options,
            createSelectOption(`Tuỳ chọn ${options.length + 1}`),
          ])
        }
      >
        <Plus className="h-3.5 w-3.5" />
        {labels.addOption}
      </button>
      <div className="flex flex-col gap-2 border-t border-slate-100 pt-2.5">
        <PropCheck
          id={`qe-${field.id}`}
          checked={Boolean(field.selectConfig?.quickEdit)}
          label={labels.quickEdit}
          hint={labels.quickEditHint}
          onChange={(quickEdit) =>
            onChange({
              ...field,
              selectConfig: { ...field.selectConfig, quickEdit },
            })
          }
        />
        <PropCheck
          id={`sa-${field.id}`}
          checked={Boolean(field.selectConfig?.sortAlphabetical)}
          label={labels.sortAlpha}
          hint={labels.sortAlphaHint}
          onChange={(sortAlphabetical) =>
            onChange({
              ...field,
              selectConfig: { ...field.selectConfig, sortAlphabetical },
            })
          }
        />
      </div>
    </div>
  )
}

function DefaultValueControl({
  field,
  labels,
  onChange,
}: {
  field: DynamicFormField
  labels: FormFieldPropertiesLabels
  onChange: (next: DynamicFormField) => void
}) {
  if (field.type === 'checkbox') {
    return (
      <PropCheck
        id={`def-${field.id}`}
        checked={Boolean(field.defaultValue)}
        label={labels.fieldDefault}
        onChange={(defaultValue) => onChange({ ...field, defaultValue })}
      />
    )
  }
  if (field.type === 'select') {
    const options = normalizeSelectOptions(field.options).filter(
      (o) => o.enabled !== false,
    )
    return (
      <div>
        <Label className="mb-1 block text-xs font-medium text-slate-600">
          {labels.fieldDefault}
        </Label>
        <div className="relative">
          <select
            className="h-9 w-full appearance-none rounded-lg border border-slate-200 bg-white px-3 pr-8 text-sm text-slate-800 outline-none transition hover:border-slate-300 focus:border-teal-500 focus:ring-2 focus:ring-teal-500/15"
            value={String(field.defaultValue ?? '')}
            onChange={(e) =>
              onChange({ ...field, defaultValue: e.target.value })
            }
          >
            <option value="">{labels.fieldDefaultNone}</option>
            {options.map((option) => (
              <option key={option.id ?? option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
          <ChevronDown className="pointer-events-none absolute right-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
        </div>
      </div>
    )
  }
  return (
    <div>
      <Label className="mb-1 block text-xs font-medium text-slate-600">
        {labels.fieldDefault}
      </Label>
      <InputText
        unstyled
        type={field.type === 'number' ? 'number' : field.type === 'date' ? 'date' : 'text'}
        className={fieldInputClass}
        value={String(field.defaultValue ?? '')}
        onChange={(e: { target: { value: string } }) =>
          onChange({
            ...field,
            defaultValue:
              field.type === 'number'
                ? e.target.value === ''
                  ? ''
                  : Number(e.target.value)
                : e.target.value,
          })
        }
      />
    </div>
  )
}

export function FormFieldProperties({
  title,
  field,
  labels,
  onChange,
  onRemove,
  onClose,
}: FormFieldPropertiesProps) {
  if (!field) return null
  const heading = title ?? labels.title
  return (
    <aside
      className="kit-df-props kit-dynamic-form-props flex max-h-full flex-col gap-3 overflow-y-auto rounded-[0.875rem] border border-slate-200 bg-white p-3.5 shadow-sm"
      aria-label={heading}
    >
      <div className="kit-df-props-header flex items-center justify-between gap-2">
        <p className="m-0 text-sm font-semibold text-slate-800">{heading}</p>
        {onClose ? (
          <button
            type="button"
            className="kit-df-props-close inline-flex h-7 w-7 items-center justify-center rounded-lg text-slate-400 transition hover:bg-slate-100 hover:text-slate-700"
            aria-label={labels.close ?? 'Đóng'}
            title={labels.close ?? 'Đóng'}
            onClick={onClose}
          >
            <X className="h-4 w-4" />
          </button>
        ) : null}
      </div>
      <div>
        <Label className="mb-1 block text-xs font-medium text-slate-600">
          {labels.fieldLabel}
          <span className="text-red-600"> *</span>
        </Label>
        <InputText
          unstyled
          className={`${fieldInputClass} bg-slate-50`}
          value={field.label}
          onChange={(e: { target: { value: string } }) =>
            onChange({ ...field, label: e.target.value })
          }
        />
      </div>
      <div>
        <Label className="mb-1 block text-xs font-medium text-slate-600">
          {labels.fieldKey}
        </Label>
        <InputText
          unstyled
          className={fieldInputClass}
          value={field.key}
          onChange={(e: { target: { value: string } }) =>
            onChange({
              ...field,
              key: e.target.value.replace(/\s+/g, '_'),
            })
          }
        />
      </div>
      {field.type !== 'checkbox' ? (
        <div>
          <Label className="mb-1 block text-xs font-medium text-slate-600">
            {labels.fieldPlaceholder}
          </Label>
          <InputText
            unstyled
            className={fieldInputClass}
            value={field.placeholder ?? ''}
            onChange={(e: { target: { value: string } }) =>
              onChange({ ...field, placeholder: e.target.value })
            }
          />
        </div>
      ) : null}
      {field.type === 'select' ? (
        <SelectOptionsEditor
          field={field}
          labels={labels}
          onChange={onChange}
        />
      ) : null}
      <DefaultValueControl
        field={field}
        labels={labels}
        onChange={onChange}
      />
      <div>
        <Label className="mb-1 block text-xs font-medium text-slate-600">
          {labels.fieldHelp}
        </Label>
        <InputText
          unstyled
          className={fieldInputClass}
          value={field.helpText ?? ''}
          onChange={(e: { target: { value: string } }) =>
            onChange({ ...field, helpText: e.target.value })
          }
        />
      </div>
      <div className="flex flex-col gap-2.5 rounded-lg border border-slate-100 bg-slate-50/80 px-2.5 py-2.5">
        <PropCheck
          id={`req-${field.id}`}
          checked={Boolean(field.required)}
          label={labels.fieldRequired}
          danger
          onChange={(required) => onChange({ ...field, required })}
        />
        <PropCheck
          id={`rem-${field.id}`}
          checked={Boolean(field.remindWhenEmpty)}
          label={labels.remindEmpty}
          hint={labels.remindEmptyHint}
          onChange={(remindWhenEmpty) =>
            onChange({ ...field, remindWhenEmpty })
          }
        />
      </div>
      <Button
        type="button"
        unstyled
        className={`${btnOutlinedClass} !border-red-200 !text-red-600 hover:!bg-red-50`}
        onClick={onRemove}
      >
        {labels.removeField}
      </Button>
    </aside>
  )
}
