import chartsMock from '../mocks/get-charts.json'
import type { ChartCatalogItem } from '../types'

/** Fake catalog — nguồn: `mocks/get-charts.json`. */
export const FAKE_DASHBOARD_CHART_CATALOG: ChartCatalogItem[] =
  chartsMock.items as ChartCatalogItem[]

export function getFakeDashboardChartCatalog(): ChartCatalogItem[] {
  return FAKE_DASHBOARD_CHART_CATALOG.map((item) => ({
    ...item,
    data: {
      labels: [...item.data.labels],
      datasets: item.data.datasets.map((ds) => ({
        ...ds,
        data: [...ds.data],
        backgroundColor: Array.isArray(ds.backgroundColor)
          ? [...ds.backgroundColor]
          : ds.backgroundColor,
        borderColor: Array.isArray(ds.borderColor)
          ? [...ds.borderColor]
          : ds.borderColor,
      })),
    },
    settingsForm: item.settingsForm.map((f) => ({
      ...f,
      options: f.options ? f.options.map((o) => ({ ...o })) : undefined,
    })),
    settings: { ...item.settings },
  }))
}
