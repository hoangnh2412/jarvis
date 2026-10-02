import {
  ChevronLeft,
  ChevronRight,
  Expand,
  Maximize2,
  Minimize2,
  Minus,
  Plus,
  RotateCcw,
} from 'lucide-react'

export type PreviewToolbarProps = {
  zoom: number
  fitMode: 'none' | 'width' | 'page'
  currentPage: number
  totalPages: number
  fullscreen: boolean
  onZoomIn: () => void
  onZoomOut: () => void
  onResetZoom: () => void
  onFitWidth: () => void
  onFitPage: () => void
  onFullscreenToggle: () => void
  onPreviousPage: () => void
  onNextPage: () => void
}

export function PreviewToolbar({
  zoom,
  fitMode,
  currentPage,
  totalPages,
  fullscreen,
  onZoomIn,
  onZoomOut,
  onResetZoom,
  onFitWidth,
  onFitPage,
  onFullscreenToggle,
  onPreviousPage,
  onNextPage,
}: PreviewToolbarProps) {
  const fitLabel =
    fitMode === 'width' ? 'Vừa chiều ngang' : fitMode === 'page' ? 'Vừa trang' : `${zoom}%`

  return (
    <div className="flex shrink-0 flex-wrap items-center gap-2 border-b border-slate-200 bg-white px-3 py-2">
      <div className="inline-flex items-center overflow-hidden rounded-md border border-slate-200 bg-slate-50">
        <button
          type="button"
          onClick={onZoomOut}
          className="inline-flex h-8 w-8 items-center justify-center text-slate-600 hover:bg-white"
          aria-label="Thu nhỏ"
        >
          <Minus className="h-4 w-4" />
        </button>
        <span className="min-w-[4.5rem] border-x border-slate-200 px-2 text-center text-xs font-medium text-slate-700">
          {fitMode === 'none' ? `${zoom}%` : fitLabel}
        </span>
        <button
          type="button"
          onClick={onZoomIn}
          className="inline-flex h-8 w-8 items-center justify-center text-slate-600 hover:bg-white"
          aria-label="Phóng to"
        >
          <Plus className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={onResetZoom}
          className="inline-flex h-8 w-8 items-center justify-center border-l border-slate-200 text-slate-600 hover:bg-white"
          aria-label="Đặt lại zoom"
        >
          <RotateCcw className="h-3.5 w-3.5" />
        </button>
      </div>

      <button
        type="button"
        onClick={onFitWidth}
        className={`inline-flex h-8 items-center gap-1 rounded-md border px-2.5 text-xs font-medium ${
          fitMode === 'width'
            ? 'border-blue-300 bg-blue-50 text-blue-700'
            : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
        }`}
      >
        <Expand className="h-3.5 w-3.5" />
        Vừa ngang
      </button>

      <button
        type="button"
        onClick={onFitPage}
        className={`inline-flex h-8 items-center rounded-md border px-2.5 text-xs font-medium ${
          fitMode === 'page'
            ? 'border-blue-300 bg-blue-50 text-blue-700'
            : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
        }`}
      >
        Vừa trang
      </button>

      <div className="inline-flex items-center overflow-hidden rounded-md border border-slate-200 bg-white">
        <button
          type="button"
          onClick={onPreviousPage}
          disabled={currentPage <= 1}
          className="inline-flex h-8 w-8 items-center justify-center text-slate-600 hover:bg-slate-50 disabled:opacity-40"
          aria-label="Trang trước"
        >
          <ChevronLeft className="h-4 w-4" />
        </button>
        <span className="border-x border-slate-200 px-2 text-xs font-medium text-slate-700">
          {currentPage}/{totalPages}
        </span>
        <button
          type="button"
          onClick={onNextPage}
          disabled={currentPage >= totalPages}
          className="inline-flex h-8 w-8 items-center justify-center text-slate-600 hover:bg-slate-50 disabled:opacity-40"
          aria-label="Trang sau"
        >
          <ChevronRight className="h-4 w-4" />
        </button>
      </div>

      <button
        type="button"
        onClick={onFullscreenToggle}
        className="ml-auto inline-flex h-8 w-8 items-center justify-center rounded-md border border-slate-200 bg-white text-slate-700 hover:bg-slate-50"
        aria-label={fullscreen ? 'Thoát toàn màn hình' : 'Toàn màn hình'}
      >
        {fullscreen ? <Minimize2 className="h-4 w-4" /> : <Maximize2 className="h-4 w-4" />}
      </button>
    </div>
  )
}
