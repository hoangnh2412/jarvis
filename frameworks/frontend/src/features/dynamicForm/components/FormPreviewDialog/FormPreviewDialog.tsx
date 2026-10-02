import { X } from 'lucide-react'
import { Button } from 'primereact/button'
import { Dialog } from 'primereact/dialog'
import type { DynamicFormDefinition } from '../../types'
import { DynamicFormRenderer } from '../DynamicFormRenderer'
import { btnOutlinedClass } from '../fieldStyles'

export type FormPreviewDialogProps = {
  open: boolean
  form: DynamicFormDefinition | null
  title?: string
  emptyMessage?: string
  closeLabel?: string
  onClose: () => void
  onSubmitPreview?: () => void
}

export function FormPreviewDialog({
  open,
  form,
  title = 'Xem trước',
  emptyMessage = 'Schema trống',
  closeLabel = 'Đóng',
  onClose,
  onSubmitPreview,
}: FormPreviewDialogProps) {
  if (!form) return null

  return (
    <Dialog.Root
      open={open}
      onOpenChange={(e: { value?: boolean }) => {
        if (!e.value) onClose()
      }}
    >
      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-[100] bg-slate-900/35" />
        <Dialog.Positioner className="fixed inset-0 z-[110] flex items-center justify-center p-3">
          <Dialog.Popup className="flex max-h-[min(90vh,800px)] w-full max-w-2xl flex-col overflow-hidden rounded-xl border border-slate-200 bg-white p-0 shadow-xl">
            <Dialog.Header className="flex items-center justify-between gap-2 border-b border-slate-100 px-3.5 py-2.5">
              <div className="min-w-0">
                <Dialog.Title className="text-base font-semibold text-slate-900">
                  {title}
                </Dialog.Title>
                {form.name ? (
                  <p className="m-0 mt-0.5 truncate text-xs text-slate-500">
                    {form.name}
                  </p>
                ) : null}
              </div>
              <Dialog.Close
                type="button"
                aria-label={closeLabel}
                className="inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-md text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                onClick={onClose}
              >
                <X className="h-4 w-4" />
              </Dialog.Close>
            </Dialog.Header>

            <Dialog.Content className="min-h-0 flex-1 overflow-y-auto px-3.5 py-3">
              <DynamicFormRenderer
                schema={form.schema}
                uiSchema={form.uiSchema}
                formData={form.formData}
                emptyMessage={emptyMessage}
                liveValidate
                onSubmit={async () => {
                  onSubmitPreview?.()
                }}
              />
            </Dialog.Content>

            <Dialog.Footer className="flex justify-end border-t border-slate-100 px-3.5 py-2.5">
              <Button
                type="button"
                unstyled
                className={btnOutlinedClass}
                onClick={onClose}
              >
                {closeLabel}
              </Button>
            </Dialog.Footer>
          </Dialog.Popup>
        </Dialog.Positioner>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
