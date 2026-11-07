import { ArrowLeft, Eye, FileDown, Save, ZoomIn, ZoomOut } from 'lucide-react'
import { Button } from 'primereact/button'
import { Toolbar } from 'primereact/toolbar'
import {
  editorBtnGhostClass,
  editorBtnOutlineClass,
  editorBtnPrimaryClass,
} from '../fieldStyles'

export type CraftPdfToolbarProps = {
  title?: string
  zoom: number
  dirty?: boolean
  saving?: boolean
  saveLabel?: string
  savingLabel?: string
  previewLabel?: string
  generateLabel?: string
  backLabel?: string
  onBack?: () => void
  onZoomIn?: () => void
  onZoomOut?: () => void
  onSave?: () => void
  onPreview?: () => void
  onGenerate?: () => void
  className?: string
}

export function CraftPdfToolbar({
  title,
  zoom,
  dirty = false,
  saving = false,
  saveLabel = 'Lưu',
  savingLabel = 'Đang lưu…',
  previewLabel = 'Preview',
  generateLabel = 'Generate PDF',
  backLabel = 'Quay lại',
  onBack,
  onZoomIn,
  onZoomOut,
  onSave,
  onPreview,
  onGenerate,
  className = '',
}: CraftPdfToolbarProps) {
  return (
    <Toolbar.Root
      className={`flex h-12 w-full shrink-0 flex-wrap items-center gap-2 border-0 border-b border-slate-200 bg-white px-3 ${className}`}
    >
      <Toolbar.Start className="flex min-w-0 items-center gap-2">
        {onBack && (
          <Button
            type="button"
            unstyled
            className={editorBtnGhostClass}
            onClick={onBack}
          >
            <ArrowLeft className="size-3.5" strokeWidth={1.75} />
            {backLabel}
          </Button>
        )}
        {title && (
          <div className="flex min-w-0 items-center gap-2">
            <h3 className="m-0 truncate text-[13px] font-semibold text-slate-900">
              {title}
            </h3>
            {dirty ? (
              <span className="shrink-0 rounded-full bg-amber-50 px-2 py-0.5 text-[10px] font-semibold text-amber-700 ring-1 ring-amber-200">
                Chưa lưu
              </span>
            ) : null}
          </div>
        )}
      </Toolbar.Start>

      <Toolbar.Center className="flex items-center gap-0.5 rounded-md bg-slate-100 p-0.5">
        <Button
          type="button"
          unstyled
          className={editorBtnGhostClass}
          onClick={onZoomOut}
          aria-label="Zoom out"
        >
          <ZoomOut className="size-3.5" strokeWidth={1.75} />
        </Button>
        <span className="min-w-[3rem] text-center text-[11px] tabular-nums text-slate-600">
          {Math.round(zoom * 100)}%
        </span>
        <Button
          type="button"
          unstyled
          className={editorBtnGhostClass}
          onClick={onZoomIn}
          aria-label="Zoom in"
        >
          <ZoomIn className="size-3.5" strokeWidth={1.75} />
        </Button>
      </Toolbar.Center>

      <Toolbar.End className="ml-auto flex flex-wrap items-center gap-1.5">
        {onPreview && (
          <Button
            type="button"
            unstyled
            className={editorBtnOutlineClass}
            onClick={onPreview}
          >
            <Eye className="size-3.5" strokeWidth={1.75} />
            {previewLabel}
          </Button>
        )}
        {onGenerate && (
          <Button
            type="button"
            unstyled
            className={editorBtnOutlineClass}
            onClick={onGenerate}
          >
            <FileDown className="size-3.5" strokeWidth={1.75} />
            {generateLabel}
          </Button>
        )}
        {onSave && (
          <Button
            type="button"
            unstyled
            className={editorBtnPrimaryClass}
            disabled={saving}
            onClick={onSave}
          >
            <Save className="size-3.5" strokeWidth={1.75} />
            {saving ? savingLabel : saveLabel}
          </Button>
        )}
      </Toolbar.End>
    </Toolbar.Root>
  )
}
