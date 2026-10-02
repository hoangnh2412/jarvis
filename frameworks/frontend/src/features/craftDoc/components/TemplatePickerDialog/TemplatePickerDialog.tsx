import { FileText, Upload } from 'lucide-react'
import { FeatureDialog } from '../../../../common/FeatureDialog'
import type { DocumentTemplate } from '../../types'

export type TemplatePickerDialogProps = {
  open: boolean
  onClose: () => void
  templates: DocumentTemplate[]
  activeTemplateId: string | null
  onSelect: (templateId: string) => void
  onUpload: () => void
  onCreateBlank: () => void
}

export function TemplatePickerDialog({
  open,
  onClose,
  templates,
  activeTemplateId,
  onSelect,
  onUpload,
  onCreateBlank,
}: TemplatePickerDialogProps) {
  return (
    <FeatureDialog
      open={open}
      onClose={onClose}
      title="Chọn mẫu tài liệu"
      description="Chọn mẫu có sẵn, tải lên file DOCX hoặc tạo mẫu mới."
      hideFooter
      size="lg"
      popupClassName="craft-doc-dialog"
    >
      <div className="mb-4 flex flex-wrap gap-2">
        <button
          type="button"
          onClick={onUpload}
          className="inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
        >
          <Upload className="h-4 w-4" />
          Tải mẫu DOCX
        </button>
        <button
          type="button"
          onClick={onCreateBlank}
          className="inline-flex items-center gap-2 rounded-xl border border-blue-200 bg-blue-50 px-4 py-2.5 text-sm font-medium text-blue-700 hover:bg-blue-100"
        >
          Tạo mẫu trống
        </button>
      </div>

      <div className="grid gap-2 sm:grid-cols-2">
        {templates.map((template) => {
          const active = template.id === activeTemplateId
          return (
            <button
              key={template.id}
              type="button"
              onClick={() => {
                onSelect(template.id)
                onClose()
              }}
              className={`flex items-start gap-3 rounded-2xl border px-4 py-3 text-left transition ${
                active
                  ? 'border-blue-300 bg-blue-50 shadow-sm'
                  : 'border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50'
              }`}
            >
              <span className="inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-slate-100 text-slate-600">
                <FileText className="h-5 w-5" />
              </span>
              <span>
                <span className="block text-sm font-semibold text-slate-900">{template.name}</span>
                <span className="block text-xs text-slate-500">{template.fileName}</span>
              </span>
            </button>
          )
        })}
      </div>
    </FeatureDialog>
  )
}
