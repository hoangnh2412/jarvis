import { useMemo, useState, type ReactNode } from 'react'
import {
  ArrowDown,
  ArrowUp,
  ArrowUpDown,
  Columns3,
  Inbox,
  Search,
} from 'lucide-react'
import { Popover } from 'primereact/popover'
import { DataTable } from 'primereact/datatable'
import { RowActions, type RowActionItem } from './RowActions'

export type { RowActionItem }
import { useListPagination } from './ListPagination'

export const ADMIN_LIST_PAGE_SIZES = [10, 20, 50] as const

export type AdminListColumn<T> = {
  key: string
  label: string
  className?: string
  sortable?: boolean
  sortValue?: (item: T) => string | number | null | undefined
  render?: (item: T) => ReactNode
  value?: (item: T) => ReactNode
  defaultVisible?: boolean
}

export type AdminListPaging = {
  total: number
  page: number
  size: number
  onPageChange: (page: number) => void
  onSizeChange: (size: number) => void
}

export type AdminListProps<T extends { id: string }> = {
  items: T[]
  columns: AdminListColumn<T>[]
  loading?: boolean
  emptyMessage?: string
  searchValue: string
  onSearchChange: (value: string) => void
  searchPlaceholder?: string
  searchLabel?: string
  primaryAction?: ReactNode
  extraActions?: ReactNode
  actionsFor?: (item: T) => RowActionItem[]
  /** Khi có, danh sách đã cắt trang ở ngoài. Không truyền thì cắt trên `items`. */
  paging?: AdminListPaging
  className?: string
}

type SortState = { key: string; dir: 'asc' | 'desc' }

function compareSortValues(
  left: string | number | null | undefined,
  right: string | number | null | undefined,
) {
  if (left == null && right == null) return 0
  if (left == null) return 1
  if (right == null) return -1
  if (typeof left === 'number' && typeof right === 'number') return left - right
  return String(left).localeCompare(String(right), 'vi')
}

function rangeLabel(total: number, page: number, size: number) {
  if (total <= 0) return '0 trong 0 bản ghi'
  const from = (page - 1) * size + 1
  const to = Math.min(page * size, total)
  return `${from}–${to} trong ${total} bản ghi`
}

export function AdminList<T extends { id: string }>({
  items,
  columns,
  loading = false,
  emptyMessage = 'Không có dữ liệu.',
  searchValue,
  onSearchChange,
  searchPlaceholder = 'Tìm…',
  searchLabel = 'Tìm',
  primaryAction,
  extraActions,
  actionsFor,
  paging,
  className = '',
}: AdminListProps<T>) {
  const [hiddenKeys, setHiddenKeys] = useState<string[]>(() =>
    columns.filter((column) => column.defaultVisible === false).map((column) => column.key),
  )
  const [columnMenuOpen, setColumnMenuOpen] = useState(false)
  const [sort, setSort] = useState<SortState | null>(null)
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [sizeMenuOpen, setSizeMenuOpen] = useState(false)

  const clientPaging = useListPagination({
    total: paging ? 0 : items.length,
    initialSize: 10,
  })

  const page = paging?.page ?? clientPaging.page
  const size = paging?.size ?? clientPaging.size
  const total = paging?.total ?? items.length
  const totalPages = Math.max(1, Math.ceil(total / size) || 1)

  const visibleColumns = columns.filter((column) => !hiddenKeys.includes(column.key))

  const sortedItems = useMemo(() => {
    if (!sort) return items
    const column = columns.find((item) => item.key === sort.key)
    if (!column) return items
    const next = [...items]
    next.sort((left, right) => {
      const leftValue = column.sortValue
        ? column.sortValue(left)
        : column.value
          ? String(column.value(left) ?? '')
          : ''
      const rightValue = column.sortValue
        ? column.sortValue(right)
        : column.value
          ? String(column.value(right) ?? '')
          : ''
      const compared = compareSortValues(leftValue, rightValue)
      return sort.dir === 'asc' ? compared : -compared
    })
    return next
  }, [columns, items, sort])

  const pageItems = paging
    ? sortedItems
    : sortedItems.slice((page - 1) * size, page * size)

  const pageIds = pageItems.map((item) => item.id)
  const allPageSelected = pageIds.length > 0 && pageIds.every((id) => selectedIds.includes(id))

  const toggleSort = (key: string) => {
    setSort((current) => {
      if (current?.key !== key) return { key, dir: 'asc' }
      if (current.dir === 'asc') return { key, dir: 'desc' }
      return null
    })
  }

  const toggleColumn = (key: string) => {
    setHiddenKeys((current) =>
      current.includes(key) ? current.filter((item) => item !== key) : [...current, key],
    )
  }

  const toggleAll = () => {
    setSelectedIds((current) => {
      if (allPageSelected) return current.filter((id) => !pageIds.includes(id))
      return [...new Set([...current, ...pageIds])]
    })
  }

  const toggleOne = (id: string) => {
    setSelectedIds((current) =>
      current.includes(id) ? current.filter((item) => item !== id) : [...current, id],
    )
  }

  const changePage = (next: number) => {
    const clamped = Math.min(totalPages, Math.max(1, next))
    if (paging) paging.onPageChange(clamped)
    else clientPaging.setPage(clamped)
  }

  const changeSize = (next: number) => {
    if (paging) paging.onSizeChange(next)
    else clientPaging.setSize(next)
  }

  const colSpan = visibleColumns.length + 2 + (actionsFor ? 1 : 0)

  return (
    <section className={['flex h-full min-h-0 flex-col gap-3', className].filter(Boolean).join(' ')}>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <label className="relative min-w-56 flex-1 sm:max-w-sm">
          <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
          <input
            aria-label={searchLabel}
            value={searchValue}
            placeholder={searchPlaceholder}
            onChange={(event) => onSearchChange(event.target.value)}
            className="box-border h-10 w-full rounded-lg border border-slate-200 bg-white pl-9 pr-3 text-sm text-slate-800 outline-none focus:border-teal-600"
          />
        </label>
        <div className="flex flex-wrap items-center gap-2">
          {extraActions}
          {primaryAction}
          <Popover.Root
            open={columnMenuOpen}
            onOpenChange={(event: { value?: boolean }) => setColumnMenuOpen(Boolean(event.value))}
          >
            <Popover.Trigger
              type="button"
              aria-label="Cột"
              className="inline-flex h-10 items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-3 text-sm font-medium text-slate-700 hover:bg-slate-50"
            >
              <Columns3 className="size-4" aria-hidden />
              Cột
            </Popover.Trigger>
            <Popover.Portal>
              <Popover.Positioner className="z-[200]" side="bottom" align="end" sideOffset={4}>
                <Popover.Popup className="min-w-48 rounded-xl border border-slate-200 bg-white p-2 shadow-lg">
                  {columns.map((column) => (
                    <label key={column.key} className="flex items-center gap-2 px-2 py-1.5 text-sm text-slate-700">
                      <input
                        type="checkbox"
                        checked={!hiddenKeys.includes(column.key)}
                        onChange={() => toggleColumn(column.key)}
                      />
                      {column.label}
                    </label>
                  ))}
                </Popover.Popup>
              </Popover.Positioner>
            </Popover.Portal>
          </Popover.Root>
        </div>
      </div>

      <DataTable.Root
        data={pageItems}
        dataKey="id"
        loading={loading}
        className="relative flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border border-slate-200 bg-white"
      >
        <DataTable.TableContainer className="min-h-0 flex-1 overflow-auto">
          <DataTable.Table className="w-full min-w-[720px] border-collapse text-left text-sm">
            <DataTable.THead>
              <DataTable.THeadRow className="border-b border-slate-200 bg-slate-50/80">
                <DataTable.THeadCell className="w-10 px-3 py-3">
                  <input
                    type="checkbox"
                    aria-label="Chọn tất cả dòng trên trang hiện tại"
                    checked={allPageSelected}
                    disabled={pageIds.length === 0}
                    onChange={toggleAll}
                  />
                </DataTable.THeadCell>
                <DataTable.THeadCell className="w-14 px-3 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                  STT
                </DataTable.THeadCell>
                {visibleColumns.map((column) => (
                  <DataTable.THeadCell key={column.key} className="px-3 py-3 text-left">
                    {column.sortable ? (
                      <button
                        type="button"
                        className="inline-flex items-center gap-1 border-0 bg-transparent p-0 text-xs font-semibold uppercase tracking-wide text-slate-500 hover:text-slate-800"
                        onClick={() => toggleSort(column.key)}
                      >
                        {column.label}
                        {sort?.key === column.key && sort.dir === 'asc' ? (
                          <ArrowUp className="size-3.5 text-teal-700" aria-hidden />
                        ) : sort?.key === column.key && sort.dir === 'desc' ? (
                          <ArrowDown className="size-3.5 text-teal-700" aria-hidden />
                        ) : (
                          <ArrowUpDown className="size-3.5 opacity-50" aria-hidden />
                        )}
                      </button>
                    ) : (
                      <span className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                        {column.label}
                      </span>
                    )}
                  </DataTable.THeadCell>
                ))}
                {actionsFor ? (
                  <DataTable.THeadCell className="w-14 px-3 py-3" />
                ) : null}
              </DataTable.THeadRow>
            </DataTable.THead>
            <DataTable.TBody>
              {({ item }) => {
                const row = item as T
                const index = pageItems.findIndex((candidate) => candidate.id === row.id)
                const order = (page - 1) * size + index + 1
                return (
                  <DataTable.Row className="border-b border-slate-100 last:border-b-0 hover:bg-slate-50">
                    <DataTable.Cell className="px-3 py-2.5">
                      <input
                        type="checkbox"
                        aria-label={`Chọn dòng số ${order}`}
                        checked={selectedIds.includes(row.id)}
                        onChange={() => toggleOne(row.id)}
                      />
                    </DataTable.Cell>
                    <DataTable.Cell className="px-3 py-2.5 tabular-nums text-slate-500">{order}</DataTable.Cell>
                    {visibleColumns.map((column) => (
                      <DataTable.Cell key={column.key} className={`px-3 py-2.5 ${column.className ?? ''}`}>
                        {column.render ? column.render(row) : column.value ? column.value(row) : null}
                      </DataTable.Cell>
                    ))}
                    {actionsFor ? (
                      <DataTable.Cell className="px-3 py-2.5 text-right">
                        <RowActions actions={actionsFor(row)} ariaLabel="Thao tác dòng" />
                      </DataTable.Cell>
                    ) : null}
                  </DataTable.Row>
                )
              }}
            </DataTable.TBody>
            <DataTable.EmptyTBody>
              <tr>
                <td colSpan={colSpan} className="px-4 py-16 text-center">
                  <Inbox className="mx-auto size-10 text-slate-300" aria-hidden />
                  <p className="mt-3 mb-0 text-sm text-slate-500">
                    {loading ? 'Đang tải…' : emptyMessage}
                  </p>
                </td>
              </tr>
            </DataTable.EmptyTBody>
          </DataTable.Table>
        </DataTable.TableContainer>
      </DataTable.Root>

      <nav className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-200 pt-3 text-sm text-slate-600" aria-label="Phân trang">
        <p className="m-0 tabular-nums">{rangeLabel(total, page, size)}</p>
        <div className="flex items-center gap-3">
          <span>Số dòng</span>
          <Popover.Root
            open={sizeMenuOpen}
            onOpenChange={(event: { value?: boolean }) => setSizeMenuOpen(Boolean(event.value))}
          >
            <Popover.Trigger
              type="button"
              aria-label={`Số dòng ${size}`}
              className="inline-flex h-9 min-w-16 items-center justify-between gap-2 rounded-lg border border-slate-200 bg-white px-2 text-sm"
            >
              {size}
            </Popover.Trigger>
            <Popover.Portal>
              <Popover.Positioner className="z-[200]" side="top" align="end" sideOffset={4}>
                <Popover.Popup className="min-w-16 overflow-hidden rounded-lg border border-slate-200 bg-white py-1 shadow-lg">
                  {ADMIN_LIST_PAGE_SIZES.map((option) => (
                    <button
                      key={option}
                      type="button"
                      className="block w-full border-0 bg-transparent px-3 py-1.5 text-left text-sm hover:bg-slate-50"
                      onClick={() => {
                        changeSize(option)
                        setSizeMenuOpen(false)
                      }}
                    >
                      {option}
                    </button>
                  ))}
                </Popover.Popup>
              </Popover.Positioner>
            </Popover.Portal>
          </Popover.Root>
          <button
            type="button"
            aria-label="Trang trước"
            className="inline-flex size-9 items-center justify-center rounded-lg border border-slate-200 bg-white disabled:opacity-40"
            disabled={loading || page <= 1}
            onClick={() => changePage(page - 1)}
          >
            ‹
          </button>
          <span className="tabular-nums">{page} / {totalPages}</span>
          <button
            type="button"
            aria-label="Trang sau"
            className="inline-flex size-9 items-center justify-center rounded-lg border border-slate-200 bg-white disabled:opacity-40"
            disabled={loading || page >= totalPages}
            onClick={() => changePage(page + 1)}
          >
            ›
          </button>
        </div>
      </nav>
    </section>
  )
}
