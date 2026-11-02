import { useCallback, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ConfirmDialog, FeatureDialog, PageHeader, Tag, notify, type RowActionItem } from '@platform/core'
import { Lock, Pencil, Plus, Search, Trash2, Unlock } from 'lucide-react'
import {
  createSystemUser,
  deleteSystemUser,
  identityQueryKeys,
  listSystemUsers,
  updateSystemUser,
  type SystemUser,
} from '../services/users'
import { listSystemRoles } from '../services/roles'
import { notifyApiError } from '../notify'
import { searchNormalize } from '../utils'
import { SimpleTable, type SimpleColumn } from '../components/SimpleTable'
import { btnPrimaryClass } from '../components/fieldStyles'
import { UserForm, type OrgUnitOption, type UserFormValues } from './UserForm'

export type UsersPageProps = {
  /** Host nạp danh sách đơn vị tổ chức cho form gán user. */
  loadOrgUnits?: () => Promise<OrgUnitOption[]>
  /** Host lưu gán đơn vị sau khi tạo/sửa user thành công. */
  saveUserOrgUnit?: (userId: string, organizationalUnitId: string) => Promise<void>
}

/** Trang danh sách người dùng của tenant; thêm/sửa qua dialog. */
export function UsersPage({ loadOrgUnits, saveUserOrgUnit }: UsersPageProps) {
  const queryClient = useQueryClient()
  const usersQuery = useQuery({ queryKey: identityQueryKeys.users, queryFn: listSystemUsers })
  const rolesQuery = useQuery({ queryKey: identityQueryKeys.roles, queryFn: listSystemRoles })
  const [search, setSearch] = useState('')
  const [formUser, setFormUser] = useState<SystemUser | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [pendingDelete, setPendingDelete] = useState<SystemUser | null>(null)
  const users = usersQuery.data ?? []
  const roles = rolesQuery.data ?? []

  const refreshUsers = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: identityQueryKeys.users })
  }, [queryClient])

  const saveMutation = useMutation({
    mutationFn: async (values: UserFormValues) => {
      const input = {
        userName: values.userName,
        fullName: values.fullName,
        email: values.email,
        phoneNumber: values.phoneNumber ?? '',
        roleIds: values.roleIds,
        isActive: values.isActive,
        password: values.password,
      }
      const saved = formUser
        ? await updateSystemUser(formUser.id, input)
        : await createSystemUser(input)
      if (values.organizationalUnitId && saveUserOrgUnit) {
        await saveUserOrgUnit(saved.id, values.organizationalUnitId)
      }
      return saved
    },
    onError: notifyApiError,
    onSuccess: async () => {
      await refreshUsers()
      setFormOpen(false)
      setFormUser(null)
      notify.success(formUser ? 'Đã cập nhật người dùng.' : 'Đã thêm người dùng.')
    },
  })

  const toggleMutation = useMutation({
    mutationFn: (user: SystemUser) => updateSystemUser(user.id, { isActive: !user.isActive }),
    onError: notifyApiError,
    onSuccess: async (_user, target) => {
      await refreshUsers()
      notify.success(target.isActive ? 'Đã ngừng kích hoạt tài khoản.' : 'Đã kích hoạt lại tài khoản.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (user: SystemUser) => deleteSystemUser(user.id),
    onError: notifyApiError,
    onSuccess: async () => {
      await refreshUsers()
      notify.success('Đã xóa người dùng.')
      setPendingDelete(null)
    },
  })

  const roleById = useMemo(() => new Map(roles.map((role) => [role.id, role])), [roles])
  const filteredUsers = useMemo(() => {
    const normalizedSearch = searchNormalize(search.trim())
    if (!normalizedSearch) return users
    return users.filter((user) =>
      searchNormalize([user.userName, user.fullName, user.email, user.phoneNumber, ...user.roleIds.map((id) => roleById.get(id)?.name ?? '')].join(' ')).includes(normalizedSearch),
    )
  }, [roleById, search, users])

  const columns = useMemo<SimpleColumn<SystemUser>[]>(() => [
    { key: 'userName', label: 'Tên đăng nhập', className: 'font-mono font-medium whitespace-nowrap', value: (user) => user.userName },
    { key: 'fullName', label: 'Họ và tên', className: 'font-medium', value: (user) => user.fullName },
    { key: 'email', label: 'Email', className: 'text-slate-500', value: (user) => user.email },
    {
      key: 'roles',
      label: 'Vai trò',
      value: (user) => user.roleIds.map((id) => roleById.get(id)?.name ?? id).join(', '),
    },
    {
      key: 'isActive',
      label: 'Trạng thái',
      render: (user) => user.isActive
        ? <Tag severity="success">Hoạt động</Tag>
        : <Tag severity="secondary">Ngừng hoạt động</Tag>,
    },
  ], [roleById])

  const actionsFor = useCallback((user: SystemUser): RowActionItem[] => [
    { id: 'edit', label: 'Chỉnh sửa', icon: Pencil, onClick: () => { setFormUser(user); setFormOpen(true) } },
    {
      id: 'toggle',
      label: user.isActive ? 'Ngừng kích hoạt' : 'Kích hoạt lại',
      icon: user.isActive ? Lock : Unlock,
      onClick: () => toggleMutation.mutate(user),
    },
    { id: 'delete', label: 'Xóa người dùng', icon: Trash2, danger: true, separatorBefore: true, onClick: () => setPendingDelete(user) },
  ], [toggleMutation])

  return (
    <section className="flex h-full min-h-0 flex-col gap-4 overflow-hidden p-4 text-sm text-slate-800 sm:p-6">
      <PageHeader
        title="Người dùng"
        description="Quản lý tài khoản người dùng của doanh nghiệp."
        actions={(
          <>
            <label className="relative min-w-56 flex-1 sm:max-w-sm">
              <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
              <input aria-label="Tìm người dùng" value={search} onChange={(event) => setSearch(event.currentTarget.value)} placeholder="Tìm tên, email, vai trò..." className="h-10 w-full rounded-xl border border-slate-200 bg-white pl-9 pr-3 text-sm outline-none focus:border-teal-600" />
            </label>
            <button type="button" onClick={() => { setFormUser(null); setFormOpen(true) }} className={btnPrimaryClass}>
              <Plus className="size-4" aria-hidden /> Thêm người dùng
            </button>
          </>
        )}
      />
      <SimpleTable
        items={filteredUsers}
        columns={columns}
        loading={usersQuery.isLoading}
        emptyMessage={usersQuery.isLoading ? 'Đang tải danh sách người dùng…' : 'Không tìm thấy người dùng phù hợp.'}
        actionsFor={actionsFor}
      />
      <FeatureDialog
        open={formOpen}
        onClose={() => { setFormOpen(false); setFormUser(null) }}
        title={formUser ? 'Sửa người dùng' : 'Thêm người dùng'}
        size="xl"
        hideFooter
      >
        {formOpen ? (
          <UserForm
            item={formUser}
            roles={roles.filter((role) => role.isActive || (formUser?.roleIds.includes(role.id) ?? false))}
            submitLabel={formUser ? 'Lưu thay đổi' : 'Tạo người dùng'}
            onCancel={() => { setFormOpen(false); setFormUser(null) }}
            onSubmit={(values) => saveMutation.mutate(values)}
            loadOrgUnits={loadOrgUnits}
          />
        ) : null}
      </FeatureDialog>
      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onClose={() => setPendingDelete(null)}
        onConfirm={() => pendingDelete && deleteMutation.mutate(pendingDelete)}
        title="Xóa người dùng?"
        description={pendingDelete ? `Tài khoản ${pendingDelete.userName} sẽ bị xóa khỏi doanh nghiệp hiện tại.` : undefined}
      />
    </section>
  )
}
