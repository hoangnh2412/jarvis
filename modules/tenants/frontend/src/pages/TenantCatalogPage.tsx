import { useCallback, useEffect, useMemo, useState } from 'react'
import { FeatureDialog, PageHeader, Tag, notify, type RowActionItem } from '@platform/core'
import { Pencil, Plus, Power, PowerOff, RefreshCw, Search } from 'lucide-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createPlatformTenant,
  getPlatformTenantConnection,
  listPlatformTenants,
  setPlatformTenantActive,
  tenantQueryKeys,
  updatePlatformTenant,
  type PlatformTenant,
  type TenantAdminInput,
} from '../services/tenants'
import { notifyApiError } from '../notify'
import { SimpleTable, type SimpleColumn } from '../components/SimpleTable'
import { btnPrimaryClass, btnSecondaryClass, fieldInputClass, labelClass } from '../components/fieldStyles'

export type TenantFormData = {
  code: string
  name: string
  connectionString: string
  parentTenantId: string
  admin: TenantAdminInput
}

const emptyAdmin: TenantAdminInput = { username: '', email: '', password: '', displayName: '' }

export function TenantForm({
  isEdit = false,
  parents,
  initialValues,
  onSubmit,
  submitting,
}: {
  isEdit?: boolean
  parents: readonly PlatformTenant[]
  initialValues?: Partial<TenantFormData>
  onSubmit: (data: TenantFormData) => Promise<void> | void
  submitting?: boolean
}) {
  const [code, setCode] = useState(initialValues?.code ?? '')
  const [name, setName] = useState(initialValues?.name ?? '')
  const [connectionString, setConnectionString] = useState(initialValues?.connectionString ?? '')
  const [parentTenantId, setParentTenantId] = useState(initialValues?.parentTenantId ?? '')
  const [admin, setAdmin] = useState<TenantAdminInput>(initialValues?.admin ?? emptyAdmin)

  useEffect(() => {
    setCode(initialValues?.code ?? '')
    setName(initialValues?.name ?? '')
    setConnectionString(initialValues?.connectionString ?? '')
    setParentTenantId(initialValues?.parentTenantId ?? '')
    if (initialValues?.admin) setAdmin(initialValues.admin)
  }, [initialValues?.code, initialValues?.name, initialValues?.connectionString, initialValues?.parentTenantId, initialValues?.admin])

  const canSubmit = code.trim().length > 0 && name.trim().length > 0 && connectionString.trim().length > 0

  return (
    <form
      className="flex flex-col gap-4"
      onSubmit={async (event) => {
        event.preventDefault()
        await onSubmit({
          code: code.trim(),
          name: name.trim(),
          connectionString: connectionString.trim(),
          parentTenantId,
          admin,
        })
      }}
    >
      <div className="grid gap-4 sm:grid-cols-2">
        <label className="block">
          <span className={labelClass}>Mã tenant <span className="text-red-600">*</span></span>
          <input className={fieldInputClass} value={code} onChange={(e) => setCode(e.target.value)} disabled={isEdit} />
        </label>
        <label className="block">
          <span className={labelClass}>Tên tenant <span className="text-red-600">*</span></span>
          <input className={fieldInputClass} value={name} onChange={(e) => setName(e.target.value)} />
        </label>
        <label className="block sm:col-span-2">
          <span className={labelClass}>Tenant cha</span>
          <select className={fieldInputClass} value={parentTenantId} onChange={(e) => setParentTenantId(e.target.value)}>
            <option value="">— Không có (tenant gốc) —</option>
            {parents.map((tenant) => (
              <option key={tenant.id} value={tenant.id}>{tenant.code} — {tenant.name}</option>
            ))}
          </select>
        </label>
        <label className="block sm:col-span-2">
          <span className={labelClass}>Connection string <span className="text-red-600">*</span></span>
          <textarea className={`${fieldInputClass} h-auto resize-y py-2`} rows={3} value={connectionString} onChange={(e) => setConnectionString(e.target.value)} />
        </label>
      </div>

      {!isEdit ? (
        <fieldset className="rounded-xl border border-slate-200 p-4">
          <legend className="px-1 text-sm font-semibold text-slate-900">Tài khoản quản trị ban đầu</legend>
          <div className="grid gap-4 sm:grid-cols-2">
            <label className="block">
              <span className={labelClass}>Tên đăng nhập</span>
              <input className={fieldInputClass} value={admin.username} onChange={(e) => setAdmin((c) => ({ ...c, username: e.target.value }))} autoComplete="username" />
            </label>
            <label className="block">
              <span className={labelClass}>Họ tên</span>
              <input className={fieldInputClass} value={admin.displayName} onChange={(e) => setAdmin((c) => ({ ...c, displayName: e.target.value }))} />
            </label>
            <label className="block">
              <span className={labelClass}>Email</span>
              <input className={fieldInputClass} type="email" value={admin.email} onChange={(e) => setAdmin((c) => ({ ...c, email: e.target.value }))} />
            </label>
            <label className="block">
              <span className={labelClass}>Mật khẩu</span>
              <input className={fieldInputClass} type="password" value={admin.password ?? ''} onChange={(e) => setAdmin((c) => ({ ...c, password: e.target.value }))} autoComplete="new-password" />
            </label>
          </div>
        </fieldset>
      ) : null}

      <div className="flex justify-end gap-2">
        <button type="submit" className={btnPrimaryClass} disabled={!canSubmit || submitting}>
          {isEdit ? 'Lưu tenant' : 'Tạo tenant'}
        </button>
      </div>
    </form>
  )
}

/** Danh mục tenant trên Master: tạo/sửa/kích hoạt. */
export function TenantCatalogPage() {
  const queryClient = useQueryClient()
  const [search, setSearch] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [editTenant, setEditTenant] = useState<PlatformTenant | null>(null)
  const [editInitial, setEditInitial] = useState<Partial<TenantFormData> | undefined>(undefined)

  const tenantsQuery = useQuery({
    queryKey: tenantQueryKeys.list(search),
    queryFn: () => listPlatformTenants({ search: search.trim() || undefined }),
  })

  useEffect(() => {
    if (tenantsQuery.isError) notifyApiError(tenantsQuery.error)
  }, [tenantsQuery.error, tenantsQuery.isError])

  const rows = tenantsQuery.data ?? []

  const refresh = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: tenantQueryKeys.all })
  }, [queryClient])

  const createMutation = useMutation({
    mutationFn: (data: TenantFormData) =>
      createPlatformTenant({
        code: data.code,
        name: data.name,
        connectionString: data.connectionString,
        parentTenantId: data.parentTenantId || null,
        admin: data.admin.username.trim() ? data.admin : undefined,
      }),
    onError: notifyApiError,
    onSuccess: async (result) => {
      await refresh()
      setCreateOpen(false)
      notify.success(
        result.initialAdmin
          ? `Đã tạo tenant ${result.tenant.code} và tài khoản ${result.initialAdmin.username}.`
          : `Đã tạo tenant ${result.tenant.code}.`,
      )
    },
  })

  const updateMutation = useMutation({
    mutationFn: (data: TenantFormData) =>
      updatePlatformTenant(editTenant!.id, {
        code: data.code,
        name: data.name,
        connectionString: data.connectionString,
      }),
    onError: notifyApiError,
    onSuccess: async () => {
      await refresh()
      setEditTenant(null)
      notify.success('Đã cập nhật tenant.')
    },
  })

  const activeMutation = useMutation({
    mutationFn: ({ tenant, isActive }: { tenant: PlatformTenant; isActive: boolean }) =>
      setPlatformTenantActive(tenant.id, isActive),
    onError: notifyApiError,
    onSuccess: async (_result, { isActive }) => {
      await refresh()
      notify.success(isActive ? 'Đã kích hoạt tenant.' : 'Đã ngừng kích hoạt tenant.')
    },
  })

  const openEdit = useCallback(async (tenant: PlatformTenant) => {
    setEditTenant(tenant)
    setEditInitial({ code: tenant.code, name: tenant.name, parentTenantId: tenant.parentTenantId ?? '', connectionString: '' })
    try {
      const connectionString = await getPlatformTenantConnection(tenant.id)
      setEditInitial({ code: tenant.code, name: tenant.name, parentTenantId: tenant.parentTenantId ?? '', connectionString })
    } catch (error) {
      notifyApiError(error)
    }
  }, [])

  const columns = useMemo<SimpleColumn<PlatformTenant>[]>(() => [
    { key: 'code', label: 'Mã tenant', className: 'font-mono font-semibold', value: (row) => row.code },
    { key: 'name', label: 'Tên tenant', className: 'font-medium', value: (row) => row.name },
    {
      key: 'isActive',
      label: 'Trạng thái',
      render: (row) => row.isActive
        ? <Tag severity="success">Hoạt động</Tag>
        : <Tag severity="secondary">Ngừng hoạt động</Tag>,
    },
  ], [])

  const actionsFor = useCallback((tenant: PlatformTenant): RowActionItem[] => [
    { id: 'edit', label: 'Chỉnh sửa', icon: Pencil, onClick: () => void openEdit(tenant) },
    tenant.isActive
      ? { id: 'deactivate', label: 'Ngừng kích hoạt', icon: PowerOff, danger: true, separatorBefore: true, onClick: () => activeMutation.mutate({ tenant, isActive: false }) }
      : { id: 'activate', label: 'Kích hoạt', icon: Power, onClick: () => activeMutation.mutate({ tenant, isActive: true }) },
  ], [activeMutation, openEdit])

  return (
    <section className="flex h-full min-h-0 flex-col gap-4 overflow-hidden p-4 text-sm text-slate-800 sm:p-6">
      <PageHeader
        title="Tenant"
        description="Danh mục tenant trên Master."
        actions={(
          <>
            <label className="relative min-w-56 flex-1 sm:max-w-sm">
              <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
              <input aria-label="Tìm tenant" value={search} onChange={(event) => setSearch(event.currentTarget.value)} placeholder="Tìm mã hoặc tên tenant…" className="h-10 w-full rounded-xl border border-slate-200 bg-white pl-9 pr-3 text-sm outline-none focus:border-teal-600" />
            </label>
            <button type="button" onClick={() => void refresh()} className={btnSecondaryClass}>
              <RefreshCw className="size-4" aria-hidden /> Tải lại
            </button>
            <button type="button" onClick={() => setCreateOpen(true)} className={btnPrimaryClass}>
              <Plus className="size-4" aria-hidden /> Thêm tenant
            </button>
          </>
        )}
      />
      <SimpleTable
        items={rows}
        columns={columns}
        loading={tenantsQuery.isLoading}
        emptyMessage={tenantsQuery.isLoading ? 'Đang tải danh mục tenant…' : 'Không tìm thấy tenant phù hợp.'}
        actionsFor={actionsFor}
      />

      <FeatureDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="Thêm tenant"
        description="Tenant mới trên Master kèm tài khoản quản trị ban đầu."
        size="xl"
        hideFooter
      >
        {createOpen ? (
          <TenantForm
            parents={rows}
            submitting={createMutation.isPending}
            onSubmit={async (data) => { await createMutation.mutateAsync(data) }}
          />
        ) : null}
      </FeatureDialog>

      <FeatureDialog
        open={Boolean(editTenant)}
        onClose={() => setEditTenant(null)}
        title="Cập nhật tenant"
        description={editTenant?.code}
        size="xl"
        hideFooter
      >
        {editTenant ? (
          <TenantForm
            isEdit
            parents={rows.filter((row) => row.id !== editTenant.id)}
            initialValues={editInitial}
            submitting={updateMutation.isPending}
            onSubmit={async (data) => { await updateMutation.mutateAsync(data) }}
          />
        ) : null}
      </FeatureDialog>
    </section>
  )
}
