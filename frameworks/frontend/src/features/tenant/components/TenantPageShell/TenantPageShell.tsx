import type { FormEventHandler, ReactNode } from 'react'
import { Button } from 'primereact/button'
import { Toolbar } from 'primereact/toolbar'
import { btnOutlinedClass, btnPrimaryClass } from '../fieldStyles'

export type TenantPageShellProps = {
  title?: string
  description?: string
  showHeader?: boolean
  headerActions?: ReactNode
  children: ReactNode
  className?: string
  /** Khi có form submit */
  onSubmit?: FormEventHandler<HTMLFormElement>
  onCancel?: () => void
  submitLabel?: string
  cancelLabel?: string
  submittingLabel?: string
  isSubmitting?: boolean
  showFooter?: boolean
  asForm?: boolean
}

export function TenantPageShell({
  title,
  description,
  showHeader = true,
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
}: TenantPageShellProps) {
  const header =
    showHeader && (title || description || headerActions) ? (
      <Toolbar.Root className="mb-5 flex w-full shrink-0 flex-col gap-3 border-0 border-b border-line bg-transparent p-0 pb-4 sm:flex-row sm:items-start sm:justify-between">
        <Toolbar.Start className="min-w-0">
          {title && (
            <h2 className="m-0 text-[1.35rem] font-semibold leading-tight tracking-tight text-ink">
              {title}
            </h2>
          )}
          {description && (
            <p className="mt-1.5 m-0 max-w-[56ch] text-sm leading-relaxed text-mute">
              {description}
            </p>
          )}
        </Toolbar.Start>
        {headerActions && (
          <Toolbar.End className="flex shrink-0 flex-wrap items-center gap-2">
            {headerActions}
          </Toolbar.End>
        )}
      </Toolbar.Root>
    ) : null

  const footer =
    showFooter || asForm ? (
      <Toolbar.Root className="mt-5 flex w-full shrink-0 justify-end gap-2.5 border-0 border-t border-line bg-transparent p-0 pt-4">
        <Toolbar.End className="flex flex-wrap items-center gap-2.5">
          {onCancel && (
            <Button
              type="button"
              unstyled
              className={btnOutlinedClass}
              onClick={onCancel}
            >
              {cancelLabel}
            </Button>
          )}
          {asForm && (
            <Button
              type="submit"
              unstyled
              className={btnPrimaryClass}
              disabled={isSubmitting}
            >
              {isSubmitting ? submittingLabel : submitLabel}
            </Button>
          )}
        </Toolbar.End>
      </Toolbar.Root>
    ) : null

  const body = (
    <>
      {header}
      <div className="min-h-0 flex-1 overflow-auto [scrollbar-width:none] [-ms-overflow-style:none] [&::-webkit-scrollbar]:hidden">{children}</div>
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
