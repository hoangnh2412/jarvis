import { Label } from 'primereact/label'
import type { DocumentField } from '../../types'
import { DocumentFieldInput } from '../DocumentDataForm/DocumentFieldInput'

export type FilledPreviewPanelProps = {
  fields: DocumentField[]
  onFieldChange: (fieldName: string, value: string) => void
}

export function FilledPreviewPanel({
  fields,
  onFieldChange,
}: FilledPreviewPanelProps) {
  return (
    <div className="border-t border-slate-200 p-4">
      <div className="mb-3">
        <p className="text-xs font-semibold uppercase tracking-[0.14em] text-slate-500">
          Filled Preview Data
        </p>
        <p className="mt-1 text-xs text-slate-500">
          Edit values used to generate the filled document preview.
        </p>
      </div>

      <div className="space-y-3">
        {fields.map((field) => {
          const fieldId = `craft-doc-filled-${field.id}`

          return (
            <div key={field.id} className="space-y-1">
              <Label htmlFor={fieldId} className="text-xs font-medium text-slate-600">
                {field.label}
              </Label>
              <DocumentFieldInput
                field={field}
                value={field.value ?? ''}
                onChange={(value) => onFieldChange(field.name, value)}
              />
            </div>
          )
        })}
      </div>
    </div>
  )
}
