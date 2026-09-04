import { getFakeDashboardChartCatalog } from '../constants'
import type { ChartCatalogResult } from '../types'
import dashboardHttp from './req'

/**
 * Load chart catalog (Chart.js data + settingsForm).
 * Falls back to fake data when API is unavailable.
 */
export async function callGetDashboardChartCatalog(): Promise<ChartCatalogResult> {
  try {
    const { data } =
      await dashboardHttp.get<ChartCatalogResult>('v1/dashboard/charts')
    const items = Array.isArray(data?.items) ? data.items : null
    if (items && items.length > 0) {
      return { items }
    }
    return { items: getFakeDashboardChartCatalog() }
  } catch {
    return { items: getFakeDashboardChartCatalog() }
  }
}

export { dashboardHttp }
