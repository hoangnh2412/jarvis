import { useCallback, useEffect, useState, type ReactNode } from 'react'
import { Database, Globe, Pencil, Power } from 'lucide-react'
import { Button } from 'primereact/button'
import { Card } from 'primereact/card'
import { ProgressSpinner } from 'primereact/progressspinner'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { TenantPageShell } from '../../components/TenantPageShell'
import { TenantStatusBadge } from '../../components/TenantStatusBadge'
import {
  btnOutlinedClass,
  btnPrimaryClass,
} from '../../components/fieldStyles'
import { getTenantMessages, type TenantLocale } from '../../localization'
import {
  getTenantConnectionsPath,
  getTenantDomainsPath,
  getTenantEditPath,
  navigateTenant,
} from '../../routes'
import { callGetTenant, callSetTenantStatus } from '../../services'
import { TenantStatus, type Tenant } from '../../types'
import { unwrapTenant } from '../../utils/apiData'
import {
  resolveTenantContent,
  type TenantSlotContent,
} from '../../utils'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'

export type TenantDetailPageContentContext = {
  tenant: Tenant | null
  loading: boolean
  reload: () => Promise<void>
  onEdit?: () => void
  onToggleStatus: () => void | Promise<void>
  onManageConnections?: () => void
  onManageDomains?: () => void
  DefaultContent: ReactNode
}

export type TenantDetailPageProps = {
  /** Nếu không truyền `tenant`, page fetch qua `callGetTenant(tenantId)` */
  tenantId?: string
  tenant?: Tenant
  loading?: boolean
  locale?: TenantLocale
  title?: string
  description?: string
  className?: string
  onEdit?: () => void
  /** Action callback kiểu jQuery ajax cho toggle status */
  callback?: {
    toggleStatus?: ActionProps<{ tenant: Tenant }, Tenant>
  }
  onManageConnections?: () => void
  onManageDomains?: () => void
  /**
   * `false` = không gắn navigate mặc định (edit / connections / domains).
   * @default true
   */
  useRoutes?: boolean
  headerActions?: ReactNode
  content?: TenantSlotContent<TenantDetailPageContentContext>
  withShell?: boolean
}

function MetaRow({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="grid grid-cols-1 gap-1 border-b border-line/70 py-3.5 last:border-b-0 sm:grid-cols-[160px_1fr] sm:gap-4">
      <dt className="text-sm font-medium text-mute">{label}</dt>
      <dd className="m-0 text-sm text-ink">{children}</dd>
    </div>
  )
}

export function TenantDetailPage({
  tenantId,
  tenant: tenantProp,
  loading: loadingProp,
  locale = 'vi',
  title,
  description,
  className,
  onEdit,
  callback,
  onManageConnections,
  onManageDomains,
  useRoutes = true,
  headerActions,
  content,
  withShell = true,
}: TenantDetailPageProps) {
  const messages = getTenantMessages(locale)
  const controlled = tenantProp != null

  const [internalTenant, setInternalTenant] = useState<Tenant | null>(null)
  const [internalLoading, setInternalLoading] = useState(
    !controlled && Boolean(tenantId),
  )

  const tenant = controlled ? tenantProp : internalTenant
  const loading = controlled ? Boolean(loadingProp) : internalLoading

  const reload = useCallback(async () => {
    if (controlled || !tenantId) return
    setInternalLoading(true)
    try {
      const res = await callGetTenant(tenantId)
      setInternalTenant(unwrapTenant(res))
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setInternalLoading(false)
    }
  }, [controlled, tenantId, messages.form.saveError])

  useEffect(() => {
    if (!controlled && tenantId) {
      void reload()
    }
  }, [controlled, tenantId, reload])

  const defaultToggleStatus = async (current: Tenant) => {
    const next =
      current.status === TenantStatus.Active
        ? TenantStatus.Inactive
        : TenantStatus.Active
    await callSetTenantStatus(current.id, { status: next })
  }

  const handleToggle = async () => {
    if (!tenant) return
    try {
      const outcome = await handleAction({
        ctx: { tenant },
        callback: callback?.toggleStatus,
        defaultSubmit: defaultToggleStatus,
        getPayload: ({ tenant: current }) => current,
        onSuccess: async () => {
          await reload()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(
        tenant.status === TenantStatus.Active
          ? 'Đã ngừng hoạt động tenant'
          : 'Đã kích hoạt tenant',
      )
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    }
  }

  const tenantIdForNav = tenant?.id ?? tenantId

  const handleEdit =
    onEdit ??
    (useRoutes && tenantIdForNav
      ? () => navigateTenant(getTenantEditPath(tenantIdForNav))
      : undefined)

  const handleManageConnections =
    onManageConnections ??
    (useRoutes && tenantIdForNav
      ? () => navigateTenant(getTenantConnectionsPath(tenantIdForNav))
      : undefined)

  const handleManageDomains =
    onManageDomains ??
    (useRoutes && tenantIdForNav
      ? () => navigateTenant(getTenantDomainsPath(tenantIdForNav))
      : undefined)

  const defaultContent =
    loading || !tenant ? (
      <div className="flex min-h-[200px] flex-col items-center justify-center gap-3 text-sm text-mute">
        <ProgressSpinner.Root className="h-8 w-8">
          <ProgressSpinner.Track />
          <ProgressSpinner.Range />
        </ProgressSpinner.Root>
        Đang tải…
      </div>
    ) : (
      <div className="grid gap-4 lg:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)]">
        <Card.Root className="overflow-hidden rounded-xl border border-line bg-white shadow-sm">
          <Card.Header className="border-b border-line px-5 py-4">
            <Card.Title className="m-0 text-base font-semibold tracking-tight text-ink">
              Thông tin tenant
            </Card.Title>
          </Card.Header>
          <Card.Body className="px-5">
            <dl className="m-0">
              <MetaRow label="Mã">
                <code className="rounded-md bg-slate-100 px-1.5 py-0.5 font-mono text-[13px]">
                  {tenant.code}
                </code>
              </MetaRow>
              <MetaRow label="Tên">{tenant.name}</MetaRow>
              <MetaRow label="Trạng thái">
                <TenantStatusBadge status={tenant.status} />
              </MetaRow>
              <MetaRow label="Parent ID">
                {tenant.parentId ? (
                  <code className="font-mono text-xs">{tenant.parentId}</code>
                ) : (
                  '—'
                )}
              </MetaRow>
              <MetaRow label="Tạo lúc">
                {tenant.createdAt
                  ? new Date(tenant.createdAt).toLocaleString()
                  : '—'}
              </MetaRow>
              <MetaRow label="Cập nhật">
                {tenant.updatedAt
                  ? new Date(tenant.updatedAt).toLocaleString()
                  : '—'}
              </MetaRow>
            </dl>
          </Card.Body>
        </Card.Root>

        <aside className="flex flex-col gap-3">
          <Card.Root className="overflow-hidden rounded-xl border border-line bg-gradient-to-br from-teal-50/80 via-white to-slate-50 shadow-sm">
            <Card.Body className="p-5">
              <p className="m-0 text-xs font-semibold uppercase tracking-wide text-teal-800/80">
                Sub-resources
              </p>
              <p className="mt-1.5 m-0 text-sm leading-relaxed text-mute">
                Quản lý nguồn dữ liệu đa cụm và domain định danh cho tenant này.
              </p>
              <div className="mt-4 flex flex-col gap-2">
                {handleManageConnections && (
                  <Button
                    type="button"
                    unstyled
                    className={`${btnOutlinedClass} !h-11 !justify-start`}
                    onClick={handleManageConnections}
                  >
                    <Database className="size-4 text-teal-700" />
                    {messages.connection.title}
                  </Button>
                )}
                {handleManageDomains && (
                  <Button
                    type="button"
                    unstyled
                    className={`${btnOutlinedClass} !h-11 !justify-start`}
                    onClick={handleManageDomains}
                  >
                    <Globe className="size-4 text-teal-700" />
                    {messages.domain.title}
                  </Button>
                )}
              </div>
            </Card.Body>
          </Card.Root>
        </aside>
      </div>
    )

  const contentCtx: TenantDetailPageContentContext = {
    tenant,
    loading,
    reload,
    onEdit: handleEdit,
    onToggleStatus: handleToggle,
    onManageConnections: handleManageConnections,
    onManageDomains: handleManageDomains,
    DefaultContent: defaultContent,
  }

  const resolved = resolveTenantContent(content, contentCtx, defaultContent)

  const actions = (
    <>
      {headerActions}
      {tenant && (
        <Button
          type="button"
          unstyled
          className={btnOutlinedClass}
          onClick={handleToggle}
        >
          <Power className="size-4" />
          Đổi trạng thái
        </Button>
      )}
      {handleEdit && (
        <Button
          type="button"
          unstyled
          className={btnPrimaryClass}
          onClick={handleEdit}
        >
          <Pencil className="size-4" />
          Sửa
        </Button>
      )}
    </>
  )

  if (!withShell) {
    return <>{resolved}</>
  }

  return (
    <TenantPageShell
      title={title ?? tenant?.name ?? messages.routes.detail}
      description={description ?? messages.routes.detail}
      headerActions={actions}
      className={className}
    >
      {resolved}
    </TenantPageShell>
  )
}
