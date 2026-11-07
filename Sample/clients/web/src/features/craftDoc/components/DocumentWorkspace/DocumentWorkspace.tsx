import { Upload } from 'lucide-react'
import type { DocumentTemplate, PreviewMode } from '../../types'
import { DocxPreview } from '../DocxPreview'
import { DocumentToolbar } from './DocumentToolbar'
import { PageControls } from './PageControls'

export type DocumentWorkspaceProps = {
  template: DocumentTemplate | null
  previewBlob: Blob | null
  previewMode: PreviewMode
  selectedFieldName: string | null
  zoom: number
  fitMode: 'none' | 'width' | 'page'
  fullscreen: boolean
  dirty?: boolean
  status: 'idle' | 'loading' | 'parsing' | 'ready' | 'error'
  errorMessage?: string | null
  currentPage: number
  totalPages: number
  onPreviewModeChange: (mode: PreviewMode) => void
  onZoomIn: () => void
  onZoomOut: () => void
  onResetZoom: () => void
  onFitWidth: () => void
  onFitPage: () => void
  onFullscreenToggle: () => void
  onSave: () => void
  onExport: () => void
  onUpload: () => void
  onFieldClick: (fieldName: string) => void
  onCurrentPageChange: (page: number) => void
  onTotalPagesChange: (total: number) => void
}

export function DocumentWorkspace({
  template,
  previewBlob,
  previewMode,
  selectedFieldName,
  zoom,
  fitMode,
  fullscreen,
  dirty,
  status,
  errorMessage,
  currentPage,
  totalPages,
  onPreviewModeChange,
  onZoomIn,
  onZoomOut,
  onResetZoom,
  onFitWidth,
  onFitPage,
  onFullscreenToggle,
  onSave,
  onExport,
  onUpload,
  onFieldClick,
  onCurrentPageChange,
  onTotalPagesChange,
}: DocumentWorkspaceProps) {
  const showEmpty = !template
  const showLoading = !showEmpty && (status === 'loading' || status === 'parsing')
  const showError = !showEmpty && status === 'error'

  return (
    <section className="flex min-w-0 flex-1 flex-col overflow-hidden">
      <DocumentToolbar
        template={template}
        previewMode={previewMode}
        zoom={zoom}
        dirty={dirty}
        onPreviewModeChange={onPreviewModeChange}
        onZoomIn={onZoomIn}
        onZoomOut={onZoomOut}
        onResetZoom={onResetZoom}
        onFitWidth={onFitWidth}
        onFitPage={onFitPage}
        onFullscreenToggle={onFullscreenToggle}
        fullscreen={fullscreen}
        onSave={onSave}
        onExport={onExport}
      />

      <div className="craft-doc-preview-shell relative min-h-0 flex-1">
        {showEmpty ? (
          <div className="craft-doc-empty-state flex h-full items-center justify-center p-8">
            <div className="max-w-md rounded-2xl border border-slate-200 bg-white p-8 text-center shadow-sm">
              <h3 className="text-lg font-semibold text-slate-900">No template selected</h3>
              <p className="mt-2 text-sm text-slate-600">
                Upload a DOCX template to get started.
              </p>
              <button
                type="button"
                onClick={onUpload}
                className="mt-5 inline-flex h-11 items-center gap-2 rounded-xl bg-teal-700 px-5 text-sm font-semibold text-white transition hover:bg-teal-800"
              >
                <Upload className="h-4 w-4" />
                Upload DOCX
              </button>
            </div>
          </div>
        ) : null}

        {showLoading ? (
          <div className="absolute inset-0 z-10 flex items-center justify-center bg-white/75">
            <div className="rounded-2xl border border-slate-200 bg-white px-5 py-4 text-sm text-slate-600 shadow-sm">
              {status === 'parsing' ? 'Analyzing template fields...' : 'Loading document...'}
            </div>
          </div>
        ) : null}

        {showError ? (
          <div className="flex h-full items-center justify-center p-8">
            <div className="max-w-md rounded-2xl border border-red-200 bg-white p-6 text-center shadow-sm">
              <h3 className="text-base font-semibold text-slate-900">
                Unable to preview this document.
              </h3>
              <p className="mt-2 text-sm text-slate-600">
                Please check that the uploaded file is a valid DOCX file.
              </p>
              {errorMessage ? (
                <p className="mt-2 text-xs text-red-600">{errorMessage}</p>
              ) : null}
            </div>
          </div>
        ) : null}

        {!showEmpty && !showError ? (
          <DocxPreview
            blob={previewBlob}
            previewMode={previewMode}
            selectedFieldName={selectedFieldName}
            zoom={zoom}
            fitMode={fitMode}
            fullscreen={fullscreen}
            currentPage={currentPage}
            onCurrentPageChange={onCurrentPageChange}
            onTotalPagesChange={onTotalPagesChange}
            onFieldClick={onFieldClick}
            className="h-full"
          />
        ) : null}
      </div>

      {!showEmpty ? (
        <PageControls
          currentPage={currentPage}
          totalPages={totalPages}
          onPrevious={() => onCurrentPageChange(Math.max(1, currentPage - 1))}
          onNext={() => onCurrentPageChange(Math.min(totalPages, currentPage + 1))}
        />
      ) : null}
    </section>
  )
}
