import { useMemo, type ReactNode } from 'react'
import { Mail, Phone, User } from 'lucide-react'

export type ProfileCardProps = {
  fullName?: string
  email?: string
  phone?: string
}

function getInitials(name: string) {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return ''
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase()
  return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase()
}

function MetaRow({ icon, value }: { icon: ReactNode; value: string }) {
  return (
    <div className="flex items-start gap-2.5 text-[0.8125rem] text-zinc-700">
      <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-accent-soft text-accent">
        {icon}
      </span>
      <span className="min-w-0 break-words pt-1">{value}</span>
    </div>
  )
}

export function ProfileCard({ fullName = '', email = '', phone = '' }: ProfileCardProps) {
  const initials = useMemo(() => getInitials(fullName || ''), [fullName])

  return (
    <aside className="flex min-h-0 flex-col overflow-visible rounded-xl border border-line bg-white">
      <div className="h-[72px] shrink-0 rounded-t-xl bg-gradient-to-br from-zinc-900 via-zinc-800 to-accent" />
      <div
        className={[
          'relative z-[1] mx-auto -mt-9 flex h-[72px] w-[72px] items-center justify-center rounded-full border-[3px] border-white text-xl font-bold tracking-wide',
          initials ? 'bg-ink text-zinc-50' : 'bg-paper text-mute',
        ].join(' ')}
      >
        {initials || <User className="h-8 w-8" strokeWidth={1.6} />}
      </div>
      <div className="flex flex-col items-center px-[1.15rem] pb-5 pt-3.5 text-center">
        <p className="m-0 break-words text-[1.05rem] font-semibold leading-snug tracking-tight text-ink">
          {fullName?.trim() || 'Chưa có tên'}
        </p>
        <p className="mt-1 m-0 break-words text-[0.8125rem] text-mute">
          {email?.trim() || 'Chưa có email'}
        </p>
        <div className="mt-[1.1rem] flex w-full flex-col gap-2.5 border-t border-line pt-4 text-left">
          <MetaRow
            icon={<Mail className="h-3.5 w-3.5" strokeWidth={1.75} />}
            value={email?.trim() || '—'}
          />
          <MetaRow
            icon={<Phone className="h-3.5 w-3.5" strokeWidth={1.75} />}
            value={phone?.trim() || 'Chưa cập nhật SĐT'}
          />
        </div>
      </div>
    </aside>
  )
}
