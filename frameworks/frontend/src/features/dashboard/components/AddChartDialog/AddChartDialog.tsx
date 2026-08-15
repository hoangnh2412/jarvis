import { BarChart3, LineChart, PieChart } from 'lucide-react'
import { Button } from 'primereact/button'
import { Dialog } from 'primereact/dialog'
import type { ChartCatalogItem, DashboardChartTypeValue } from '../../types'
import { DASHBOARD_CHART_TYPE_LABEL } from '../../types'
import { btnOutlinedClass } from '../fieldStyles'

export type AddChartDialogProps = {
  visible: boolean
  onHide: () => void
  items: ChartCatalogItem[]
  loading?: boolean
  title?: string
  hint?: string
  addLabel?: string
  cancelLabel?: string
  emptyLabel?: string
  onAdd: (item: ChartCatalogItem) => void
}

function ChartTypeIcon({ type }: { type: DashboardChartTypeValue }) {
  const className = 'h-5 w-5 text-teal-700'
  if (type === 'bar') return <BarChart3 className={className} aria-hidden />
  if (type === 'pie' || type === 'doughnut')
    return <PieChart className={className} aria-hidden />
  return <LineChart className={className} aria-hidden />
}

export function AddChartDialog({
  visible,
  onHide,
  items = [],
  loading = false,
  title = 'Chọn biểu đồ',
  hint,
  addLabel = 'Thêm vào canvas',
  cancelLabel = 'Hủy',
  emptyLabel = 'Không có biểu đồ nào trong catalog',
  onAdd,
}: AddChartDialogProps) {
  return (
    <Dialog.Root
      open={visible}
      onOpenChange={(e: { value?: boolean }) => {
        if (!e.value) onHide()
      }}
    >
      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-[100] bg-slate-900/40 backdrop-blur-[2px]" />
        <Dialog.Positioner className="fixed inset-0 z-[110] flex items-center justify-center p-4">
          <Dialog.Popup className="flex max-h-[85vh] w-full max-w-[720px] flex-col overflow-hidden rounded-xl border border-slate-200 bg-white p-0 shadow-xl">
            <Dialog.Header className="flex items-start justify-between gap-3 border-b border-slate-100 px-5 py-4">
              <div>
                <Dialog.Title className="text-lg font-semibold text-slate-900">
                  {title}
                </Dialog.Title>
                {hint && (
                  <p className="mt-1 m-0 text-sm text-slate-500">{hint}</p>
                )}
              </div>
              <Dialog.Close
                className="rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                aria-label="Đóng"
              />
            </Dialog.Header>

            <Dialog.Content className="min-h-0 flex-1 overflow-auto bg-slate-50/80 p-4">
              {loading ? (
                <p className="m-0 text-sm text-slate-500">Đang tải…</p>
              ) : items.length === 0 ? (
                <p className="m-0 text-sm text-slate-500">{emptyLabel}</p>
              ) : (
                <ul className="m-0 grid list-none grid-cols-1 gap-3 p-0 sm:grid-cols-2">
                  {items.map((item) => (
                    <li key={item.id}>
                      <button
                        type="button"
                        className="group flex w-full flex-col rounded-xl border border-slate-200 bg-white p-4 text-left shadow-sm transition hover:-translate-y-px hover:border-teal-300 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30"
                        onClick={() => {
                          onAdd(item)
                          onHide()
                        }}
                      >
                        <div className="mb-3 flex items-start justify-between gap-2">
                          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-teal-50 ring-1 ring-teal-100">
                            <ChartTypeIcon type={item.chartType} />
                          </div>
                          <span className="rounded-md bg-slate-100 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-slate-600">
                            {DASHBOARD_CHART_TYPE_LABEL[item.chartType] ??
                              item.chartType}
                          </span>
                        </div>
                        <p className="m-0 text-sm font-semibold text-slate-900">
                          {item.name}
                        </p>
                        {item.description && (
                          <p className="mt-1 m-0 text-xs leading-relaxed text-slate-500">
                            {item.description}
                          </p>
                        )}
                        <span className="mt-3 inline-flex items-center text-xs font-semibold text-teal-700 opacity-0 transition group-hover:opacity-100">
                          {addLabel} →
                        </span>
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </Dialog.Content>

            <Dialog.Footer className="flex justify-end gap-2 border-t border-slate-100 px-5 py-3">
              <Button
                type="button"
                unstyled
                className={btnOutlinedClass}
                onClick={onHide}
              >
                {cancelLabel}
              </Button>
            </Dialog.Footer>
          </Dialog.Popup>
        </Dialog.Positioner>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
