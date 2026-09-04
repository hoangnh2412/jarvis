import type { ReactNode } from 'react'
import { Plus, RotateCcw, Save } from 'lucide-react'
import { Button } from 'primereact/button'
import { Toolbar } from 'primereact/toolbar'
import {
  dashboardToolbarBtnClass,
  dashboardToolbarBtnDangerClass,
  dashboardToolbarBtnPrimaryClass,
} from '../fieldStyles'

export type DashboardHeaderProps = {
  title?: string
  description?: string
  chartCount?: number
  chartCountLabel?: string
  addLabel?: string
  saveLabel?: string
  resetLabel?: string
  onAddChart?: () => void
  onSave?: () => void
  onReset?: () => void
  onReload?: () => void
  loading?: boolean
  actions?: ReactNode
  className?: string
}

export function DashboardHeader({
  title,
  description,
  chartCount,
  chartCountLabel,
  addLabel = 'Thêm biểu đồ',
  saveLabel = 'Lưu bố cục',
  resetLabel = 'Xóa hết',
  onAddChart,
  onSave,
  onReset,
  loading = false,
  actions,
  className = '',
}: DashboardHeaderProps) {
  const hasToolbarActions = Boolean(onReset || onSave || onAddChart || actions)

  return (
    <Toolbar.Root
      className={`mb-3 flex w-full shrink-0 flex-col gap-2.5 border-0 border-b border-slate-200 bg-transparent p-0 pb-3 sm:flex-row sm:items-center sm:justify-between ${className}`}
    >
      <Toolbar.Start className="min-w-0">
        {title && (
          <h2 className="m-0 text-lg font-semibold leading-tight tracking-tight text-slate-900">
            {title}
          </h2>
        )}
        {description ? (
          <p className="mt-1 m-0 text-sm text-slate-600">{description}</p>
        ) : null}
        {typeof chartCount === 'number' && chartCountLabel && (
          <p className="mt-1 m-0 text-xs font-medium text-slate-500">
            {chartCountLabel.replace('{count}', String(chartCount))}
          </p>
        )}
      </Toolbar.Start>

      {hasToolbarActions && (
        <Toolbar.End className="inline-flex flex-wrap items-center gap-1 rounded-[10px] p-0.5">
          {actions}
          {onReset && (
            <Button
              type="button"
              unstyled
              className={dashboardToolbarBtnDangerClass}
              onClick={onReset}
              disabled={loading}
            >
              <RotateCcw className="h-3.5 w-3.5" aria-hidden />
              {resetLabel}
            </Button>
          )}
          {onSave && (
            <Button
              type="button"
              unstyled
              className={dashboardToolbarBtnClass}
              onClick={onSave}
              disabled={loading}
            >
              <Save className="h-3.5 w-3.5" aria-hidden />
              {saveLabel}
            </Button>
          )}
          {(onReset || onSave) && onAddChart && (
            <span
              className="mx-0.5 h-5 w-px shrink-0 bg-slate-200"
              aria-hidden
            />
          )}
          {onAddChart && (
            <Button
              type="button"
              unstyled
              className={dashboardToolbarBtnPrimaryClass}
              onClick={onAddChart}
              disabled={loading}
            >
              <Plus className="h-3.5 w-3.5" aria-hidden />
              {addLabel}
            </Button>
          )}
        </Toolbar.End>
      )}
    </Toolbar.Root>
  )
}
