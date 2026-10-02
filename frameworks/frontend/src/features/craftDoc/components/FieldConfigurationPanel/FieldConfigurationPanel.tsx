import { Trash2 } from 'lucide-react'
import { Button } from 'primereact/button'
import { InputText } from 'primereact/inputtext'
import { Label } from 'primereact/label'
import { Textarea } from 'primereact/textarea'
import { ToggleSwitch } from 'primereact/toggleswitch'
import { FIELD_TYPE_OPTIONS } from '../../types'
import type { DocumentField, UpdateDocumentFieldPayload } from '../../types'
import { CraftDocSelect } from '../CraftDocSelect'
import {
  btnOutlinedClass,
  fieldInputClass,
  fieldInputMonoClass,
  fieldLabelClass,
  fieldTextareaClass,
} from '../fieldStyles'

export type FieldConfigurationPanelProps = {
  field: DocumentField | null
  collapsed: boolean
  onToggleCollapse: () => void
  onChange: (fieldId: string, patch: UpdateDocumentFieldPayload) => void
  onDelete: (fieldId: string) => void
  embedded?: boolean
}

function FieldConfigurationContent({
  field,
  onChange,
  onDelete,
}: {
  field: DocumentField | null
  onChange: (fieldId: string, patch: UpdateDocumentFieldPayload) => void
  onDelete: (fieldId: string) => void
}) {
  if (!field) {
    return (
      <div className="text-sm text-slate-500">
        Chọn một field trong danh sách để cấu hình.
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="space-y-1.5">
        <Label htmlFor="craft-doc-field-name" className={fieldLabelClass}>
          Field name
        </Label>
        <InputText
          id="craft-doc-field-name"
          value={field.name}
          unstyled
          className={fieldInputClass}
          onChange={(event: { target: { value: string } }) =>
            onChange(field.id, { name: event.target.value })
          }
        />
      </div>

      <div className="space-y-1.5">
        <Label htmlFor="craft-doc-field-label" className={fieldLabelClass}>
          Label
        </Label>
        <InputText
          id="craft-doc-field-label"
          value={field.label}
          unstyled
          className={fieldInputClass}
          onChange={(event: { target: { value: string } }) =>
            onChange(field.id, { label: event.target.value })
          }
        />
      </div>

      <div className="space-y-1.5">
        <Label htmlFor="craft-doc-field-type" className={fieldLabelClass}>
          Type
        </Label>
        <CraftDocSelect
          id="craft-doc-field-type"
          value={field.type}
          options={FIELD_TYPE_OPTIONS}
          onChange={(value) => onChange(field.id, { type: value })}
        />
      </div>

      <div className="space-y-1.5">
        <Label htmlFor="craft-doc-field-placeholder" className={fieldLabelClass}>
          Placeholder
        </Label>
        <InputText
          id="craft-doc-field-placeholder"
          value={field.placeholder}
          unstyled
          className={fieldInputMonoClass}
          onChange={(event: { target: { value: string } }) =>
            onChange(field.id, { placeholder: event.target.value })
          }
        />
      </div>

      <div className="flex items-center gap-2">
        <ToggleSwitch.Root
          checked={field.required}
          onCheckedChange={(event: { checked: boolean }) =>
            onChange(field.id, { required: event.checked })
          }
        >
          <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-5 w-9 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-blue-600">
            <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-4 w-4 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-4" />
          </ToggleSwitch.Control>
        </ToggleSwitch.Root>
        <Label className="text-sm text-slate-700">Required</Label>
      </div>

      <div className="space-y-1.5">
        <Label htmlFor="craft-doc-field-default" className={fieldLabelClass}>
          Default value
        </Label>
        <InputText
          id="craft-doc-field-default"
          value={field.defaultValue ?? ''}
          unstyled
          className={fieldInputClass}
          onChange={(event: { target: { value: string } }) =>
            onChange(field.id, { defaultValue: event.target.value })
          }
        />
      </div>

      <div className="space-y-1.5">
        <Label htmlFor="craft-doc-field-description" className={fieldLabelClass}>
          Description
        </Label>
        <Textarea
          id="craft-doc-field-description"
          value={field.description ?? ''}
          rows={4}
          unstyled
          className={fieldTextareaClass}
          onChange={(event: { target: { value: string } }) =>
            onChange(field.id, { description: event.target.value })
          }
        />
      </div>

      <Button
        type="button"
        unstyled
        className={`${btnOutlinedClass} h-10 w-full border-red-200 text-red-700 hover:bg-red-50`}
        onClick={() => onDelete(field.id)}
      >
        <Trash2 className="h-4 w-4" />
        Delete Field
      </Button>
    </div>
  )
}

export function FieldConfigurationPanel({
  field,
  collapsed,
  onToggleCollapse,
  onChange,
  onDelete,
  embedded = false,
}: FieldConfigurationPanelProps) {
  if (embedded) {
    return <FieldConfigurationContent field={field} onChange={onChange} onDelete={onDelete} />
  }

  if (collapsed) {
    return (
      <aside className="craft-doc-sidebar w-12 shrink-0 border-l border-slate-200 bg-slate-50">
        <button
          type="button"
          onClick={onToggleCollapse}
          className="flex h-full w-full items-start justify-center pt-3 text-xs font-semibold text-slate-500"
        >
          CFG
        </button>
      </aside>
    )
  }

  return (
    <aside className="craft-doc-sidebar w-[320px] shrink-0 border-l border-slate-200 bg-slate-50">
      <div className="flex items-center justify-between border-b border-slate-200 px-4 py-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.14em] text-slate-500">
            Field Configuration
          </p>
          <h3 className="text-sm font-semibold text-slate-900">
            {field?.label ?? 'Select a field'}
          </h3>
        </div>
        <button
          type="button"
          onClick={onToggleCollapse}
          className="text-xs font-medium text-slate-500 hover:text-slate-800"
        >
          Hide
        </button>
      </div>
      <div className="overflow-auto p-4">
        <FieldConfigurationContent field={field} onChange={onChange} onDelete={onDelete} />
      </div>
    </aside>
  )
}
