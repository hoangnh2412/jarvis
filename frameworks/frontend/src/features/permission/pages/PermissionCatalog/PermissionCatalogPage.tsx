import { useCallback, useEffect, useRef, useState } from 'react'
import { Toolbar } from 'primereact/toolbar'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import {
  getPermissionMessages,
  type PermissionLocale,
} from '../../localization'
import {
  mockGetAuthorizationCatalog,
  type AuthorizationCatalog,
} from '../../services'

export type PermissionCatalogPageProps = {
  locale?: PermissionLocale
  className?: string
}

export function PermissionCatalogPage({
  locale = 'vi',
  className = '',
}: PermissionCatalogPageProps) {
  const messages = getPermissionMessages(locale)
  const requestId = useRef(0)
  const [catalog, setCatalog] = useState<AuthorizationCatalog | null>(null)
  const [loading, setLoading] = useState(true)

  const reload = useCallback(async () => {
    const current = ++requestId.current
    setLoading(true)
    try {
      const next = await mockGetAuthorizationCatalog()
      if (current !== requestId.current) return
      setCatalog(next)
    } catch (error) {
      if (current !== requestId.current) return
      notify.error(getErrorMessage(error, messages.list.empty))
    } finally {
      if (current === requestId.current) setLoading(false)
    }
  }, [messages.list.empty])

  useEffect(() => {
    void reload()
  }, [reload])

  return (
    <div className={`flex h-full min-h-0 w-full flex-col font-sans text-ink ${className}`}>
      <Toolbar.Root className="mb-5 flex w-full flex-col gap-3 border-0 border-b border-line bg-transparent p-0 pb-4">
        <Toolbar.Start className="min-w-0">
          <h2 className="m-0 text-[1.35rem] font-semibold tracking-tight text-ink">
            {messages.list.title}
          </h2>
          <p className="mt-1.5 m-0 max-w-[62ch] text-sm leading-relaxed text-mute">
            {messages.list.description}
          </p>
        </Toolbar.Start>
      </Toolbar.Root>

      {loading || !catalog ? (
        <p className="m-0 text-sm text-mute">{messages.list.loading}</p>
      ) : (
        <div className="grid gap-6 lg:grid-cols-2">
          <section className="overflow-hidden rounded-xl border border-line bg-white shadow-sm">
            <h3 className="m-0 border-b border-line px-4 py-3 text-sm font-semibold text-ink">
              {messages.list.permissions}
            </h3>
            <table className="w-full border-collapse text-left text-sm">
              <thead>
                <tr className="border-b border-line bg-slate-50/80">
                  <th className="px-4 py-2 text-xs font-semibold uppercase tracking-wide text-mute">
                    {messages.list.code}
                  </th>
                  <th className="px-4 py-2 text-xs font-semibold uppercase tracking-wide text-mute">
                    {messages.list.name}
                  </th>
                </tr>
              </thead>
              <tbody>
                {catalog.permissions.map((permission) => (
                  <tr key={permission.code} className="border-b border-line/80 last:border-b-0">
                    <td className="px-4 py-2.5">
                      <code className="font-mono text-[13px]">{permission.code}</code>
                    </td>
                    <td className="px-4 py-2.5 text-ink">{permission.label}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>

          <section className="overflow-hidden rounded-xl border border-line bg-white shadow-sm">
            <h3 className="m-0 border-b border-line px-4 py-3 text-sm font-semibold text-ink">
              {messages.list.policies}
            </h3>
            <table className="w-full border-collapse text-left text-sm">
              <thead>
                <tr className="border-b border-line bg-slate-50/80">
                  <th className="px-4 py-2 text-xs font-semibold uppercase tracking-wide text-mute">
                    {messages.list.code}
                  </th>
                  <th className="px-4 py-2 text-xs font-semibold uppercase tracking-wide text-mute">
                    {messages.list.rule}
                  </th>
                </tr>
              </thead>
              <tbody>
                {catalog.policies.map((policy) => (
                  <tr key={policy.code} className="border-b border-line/80 last:border-b-0">
                    <td className="px-4 py-2.5 align-top">
                      <div className="font-medium text-ink">{policy.name}</div>
                      <code className="font-mono text-[12px] text-slate-500">{policy.code}</code>
                    </td>
                    <td className="px-4 py-2.5 align-top text-slate-600">{policy.summary}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        </div>
      )}
    </div>
  )
}
