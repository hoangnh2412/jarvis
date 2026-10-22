import type { ReactNode } from 'react'
import type { UserMessages } from '../../localization'
import type { AdminUser } from '../../types'

export type UserDetailViewProps = {
  user: AdminUser
  messages: UserMessages
}

function Row({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="grid grid-cols-[140px_1fr] gap-3 border-b border-slate-100 py-3 last:border-b-0">
      <span className="text-sm font-medium text-slate-500">{label}</span>
      <span className="text-sm text-slate-800">{value}</span>
    </div>
  )
}

export function UserDetailView({ user, messages }: UserDetailViewProps) {
  return (
    <div className="rounded-xl border border-slate-200 bg-slate-50/40 px-4">
      <Row
        label={messages.form.username}
        value={<code className="font-mono">{user.username}</code>}
      />
      <Row label={messages.form.email} value={user.email} />
      <Row label={messages.form.role} value={user.roleName} />
      <Row label={messages.form.tenantId} value={user.tenantId || '—'} />
      <Row label={messages.form.orgId} value={user.orgId || '—'} />
      <Row
        label={messages.status.column}
        value={user.isLocked ? messages.status.locked : messages.status.active}
      />
    </div>
  )
}
