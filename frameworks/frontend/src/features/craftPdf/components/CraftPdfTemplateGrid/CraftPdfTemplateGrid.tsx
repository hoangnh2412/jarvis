import { FilePenLine, Trash2 } from 'lucide-react'
import { Button } from 'primereact/button'
import { Tag } from 'primereact/tag'
import {
  PDF_TEMPLATE_STATUS_LABEL,
  PdfTemplateStatus,
  type PdfTemplate,
} from '../../types'
import { btnOutlinedClass, btnTextClass } from '../fieldStyles'

export type CraftPdfTemplateCardProps = {
  template: PdfTemplate
  onEdit?: (template: PdfTemplate) => void
  onDelete?: (template: PdfTemplate) => void
  editLabel?: string
  className?: string
}

export function CraftPdfTemplateCard({
  template,
  onEdit,
  onDelete,
  editLabel = 'Mở editor',
  className = '',
}: CraftPdfTemplateCardProps) {
  const severity =
    template.status === PdfTemplateStatus.Published ? 'success' : 'secondary'

  return (
    <article
      className={`flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm transition-shadow hover:shadow-md ${className}`}
    >
      <div className="flex aspect-[3/4] max-h-48 items-center justify-center bg-gradient-to-b from-slate-50 to-slate-100">
        <div className="flex h-[72%] w-[55%] flex-col gap-1 rounded-sm bg-white p-2 shadow ring-1 ring-slate-200">
          <div className="h-2 w-2/3 rounded bg-teal-700/80" />
          <div className="h-1.5 w-full rounded bg-slate-200" />
          <div className="h-1.5 w-5/6 rounded bg-slate-200" />
          <div className="mt-auto grid grid-cols-3 gap-0.5">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="h-3 rounded-sm bg-slate-100" />
            ))}
          </div>
        </div>
      </div>
      <div className="flex flex-1 flex-col gap-2 p-3.5">
        <div className="flex items-start justify-between gap-2">
          <h3 className="m-0 text-sm font-semibold text-slate-800">
            {template.name}
          </h3>
          <Tag
            value={PDF_TEMPLATE_STATUS_LABEL[template.status]}
            severity={severity}
            className="shrink-0 text-[10px]"
          />
        </div>
        {template.description && (
          <p className="m-0 line-clamp-2 text-xs leading-relaxed text-slate-500">
            {template.description}
          </p>
        )}
        <p className="m-0 text-[11px] text-slate-400">
          {template.pageSize.label} · {template.elements.length} elements ·{' '}
          {new Date(template.updatedAt).toLocaleDateString()}
        </p>
        <div className="mt-auto flex gap-2 pt-1">
          {onEdit && (
            <Button
              type="button"
              unstyled
              className={`${btnOutlinedClass} !h-9 flex-1 !text-xs`}
              onClick={() => onEdit(template)}
            >
              <FilePenLine className="size-3.5" />
              {editLabel}
            </Button>
          )}
          {onDelete && (
            <Button
              type="button"
              unstyled
              className={`${btnTextClass} !text-red-600`}
              onClick={() => onDelete(template)}
              aria-label="Delete"
            >
              <Trash2 className="size-4" />
            </Button>
          )}
        </div>
      </div>
    </article>
  )
}

export type CraftPdfTemplateGridProps = {
  items: PdfTemplate[]
  emptyLabel?: string
  onEdit?: (template: PdfTemplate) => void
  onDelete?: (template: PdfTemplate) => void
  className?: string
}

export function CraftPdfTemplateGrid({
  items,
  emptyLabel = 'Chưa có template nào.',
  onEdit,
  onDelete,
  className = '',
}: CraftPdfTemplateGridProps) {
  if (!items.length) {
    return (
      <div
        className={`rounded-xl border border-dashed border-slate-200 bg-slate-50 px-6 py-16 text-center text-sm text-slate-400 ${className}`}
      >
        {emptyLabel}
      </div>
    )
  }

  return (
    <div
      className={`grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 ${className}`}
    >
      {items.map((tpl) => (
        <CraftPdfTemplateCard
          key={tpl.id}
          template={tpl}
          onEdit={onEdit}
          onDelete={onDelete}
        />
      ))}
    </div>
  )
}
