import { X } from 'lucide-react'
import { useState } from 'react'
import { notify } from '../../../../common/Toaster'
import { getCraftDocMessages, type CraftDocLocale } from '../../localization'
import type { CraftDocTemplateDesignDrawerProps } from '../../types'
import { AddFieldModal } from '../AddFieldModal'
import { DeleteFieldDialog } from '../DeleteFieldDialog'
import { FieldConfigurationPanel } from '../FieldConfigurationPanel'
import { FieldList } from '../TemplateSidebar/FieldList'

export type TemplateDesignDrawerProps = CraftDocTemplateDesignDrawerProps

export function TemplateDesignDrawer({
  locale = 'vi',
  open,
  onClose,
  template,
  design,
}: TemplateDesignDrawerProps) {
  const messages = getCraftDocMessages(locale)
  const [addFieldOpen, setAddFieldOpen] = useState(false)
  const [deleteFieldId, setDeleteFieldId] = useState<string | null>(null)

  if (!open) return null

  const deleteFieldLabel = template?.fields.find(
    (field) => field.id === deleteFieldId,
  )?.label

  return (
    <>
      <div className="fixed inset-0 z-40 bg-slate-900/30" onClick={onClose} />
      <aside className="fixed inset-y-0 right-0 z-50 flex w-full max-w-[420px] flex-col border-l border-slate-200 bg-white shadow-2xl">
        <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
          <div>
            <h2 className="text-base font-semibold text-slate-900">
              {messages.design.title}
            </h2>
            <p className="text-sm text-slate-500">{template?.name}</p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="inline-flex h-9 w-9 items-center justify-center rounded-lg text-slate-500 hover:bg-slate-100"
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        <div className="min-h-0 flex-1 overflow-auto p-4">
          <FieldList
            fields={design.filteredFields}
            selectedFieldId={design.selectedFieldId}
            search={design.fieldSearch}
            typeFilter={design.fieldTypeFilter}
            onSearchChange={design.onSearchChange}
            onTypeFilterChange={design.onTypeFilterChange}
            onSelect={design.onSelectField}
            onAdd={() => setAddFieldOpen(true)}
          />

          <div className="mt-6 border-t border-slate-200 pt-4">
            <FieldConfigurationPanel
              field={design.selectedField}
              collapsed={false}
              onToggleCollapse={() => undefined}
              onChange={(fieldId, patch) => void design.onUpdateField(fieldId, patch)}
              onDelete={(fieldId) => setDeleteFieldId(fieldId)}
              embedded
            />
          </div>
        </div>
      </aside>

      <AddFieldModal
        open={addFieldOpen}
        onClose={() => setAddFieldOpen(false)}
        onSubmit={async (payload) => {
          await design.onAddField(payload)
          notify.success(messages.design.fieldAdded)
        }}
      />

      <DeleteFieldDialog
        open={Boolean(deleteFieldId)}
        fieldLabel={deleteFieldLabel}
        onClose={() => setDeleteFieldId(null)}
        onConfirm={async () => {
          if (!deleteFieldId) return
          await design.onDeleteField(deleteFieldId)
          setDeleteFieldId(null)
          notify.success(messages.design.fieldDeleted)
        }}
      />
    </>
  )
}
