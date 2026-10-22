import { memo } from 'react'
import { Eye, Pencil, Power, PowerOff } from 'lucide-react'
import {
  DataTable,
  ProgressSpinner,
  RowActions,
  Tag,
  type RowActionItem,
} from '@platform/core'
import type { Org } from '../../types'
import { getOrgMessages, type OrgLocale } from '../../localization'

export type OrgTableProps = {
  items: Org[]
  loading?: boolean
  locale?: OrgLocale
  emptyMessage?: string
  onView?: (org: Org) => void
  onEdit?: (org: Org) => void
  onActivate?: (org: Org) => void
  onDeactivate?: (org: Org) => void
  className?: string
}

export const OrgTable = memo(function OrgTable({
  items,
  loading = false,
  locale = 'vi',
  emptyMessage,
  onView,
  onEdit,
  onActivate,
  onDeactivate,
  className = '',
}: OrgTableProps) {
  const messages = getOrgMessages(locale)
  const empty = emptyMessage ?? messages.list.empty

  return (
    <DataTable.Root
      data={items}
      dataKey="id"
      loading={loading}
      className={`relative overflow-hidden rounded-xl border border-line bg-white shadow-sm ${className}`}
    >
      <DataTable.TableContainer className="overflow-x-auto">
        <DataTable.Table className="w-full min-w-[640px] border-collapse text-left text-sm">
          <DataTable.THead>
            <DataTable.THeadRow className="border-b border-line bg-slate-50/80">
              <DataTable.THeadCell className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-mute">
                {messages.table.code}
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-mute">
                {messages.table.name}
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-mute">
                {messages.table.status}
              </DataTable.THeadCell>
              <DataTable.THeadCell className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-mute">
                {messages.table.actions}
              </DataTable.THeadCell>
            </DataTable.THeadRow>
          </DataTable.THead>
          <DataTable.TBody>
            {({ item }) => {
              const org = item as Org
              const actions: RowActionItem[] = []
              if (onView) {
                actions.push({
                  id: 'view',
                  label: messages.table.view,
                  icon: Eye,
                  onClick: () => onView(org),
                })
              }
              if (onEdit) {
                actions.push({
                  id: 'edit',
                  label: messages.table.edit,
                  icon: Pencil,
                  onClick: () => onEdit(org),
                })
              }
              if (org.isActive && onDeactivate) {
                actions.push({
                  id: 'deactivate',
                  label: messages.table.deactivate,
                  icon: PowerOff,
                  danger: true,
                  separatorBefore: actions.length > 0,
                  onClick: () => onDeactivate(org),
                })
              }
              if (!org.isActive && onActivate) {
                actions.push({
                  id: 'activate',
                  label: messages.table.activate,
                  icon: Power,
                  separatorBefore: actions.length > 0,
                  onClick: () => onActivate(org),
                })
              }

              return (
                <DataTable.Row className="border-b border-line/80 transition-colors last:border-b-0 hover:bg-teal-50/30">
                  <DataTable.Cell className="px-4 py-3">
                    <code className="rounded-md bg-slate-100 px-1.5 py-0.5 font-mono text-[13px] text-slate-800">
                      {org.code}
                    </code>
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 font-medium text-ink">
                    {org.name}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3">
                    {org.isActive ? (
                      <Tag severity="success">{messages.detail.active}</Tag>
                    ) : (
                      <Tag severity="secondary">{messages.detail.inactive}</Tag>
                    )}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 text-right">
                    <RowActions actions={actions} />
                  </DataTable.Cell>
                </DataTable.Row>
              )
            }}
          </DataTable.TBody>
          <DataTable.EmptyTBody>
            <tr>
              <td colSpan={4} className="px-2 py-12 text-center">
                {!loading ? <p className="m-0 text-sm text-mute">{empty}</p> : null}
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
