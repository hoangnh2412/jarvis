import type { ReactNode } from 'react'
import type { Org } from '../../types'

export type OrgDetailViewProps = {
  org: Org
  labels: {
    code: string
    name: string
    status: string
    tenant: string
    active: string
    inactive: string
  }
}

function Row({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="grid grid-cols-[140px_1fr] gap-3 border-b border-slate-100 py-3 last:border-b-0">
      <span className="text-sm font-medium text-slate-500">{label}</span>
      <span className="text-sm break-all text-slate-800">{value}</span>
    </div>
  )
}

export function OrgDetailView({ org, labels }: OrgDetailViewProps) {
  return (
    <div className="rounded-xl border border-slate-200 bg-slate-50/40 px-4">
      <Row label={labels.code} value={<code className="font-mono">{org.code}</code>} />
      <Row label={labels.name} value={org.name} />
      <Row
        label={labels.status}
        value={org.isActive ? labels.active : labels.inactive}
      />
      <Row label={labels.tenant} value={<code className="font-mono">{org.tenantId}</code>} />
    </div>
  )
}
