import { FileText, Plus, Upload } from 'lucide-react'
import type { DocumentTemplate } from '../../types'

export type TemplateListProps = {
  templates: DocumentTemplate[]
  activeTemplateId: string | null
  onSelect: (templateId: string) => void
  onUpload: () => void
}

export function TemplateList({
  templates,
  activeTemplateId,
  onSelect,
  onUpload,
}: TemplateListProps) {
  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between gap-2">
        <h3 className="text-xs font-semibold uppercase tracking-[0.14em] text-slate-500">
          Templates
        </h3>
        <button
          type="button"
          onClick={onUpload}
          className="inline-flex h-8 items-center gap-1 rounded-lg border border-slate-200 bg-white px-2.5 text-xs font-medium text-slate-700 transition hover:border-teal-300 hover:text-teal-700"
        >
          <Upload className="h-3.5 w-3.5" />
          Upload
        </button>
      </div>

      <div className="space-y-1.5">
        {templates.map((template) => {
          const active = template.id === activeTemplateId
          return (
            <button
              key={template.id}
              type="button"
              onClick={() => onSelect(template.id)}
              className={`flex w-full items-start gap-2 rounded-xl border px-3 py-2.5 text-left transition ${
                active
                  ? 'border-teal-300 bg-teal-50 text-teal-900 shadow-sm'
                  : 'border-transparent bg-white text-slate-700 hover:border-slate-200 hover:bg-slate-50'
              }`}
            >
              <FileText className="mt-0.5 h-4 w-4 shrink-0" />
              <span className="min-w-0">
                <span className="block truncate text-sm font-medium">{template.name}</span>
                <span className="block truncate text-xs text-slate-500">{template.fileName}</span>
              </span>
            </button>
          )
        })}
      </div>

      <button
        type="button"
        onClick={onUpload}
        className="inline-flex w-full items-center justify-center gap-2 rounded-xl border border-dashed border-slate-300 bg-white px-3 py-2.5 text-sm font-medium text-slate-700 transition hover:border-teal-300 hover:bg-teal-50 hover:text-teal-800"
      >
        <Plus className="h-4 w-4" />
        New Template
      </button>
    </div>
  )
}
