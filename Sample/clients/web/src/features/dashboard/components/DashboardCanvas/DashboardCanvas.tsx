import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  type ComponentType,
  type ReactNode,
} from 'react'
import { BarChart3 } from 'lucide-react'
import type { GridItemHTMLElement, GridStackOptions } from 'gridstack'
import {
  GridStack,
  useGridStack,
  type GridStackHandle,
} from 'gridstack/dist/react'
import {
  DASHBOARD_CELL_HEIGHT,
  DASHBOARD_CHART_DRAG_HANDLE,
  DASHBOARD_GRID_COLUMNS,
  DASHBOARD_GRID_MARGIN,
} from '../../constants'
import type { CanvasChartInstance } from '../../types'
import { toGridStackWidget } from '../../utils'
import { ensureGridstackStyles } from '../../utils/ensureGridstackStyles'
import { ensureTwAnimateStyles } from '../../utils/ensureTwAnimateStyles'
import { DashboardChartWidget } from '../DashboardChartCard/DashboardChartCard'
import {
  DashboardCanvasProvider,
  useLayoutSyncHandler,
} from './canvasContext'

export type DashboardCanvasProps = {
  charts: CanvasChartInstance[]
  selectedId: string | null
  onSelect: (id: string | null) => void
  onChangeChart: (id: string, patch: Partial<CanvasChartInstance>) => void
  /** Batch layout commit (preferred for smooth GridStack sync) */
  onChangeCharts?: (
    patches: Array<{ id: string; patch: Partial<CanvasChartInstance> }>,
  ) => void
  onEditChart: (id: string) => void
  onDeleteChart: (id: string) => void
  emptyTitle?: string
  emptyHint?: string
  editLabel?: string
  deleteLabel?: string
  className?: string
  emptyContent?: ReactNode
}

const GRID_COMPONENTS = {
  DashboardChartWidget:
    DashboardChartWidget as unknown as ComponentType<Record<string, unknown>>,
}

/** Sync add/remove by id only — không chạy lại khi chỉ đổi x/y/w/h (tránh giật khi kéo) */
function GridSync({ charts }: { charts: CanvasChartInstance[] }) {
  const { grid, addWidget, removeWidget, removeAll } = useGridStack()
  const knownRef = useRef<Set<string> | null>(null)
  const chartsRef = useRef(charts)
  chartsRef.current = charts

  const idKey = useMemo(
    () => charts.map((c) => c.instanceId).join('|'),
    [charts],
  )

  useEffect(() => {
    if (!grid) return
    const list = chartsRef.current
    const ids = new Set(list.map((c) => c.instanceId))

    if (knownRef.current === null) {
      knownRef.current = new Set()
      const existing = new Set(
        (grid.engine.nodes ?? [])
          .map((n) => (n.id != null ? String(n.id) : ''))
          .filter(Boolean),
      )
      for (const c of list) {
        if (!existing.has(c.instanceId)) {
          addWidget(toGridStackWidget(c))
        }
        knownRef.current.add(c.instanceId)
      }
      return
    }

    for (const id of [...knownRef.current]) {
      if (!ids.has(id)) {
        const node = grid.engine.nodes.find((n) => String(n.id) === id)
        if (node?.el) removeWidget(node.el, true, false)
        knownRef.current.delete(id)
      }
    }

    for (const c of list) {
      if (!knownRef.current.has(c.instanceId)) {
        addWidget(toGridStackWidget(c))
        knownRef.current.add(c.instanceId)
      }
    }

    if (list.length === 0 && knownRef.current.size > 0) {
      removeAll(true)
      knownRef.current.clear()
    }
  }, [addWidget, grid, idKey, removeAll, removeWidget])

  return null
}

function DefaultEmpty({
  title,
  hint,
}: {
  title: string
  hint: string
}) {
  return (
    <div className="pointer-events-none absolute inset-0 z-[1] flex flex-col items-center justify-center px-6 text-center">
      <div className="mb-3 flex h-14 w-14 items-center justify-center rounded-2xl bg-teal-50 text-teal-700 ring-1 ring-teal-100">
        <BarChart3 className="h-7 w-7" />
      </div>
      <p className="m-0 text-base font-semibold text-slate-800">{title}</p>
      <p className="mt-1.5 m-0 max-w-sm text-sm leading-relaxed text-slate-500">
        {hint}
      </p>
    </div>
  )
}

export function DashboardCanvas({
  charts,
  selectedId,
  onSelect,
  onChangeChart,
  onChangeCharts,
  onEditChart,
  onDeleteChart,
  emptyTitle = 'Dashboard trống',
  emptyHint = 'Thêm biểu đồ từ header để bắt đầu.',
  editLabel = 'Sửa',
  deleteLabel = 'Xóa',
  className = '',
  emptyContent,
}: DashboardCanvasProps) {
  const gridRef = useRef<GridStackHandle>(null)
  const syncLayout = useLayoutSyncHandler(onChangeChart, onChangeCharts)
  const isEmpty = charts.length === 0

  useEffect(() => {
    ensureGridstackStyles()
    ensureTwAnimateStyles()
  }, [])

  const options = useMemo<GridStackOptions>(
    () => ({
      column: DASHBOARD_GRID_COLUMNS,
      cellHeight: DASHBOARD_CELL_HEIGHT,
      margin: DASHBOARD_GRID_MARGIN,
      float: true,
      animate: true,
      minRow: 4,
      disableOneColumnMode: true,
      alwaysShowResizeHandle: true,
      draggable: {
        handle: `.${DASHBOARD_CHART_DRAG_HANDLE}`,
        cancel: '.dashboard-chart-no-drag',
        scroll: true,
      },
      resizable: {
        handles: 'e, se, s, sw, w, n, ne, nw',
        autoHide: false,
      },
      children: [],
    }),
    [],
  )

  const ctxValue = useMemo(
    () => ({
      charts,
      selectedId,
      onSelect,
      onEditChart,
      onDeleteChart,
      editLabel,
      deleteLabel,
    }),
    [
      charts,
      selectedId,
      onSelect,
      onEditChart,
      onDeleteChart,
      editLabel,
      deleteLabel,
    ],
  )

  const commitLayout = useCallback(() => {
    const grid = gridRef.current?.getGrid()
    if (!grid?.engine?.nodes) return
    syncLayout(
      grid.engine.nodes
        .filter((n) => n.id != null)
        .map((n) => ({
          id: n.id!,
          x: n.x,
          y: n.y,
          w: n.w,
          h: n.h,
        })),
    )
  }, [syncLayout])

  const commitNode = useCallback(
    (_e: Event, _el: GridItemHTMLElement) => {
      commitLayout()
    },
    [commitLayout],
  )

  return (
    <DashboardCanvasProvider value={ctxValue}>
      <div
        className={`dashboard-grid-viewport relative min-h-0 flex-1 overflow-auto ${className}`}
        onClick={() => onSelect(null)}
      >
        <div
          className="dashboard-grid-board relative min-h-full w-full"
          onClick={(e) => e.stopPropagation()}
        >
          {/*
            Empty state tự quản theo charts.length — không dùng GridStack emptyContent.
            GS isEmpty hay kẹt true khi addWidget chạy trước listener "added".
          */}
          {isEmpty
            ? (emptyContent ?? (
                <DefaultEmpty title={emptyTitle} hint={emptyHint} />
              ))
            : null}

          <GridStack
            ref={gridRef}
            options={options}
            components={GRID_COMPONENTS}
            className="dashboard-grid-stack"
            onDragStop={commitNode}
            onResizeStop={commitNode}
          >
            <GridSync charts={charts} />
          </GridStack>
        </div>
      </div>
    </DashboardCanvasProvider>
  )
}
