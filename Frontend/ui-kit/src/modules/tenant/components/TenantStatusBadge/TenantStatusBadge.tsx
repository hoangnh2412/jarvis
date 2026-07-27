import { Tag } from 'primereact/tag'
import {
  TENANT_STATUS_LABEL,
  TenantStatus,
  type TenantStatusValue,
} from '../../types'

export type TenantStatusBadgeProps = {
  status: TenantStatusValue
  className?: string
}

export function TenantStatusBadge({
  status,
  className = '',
}: TenantStatusBadgeProps) {
  const active = status === TenantStatus.Active

  return (
    <Tag
      severity={active ? 'success' : 'secondary'}
      rounded
      className={[
        'inline-flex items-center gap-1.5 px-2.5 py-1 text-xs font-medium tracking-wide',
        active
          ? 'bg-teal-50 text-teal-800'
          : 'bg-slate-100 text-slate-600',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <span
        className={[
          'size-1.5 rounded-full',
          active ? 'bg-teal-600' : 'bg-slate-400',
        ].join(' ')}
        aria-hidden
      />
      {TENANT_STATUS_LABEL[status]}
    </Tag>
  )
}
