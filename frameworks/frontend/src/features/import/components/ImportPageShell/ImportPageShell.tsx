import type { ReactNode } from 'react'
import { Toolbar } from 'primereact/toolbar'

export type ImportPageShellProps = {
  title?: string
  description?: string
  showHeader?: boolean
  headerActions?: ReactNode
  children: ReactNode
  className?: string
}

export function ImportPageShell({
  title,
  description,
  showHeader = true,
  headerActions,
  children,
  className = '',
}: ImportPageShellProps) {
  const header =
    showHeader && (title || description || headerActions) ? (
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
        {headerActions ? (
          <Toolbar.End className="flex shrink-0 flex-wrap items-center gap-2">
            {headerActions}
          </Toolbar.End>
        ) : null}
      </Toolbar.Root>
    ) : null

  return (
    <div
      className={[
        'kit-import-page flex w-full flex-col font-sans text-ink',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      {header}
      <div>{children}</div>
    </div>
  )
}
