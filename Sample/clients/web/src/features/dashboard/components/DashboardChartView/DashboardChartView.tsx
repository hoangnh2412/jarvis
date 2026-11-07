import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler,
  type ChartOptions,
  type ChartType,
  type TooltipItem,
} from 'chart.js'
import { Bar, Doughnut, Line, Pie } from 'react-chartjs-2'
import type {
  CanvasChartInstance,
  DashboardChartTypeValue,
} from '../../types'

let registered = false

export function ensureChartJsRegistered() {
  if (registered) return
  ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    BarElement,
    ArcElement,
    Title,
    Tooltip,
    Legend,
    Filler,
  )
  registered = true
}

export type DashboardChartViewProps = {
  instance: CanvasChartInstance
  className?: string
}

function ChartByType({
  type,
  data,
  options,
}: {
  type: DashboardChartTypeValue
  data: CanvasChartInstance['data']
  options: ChartOptions
}) {
  switch (type) {
    case 'bar':
      return <Bar data={data} options={options as ChartOptions<'bar'>} />
    case 'pie':
      return <Pie data={data} options={options as ChartOptions<'pie'>} />
    case 'doughnut':
      return (
        <Doughnut data={data} options={options as ChartOptions<'doughnut'>} />
      )
    case 'line':
    default:
      return <Line data={data} options={options as ChartOptions<'line'>} />
  }
}

function formatTooltipValue(value: unknown): string {
  if (typeof value === 'number') {
    return Number.isInteger(value)
      ? value.toLocaleString('vi-VN')
      : value.toLocaleString('vi-VN', { maximumFractionDigits: 2 })
  }
  if (value == null) return '—'
  return String(value)
}

function resolveTooltipNumber(item: TooltipItem<ChartType>): number | null {
  const parsed = item.parsed as unknown
  if (typeof parsed === 'number') return parsed
  if (parsed && typeof parsed === 'object' && 'y' in parsed) {
    const y = (parsed as { y: unknown }).y
    if (typeof y === 'number') return y
  }
  if (typeof item.raw === 'number') return item.raw
  return null
}

function buildChartOptions(
  instance: CanvasChartInstance,
): ChartOptions {
  const isArc =
    instance.chartType === 'pie' || instance.chartType === 'doughnut'

  return {
    responsive: true,
    maintainAspectRatio: false,
    interaction: {
      mode: isArc ? 'nearest' : 'index',
      intersect: isArc,
    },
    plugins: {
      legend: {
        display: Boolean(instance.settings.showLegend),
        position: 'bottom',
        labels: {
          boxWidth: 12,
          font: { size: 11 },
          color: '#475569',
        },
      },
      title: { display: false },
      tooltip: {
        enabled: true,
        backgroundColor: 'rgba(15, 23, 42, 0.92)',
        titleColor: '#f8fafc',
        bodyColor: '#e2e8f0',
        borderColor: 'rgba(148, 163, 184, 0.35)',
        borderWidth: 1,
        padding: 12,
        cornerRadius: 8,
        displayColors: true,
        boxPadding: 4,
        callbacks: {
          title(items) {
            if (!items.length) return ''
            const item = items[0]
            if (isArc) {
              const label =
                item.label ||
                item.chart.data.labels?.[item.dataIndex]
              return label != null ? String(label) : ''
            }
            return String(item.label ?? '')
          },
          label(item) {
            const num = resolveTooltipNumber(item)
            const formatted = formatTooltipValue(num ?? item.raw)
            const dsLabel = item.dataset.label?.trim()

            if (isArc) {
              const data = item.dataset.data as Array<number | null>
              const total = data.reduce<number>(
                (sum, n) => sum + (typeof n === 'number' ? n : 0),
                0,
              )
              const pct =
                total > 0 && num != null
                  ? ` (${((num / total) * 100).toFixed(1)}%)`
                  : ''
              return ` ${formatted}${pct}`
            }

            return dsLabel ? ` ${dsLabel}: ${formatted}` : ` ${formatted}`
          },
        },
      },
    },
    scales: isArc
      ? undefined
      : {
          x: {
            grid: { color: 'rgba(148, 163, 184, 0.2)' },
            ticks: { color: '#64748b', font: { size: 11 } },
          },
          y: {
            grid: { color: 'rgba(148, 163, 184, 0.2)' },
            ticks: { color: '#64748b', font: { size: 11 } },
            beginAtZero: true,
          },
        },
  }
}

export function DashboardChartView({
  instance,
  className = '',
}: DashboardChartViewProps) {
  ensureChartJsRegistered()
  const options = buildChartOptions(instance)

  return (
    <div
      className={`dashboard-chart-view relative h-full w-full min-h-0 ${className}`}
    >
      <ChartByType
        type={instance.chartType}
        data={instance.data}
        options={options}
      />
    </div>
  )
}
