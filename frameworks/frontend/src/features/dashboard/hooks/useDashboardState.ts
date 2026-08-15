import { useCallback, useEffect, useState } from 'react'
import { notify } from '../../../common/Toaster'
import { getErrorMessage } from '../../../lib'
import { DASHBOARD_CHART_MIN_H, DASHBOARD_CHART_MIN_W } from '../constants'
import { callGetDashboardChartCatalog } from '../services'
import type {
  CanvasChartInstance,
  ChartCatalogItem,
  ChartSettings,
  DashboardLayoutSnapshot,
} from '../types'
import {
  clearDashboardLayout,
  createChartInstanceFromCatalog,
  loadDashboardLayout,
  patchInstanceSettings,
  saveDashboardLayout,
} from '../utils'

export type UseDashboardStateOptions = {
  /** Restore layout from localStorage on mount */
  persistLayout?: boolean
}

export type DashboardState = {
  catalog: ChartCatalogItem[]
  loadingCatalog: boolean
  charts: CanvasChartInstance[]
  selectedId: string | null
  setSelectedId: (id: string | null) => void
  reloadCatalog: () => Promise<void>
  addChart: (item: ChartCatalogItem, at?: { x: number; y: number }) => void
  updateChart: (id: string, patch: Partial<CanvasChartInstance>) => void
  updateCharts: (
    patches: Array<{ id: string; patch: Partial<CanvasChartInstance> }>,
  ) => void
  applySettings: (id: string, settings: ChartSettings) => void
  removeChart: (id: string) => void
  clearCharts: () => void
  saveLayout: () => void
  getSnapshot: () => DashboardLayoutSnapshot
}

function readPersistedCharts(): CanvasChartInstance[] {
  const saved = loadDashboardLayout()
  if (!saved?.charts?.length) return []
  return saved.charts.map((c) => ({
    ...c,
    w: Math.max(DASHBOARD_CHART_MIN_W, c.w),
    h: Math.max(DASHBOARD_CHART_MIN_H, c.h),
  }))
}

export function useDashboardState(
  options: UseDashboardStateOptions = {},
): DashboardState {
  const { persistLayout = true } = options
  const [catalog, setCatalog] = useState<ChartCatalogItem[]>([])
  const [loadingCatalog, setLoadingCatalog] = useState(false)
  /** Hydrate sync ngay lần đầu — tránh GridSync khởi tạo với charts=[] rồi empty overlay kẹt */
  const [charts, setCharts] = useState<CanvasChartInstance[]>(() =>
    persistLayout ? readPersistedCharts() : [],
  )
  const [selectedId, setSelectedId] = useState<string | null>(null)

  const reloadCatalog = useCallback(async () => {
    setLoadingCatalog(true)
    try {
      const result = await callGetDashboardChartCatalog()
      setCatalog(result.items ?? [])
    } catch (err) {
      notify.error(getErrorMessage(err, 'Không tải được catalog biểu đồ'))
    } finally {
      setLoadingCatalog(false)
    }
  }, [])

  useEffect(() => {
    void reloadCatalog()
  }, [reloadCatalog])

  const addChart = useCallback(
    (item: ChartCatalogItem, at?: { x: number; y: number }) => {
      setCharts((prev) => {
        const instance = createChartInstanceFromCatalog(item, at, prev)
        setSelectedId(instance.instanceId)
        return [...prev, instance]
      })
    },
    [],
  )

  const updateChart = useCallback(
    (id: string, patch: Partial<CanvasChartInstance>) => {
      setCharts((prev) =>
        prev.map((c) => (c.instanceId === id ? { ...c, ...patch } : c)),
      )
    },
    [],
  )

  const updateCharts = useCallback(
    (
      patches: Array<{ id: string; patch: Partial<CanvasChartInstance> }>,
    ) => {
      if (!patches.length) return
      const map = new Map(patches.map((p) => [p.id, p.patch]))
      setCharts((prev) =>
        prev.map((c) => {
          const patch = map.get(c.instanceId)
          return patch ? { ...c, ...patch } : c
        }),
      )
    },
    [],
  )

  const applySettings = useCallback((id: string, settings: ChartSettings) => {
    setCharts((prev) =>
      prev.map((c) =>
        c.instanceId === id ? patchInstanceSettings(c, settings) : c,
      ),
    )
  }, [])

  const removeChart = useCallback((id: string) => {
    setCharts((prev) => prev.filter((c) => c.instanceId !== id))
    setSelectedId((cur) => (cur === id ? null : cur))
  }, [])

  const clearCharts = useCallback(() => {
    setCharts([])
    setSelectedId(null)
    clearDashboardLayout()
  }, [])

  const getSnapshot = useCallback(
    (): DashboardLayoutSnapshot => ({ version: 2, charts }),
    [charts],
  )

  const saveLayout = useCallback(() => {
    saveDashboardLayout({ version: 2, charts })
    notify.success('Đã lưu bố cục')
  }, [charts])

  return {
    catalog,
    loadingCatalog,
    charts,
    selectedId,
    setSelectedId,
    reloadCatalog,
    addChart,
    updateChart,
    updateCharts,
    applySettings,
    removeChart,
    clearCharts,
    saveLayout,
    getSnapshot,
  }
}
