import { AlertTriangle } from 'lucide-react'
import { Button } from 'primereact/button'
import { Dialog } from 'primereact/dialog'

export type ConfirmDialogProps = {
  open: boolean
  onClose: () => void
  onConfirm: () => void
  title?: string
  description?: string
  confirmText?: string
  cancelText?: string
  loading?: boolean
}

/** Confirm dialog trên Dialog của PrimeReact (thay ConfirmModal cũ). */
export default function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title = 'Xác nhận',
  description = 'Bạn có chắc muốn thực hiện thao tác này?',
  confirmText = 'Xác nhận',
  cancelText = 'Hủy',
  loading = false,
}: ConfirmDialogProps) {
  return (
    <Dialog.Root
      open={open}
      onOpenChange={(e: { value?: boolean }) => {
        if (!e.value) onClose()
      }}
    >
      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-[100] bg-slate-900/40 backdrop-blur-[2px]" />
        <Dialog.Positioner className="fixed inset-0 z-[110] flex items-center justify-center p-4">
          <Dialog.Popup className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-0 shadow-xl">
            <Dialog.Header className="flex items-start justify-between gap-3 border-b border-slate-100 px-5 py-4">
              <Dialog.Title className="text-lg font-semibold text-slate-900">
                {title}
              </Dialog.Title>
              <Dialog.Close
                className="rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                aria-label="Đóng"
              />
            </Dialog.Header>
            <Dialog.Content className="px-5 py-4">
              <div className="flex gap-3">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-red-50 text-red-600">
                  <AlertTriangle className="h-5 w-5" />
                </div>
                <p className="leading-relaxed text-slate-600">
                    {description}
                  </p>
              </div>
            </Dialog.Content>
            <Dialog.Footer className="flex justify-end gap-2 border-t border-slate-100 px-5 py-4">
              <Button
                type="button"
                unstyled
                className="pr-btn-outlined inline-flex h-10 items-center justify-center gap-2 rounded-xl border px-4 text-sm font-medium shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60"
                disabled={loading}
                onClick={onClose}
              >
                {cancelText}
              </Button>
              <Button
                type="button"
                unstyled
                className="pr-btn-danger inline-flex h-10 items-center justify-center gap-2 rounded-xl border-0 px-4 text-sm font-semibold shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-600/30 disabled:cursor-not-allowed disabled:opacity-60"
                disabled={loading}
                onClick={onConfirm}
              >
                {loading ? 'Đang xử lý…' : confirmText}
              </Button>
            </Dialog.Footer>
          </Dialog.Popup>
        </Dialog.Positioner>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
