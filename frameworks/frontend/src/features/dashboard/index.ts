export { DashboardPage } from './pages/DashboardEditor'
export type {
  DashboardPageProps,
  DashboardPageContentContext,
} from './pages/DashboardEditor'

export { DashboardHeader } from './components/DashboardHeader'
export type { DashboardHeaderProps } from './components/DashboardHeader'

export { DashboardCanvas } from './components/DashboardCanvas'
export type { DashboardCanvasProps } from './components/DashboardCanvas'

export { DashboardChartCard, DashboardChartWidget } from './components/DashboardChartCard'
export type {
  DashboardChartCardProps,
  DashboardChartWidgetProps,
} from './components/DashboardChartCard'

export {
  DashboardChartView,
  ensureChartJsRegistered,
} from './components/DashboardChartView'
export type { DashboardChartViewProps } from './components/DashboardChartView'

export { AddChartDialog } from './components/AddChartDialog'
export type { AddChartDialogProps } from './components/AddChartDialog'

export { ChartSettingsDialog } from './components/ChartSettingsDialog'
export type { ChartSettingsDialogProps } from './components/ChartSettingsDialog'

export {
  DashboardChartType,
  DASHBOARD_CHART_TYPE_LABEL,
  DASHBOARD_CHART_TYPE_OPTIONS,
  SettingFieldType,
} from './types'
export type {
  DashboardChartTypeValue,
  ChartJsDataset,
  ChartJsData,
  SettingFieldTypeValue,
  SettingFieldOption,
  SettingField,
  ChartSettings,
  ChartCatalogItem,
  ChartCatalogResult,
  CanvasChartInstance,
  DashboardLayoutSnapshot,
} from './types'

export {
  DASHBOARD_GRID_COLUMNS,
  DASHBOARD_CELL_HEIGHT,
  DASHBOARD_GRID_MARGIN,
  DASHBOARD_CHART_MIN_W,
  DASHBOARD_CHART_MIN_H,
  DASHBOARD_CHART_DEFAULT_W,
  DASHBOARD_CHART_DEFAULT_H,
  DASHBOARD_CANVAS_DEFAULT,
  DASHBOARD_CANVAS_PAD,
  DASHBOARD_CHART_MIN_SIZE,
  DASHBOARD_CHART_MIN_WIDTH,
  DASHBOARD_CHART_MIN_HEIGHT,
  DASHBOARD_CHART_DEFAULT_SIZE,
  DASHBOARD_CHART_DEFAULT_WIDTH,
  DASHBOARD_CHART_DEFAULT_HEIGHT,
  DASHBOARD_CHART_DRAG_HANDLE,
  DASHBOARD_LAYOUT_STORAGE_KEY,
  BASE_URL_DASHBOARD,
  FAKE_DASHBOARD_CHART_CATALOG,
  getFakeDashboardChartCatalog,
  DASHBOARD_CHART_ANIMATION_NONE,
  DASHBOARD_CHART_ANIMATION_OPTIONS,
  DASHBOARD_CHART_ANIMATION_FIELD_NAME,
  resolveChartAnimationClass,
} from './constants'

export { useDashboardState } from './hooks'
export type {
  UseDashboardStateOptions,
  DashboardState,
} from './hooks'

export {
  dashboardHttp,
  callGetDashboardChartCatalog,
} from './services'

export {
  DASHBOARD_ROUTES,
  getDashboardHomePath,
  getDashboardRouteList,
  configureDashboardNavigate,
  navigateDashboard,
  dashboardPaths,
} from './routes'
export type {
  DashboardRouteKey,
  DashboardRouteItem,
  DashboardNavigateFn,
} from './routes'

export { dashboardMenuItems } from './menu'
export type { DashboardMenuItem } from './menu'

export {
  DASHBOARD_PERMISSIONS,
  hasDashboardPermission,
} from './permission'
export type { DashboardPermissionKey } from './permission'

export {
  dashboardMessages,
  getDashboardMessages,
} from './localization'
export type { DashboardLocale, DashboardMessages } from './localization'

export { defaultDashboardTheme } from './theme'
export type { DashboardTheme } from './theme'

export {
  resolveDashboardContent,
  createInstanceId,
  cloneChartData,
  applyColorToData,
  createChartInstanceFromCatalog,
  patchInstanceSettings,
  loadDashboardLayout,
  saveDashboardLayout,
  clearDashboardLayout,
  toGridStackWidget,
  ensureGridstackStyles,
  ensureTwAnimateStyles,
} from './utils'
export type { DashboardSlotContent } from './utils'
