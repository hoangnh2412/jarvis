import type { FormEventHandler, ReactNode } from 'react'
import { Button } from 'primereact/button'
import { Dialog } from 'primereact/dialog'

const btnOutlinedClass =
  'pr-btn-outlined inline-flex h-10 items-center justify-center gap-2 rounded-xl border px-4 text-sm font-medium shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60'

const btnPrimaryClass =
  'pr-btn-primary inline-flex h-10 items-center justify-center gap-2 rounded-xl border-0 px-4 text-sm font-semibold shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30 disabled:cursor-not-allowed disabled:opacity-60'

export type FeatureDialogSize = 'sm' | 'md' | 'lg' | 'xl' | '2xl'

const sizeClass: Record<FeatureDialogSize, string> = {
  sm: 'max-w-md',
  md: 'max-w-lg',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl',
  '2xl': 'max-w-6xl',
}

export type FeatureDialogProps = {
  open: boolean
  onClose: () => void
  title: ReactNode
  description?: ReactNode
  children: ReactNode
  footer?: ReactNode
  size?: FeatureDialogSize
  /** Gắn form submit — render `<form>` bọc content + footer */
  onSubmit?: FormEventHandler<HTMLFormElement>
  submitLabel?: string
  cancelLabel?: string
  submitting?: boolean
  hideFooter?: boolean
  closeOnBackdrop?: boolean
}

/** Popup chung cho các feature — PrimeReact Dialog compound API. */
export function FeatureDialog({
  open,
  onClose,
  title,
  description,
  children,
  footer,
  size = 'md',
  onSubmit,
  submitLabel = 'Lưu',
  cancelLabel = 'Hủy',
  submitting = false,
  hideFooter = false,
  closeOnBackdrop = true,
}: FeatureDialogProps) {
  const defaultFooter = (
    <>
      <Button
        type="button"
        unstyled
        className={btnOutlinedClass}
        disabled={submitting}
        onClick={onClose}
      >
        {cancelLabel}
      </Button>
      {onSubmit ? (
        <Button
          type="submit"
          unstyled
          className={btnPrimaryClass}
          disabled={submitting}
        >
          {submitting ? 'Đang xử lý…' : submitLabel}
        </Button>
      ) : null}
    </>
  )

  const body = (
    <>
      <Dialog.Header className="flex items-start justify-between gap-3 border-b border-slate-100 px-5 py-4">
        <div className="min-w-0">
          <Dialog.Title className="text-lg font-semibold text-slate-900">
            {title}
          </Dialog.Title>
          {description ? (
            <p className="m-0 mt-1 text-sm leading-relaxed text-slate-500">
              {description}
            </p>
          ) : null}
        </div>
        <Dialog.Close
          className="rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
          aria-label="Đóng"
        />
      </Dialog.Header>

      <Dialog.Content className="max-h-[min(70vh,720px)] overflow-y-auto px-5 py-4">
        {children}
      </Dialog.Content>

      {!hideFooter ? (
        <Dialog.Footer className="flex justify-end gap-2 border-t border-slate-100 px-5 py-4">
          {footer ?? defaultFooter}
        </Dialog.Footer>
      ) : null}
    </>
  )

  return (
    <Dialog.Root
      open={open}
      onOpenChange={(e: { value?: boolean }) => {
        if (!e.value && closeOnBackdrop) onClose()
      }}
    >
      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-[100] bg-slate-900/40 backdrop-blur-[2px]" />
        <Dialog.Positioner className="fixed inset-0 z-[110] flex items-center justify-center p-4">
          <Dialog.Popup
            className={`flex w-full ${sizeClass[size]} max-h-[min(92vh,900px)] flex-col overflow-hidden rounded-xl border border-slate-200 bg-white p-0 shadow-xl`}
          >
            {onSubmit ? (
              <form onSubmit={onSubmit} className="flex min-h-0 flex-1 flex-col" noValidate>
                {body}
              </form>
            ) : (
              body
            )}
          </Dialog.Popup>
        </Dialog.Positioner>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
