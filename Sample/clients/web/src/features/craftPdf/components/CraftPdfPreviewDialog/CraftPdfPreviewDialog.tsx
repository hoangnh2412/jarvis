import { useEffect, useState, type ReactNode } from 'react'
import { Button } from 'primereact/button'
import { Dialog } from 'primereact/dialog'
import { Document, Page, pdfjs } from 'react-pdf'
import type { PdfElement, PdfPageSize } from '../../types'
import { renderElementPreview } from '../CraftPdfCanvas/renderElementPreview'
import { btnOutlinedClass } from '../fieldStyles'

// App host nên import CSS của react-pdf nếu cần text/annotation layer:
// import 'react-pdf/dist/Page/AnnotationLayer.css'
// import 'react-pdf/dist/Page/TextLayer.css'

let workerConfigured = false

export function configureCraftPdfWorker(workerSrc?: string) {
  pdfjs.GlobalWorkerOptions.workerSrc =
    workerSrc ??
    `https://unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`
  workerConfigured = true
}

function ensureWorker() {
  if (!workerConfigured) configureCraftPdfWorker()
}

export type CraftPdfPreviewDialogProps = {
  visible: boolean
  onHide: () => void
  title?: string
  closeLabel?: string
  htmlHint?: string
  fileUrl?: string | null
  pageSize?: PdfPageSize
  elements?: PdfElement[]
  /** Sample map để resolve `{{...}}` trong HTML preview. */
  sampleData?: Record<string, string>
  footer?: ReactNode
  className?: string
}

export function CraftPdfPreviewDialog({
  visible,
  onHide,
  title = 'Preview PDF',
  closeLabel = 'Đóng',
  htmlHint = 'HTML preview từ canvas (chưa có file PDF).',
  fileUrl,
  pageSize,
  elements = [],
  sampleData,
  footer,
  className = '',
}: CraftPdfPreviewDialogProps) {
  const [numPages, setNumPages] = useState(0)
  const [pdfError, setPdfError] = useState<string | null>(null)

  useEffect(() => {
    if (visible && fileUrl) ensureWorker()
  }, [visible, fileUrl])

  useEffect(() => {
    if (!visible) {
      setNumPages(0)
      setPdfError(null)
    }
  }, [visible])

  return (
    <Dialog.Root
      open={visible}
      onOpenChange={(e: { value?: boolean }) => {
        if (!e.value) onHide()
      }}
    >
      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-[100] bg-slate-900/40 backdrop-blur-[2px]" />
        <Dialog.Positioner className="fixed inset-0 z-[110] flex items-center justify-center p-4">
          <Dialog.Popup
            className={`flex max-h-[90vh] w-full max-w-[920px] flex-col overflow-hidden rounded-xl border border-slate-200 bg-white p-0 shadow-xl ${className}`}
          >
            <Dialog.Header className="flex items-start justify-between gap-3 border-b border-slate-100 px-5 py-4">
              <Dialog.Title className="text-lg font-semibold text-slate-900">
                {title}
              </Dialog.Title>
              <Dialog.Close
                className="rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                aria-label="Đóng"
              />
            </Dialog.Header>

            <Dialog.Content className="min-h-0 flex-1 overflow-auto bg-slate-100 p-4">
              {fileUrl ? (
                pdfError ? (
                  <p className="m-0 text-sm text-red-600">{pdfError}</p>
                ) : (
                  <Document
                    file={fileUrl}
                    onLoadSuccess={(info) => setNumPages(info.numPages)}
                    onLoadError={(err) =>
                      setPdfError(err.message || 'Không tải được PDF')
                    }
                    loading={
                      <p className="m-0 text-sm text-slate-500">Đang tải PDF…</p>
                    }
                  >
                    {Array.from({ length: numPages }, (_, i) => (
                      <Page
                        key={`page-${i + 1}`}
                        pageNumber={i + 1}
                        width={720}
                        className="mb-3 shadow-md"
                      />
                    ))}
                  </Document>
                )
              ) : pageSize ? (
                <div className="flex flex-col items-center gap-3">
                  <p className="m-0 text-xs text-slate-500">{htmlHint}</p>
                  <div
                    className="relative bg-white shadow-md ring-1 ring-slate-200"
                    style={{
                      width: pageSize.width * 0.75,
                      height: pageSize.height * 0.75,
                    }}
                  >
                    <div
                      className="absolute left-0 top-0 origin-top-left"
                      style={{
                        width: pageSize.width,
                        height: pageSize.height,
                        transform: 'scale(0.75)',
                      }}
                    >
                      {elements.map((el) => (
                        <div
                          key={el.id}
                          className="absolute overflow-hidden"
                          style={{
                            left: el.x,
                            top: el.y,
                            width: el.width,
                            height: el.height,
                            zIndex: el.zIndex,
                          }}
                        >
                          {renderElementPreview(el, {
                            data: sampleData,
                            resolveBindings: true,
                          })}
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              ) : (
                <p className="m-0 text-sm text-slate-500">
                  Không có nội dung preview.
                </p>
              )}
            </Dialog.Content>

            <Dialog.Footer className="flex justify-end gap-2 border-t border-slate-100 px-5 py-4">
              {footer ?? (
                <Button
                  type="button"
                  unstyled
                  className={btnOutlinedClass}
                  onClick={onHide}
                >
                  {closeLabel}
                </Button>
              )}
            </Dialog.Footer>
          </Dialog.Popup>
        </Dialog.Positioner>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
