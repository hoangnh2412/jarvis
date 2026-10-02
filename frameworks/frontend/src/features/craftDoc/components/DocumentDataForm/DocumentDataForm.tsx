import {
  Download,
  FileUp,
  Plus,
  Save,
  Settings2,
} from 'lucide-react'
import { useMemo, useState } from 'react'
import { Button } from 'primereact/button'
import { Label } from 'primereact/label'
import { Message } from 'primereact/message'
import { getCraftDocMessages, type CraftDocLocale } from '../../localization'
import type { CraftDocFormPanelProps } from '../../types'
import type { DocumentField } from '../../types'
import {
  compositeDatePartsToUpdates,
  groupFormFieldItems,
  validateFormFieldItems,
} from '../../utils/compositeDateFields'
import {
  btnAccentOutlinedClass,
  btnCompactOutlinedClass,
  btnLinkClass,
  btnOutlinedClass,
  btnPrimaryClass,
} from '../fieldStyles'
import { DocumentFieldInput } from './DocumentFieldInput'
import { CompositeDateFieldInput } from './CompositeDateFieldInput'

export type DocumentDataFormProps = CraftDocFormPanelProps

export function DocumentDataForm({
  locale = 'vi',
  template,
  saving = false,
  onFieldChange,
  onFieldValuesChange,
  onUpload,
  onCreateTemplate,
  onOpenDesign,
  onOpenTemplatePicker,
  onSave,
  onExport,
}: DocumentDataFormProps) {
  const messages = getCraftDocMessages(locale)
  const [showValidation, setShowValidation] = useState(false)
  const fields = template?.fields ?? []

  const groupedFields = useMemo(() => {
    if (!template) return []
    return groupFormFieldItems(fields, template.id)
  }, [fields, template])

  const formFieldItems = useMemo(
    () => groupedFields.flatMap((group) => group.items),
    [groupedFields],
  )

  const validationErrors = useMemo(
    () => validateFormFieldItems(formFieldItems),
    [formFieldItems],
  )

  const handleFieldChange = (field: DocumentField, value: string) => {
    onFieldChange(field.name, value)
  }

  const handleCompositeDateChange = (
    composite: Parameters<typeof compositeDatePartsToUpdates>[0],
    formDateValue: string,
  ) => {
    const updates = compositeDatePartsToUpdates(composite, formDateValue)
    if (onFieldValuesChange) {
      onFieldValuesChange(updates)
      return
    }
    for (const [name, value] of Object.entries(updates)) {
      onFieldChange(name, value)
    }
  }

  const handleSave = () => {
    setShowValidation(true)
    if (validationErrors.length) return
    onSave()
  }

  return (
    <aside className="craft-doc-form-panel flex h-full min-h-0 flex-col overflow-hidden border border-slate-200 bg-white">
      <div className="shrink-0 border-b border-slate-200 px-3 py-2.5">
        <div className="flex items-center justify-between gap-2">
          <h1 className="text-sm font-semibold text-slate-900">
            {template?.name ?? messages.form.title}
          </h1>
          <Button
            type="button"
            unstyled
            className={btnLinkClass}
            onClick={onOpenTemplatePicker}
          >
            {messages.form.changeTemplate}
          </Button>
        </div>

        <div className="mt-2 flex flex-wrap gap-1">
          <Button type="button" unstyled className={btnCompactOutlinedClass} onClick={onUpload}>
            <FileUp className="h-3 w-3" />
            {messages.form.upload}
          </Button>
          <Button
            type="button"
            unstyled
            className={btnCompactOutlinedClass}
            onClick={onCreateTemplate}
          >
            <Plus className="h-3 w-3" />
            {messages.form.create}
          </Button>
          <Button type="button" unstyled className={btnAccentOutlinedClass} onClick={onOpenDesign}>
            <Settings2 className="h-3 w-3" />
            {messages.form.editTemplate}
          </Button>
        </div>
      </div>

      <div className="craft-doc-form-scroll min-h-0 flex-1 overflow-y-auto overflow-x-hidden">
        {!template ? (
          <p className="px-3 py-4 text-sm text-slate-500">{messages.form.empty}</p>
        ) : (
          groupedFields.map((group) => (
            <section key={group.id} className="border-b border-slate-200 px-3 py-3">
              <h2 className="mb-2.5 text-xs font-semibold uppercase tracking-wide text-slate-500">
                {group.label}
              </h2>
              <div className="space-y-2.5">
                {group.items.map((item) => {
                  if (item.kind === 'date-composite') {
                    const missing =
                      showValidation &&
                      item.composite.required &&
                      !item.composite.value.trim()
                    const fieldId = `craft-doc-composite-date-${item.composite.id}`

                    return (
                      <div key={item.composite.id} className="space-y-1">
                        <Label htmlFor={fieldId} className="text-xs font-medium text-slate-700">
                          {item.composite.label}
                          {item.composite.required ? (
                            <span className="text-red-500"> *</span>
                          ) : null}
                        </Label>
                        <CompositeDateFieldInput
                          composite={item.composite}
                          highlighted={missing}
                          onChange={(value) =>
                            handleCompositeDateChange(item.composite, value)
                          }
                        />
                      </div>
                    )
                  }

                  const field = item.field
                  const missing =
                    showValidation &&
                    field.required &&
                    !field.value?.trim()
                  const fieldId = `craft-doc-field-${field.id}`

                  return (
                    <div key={field.id} className="space-y-1">
                      <Label htmlFor={fieldId} className="text-xs font-medium text-slate-700">
                        {field.label}
                        {field.required ? <span className="text-red-500"> *</span> : null}
                      </Label>
                      <DocumentFieldInput
                        field={field}
                        value={field.value ?? ''}
                        onChange={(value) => handleFieldChange(field, value)}
                        highlighted={missing}
                      />
                    </div>
                  )
                })}
              </div>
            </section>
          ))
        )}
      </div>

      <div className="shrink-0 border-t border-slate-200 px-3 py-2.5">
        {showValidation && validationErrors.length ? (
          <Message.Root severity="error" className="mb-2 border-0 bg-transparent p-0">
            <Message.Content>
              <Message.Text className="text-xs text-red-700">
                {validationErrors[0]}
              </Message.Text>
            </Message.Content>
          </Message.Root>
        ) : null}

        <div className="flex gap-2">
          <Button
            type="button"
            unstyled
            className={`${btnPrimaryClass} flex-1`}
            disabled={saving || !template}
            onClick={handleSave}
          >
            <Save className="h-3.5 w-3.5" />
            {messages.form.save}
          </Button>
          <Button
            type="button"
            unstyled
            className={btnOutlinedClass}
            disabled={!template}
            onClick={onExport}
          >
            <Download className="h-3.5 w-3.5" />
            {messages.form.export}
          </Button>
        </div>
      </div>
    </aside>
  )
}
