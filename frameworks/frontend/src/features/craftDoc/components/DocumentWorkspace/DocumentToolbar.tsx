import {
  Download,
  Expand,
  Maximize2,
  Minimize2,
  Minus,
  Plus,
  RotateCcw,
  Save,
} from 'lucide-react'
import type { DocumentTemplate, PreviewMode } from '../../types'

export type DocumentToolbarProps = {
  template: DocumentTemplate | null
  previewMode: PreviewMode
  zoom: number
  dirty?: boolean
  onPreviewModeChange: (mode: PreviewMode) => void
  onZoomIn: () => void
  onZoomOut: () => void
  onResetZoom: () => void
  onFitWidth: () => void
  onFitPage: () => void
  onFullscreenToggle: () => void
  fullscreen: boolean
  onSave: () => void
  onExport: () => void
}

export function DocumentToolbar({
  template,
  previewMode,
  zoom,
  dirty,
  onPreviewModeChange,
  onZoomIn,
  onZoomOut,
  onResetZoom,
  onFitWidth,
  onFitPage,
  onFullscreenToggle,
  fullscreen,
  onSave,
  onExport,
}: DocumentToolbarProps) {
  return (
    <div className="flex flex-wrap items-center gap-3 border-b border-slate-200 bg-white px-4 py-3">
      <div className="min-w-0 flex-1">
        <p className="text-xs font-medium uppercase tracking-[0.14em] text-slate-500">
          Template
        </p>
        <h2 className="truncate text-base font-semibold text-slate-900">
          {template?.name ?? 'No template selected'}
        </h2>
      </div>

      <div className="inline-flex rounded-xl border border-slate-200 bg-slate-50 p-1">
        <button
          type="button"
          onClick={() => onPreviewModeChange('template')}
          className={`rounded-lg px-3 py-1.5 text-sm font-medium transition ${
            previewMode === 'template'
              ? 'bg-white text-teal-800 shadow-sm'
              : 'text-slate-600 hover:text-slate-900'
          }`}
        >
          Template Preview
        </button>
        <button
          type="button"
          onClick={() => onPreviewModeChange('filled')}
          className={`rounded-lg px-3 py-1.5 text-sm font-medium transition ${
            previewMode === 'filled'
              ? 'bg-white text-teal-800 shadow-sm'
              : 'text-slate-600 hover:text-slate-900'
          }`}
        >
          Filled Preview
        </button>
      </div>

      <div className="flex items-center gap-1 rounded-xl border border-slate-200 bg-white px-2 py-1">
        <button
          type="button"
          onClick={onZoomOut}
          className="inline-flex h-8 w-8 items-center justify-center rounded-lg text-slate-600 hover:bg-slate-50"
          aria-label="Zoom out"
        >
          <Minus className="h-4 w-4" />
        </button>
        <span className="min-w-14 text-center text-sm font-medium text-slate-700">{zoom}%</span>
        <button
          type="button"
          onClick={onZoomIn}
          className="inline-flex h-8 w-8 items-center justify-center rounded-lg text-slate-600 hover:bg-slate-50"
          aria-label="Zoom in"
        >
          <Plus className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={onResetZoom}
          className="inline-flex h-8 w-8 items-center justify-center rounded-lg text-slate-600 hover:bg-slate-50"
          aria-label="Reset zoom"
        >
          <RotateCcw className="h-4 w-4" />
        </button>
      </div>

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={onFitWidth}
          className="inline-flex h-9 items-center gap-1 rounded-xl border border-slate-200 bg-white px-3 text-sm font-medium text-slate-700 hover:bg-slate-50"
        >
          <Expand className="h-4 w-4" />
          Fit Width
        </button>
        <button
          type="button"
          onClick={onFitPage}
          className="inline-flex h-9 items-center gap-1 rounded-xl border border-slate-200 bg-white px-3 text-sm font-medium text-slate-700 hover:bg-slate-50"
        >
          Fit Page
        </button>
        <button
          type="button"
          onClick={onFullscreenToggle}
          className="inline-flex h-9 w-9 items-center justify-center rounded-xl border border-slate-200 bg-white text-slate-700 hover:bg-slate-50"
          aria-label={fullscreen ? 'Exit fullscreen' : 'Enter fullscreen'}
        >
          {fullscreen ? <Minimize2 className="h-4 w-4" /> : <Maximize2 className="h-4 w-4" />}
        </button>
      </div>

      <div className="ml-auto flex items-center gap-2">
        <button
          type="button"
          onClick={onSave}
          className="inline-flex h-10 items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 text-sm font-semibold text-slate-700 hover:bg-slate-50"
        >
          <Save className="h-4 w-4" />
          Save{dirty ? ' *' : ''}
        </button>
        <button
          type="button"
          onClick={onExport}
          disabled={!template}
          className="inline-flex h-10 items-center gap-2 rounded-xl bg-teal-700 px-4 text-sm font-semibold text-white transition hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <Download className="h-4 w-4" />
          Export
        </button>
      </div>
    </div>
  )
}
