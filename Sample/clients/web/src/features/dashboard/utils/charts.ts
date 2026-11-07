import {
  DASHBOARD_CHART_DEFAULT_H,
  DASHBOARD_CHART_DEFAULT_W,
  DASHBOARD_CHART_MIN_H,
  DASHBOARD_CHART_MIN_W,
  DASHBOARD_LAYOUT_STORAGE_KEY,
} from '../constants'
import type {
  CanvasChartInstance,
  ChartCatalogItem,
  ChartJsData,
  ChartSettings,
  DashboardChartTypeValue,
  DashboardLayoutSnapshot,
} from '../types'

export function createInstanceId(): string {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
    return crypto.randomUUID()
  }
  return `chart-${Date.now()}-${Math.random().toString(36).slice(2, 9)}`
}

export function cloneChartData(data: ChartJsData): ChartJsData {
  return {
    labels: [...data.labels],
    datasets: data.datasets.map((ds) => ({
      ...ds,
      data: [...ds.data],
      backgroundColor: Array.isArray(ds.backgroundColor)
        ? [...ds.backgroundColor]
        : ds.backgroundColor,
      borderColor: Array.isArray(ds.borderColor)
        ? [...ds.borderColor]
        : ds.borderColor,
    })),
  }
}

/** Apply color setting onto first dataset when it's a single-series chart */
export function applyColorToData(
  data: ChartJsData,
  color: string | undefined,
  chartType: DashboardChartTypeValue,
): ChartJsData {
  if (!color || !data.datasets[0]) return data
  const next = cloneChartData(data)
  const ds = next.datasets[0]
  if (chartType === 'line' || chartType === 'bar') {
    ds.borderColor = color
    if (chartType === 'line') {
      ds.backgroundColor = hexToRgba(color, 0.12)
    } else {
      ds.backgroundColor = color
    }
  }
  return next
}

function hexToRgba(hex: string, alpha: number): string {
  const raw = hex.replace('#', '')
  if (raw.length !== 6) return hex
  const r = parseInt(raw.slice(0, 2), 16)
  const g = parseInt(raw.slice(2, 4), 16)
  const b = parseInt(raw.slice(4, 6), 16)
  return `rgba(${r}, ${g}, ${b}, ${alpha})`
}

function nextGridY(existing: CanvasChartInstance[]): number {
  if (existing.length === 0) return 0
  return existing.reduce((max, c) => Math.max(max, c.y + c.h), 0)
}

export function createChartInstanceFromCatalog(
  item: ChartCatalogItem,
  at: { x: number; y: number } | undefined,
  existing: CanvasChartInstance[],
): CanvasChartInstance {
  const chartType = (item.settings.chartType ??
    item.chartType) as DashboardChartTypeValue
  const settings: ChartSettings = { ...item.settings }
  const data = applyColorToData(
    cloneChartData(item.data),
    typeof settings.color === 'string' ? settings.color : undefined,
    chartType,
  )

  const w = Math.max(
    DASHBOARD_CHART_MIN_W,
    item.defaultW ?? DASHBOARD_CHART_DEFAULT_W,
  )
  const h = Math.max(
    DASHBOARD_CHART_MIN_H,
    item.defaultH ?? DASHBOARD_CHART_DEFAULT_H,
  )

  return {
    instanceId: createInstanceId(),
    catalogId: item.id,
    x: at?.x ?? 0,
    y: at?.y ?? nextGridY(existing),
    w,
    h,
    chartType,
    data,
    settings,
    settingsForm: item.settingsForm.map((f) => ({
      ...f,
      options: f.options ? f.options.map((o) => ({ ...o })) : undefined,
    })),
  }
}

export function patchInstanceSettings(
  instance: CanvasChartInstance,
  settings: ChartSettings,
): CanvasChartInstance {
  const chartType = (settings.chartType ??
    instance.chartType) as DashboardChartTypeValue
  const color =
    typeof settings.color === 'string' ? settings.color : undefined
  return {
    ...instance,
    chartType,
    settings: { ...settings },
    data: applyColorToData(instance.data, color, chartType),
  }
}

function normalizeLoadedChart(raw: Record<string, unknown>): CanvasChartInstance | null {
  if (typeof raw.instanceId !== 'string' || typeof raw.catalogId !== 'string') {
    return null
  }
  // Reject legacy pixel layouts (width/height typically >> 12)
  if ('width' in raw && !('w' in raw)) return null

  const w = Math.max(DASHBOARD_CHART_MIN_W, Number(raw.w) || DASHBOARD_CHART_DEFAULT_W)
  const h = Math.max(DASHBOARD_CHART_MIN_H, Number(raw.h) || DASHBOARD_CHART_DEFAULT_H)

  return {
    instanceId: raw.instanceId,
    catalogId: raw.catalogId,
    x: Math.max(0, Number(raw.x) || 0),
    y: Math.max(0, Number(raw.y) || 0),
    w,
    h,
    chartType: raw.chartType as DashboardChartTypeValue,
    data: raw.data as ChartJsData,
    settings: raw.settings as ChartSettings,
    settingsForm: raw.settingsForm as CanvasChartInstance['settingsForm'],
  }
}

export function loadDashboardLayout(): DashboardLayoutSnapshot | null {
  if (typeof window === 'undefined') return null
  try {
    const raw = window.localStorage.getItem(DASHBOARD_LAYOUT_STORAGE_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw) as {
      version?: number
      charts?: Record<string, unknown>[]
    }
    if (!parsed.charts?.length) return null
    const charts = parsed.charts
      .map(normalizeLoadedChart)
      .filter((c): c is CanvasChartInstance => c != null)
    if (!charts.length) return null
    return { version: 2, charts }
  } catch {
    return null
  }
}

export function saveDashboardLayout(snapshot: DashboardLayoutSnapshot): void {
  if (typeof window === 'undefined') return
  window.localStorage.setItem(
    DASHBOARD_LAYOUT_STORAGE_KEY,
    JSON.stringify({ version: 2, charts: snapshot.charts }),
  )
}

export function clearDashboardLayout(): void {
  if (typeof window === 'undefined') return
  window.localStorage.removeItem(DASHBOARD_LAYOUT_STORAGE_KEY)
}

export function toGridStackWidget(chart: CanvasChartInstance) {
  return {
    id: chart.instanceId,
    x: chart.x,
    y: chart.y,
    w: chart.w,
    h: chart.h,
    minW: DASHBOARD_CHART_MIN_W,
    minH: DASHBOARD_CHART_MIN_H,
    component: 'DashboardChartWidget',
    props: { instanceId: chart.instanceId },
  }
}
