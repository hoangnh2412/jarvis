import { X } from 'lucide-react'
import { Button } from 'primereact/button'
import { Dialog } from 'primereact/dialog'
import { notify } from '../../../../common/Toaster'
import { prettyJson } from '../../utils'
import { btnOutlinedClass, btnPrimaryClass } from '../fieldStyles'

export type SubmitResultModalProps = {
  open: boolean
  data: Record<string, unknown> | null
  onClose: () => void
}

export function SubmitResultModal({
  open,
  data,
  onClose,
}: SubmitResultModalProps) {
  if (!data) return null

  const json = prettyJson(data)

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(json)
      notify.success('Data copied')
    } catch {
      notify.error('Copy failed')
    }
  }

  return (
    <Dialog.Root
      open={open}
      onOpenChange={(e: { value?: boolean }) => {
        if (!e.value) onClose()
      }}
    >
      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-[100] bg-slate-900/30" />
        <Dialog.Positioner className="fixed inset-0 z-[110] flex items-center justify-center p-4">
          <Dialog.Popup className="kit-dfb-modal w-full max-w-md overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xl">
            <Dialog.Header className="flex items-start justify-between gap-3 border-b border-slate-100 px-4 py-3">
              <div>
                <Dialog.Title className="text-base font-semibold text-slate-900">
                  Form Submitted Successfully
                </Dialog.Title>
                <p className="m-0 mt-0.5 text-xs text-slate-500">
                  Submitted form data
                </p>
              </div>
              <Dialog.Close
                type="button"
                className="inline-flex h-7 w-7 items-center justify-center rounded-md text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                onClick={onClose}
              >
                <X className="h-4 w-4" />
              </Dialog.Close>
            </Dialog.Header>
            <Dialog.Content className="px-4 py-3">
              <pre className="kit-dfb-modal__code m-0 overflow-auto rounded-lg bg-slate-900 p-3 text-xs leading-relaxed text-slate-200">
                {json}
              </pre>
            </Dialog.Content>
            <Dialog.Footer className="flex justify-end gap-2 border-t border-slate-100 px-4 py-3">
              <Button
                type="button"
                unstyled
                className={btnOutlinedClass}
                onClick={() => void copy()}
              >
                Copy Data
              </Button>
              <Button
                type="button"
                unstyled
                className={btnPrimaryClass}
                onClick={onClose}
              >
                Close
              </Button>
            </Dialog.Footer>
          </Dialog.Popup>
        </Dialog.Positioner>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
