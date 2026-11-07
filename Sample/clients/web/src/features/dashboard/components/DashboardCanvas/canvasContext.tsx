import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  type ReactNode,
} from 'react'
import type { CanvasChartInstance } from '../../types'

export type DashboardCanvasContextValue = {
  charts: CanvasChartInstance[]
  selectedId: string | null
  onSelect: (id: string | null) => void
  onEditChart: (id: string) => void
  onDeleteChart: (id: string) => void
  editLabel: string
  deleteLabel: string
}

const DashboardCanvasContext =
  createContext<DashboardCanvasContextValue | null>(null)

export function DashboardCanvasProvider({
  value,
  children,
}: {
  value: DashboardCanvasContextValue
  children: ReactNode
}) {
  return (
    <DashboardCanvasContext.Provider value={value}>
      {children}
    </DashboardCanvasContext.Provider>
  )
}

export function useDashboardCanvasContext(): DashboardCanvasContextValue {
  const ctx = useContext(DashboardCanvasContext)
  if (!ctx) {
    throw new Error(
      'useDashboardCanvasContext must be used within DashboardCanvasProvider',
    )
  }
  return ctx
}

export function useDashboardChartInstance(
  instanceId: string,
): CanvasChartInstance | undefined {
  const { charts } = useDashboardCanvasContext()
  return useMemo(
    () => charts.find((c) => c.instanceId === instanceId),
    [charts, instanceId],
  )
}

/** Stable ref of latest charts for GridSync without re-subscribing every render */
export function useChartsRef(charts: CanvasChartInstance[]) {
  const ref = useRef(charts)
  useEffect(() => {
    ref.current = charts
  }, [charts])
  return ref
}

export function useLayoutSyncHandler(
  onChangeChart: (id: string, patch: Partial<CanvasChartInstance>) => void,
  onChangeCharts?: (
    patches: Array<{ id: string; patch: Partial<CanvasChartInstance> }>,
  ) => void,
) {
  return useCallback(
    (
      nodes: Array<{
        id?: string | number
        x?: number
        y?: number
        w?: number
        h?: number
      }>,
    ) => {
      const patches = nodes
        .filter((n) => n.id != null)
        .map((n) => ({
          id: String(n.id),
          patch: {
            x: n.x ?? 0,
            y: n.y ?? 0,
            w: n.w ?? 1,
            h: n.h ?? 1,
          },
        }))
      if (!patches.length) return
      if (onChangeCharts) {
        onChangeCharts(patches)
        return
      }
      for (const p of patches) {
        onChangeChart(p.id, p.patch)
      }
    },
    [onChangeChart, onChangeCharts],
  )
}
