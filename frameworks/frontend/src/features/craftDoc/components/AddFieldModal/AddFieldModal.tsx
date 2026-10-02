import { useEffect, useState } from 'react'
import { FeatureDialog } from '../../../../common/FeatureDialog'
import { InputText } from 'primereact/inputtext'
import { Label } from 'primereact/label'
import { ToggleSwitch } from 'primereact/toggleswitch'
import { FieldType, FIELD_TYPE_OPTIONS } from '../../types'
import type { CreateDocumentFieldPayload, FieldTypeValue } from '../../types'
import { CraftDocSelect } from '../CraftDocSelect'
import { fieldInputClass, fieldLabelClass } from '../fieldStyles'

export type AddFieldModalProps = {
  open: boolean
  onClose: () => void
  onSubmit: (payload: CreateDocumentFieldPayload) => Promise<void> | void
}

export function AddFieldModal({ open, onClose, onSubmit }: AddFieldModalProps) {
  const [name, setName] = useState('')
  const [label, setLabel] = useState('')
  const [type, setType] = useState<FieldTypeValue>(FieldType.Text)
  const [required, setRequired] = useState(false)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!open) return
    setName('')
    setLabel('')
    setType(FieldType.Text)
    setRequired(false)
  }, [open])

  const handleSubmit = async () => {
    if (!name.trim()) return
    setSubmitting(true)
    try {
      await onSubmit({
        name: name.trim(),
        label: label.trim(),
        type,
        required,
      })
      onClose()
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <FeatureDialog
      open={open}
      onClose={onClose}
      title="Add Field"
      description="Create a new placeholder field for the current template."
      submitLabel="Add Field"
      submitting={submitting}
      onSubmit={(event) => {
        event.preventDefault()
        void handleSubmit()
      }}
      popupClassName="craft-doc-dialog"
    >
      <div className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="craft-doc-add-field-name" className={fieldLabelClass}>
            Field name
          </Label>
          <InputText
            id="craft-doc-add-field-name"
            value={name}
            unstyled
            placeholder="employeeName"
            className={fieldInputClass}
            onChange={(event: { target: { value: string } }) => setName(event.target.value)}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="craft-doc-add-field-label" className={fieldLabelClass}>
            Label
          </Label>
          <InputText
            id="craft-doc-add-field-label"
            value={label}
            unstyled
            placeholder="Tên nhân viên"
            className={fieldInputClass}
            onChange={(event: { target: { value: string } }) => setLabel(event.target.value)}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="craft-doc-add-field-type" className={fieldLabelClass}>
            Type
          </Label>
          <CraftDocSelect
            id="craft-doc-add-field-type"
            value={type}
            options={FIELD_TYPE_OPTIONS}
            onChange={setType}
          />
        </div>

        <div className="flex items-center gap-2">
          <ToggleSwitch.Root
            checked={required}
            onCheckedChange={(event: { checked: boolean }) => setRequired(event.checked)}
          >
            <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-5 w-9 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-blue-600">
              <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-4 w-4 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-4" />
            </ToggleSwitch.Control>
          </ToggleSwitch.Root>
          <Label className="text-sm text-slate-700">Required</Label>
        </div>
      </div>
    </FeatureDialog>
  )
}
