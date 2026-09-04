import type { Role } from '../../types'

export type RoleDetailViewProps = {
  role: Role
}

function Row({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="grid grid-cols-[140px_1fr] gap-3 border-b border-slate-100 py-3 last:border-b-0">
      <span className="text-sm font-medium text-slate-500">{label}</span>
      <span className="text-sm text-slate-800">{value}</span>
    </div>
  )
}

export function RoleDetailView({ role }: RoleDetailViewProps) {
  return (
    <div className="rounded-xl border border-slate-200 bg-slate-50/40 px-4">
      <Row label="Tên" value={<code className="font-mono">{role.name}</code>} />
      <Row label="Hiển thị" value={role.displayName} />
      <Row label="Mô tả" value={role.description || '—'} />
      <Row
        label="Mặc định"
        value={role.isDefault ? 'Có' : 'Không'}
      />
      <Row label="Public" value={role.isPublic ? 'Có' : 'Không'} />
      <Row label="Số quyền" value={role.permissions.length} />
      <Row
        label="Tạo lúc"
        value={new Date(role.createdAt).toLocaleString('vi-VN')}
      />
    </div>
  )
}
