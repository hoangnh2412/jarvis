import { ChevronLeft, ChevronRight } from 'lucide-react'

export type PageControlsProps = {
  currentPage: number
  totalPages: number
  onPrevious: () => void
  onNext: () => void
}

export function PageControls({
  currentPage,
  totalPages,
  onPrevious,
  onNext,
}: PageControlsProps) {
  return (
    <div className="flex items-center justify-center gap-3 border-t border-slate-200 bg-white px-4 py-2.5">
      <button
        type="button"
        onClick={onPrevious}
        disabled={currentPage <= 1}
        className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-700 disabled:cursor-not-allowed disabled:opacity-40"
        aria-label="Previous page"
      >
        <ChevronLeft className="h-4 w-4" />
      </button>
      <span className="text-sm font-medium text-slate-700">
        Page {currentPage} / {totalPages}
      </span>
      <button
        type="button"
        onClick={onNext}
        disabled={currentPage >= totalPages}
        className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-700 disabled:cursor-not-allowed disabled:opacity-40"
        aria-label="Next page"
      >
        <ChevronRight className="h-4 w-4" />
      </button>
    </div>
  )
}
