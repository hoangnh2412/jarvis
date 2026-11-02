import { zodResolver } from '@hookform/resolvers/zod'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { ConfirmDialog, FeatureDialog, PageHeader, Tag, notify, type RowActionItem } from '@platform/core'
import { Pencil, Plus, Search, ShieldCheck, SlidersHorizontal, Trash2 } from 'lucide-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createSystemRole,
  deleteSystemRole,
  listPermissionCatalog,
  listSystemRoles,
  setSystemRolePermissions,
  setSystemRolePolicies,
  updateSystemRole,
  type SystemRole,
} from '../services/roles'
import { identityQueryKeys } from '../services/users'
import { notifyApiError } from '../notify'
import { searchNormalize } from '../utils'
import { SimpleTable, type SimpleColumn } from '../components/SimpleTable'
import { btnAuxClass, btnPrimaryClass, fieldInputClass, labelClass } from '../components/fieldStyles'
import { RoleAuthorizationDialog } from './RoleAuthorizationDialog'
import { RolePolicyDialog } from './RolePolicyDialog'

const roleFormSchema = z.object({
  name: z.string().trim().min(2, 'Tên vai trò cần có ít nhất 2 ký tự.'),
  description: z.string().trim().optional(),
  isActive: z.boolean(),
})

type RoleFormValues = z.infer<typeof roleFormSchema>

function RoleFormDialog({
  open,
  item,
  onClose,
  onSubmit,
}: {
  open: boolean
  item: SystemRole | null
  onClose: () => void
  onSubmit: (values: RoleFormValues) => void
}) {
  const { register, handleSubmit, reset, formState: { errors } } = useForm<RoleFormValues>({
    resolver: zodResolver(roleFormSchema),
    defaultValues: { name: '', description: '', isActive: true },
  })

  useEffect(() => {
    if (open) reset(item ? { name: item.name, description: item.description, isActive: item.isActive } : { name: '', description: '', isActive: true })
  }, [item, open, reset])

  return (
    <FeatureDialog
      open={open}
      onClose={onClose}
      title={item ? 'Cập nhật vai trò' : 'Thêm vai trò'}
      description="Tên và mô tả nhóm quyền"
      size="lg"
      onSubmit={handleSubmit(onSubmit)}
      submitLabel="Lưu vai trò"
      cancelLabel="Hủy"
    >
      {open ? (
        <div className="space-y-4">
          <label className="block">
            <span className={labelClass}>Tên vai trò <span className="text-red-600">*</span></span>
            <input className={`${fieldInputClass} ${errors.name ? '!border-red-500' : ''}`} {...register('name')} />
            {errors.name && <span className="mt-1 block text-xs text-red-600">{errors.name.message}</span>}
          </label>
          <label className="block">
            <span className={labelClass}>Mô tả</span>
            <textarea rows={3} className={`${fieldInputClass} h-auto resize-y py-2`} {...register('description')} />
          </label>
          <label className="inline-flex items-center gap-2 text-sm">
            <input type="checkbox" className="size-4 accent-teal-700" {...register('isActive')} />
            Vai trò đang hoạt động
          </label>
        </div>
      ) : null}
    </FeatureDialog>
  )
}

export type RolesPageProps = {
  /** Gợi ý sử dụng theo mã quyền do host cung cấp (map code → mô tả). */
  permissionUsageHintFor?: (code: string) => string | null | undefined
  /** Đường dẫn màn quản lý phạm vi của host (nút chuyển từ dialog Phạm vi). */
  policyCatalogPath?: string
}

/** Trang quản lý vai trò và quyền theo nhóm chức năng. */
export function RolesPage({ permissionUsageHintFor, policyCatalogPath }: RolesPageProps) {
  const queryClient = useQueryClient()
  const rolesQuery = useQuery({ queryKey: identityQueryKeys.roles, queryFn: listSystemRoles })
  const catalogQuery = useQuery({
    queryKey: identityQueryKeys.permissionCatalog,
    queryFn: () => listPermissionCatalog(permissionUsageHintFor),
  })
  const roles = rolesQuery.data ?? []
  const [search, setSearch] = useState('')
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingRole, setEditingRole] = useState<SystemRole | null>(null)
  const [authorizationRole, setAuthorizationRole] = useState<SystemRole | null>(null)
  const [policyRole, setPolicyRole] = useState<SystemRole | null>(null)
  const [pendingDelete, setPendingDelete] = useState<SystemRole | null>(null)

  const refreshRoles = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: identityQueryKeys.roles })
  }, [queryClient])

  const filteredRoles = useMemo(() => {
    const normalizedSearch = searchNormalize(search.trim())
    if (!normalizedSearch) return roles
    return roles.filter((role) => searchNormalize(`${role.name} ${role.description}`).includes(normalizedSearch))
  }, [roles, search])

  const saveMutation = useMutation({
    mutationFn: (values: RoleFormValues) => {
      const input = { name: values.name, description: values.description ?? '', isActive: values.isActive }
      return editingRole ? updateSystemRole(editingRole, input) : createSystemRole(input)
    },
    onError: notifyApiError,
    onSuccess: async () => {
      await refreshRoles()
      setDialogOpen(false)
      setEditingRole(null)
      notify.success('Đã lưu vai trò.')
    },
  })

  const permissionMutation = useMutation({
    mutationFn: (permissionKeys: string[]) => setSystemRolePermissions(authorizationRole?.id ?? '', permissionKeys),
    onError: notifyApiError,
    onSuccess: async () => {
      await refreshRoles()
      notify.success('Đã lưu quyền chức năng.')
    },
  })

  const policyMutation = useMutation({
    mutationFn: (policyCodes: string[]) => setSystemRolePolicies(policyRole?.id ?? '', policyCodes),
    onError: notifyApiError,
    onSuccess: async (_data, policyCodes) => {
      await refreshRoles()
      setPolicyRole((current) => (current ? { ...current, policyCodes } : null))
      notify.success('Đã lưu phạm vi dữ liệu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (role: SystemRole) => deleteSystemRole(role.id),
    onError: notifyApiError,
    onSuccess: async () => {
      await refreshRoles()
      setPendingDelete(null)
      notify.success('Đã xóa vai trò.')
    },
  })

  const roleSave = useCallback((values: RoleFormValues) => {
    const duplicate = roles.some((role) =>
      role.id !== editingRole?.id && role.name.trim().toLocaleLowerCase('vi-VN') === values.name.trim().toLocaleLowerCase('vi-VN'),
    )
    if (duplicate) {
      notify.error('Tên vai trò đã tồn tại.')
      return
    }
    saveMutation.mutate(values)
  }, [editingRole, roles, saveMutation])

  const roleSavePermissions = useCallback((permissionKeys: string[]) => {
    if (!authorizationRole || authorizationRole.isSystemRole) return
    permissionMutation.mutate(permissionKeys)
  }, [authorizationRole, permissionMutation])

  const roleSavePolicies = useCallback(async (policyCodes: string[]) => {
    if (!policyRole || policyRole.isSystemRole) return
    await policyMutation.mutateAsync(policyCodes)
  }, [policyRole, policyMutation])

  const columns = useMemo<SimpleColumn<SystemRole>[]>(() => [
    { key: 'name', label: 'Tên vai trò', className: 'font-semibold', value: (role) => role.name },
    { key: 'description', label: 'Mô tả', className: 'text-slate-500', value: (role) => role.description },
    {
      key: 'isActive',
      label: 'Trạng thái',
      render: (role) => role.isActive
        ? <Tag severity="success">Hoạt động</Tag>
        : <Tag severity="secondary">Ngừng hoạt động</Tag>,
    },
    {
      key: 'grants',
      label: 'Phân quyền & phạm vi',
      render: (role) => (
        <div className="flex flex-wrap items-center gap-1.5">
          <button
            type="button"
            onClick={(event) => {
              event.stopPropagation()
              setAuthorizationRole(role)
            }}
            className={btnAuxClass}
          >
            <ShieldCheck className="size-4 shrink-0" aria-hidden />
            Phân quyền
          </button>
          <button
            type="button"
            onClick={(event) => {
              event.stopPropagation()
              setPolicyRole(role)
            }}
            className={btnAuxClass}
          >
            <SlidersHorizontal className="size-4 shrink-0" aria-hidden />
            Phạm vi
          </button>
        </div>
      ),
    },
  ], [])

  const actionsFor = useCallback((row: SystemRole): RowActionItem[] => {
    const role = roles.find((candidate) => candidate.id === row.id) ?? row
    if (role.isSystemRole) return []
    return [
      { id: 'edit', label: 'Chỉnh sửa vai trò', icon: Pencil, onClick: () => { setEditingRole(role); setDialogOpen(true) } },
      { id: 'delete', label: 'Xóa vai trò', icon: Trash2, danger: true, separatorBefore: true, onClick: () => setPendingDelete(role) },
    ]
  }, [roles])

  return (
    <section className="flex h-full min-h-0 flex-col gap-4 overflow-hidden p-4 text-sm text-slate-800 sm:p-6">
      <PageHeader
        title="Vai trò"
        description="Quản lý vai trò, quyền chức năng và phạm vi dữ liệu."
        actions={(
          <>
            <label className="relative min-w-56 flex-1 sm:max-w-sm">
              <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
              <input aria-label="Tìm vai trò" value={search} onChange={(event) => setSearch(event.currentTarget.value)} placeholder="Tìm tên hoặc mô tả vai trò..." className="h-10 w-full rounded-xl border border-slate-200 bg-white pl-9 pr-3 text-sm outline-none focus:border-teal-600" />
            </label>
            <button type="button" onClick={() => { setEditingRole(null); setDialogOpen(true) }} className={btnPrimaryClass}>
              <Plus className="size-4" aria-hidden /> Thêm vai trò
            </button>
          </>
        )}
      />
      <SimpleTable
        items={filteredRoles}
        columns={columns}
        loading={rolesQuery.isLoading}
        emptyMessage={rolesQuery.isLoading ? 'Đang tải danh sách vai trò…' : 'Không tìm thấy vai trò phù hợp.'}
        actionsFor={actionsFor}
      />
      <RoleFormDialog open={dialogOpen} item={editingRole} onClose={() => { setDialogOpen(false); setEditingRole(null) }} onSubmit={roleSave} />
      <RoleAuthorizationDialog
        role={authorizationRole}
        groups={catalogQuery.data ?? []}
        loading={catalogQuery.isLoading}
        onClose={() => setAuthorizationRole(null)}
        onSavePermissions={roleSavePermissions}
      />
      <RolePolicyDialog
        open={Boolean(policyRole)}
        role={policyRole}
        onClose={() => setPolicyRole(null)}
        onSave={async (codes) => {
          await roleSavePolicies(codes)
          setPolicyRole(null)
        }}
        saving={policyMutation.isPending}
        policyCatalogPath={policyCatalogPath}
      />
      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onClose={() => setPendingDelete(null)}
        onConfirm={() => pendingDelete && deleteMutation.mutate(pendingDelete)}
        title="Xóa vai trò?"
        description={pendingDelete ? `Vai trò ${pendingDelete.name} sẽ bị xóa. Vai trò đang gán cho người dùng không thể xóa.` : undefined}
      />
    </section>
  )
}
