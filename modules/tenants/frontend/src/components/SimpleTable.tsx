import type { ReactNode } from 'react'
import { DataTable, RowActions, type RowActionItem } from '@platform/core'

export type SimpleColumn<T> = {
  key: string
  label: string
  className?: string
  headerClassName?: string
  render?: (item: T) => ReactNode
  value?: (item: T) => ReactNode
}

export type SimpleTableProps<T extends { id: string }> = {
  items: T[]
  columns: SimpleColumn<T>[]
  loading?: boolean
  emptyMessage?: string
  actionsFor?: (item: T) => RowActionItem[]
}

export function SimpleTable<T extends { id: string }>({
  items,
  columns,
  loading,
  emptyMessage = 'Không có dữ liệu.',
  actionsFor,
}: SimpleTableProps<T>) {
  const colSpan = columns.length + (actionsFor ? 1 : 0)
  return (
    <DataTable.Root
      data={items}
      dataKey="id"
      loading={loading}
      className="relative overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm"
    >
      <DataTable.TableContainer className="overflow-x-auto">
        <DataTable.Table className="w-full border-collapse text-left text-sm">
          <DataTable.THead>
            <DataTable.THeadRow className="border-b border-slate-200 bg-slate-50/80">
              {columns.map((column) => (
                <DataTable.THeadCell
                  key={column.key}
                  className={`px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500 ${column.headerClassName ?? ''}`}
                >
                  {column.label}
                </DataTable.THeadCell>
              ))}
              {actionsFor ? (
                <DataTable.THeadCell className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-500">
                  Thao tác
                </DataTable.THeadCell>
              ) : null}
            </DataTable.THeadRow>
          </DataTable.THead>
          <DataTable.TBody>
            {({ item }) => {
              const row = item as T
              return (
                <DataTable.Row className="border-b border-slate-100 transition-colors last:border-b-0 hover:bg-slate-50">
                  {columns.map((column) => (
                    <DataTable.Cell key={column.key} className={`px-4 py-3 ${column.className ?? ''}`}>
                      {column.render ? column.render(row) : column.value ? column.value(row) : null}
                    </DataTable.Cell>
                  ))}
                  {actionsFor ? (
                    <DataTable.Cell className="px-4 py-3 text-right">
                      <RowActions actions={actionsFor(row)} />
                    </DataTable.Cell>
                  ) : null}
                </DataTable.Row>
              )
            }}
          </DataTable.TBody>
          <DataTable.EmptyTBody>
            <tr>
              <td colSpan={colSpan} className="px-2 py-12 text-center text-sm text-slate-500">
                {loading ? 'Đang tải…' : emptyMessage}
              </td>
            </tr>
          </DataTable.EmptyTBody>
        </DataTable.Table>
      </DataTable.TableContainer>
    </DataTable.Root>
  )
}
