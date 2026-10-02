import { useState } from 'react'
import { FileText } from 'lucide-react'
import { getCraftDocMessages, type CraftDocLocale } from '../../localization'
import type { CraftDocPreviewPanelProps } from '../../types'
import { DocxPreview } from '../DocxPreview'
import { RequestJsonEditor } from '../RequestJsonEditor'
import { PreviewToolbar } from './PreviewToolbar'

export type DocumentPreviewPaneProps = CraftDocPreviewPanelProps

type PreviewPanelTab = 'preview' | 'requestJson'

export function DocumentPreviewPane({
  locale = 'vi',
  template,
  fields,
  previewBlob,
  status,
  preview,
  onChangeTemplate,
}: DocumentPreviewPaneProps) {
  const messages = getCraftDocMessages(locale)
  const loading = status === 'loading' || status === 'parsing'
  const [panelTab, setPanelTab] = useState<PreviewPanelTab>('preview')

  return (
    <section className="craft-doc-preview-panel flex h-full min-h-0 min-w-0 flex-col overflow-hidden border border-slate-200 bg-[#eef1f5]">
      <div className="flex shrink-0 flex-wrap items-center justify-between gap-2 border-b border-slate-200 bg-white px-3 py-2">
        <div className="flex min-w-0 flex-1 flex-wrap items-center gap-3">
          <div className="inline-flex items-center gap-2 text-sm font-medium text-slate-800">
            <FileText className="h-4 w-4 text-blue-600" />
            {messages.preview.title}
            {template?.name ? ` ${template.name.toLowerCase()}` : ''}
          </div>

          {template ? (
            <nav className="craft-doc-preview-tabs" aria-label="Preview panel tabs">
              {(
                [
                  ['preview', messages.preview.tabPreview],
                  ['requestJson', messages.preview.tabRequestJson],
                ] as const
              ).map(([id, label]) => (
                <button
                  key={id}
                  type="button"
                  className={[
                    'craft-doc-preview-tabs__tab',
                    panelTab === id ? 'is-active' : '',
                  ]
                    .filter(Boolean)
                    .join(' ')}
                  onClick={() => setPanelTab(id)}
                >
                  {label}
                </button>
              ))}
            </nav>
          ) : null}
        </div>

        <button
          type="button"
          onClick={onChangeTemplate}
          className="text-xs font-medium text-blue-600 hover:text-blue-700"
        >
          {messages.preview.changeTemplate}
        </button>
      </div>

      {template && panelTab === 'preview' ? (
        <PreviewToolbar
          zoom={preview.zoom}
          fitMode={preview.fitMode}
          currentPage={preview.currentPage}
          totalPages={preview.totalPages}
          fullscreen={preview.fullscreen}
          onZoomIn={preview.onZoomIn}
          onZoomOut={preview.onZoomOut}
          onResetZoom={preview.onResetZoom}
          onFitWidth={preview.onFitWidth}
          onFitPage={preview.onFitPage}
          onFullscreenToggle={preview.onFullscreenToggle}
          onPreviousPage={preview.onPreviousPage}
          onNextPage={preview.onNextPage}
        />
      ) : null}

      <div className="relative min-h-0 min-w-0 flex-1 overflow-hidden">
        {!template ? (
          <div className="flex h-full items-center justify-center p-6">
            <p className="text-sm text-slate-500">{messages.preview.empty}</p>
          </div>
        ) : null}

        {template && panelTab === 'preview' && loading ? (
          <div className="absolute inset-0 z-10 flex items-center justify-center bg-[#eef1f5]/80">
            <p className="border border-slate-200 bg-white px-3 py-2 text-sm text-slate-600">
              {messages.preview.loading}
            </p>
          </div>
        ) : null}

        {template && panelTab === 'preview' && status === 'error' ? (
          <div className="flex h-full items-center justify-center p-6">
            <p className="text-sm text-red-600">{messages.preview.error}</p>
          </div>
        ) : null}

        {template && panelTab === 'preview' && status !== 'error' ? (
          <DocxPreview
            blob={previewBlob}
            previewMode="filled"
            zoom={preview.zoom}
            fitMode={preview.fitMode}
            fullscreen={preview.fullscreen}
            currentPage={preview.currentPage}
            onCurrentPageChange={preview.onCurrentPageChange}
            onTotalPagesChange={preview.onTotalPagesChange}
            showLoadingOverlay={loading}
            className="h-full"
          />
        ) : null}

        {template && panelTab === 'requestJson' ? (
          <RequestJsonEditor template={template} fields={fields} messages={messages} />
        ) : null}
      </div>
    </section>
  )
}
