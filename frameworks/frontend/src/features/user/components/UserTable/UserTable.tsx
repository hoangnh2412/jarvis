import { memo } from 'react'
import { DataTable } from 'primereact/datatable'
import { ProgressSpinner } from 'primereact/progressspinner'
import { Tag } from 'primereact/tag'
import type { UserMessages } from '../../localization'
import type { AdminUser } from '../../types'
import { UserRowActions } from '../UserRowActions'

export type UserTableProps = {
  items: AdminUser[]
  loading?: boolean
  messages: UserMessages
  onView?: (user: AdminUser) => void
  onEdit?: (user: AdminUser) => void
  onDelete?: (user: AdminUser) => void
  className?: string
}

const COLUMN_COUNT = 5

export const UserTable = memo(function UserTable({
  items,
  loading = false,
  messages,
  onView,
  onEdit,
  onDelete,
  className = '',
}: UserTableProps) {
  return (
    <DataTable.Root
      data={items}
      dataKey="id"
      loading={loading}
      className={`relative overflow-hidden rounded-xl border border-line bg-white shadow-sm ${className}`}
    >
      <DataTable.TableContainer className="overflow-x-auto">
        <DataTable.Table className="w-full min-w-[720px] border-collapse text-left text-sm">
          <DataTable.THead>
            <DataTable.THeadRow className="border-b border-line bg-slate-50/80">
              <DataTable.THeadCell className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-mute">
                {messages.form.username}
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-mute">
                {messages.form.email}
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-mute">
                {messages.form.role}
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-mute">
                {messages.status.column}
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-mute">
                {messages.actions.menu}
              </DataTable.THeadCell>
            </DataTable.THeadRow>
          </DataTable.THead>
          <DataTable.TBody>
            {({ item }) => {
              const user = item as AdminUser
              return (
                <DataTable.Row className="border-b border-line/80 transition-colors last:border-b-0 hover:bg-teal-50/30">
                  <DataTable.Cell className="px-4 py-3">
                    <code className="rounded-md bg-slate-100 px-1.5 py-0.5 font-mono text-[13px] text-slate-800">
                      {user.username}
                    </code>
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 text-ink">
                    {user.email}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 font-medium text-ink">
                    {user.roleName}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3">
                    {user.isLocked ? (
                      <Tag severity="danger" className="font-medium">
                        {messages.status.locked}
                      </Tag>
                    ) : (
                      <Tag severity="success" className="font-medium">
                        {messages.status.active}
                      </Tag>
                    )}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 text-right">
                    <UserRowActions
                      user={user}
                      messages={messages}
                      onView={onView}
                      onEdit={onEdit}
                      onDelete={onDelete}
                    />
                  </DataTable.Cell>
                </DataTable.Row>
              )
            }}
          </DataTable.TBody>
          <DataTable.EmptyTBody>
            <tr>
              <td colSpan={COLUMN_COUNT} className="px-2 py-12 text-center">
                {!loading ? (
                  <p className="m-0 text-sm text-mute">{messages.list.empty}</p>
                ) : null}
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
        <span className="text-sm text-mute">{messages.list.loading}</span>
      </DataTable.Loading>
    </DataTable.Root>
  )
})
