import { PanelLeftClose, PanelLeftOpen } from 'lucide-react'
import type { DocumentField, DocumentTemplate, FieldTypeValue } from '../../types'
import { FieldList } from './FieldList'
import { TemplateList } from './TemplateList'

export type TemplateSidebarProps = {
  collapsed: boolean
  onToggleCollapse: () => void
  templates: DocumentTemplate[]
  activeTemplateId: string | null
  fields: DocumentField[]
  selectedFieldId: string | null
  fieldSearch: string
  fieldTypeFilter: FieldTypeValue | 'all'
  onTemplateSelect: (templateId: string) => void
  onFieldSelect: (fieldId: string) => void
  onFieldSearchChange: (value: string) => void
  onFieldTypeFilterChange: (value: FieldTypeValue | 'all') => void
  onUploadTemplate: () => void
  onAddField: () => void
}

export function TemplateSidebar({
  collapsed,
  onToggleCollapse,
  templates,
  activeTemplateId,
  fields,
  selectedFieldId,
  fieldSearch,
  fieldTypeFilter,
  onTemplateSelect,
  onFieldSelect,
  onFieldSearchChange,
  onFieldTypeFilterChange,
  onUploadTemplate,
  onAddField,
}: TemplateSidebarProps) {
  return (
    <aside
      className={`craft-doc-sidebar flex shrink-0 flex-col border-r ${collapsed ? 'is-collapsed w-12' : 'w-[300px]'} transition-[width] duration-200`}
    >
      <div className="flex items-center justify-between border-b border-slate-200 px-3 py-2">
        {!collapsed ? (
          <span className="text-sm font-semibold text-slate-800">Template Builder</span>
        ) : null}
        <button
          type="button"
          onClick={onToggleCollapse}
          className="inline-flex h-8 w-8 items-center justify-center rounded-lg text-slate-500 transition hover:bg-white hover:text-slate-800"
          aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {collapsed ? <PanelLeftOpen className="h-4 w-4" /> : <PanelLeftClose className="h-4 w-4" />}
        </button>
      </div>

      {!collapsed ? (
        <div className="min-h-0 flex-1 space-y-6 overflow-auto p-4">
          <TemplateList
            templates={templates}
            activeTemplateId={activeTemplateId}
            onSelect={onTemplateSelect}
            onUpload={onUploadTemplate}
          />
          <FieldList
            fields={fields}
            selectedFieldId={selectedFieldId}
            search={fieldSearch}
            typeFilter={fieldTypeFilter}
            onSearchChange={onFieldSearchChange}
            onTypeFilterChange={onFieldTypeFilterChange}
            onSelect={onFieldSelect}
            onAdd={onAddField}
          />
        </div>
      ) : null}
    </aside>
  )
}
