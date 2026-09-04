import type { ChartCatalogItem, SettingField } from '../types'
import { DashboardChartType, SettingFieldType } from '../types'
import {
  DASHBOARD_CHART_ANIMATION_FIELD_NAME,
  DASHBOARD_CHART_ANIMATION_NONE,
  DASHBOARD_CHART_ANIMATION_OPTIONS,
} from './chartAnimations'

const commonSettingsForm = (
  chartTypeOptions: Array<{ label: string; value: string }>,
): SettingField[] => [
  {
    name: 'title',
    label: 'Tiêu đề',
    type: SettingFieldType.text,
    required: true,
    placeholder: 'Nhập tiêu đề biểu đồ',
  },
  {
    name: 'chartType',
    label: 'Loại biểu đồ',
    type: SettingFieldType.select,
    required: true,
    options: chartTypeOptions,
  },
  {
    name: DASHBOARD_CHART_ANIMATION_FIELD_NAME,
    label: 'CSS Animation',
    type: SettingFieldType.select,
    options: DASHBOARD_CHART_ANIMATION_OPTIONS,
  },
  {
    name: 'showLegend',
    label: 'Hiện chú thích',
    type: SettingFieldType.switch,
  },
  {
    name: 'color',
    label: 'Màu chủ đạo',
    type: SettingFieldType.color,
  },
]

/** Fake catalog — 4 charts with Chart.js-ready data for local testing */
export const FAKE_DASHBOARD_CHART_CATALOG: ChartCatalogItem[] = [
  {
    id: 'revenue-daily',
    name: 'Doanh thu theo ngày',
    description: 'Theo dõi doanh thu 7 ngày gần nhất (line chart).',
    chartType: DashboardChartType.line,
    defaultW: 4,
    defaultH: 4,
    data: {
      labels: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'],
      datasets: [
        {
          label: 'Doanh thu (triệu)',
          data: [42, 58, 51, 73, 66, 89, 77],
          borderColor: '#0d9488',
          backgroundColor: 'rgba(13, 148, 136, 0.12)',
          borderWidth: 2,
          fill: true,
          tension: 0.35,
        },
      ],
    },
    settingsForm: commonSettingsForm([
      { label: 'Đường', value: 'line' },
      { label: 'Cột', value: 'bar' },
    ]),
    settings: {
      title: 'Doanh thu theo ngày',
      chartType: DashboardChartType.line,
      showLegend: true,
      color: '#0d9488',
      cssAnimation: DASHBOARD_CHART_ANIMATION_NONE,
    },
  },
  {
    id: 'orders-by-status',
    name: 'Đơn hàng theo trạng thái',
    description: 'Tỷ trọng trạng thái đơn (doughnut).',
    chartType: DashboardChartType.doughnut,
    defaultW: 4,
    defaultH: 4,
    data: {
      labels: ['Mới', 'Đang xử lý', 'Đang giao', 'Hoàn tất', 'Hủy'],
      datasets: [
        {
          label: 'Đơn hàng',
          data: [48, 32, 27, 94, 11],
          backgroundColor: [
            '#0d9488',
            '#0284c7',
            '#ca8a04',
            '#16a34a',
            '#dc2626',
          ],
          borderColor: '#ffffff',
          borderWidth: 2,
        },
      ],
    },
    settingsForm: commonSettingsForm([
      { label: 'Vòng', value: 'doughnut' },
      { label: 'Tròn', value: 'pie' },
    ]),
    settings: {
      title: 'Đơn hàng theo trạng thái',
      chartType: DashboardChartType.doughnut,
      showLegend: true,
      color: '#0d9488',
      cssAnimation: DASHBOARD_CHART_ANIMATION_NONE,
    },
  },
  {
    id: 'new-customers',
    name: 'Khách hàng mới',
    description: 'Số khách đăng ký theo tháng (bar chart).',
    chartType: DashboardChartType.bar,
    defaultW: 4,
    defaultH: 4,
    data: {
      labels: ['Th1', 'Th2', 'Th3', 'Th4', 'Th5', 'Th6'],
      datasets: [
        {
          label: 'Khách mới',
          data: [120, 145, 132, 178, 166, 201],
          backgroundColor: '#0284c7',
          borderColor: '#0284c7',
          borderWidth: 0,
        },
      ],
    },
    settingsForm: commonSettingsForm([
      { label: 'Cột', value: 'bar' },
      { label: 'Đường', value: 'line' },
    ]),
    settings: {
      title: 'Khách hàng mới',
      chartType: DashboardChartType.bar,
      showLegend: true,
      color: '#0284c7',
      cssAnimation: DASHBOARD_CHART_ANIMATION_NONE,
    },
  },
  {
    id: 'sales-by-category',
    name: 'Doanh số theo danh mục',
    description: 'Phân bổ doanh số theo nhóm sản phẩm (pie).',
    chartType: DashboardChartType.pie,
    defaultW: 4,
    defaultH: 4,
    data: {
      labels: ['Điện tử', 'Thời trang', 'Gia dụng', 'Thực phẩm', 'Khác'],
      datasets: [
        {
          label: 'Doanh số',
          data: [35, 22, 18, 15, 10],
          backgroundColor: [
            '#0f766e',
            '#0369a1',
            '#a16207',
            '#15803d',
            '#9333ea',
          ],
          borderColor: '#ffffff',
          borderWidth: 2,
        },
      ],
    },
    settingsForm: commonSettingsForm([
      { label: 'Tròn', value: 'pie' },
      { label: 'Vòng', value: 'doughnut' },
    ]),
    settings: {
      title: 'Doanh số theo danh mục',
      chartType: DashboardChartType.pie,
      showLegend: true,
      color: '#0f766e',
      cssAnimation: DASHBOARD_CHART_ANIMATION_NONE,
    },
  },
]

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
