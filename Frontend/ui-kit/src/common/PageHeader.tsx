import type { ReactNode } from 'react'
import { Toolbar } from 'primereact/toolbar'

type PageHeaderProps = {
  title: string
  description?: string
  actions?: ReactNode
  className?: string
}

export default function PageHeader({
  title,
  description,
  actions,
  className = '',
}: PageHeaderProps) {
  return (
    <Toolbar.Root
      className={[
        'mb-2 flex w-full flex-col gap-2 border-0 bg-transparent p-0 sm:flex-row sm:items-center sm:justify-between',
        className,
      ].join(' ')}
    >
      <Toolbar.Start className="min-w-0">
        <h2 className="truncate text-xl font-semibold tracking-tight text-slate-900">
          {title}
        </h2>
        {description && (
          <p className="mt-1 text-sm text-slate-500">{description}</p>
        )}
      </Toolbar.Start>
      {actions && (
        <Toolbar.End className="flex shrink-0 flex-wrap items-center gap-2">
          {actions}
        </Toolbar.End>
      )}
    </Toolbar.Root>
  )
}

export type { PageHeaderProps }
