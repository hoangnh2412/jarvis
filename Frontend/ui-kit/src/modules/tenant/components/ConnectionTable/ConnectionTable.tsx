import { Pencil, Trash2 } from 'lucide-react'
import { Button } from 'primereact/button'
import { DataTable } from 'primereact/datatable'
import { Message } from 'primereact/message'
import { Tag } from 'primereact/tag'
import {
  DB_PROVIDER_LABEL,
  type TenantConnection,
} from '../../types'
import { btnTextClass } from '../fieldStyles'

export type ConnectionTableProps = {
  items: TenantConnection[]
  loading?: boolean
  emptyMessage?: string
  onEdit?: (item: TenantConnection) => void
  onDelete?: (item: TenantConnection) => void
  className?: string
}

function maskConnectionString(value: string) {
  if (value.length <= 24) return '••••••••'
  return `${value.slice(0, 18)}…`
}

export function ConnectionTable({
  items,
  loading = false,
  emptyMessage = 'Chưa có nguồn dữ liệu.',
  onEdit,
  onDelete,
  className = '',
}: ConnectionTableProps) {
  return (
    <DataTable.Root
      data={items}
      dataKey="id"
      loading={loading}
      className={[
        'relative overflow-hidden rounded-xl border border-line bg-white shadow-sm',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <DataTable.TableContainer className="overflow-x-auto">
        <DataTable.Table className="w-full min-w-[720px] border-collapse text-left text-sm">
          <DataTable.THead>
            <DataTable.THeadRow className="border-b border-line bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-mute">
              <DataTable.THeadCell className="px-4 py-3">
                Provider
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3">
                Connection
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3">
                Partition
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3">
                Default
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3 text-right">
                Thao tác
              </DataTable.THeadCell>
            </DataTable.THeadRow>
          </DataTable.THead>

          <DataTable.TBody>
            {({ item }) => {
              const row = item as TenantConnection
              return (
                <DataTable.Row className="border-b border-line/80 transition-colors last:border-b-0 hover:bg-teal-50/30">
                  <DataTable.Cell className="px-4 py-3 font-medium text-ink">
                    {DB_PROVIDER_LABEL[row.providerType]}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3">
                    <code className="rounded-md bg-slate-100 px-1.5 py-0.5 font-mono text-[12px] text-slate-700">
                      {maskConnectionString(row.connectionString)}
                    </code>
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 text-mute">
                    {row.partitionFrom || row.partitionTo
                      ? `${row.partitionFrom ?? '…'} → ${row.partitionTo ?? '…'}`
                      : '—'}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3">
                    {row.isDefault ? (
                      <Tag
                        severity="success"
                        rounded
                        className="bg-teal-50 px-2 py-0.5 text-xs font-medium text-teal-800"
                      >
                        Default
                      </Tag>
                    ) : (
                      <span className="text-mute">—</span>
                    )}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3">
                    <div className="flex items-center justify-end gap-0.5">
                      {onEdit && (
                        <Button
                          type="button"
                          unstyled
                          className={btnTextClass}
                          aria-label="Sửa"
                          onClick={() => onEdit(row)}
                        >
                          <Pencil className="size-4" />
                        </Button>
                      )}
                      {onDelete && (
                        <Button
                          type="button"
                          unstyled
                          className={`${btnTextClass} !text-red-600 hover:!bg-red-50`}
                          aria-label="Xoá"
                          onClick={() => onDelete(row)}
                        >
                          <Trash2 className="size-4" />
                        </Button>
                      )}
                    </div>
                  </DataTable.Cell>
                </DataTable.Row>
              )
            }}
          </DataTable.TBody>

          <DataTable.EmptyTBody>
            <tr>
              <td colSpan={5} className="px-4 py-10">
                <Message.Root
                  severity="secondary"
                  className="mx-auto flex max-w-md items-start gap-3 rounded-xl border border-dashed border-slate-200 bg-slate-50/80 px-4 py-3"
                >
                  <Message.Content>
                    <Message.Text className="text-sm text-mute">
                      {emptyMessage}
                    </Message.Text>
                  </Message.Content>
                </Message.Root>
              </td>
            </tr>
          </DataTable.EmptyTBody>
        </DataTable.Table>
      </DataTable.TableContainer>

      <DataTable.Loading className="absolute inset-0 flex items-center justify-center bg-white/70 text-sm text-mute">
        Đang tải…
      </DataTable.Loading>
    </DataTable.Root>
  )
}
