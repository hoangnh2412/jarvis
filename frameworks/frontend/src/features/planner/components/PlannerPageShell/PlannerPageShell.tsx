import type { ReactNode } from 'react'

export type PlannerPageShellProps = {
  title?: string
  description?: string
  headerActions?: ReactNode
  children: ReactNode
  className?: string
}

export function PlannerPageShell({
  title,
  description,
  headerActions,
  children,
  className = '',
}: PlannerPageShellProps) {
  return (
    <div
      className={[
        'kit-planner-page font-sans text-ink',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      {(title || description || headerActions) && (
        <div className="mb-4 flex w-full shrink-0 flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div className="min-w-0">
            {title ? (
              <h2 className="m-0 text-xl font-semibold leading-tight tracking-tight text-slate-900">
                {title}
              </h2>
            ) : null}
            {description ? (
              <p className="m-0 mt-1 max-w-[60ch] text-sm leading-relaxed text-slate-500">
                {description}
              </p>
            ) : null}
          </div>
          {headerActions ? (
            <div className="flex shrink-0 flex-wrap items-center gap-2">
              {headerActions}
            </div>
          ) : null}
        </div>
      )}
      <div className="flex min-h-0 flex-1 flex-col">{children}</div>
    </div>
  )
}
