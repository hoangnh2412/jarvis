import { Eye, Pencil, Power, Trash2 } from 'lucide-react'
import { Button } from 'primereact/button'
import { DataTable } from 'primereact/datatable'
import { ProgressSpinner } from 'primereact/progressspinner'
import type { Tenant } from '../../types'
import { TenantStatusBadge } from '../TenantStatusBadge'
import { btnTextClass } from '../fieldStyles'

export type TenantTableProps = {
  items: Tenant[]
  loading?: boolean
  emptyMessage?: string
  onView?: (tenant: Tenant) => void
  onEdit?: (tenant: Tenant) => void
  onDelete?: (tenant: Tenant) => void
  onToggleStatus?: (tenant: Tenant) => void
  className?: string
}

export function TenantTable({
  items,
  loading = false,
  emptyMessage = 'Không có dữ liệu',
  onView,
  onEdit,
  onDelete,
  onToggleStatus,
  className = '',
}: TenantTableProps) {
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
        <DataTable.Table className="w-full min-w-[640px] border-collapse text-left text-sm">
          <DataTable.THead>
            <DataTable.THeadRow className="border-b border-line bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-mute">
              <DataTable.THeadCell className="px-4 py-3">Mã</DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3">Tên</DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3">
                Trạng thái
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3">Parent</DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3 text-right">
                Thao tác
              </DataTable.THeadCell>
            </DataTable.THeadRow>
          </DataTable.THead>

          <DataTable.TBody>
            {({ item }) => {
              const tenant = item as Tenant
              return (
                <DataTable.Row className="border-b border-line/80 transition-colors last:border-b-0 hover:bg-teal-50/30">
                  <DataTable.Cell className="px-4 py-3">
                    <code className="rounded-md bg-slate-100 px-1.5 py-0.5 font-mono text-[13px] text-slate-800">
                      {tenant.code}
                    </code>
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 font-medium text-ink">
                    {tenant.name}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3">
                    <TenantStatusBadge status={tenant.status} />
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 text-mute">
                    {tenant.parentId ? (
                      <code className="font-mono text-xs">
                        {tenant.parentId.slice(0, 8)}…
                      </code>
                    ) : (
                      '—'
                    )}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3">
                    <div className="flex flex-wrap items-center justify-end gap-0.5">
                      {onView && (
                        <Button
                          type="button"
                          unstyled
                          className={btnTextClass}
                          aria-label="Xem"
                          onClick={() => onView(tenant)}
                        >
                          <Eye className="size-4" />
                        </Button>
                      )}
                      {onEdit && (
                        <Button
                          type="button"
                          unstyled
                          className={btnTextClass}
                          aria-label="Sửa"
                          onClick={() => onEdit(tenant)}
                        >
                          <Pencil className="size-4" />
                        </Button>
                      )}
                      {onToggleStatus && (
                        <Button
                          type="button"
                          unstyled
                          className={btnTextClass}
                          aria-label="Đổi trạng thái"
                          onClick={() => onToggleStatus(tenant)}
                        >
                          <Power className="size-4" />
                        </Button>
                      )}
                      {onDelete && (
                        <Button
                          type="button"
                          unstyled
                          className={`${btnTextClass} !text-red-600 hover:!bg-red-50`}
                          aria-label="Xoá"
                          onClick={() => onDelete(tenant)}
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
              <td
                colSpan={5}
                className="px-2 text-center"
                style={{ paddingTop: '3rem', paddingBottom: '3rem' }}
              >
                {!loading && (
                  <p className="m-0 text-sm text-mute">{emptyMessage}</p>
                )}
              </td>
            </tr>
          </DataTable.EmptyTBody>
        </DataTable.Table>
      </DataTable.TableContainer>

      <DataTable.Loading className="absolute inset-0 flex flex-col items-center justify-center gap-3 bg-white/80">
        <ProgressSpinner.Root className="h-8 w-8">
          <ProgressSpinner.Track />
          <ProgressSpinner.Range />
        </ProgressSpinner.Root>
        <span className="text-sm text-mute">Đang tải danh sách…</span>
      </DataTable.Loading>
    </DataTable.Root>
  )
}
