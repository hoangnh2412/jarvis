export { DASHBOARD_ROUTES } from '../routes/paths'
export {
  FAKE_DASHBOARD_CHART_CATALOG,
  getFakeDashboardChartCatalog,
} from './fakeCharts'
export {
  DASHBOARD_CHART_ANIMATION_NONE,
  DASHBOARD_CHART_ANIMATION_OPTIONS,
  DASHBOARD_CHART_ANIMATION_FIELD_NAME,
  resolveChartAnimationClass,
} from './chartAnimations'

/** GridStack layout (demo-style) */
export const DASHBOARD_GRID_COLUMNS = 12
export const DASHBOARD_CELL_HEIGHT = 70
export const DASHBOARD_GRID_MARGIN = 8

/** Min block = square 2×2 cells; max unbounded (up to full columns) */
export const DASHBOARD_CHART_MIN_W = 2
export const DASHBOARD_CHART_MIN_H = 2
export const DASHBOARD_CHART_DEFAULT_W = 4
export const DASHBOARD_CHART_DEFAULT_H = 4

export const DASHBOARD_CHART_DRAG_HANDLE = 'dashboard-chart-drag-handle'

/** v2 = GridStack cell layout (invalidates old react-rnd px layouts) */
export const DASHBOARD_LAYOUT_STORAGE_KEY = 'fe-ui-kit:dashboard-layout-v2'

/** @deprecated kept for type compatibility — canvas is full-page GridStack */
export const DASHBOARD_CANVAS_DEFAULT = {
  width: 1280,
  height: 820,
} as const

export const DASHBOARD_CANVAS_PAD = 48
export const DASHBOARD_CHART_MIN_SIZE = DASHBOARD_CHART_MIN_W
export const DASHBOARD_CHART_MIN_WIDTH = DASHBOARD_CHART_MIN_W
export const DASHBOARD_CHART_MIN_HEIGHT = DASHBOARD_CHART_MIN_H
export const DASHBOARD_CHART_DEFAULT_SIZE = DASHBOARD_CHART_DEFAULT_W
export const DASHBOARD_CHART_DEFAULT_WIDTH = DASHBOARD_CHART_DEFAULT_W
export const DASHBOARD_CHART_DEFAULT_HEIGHT = DASHBOARD_CHART_DEFAULT_H

/** Base URL axios — app set VITE_API_URL_DASHBOARD */
/** @deprecated Use shared `BASE_URL` from `@jarvis/core` / `lib/http` */
export { BASE_URL as BASE_URL_DASHBOARD } from '../../../lib/http/constants'
