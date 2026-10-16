import type { FormEventHandler, ReactNode } from 'react'
import { Button, Toolbar } from '@platform/core'
import { btnOutlinedClass, btnPrimaryClass } from './fieldStyles'

export type ImportExportPageShellProps = {
  title?: string
  description?: string
  showHeader?: boolean
  /** @deprecated Use headerActions */
  actions?: ReactNode
  headerActions?: ReactNode
  children: ReactNode
  className?: string
  onSubmit?: FormEventHandler<HTMLFormElement>
  onCancel?: () => void
  submitLabel?: string
  cancelLabel?: string
  submittingLabel?: string
  isSubmitting?: boolean
  showFooter?: boolean
  asForm?: boolean
}

export function ImportExportPageShell({
  title,
  description,
  showHeader = true,
  actions,
  headerActions,
  children,
  className = '',
  onSubmit,
  onCancel,
  submitLabel = 'Lưu',
  cancelLabel = 'Hủy',
  submittingLabel = 'Đang lưu…',
  isSubmitting = false,
  showFooter = false,
  asForm = false,
}: ImportExportPageShellProps) {
  const resolvedHeaderActions = headerActions ?? actions

  const header =
    showHeader && (title || description || resolvedHeaderActions) ? (
      <Toolbar.Root className="mb-5 flex w-full shrink-0 flex-col gap-3 border-0 border-b border-line bg-transparent p-0 pb-4 sm:flex-row sm:items-start sm:justify-between">
        <Toolbar.Start className="min-w-0">
          {title ? (
            <h2 className="m-0 text-[1.35rem] font-semibold leading-tight tracking-tight text-ink">
              {title}
            </h2>
          ) : null}
          {description ? (
            <p className="mt-1.5 m-0 max-w-[56ch] text-sm leading-relaxed text-mute">
              {description}
            </p>
          ) : null}
        </Toolbar.Start>
        {resolvedHeaderActions ? (
          <Toolbar.End className="flex shrink-0 flex-wrap items-center gap-2">
            {resolvedHeaderActions}
          </Toolbar.End>
        ) : null}
      </Toolbar.Root>
    ) : null

  const footer =
    showFooter || asForm ? (
      <Toolbar.Root className="mt-5 flex w-full shrink-0 justify-end gap-2.5 border-0 border-t border-line bg-transparent p-0 pt-4">
        <Toolbar.End className="flex flex-wrap items-center gap-2.5">
          {onCancel ? (
            <Button
              type="button"
              unstyled
              className={btnOutlinedClass}
              onClick={onCancel}
            >
              {cancelLabel}
            </Button>
          ) : null}
          {asForm ? (
            <Button
              type="submit"
              unstyled
              className={btnPrimaryClass}
              disabled={isSubmitting}
            >
              {isSubmitting ? submittingLabel : submitLabel}
            </Button>
          ) : null}
        </Toolbar.End>
      </Toolbar.Root>
    ) : null

  const body = (
    <>
      {header}
      <div className="min-h-0 flex-1 overflow-auto [scrollbar-width:none] [-ms-overflow-style:none] [&::-webkit-scrollbar]:hidden">
        {children}
      </div>
      {footer}
    </>
  )

  const shellClass = [
    'flex h-full min-h-0 w-full animate-slide-up flex-col overflow-auto font-sans text-ink motion-reduce:animate-none',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  if (asForm) {
    return (
      <form onSubmit={onSubmit} className={shellClass} noValidate>
        {body}
      </form>
    )
  }

  return <div className={shellClass}>{body}</div>
}
