import { useEffect, useRef } from 'react'
import { Pencil, Trash2 } from 'lucide-react'
import { Button } from 'primereact/button'
import { useGridStack } from 'gridstack/dist/react'
import { DASHBOARD_CHART_DRAG_HANDLE } from '../../constants'
import { resolveChartAnimationClass } from '../../constants/chartAnimations'
import { DashboardChartView } from '../DashboardChartView'
import { btnIconClass } from '../fieldStyles'
import {
  useDashboardCanvasContext,
  useDashboardChartInstance,
} from '../DashboardCanvas/canvasContext'

export type DashboardChartWidgetProps = {
  instanceId: string
}

/**
 * GridStack widget: kéo từ header; body chart nhận hover → tooltip Chart.js.
 * `cssAnimation` (tw-animate-css classes) áp dụng trên card; đổi setting → remount để chạy lại.
 */
export function DashboardChartWidget({
  instanceId,
}: DashboardChartWidgetProps) {
  const instance = useDashboardChartInstance(instanceId)
  const {
    selectedId,
    onSelect,
    onEditChart,
    onDeleteChart,
    editLabel,
    deleteLabel,
  } = useDashboardCanvasContext()
  const { grid } = useGridStack()
  const rootRef = useRef<HTMLDivElement>(null)

  const animationClass = resolveChartAnimationClass(
    instance?.settings.cssAnimation,
  )

  useEffect(() => {
    const itemEl = rootRef.current?.closest(
      '.grid-stack-item',
    ) as HTMLElement | null
    if (!grid || !itemEl) return
    grid.refreshDragHandles(itemEl)
  }, [grid, instanceId, animationClass])

  if (!instance) return null

  const selected = selectedId === instanceId
  const title =
    (typeof instance.settings.title === 'string' && instance.settings.title) ||
    instance.catalogId

  return (
    <div
      key={animationClass || 'no-anim'}
      ref={rootRef}
      className={[
        'dashboard-chart-widget',
        selected ? 'is-selected' : '',
        animationClass,
      ]
        .filter(Boolean)
        .join(' ')}
      onMouseDown={() => onSelect(instanceId)}
    >
      <div
        className={`${DASHBOARD_CHART_DRAG_HANDLE} dashboard-chart-widget__header`}
      >
        <div className="dashboard-chart-widget__titles">
          <p className="dashboard-chart-widget__title">{title}</p>
          <p className="dashboard-chart-widget__meta">
            {instance.chartType} · {instance.w}×{instance.h}
          </p>
        </div>

        <div className="dashboard-chart-no-drag dashboard-chart-widget__actions">
          <Button
            type="button"
            unstyled
            className={btnIconClass}
            title={editLabel}
            aria-label={editLabel}
            onClick={(e: { stopPropagation: () => void }) => {
              e.stopPropagation()
              onEditChart(instanceId)
            }}
            onMouseDown={(e: { stopPropagation: () => void }) =>
              e.stopPropagation()
            }
          >
            <Pencil className="h-3.5 w-3.5" />
          </Button>
          <Button
            type="button"
            unstyled
            className={`${btnIconClass} hover:!text-red-600`}
            title={deleteLabel}
            aria-label={deleteLabel}
            onClick={(e: { stopPropagation: () => void }) => {
              e.stopPropagation()
              onDeleteChart(instanceId)
            }}
            onMouseDown={(e: { stopPropagation: () => void }) =>
              e.stopPropagation()
            }
          >
            <Trash2 className="h-3.5 w-3.5" />
          </Button>
        </div>
      </div>

      <div className="dashboard-chart-no-drag dashboard-chart-widget__body">
        <DashboardChartView instance={instance} />
      </div>
    </div>
  )
}
