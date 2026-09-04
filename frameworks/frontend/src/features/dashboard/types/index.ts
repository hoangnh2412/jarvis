/** Chart.js chart types supported by the dashboard module */
export const DashboardChartType = {
  line: 'line',
  bar: 'bar',
  pie: 'pie',
  doughnut: 'doughnut',
} as const

export type DashboardChartTypeValue =
  (typeof DashboardChartType)[keyof typeof DashboardChartType]

export const DASHBOARD_CHART_TYPE_LABEL: Record<
  DashboardChartTypeValue,
  string
> = {
  line: 'Đường',
  bar: 'Cột',
  pie: 'Tròn',
  doughnut: 'Vòng',
}

export const DASHBOARD_CHART_TYPE_OPTIONS = (
  Object.keys(DashboardChartType) as DashboardChartTypeValue[]
).map((value) => ({
  value,
  label: DASHBOARD_CHART_TYPE_LABEL[value],
}))

/** Chart.js-compatible dataset payload from backend */
export type ChartJsDataset = {
  label?: string
  data: number[]
  backgroundColor?: string | string[]
  borderColor?: string | string[]
  borderWidth?: number
  fill?: boolean
  tension?: number
}

export type ChartJsData = {
  labels: string[]
  datasets: ChartJsDataset[]
}

export const SettingFieldType = {
  text: 'text',
  select: 'select',
  switch: 'switch',
  color: 'color',
  number: 'number',
} as const

export type SettingFieldTypeValue =
  (typeof SettingFieldType)[keyof typeof SettingFieldType]

export type SettingFieldOption = {
  label: string
  value: string
}

/** Backend-driven form schema for chart settings */
export type SettingField = {
  name: string
  label: string
  type: SettingFieldTypeValue
  required?: boolean
  options?: SettingFieldOption[]
  min?: number
  max?: number
  step?: number
  placeholder?: string
}

export type ChartSettings = {
  title: string
  chartType: DashboardChartTypeValue
  showLegend: boolean
  color?: string
  /** tw-animate-css class string, e.g. `animate-in fade-in duration-500` */
  cssAnimation?: string
  [key: string]: unknown
}

/** Catalog item from backend — pickable in Add chart dialog */
export type ChartCatalogItem = {
  id: string
  name: string
  description?: string
  chartType: DashboardChartTypeValue
  data: ChartJsData
  settingsForm: SettingField[]
  settings: ChartSettings
  /** Default grid size (columns / rows) when added */
  defaultW?: number
  defaultH?: number
}

export type ChartCatalogResult = {
  items: ChartCatalogItem[]
}

/**
 * Chart instance on GridStack canvas.
 * `x/y/w/h` are grid cells (not pixels) — same model as gridstackjs.com demos.
 */
export type CanvasChartInstance = {
  instanceId: string
  catalogId: string
  x: number
  y: number
  w: number
  h: number
  chartType: DashboardChartTypeValue
  data: ChartJsData
  settings: ChartSettings
  settingsForm: SettingField[]
}

export type DashboardLayoutSnapshot = {
  version: 2
  charts: CanvasChartInstance[]
}
