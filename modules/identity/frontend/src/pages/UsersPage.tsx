import { type ReactNode, useCallback, useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { AdminList, ConfirmDialog, FeatureDialog, Tag, notify, type AdminListColumn, type RowActionItem } from '@platform/core'
import { Lock, Pencil, Plus, Trash2, Unlock } from 'lucide-react'
import {
  createSystemUser,
  deleteSystemUser,
  identityQueryKeys,
  listSystemUsers,
  updateSystemUser,
  type SystemUser,
} from '../services/users'
import { listSystemRoles, type SystemRole } from '../services/roles'
import { notifyApiError } from '../notify'
import { searchNormalize } from '../utils'
import { btnPrimaryClass } from '../components/fieldStyles'
import { UserForm, type OrgUnitOption, type UserFormValues } from './UserForm'

/* ------------------------------------------------------------------ */
/*  Context — tất cả logic, state & handler của trang danh sách user  */
/* ------------------------------------------------------------------ */
export type UsersPageContext = {
  /* --- Data --- */
  users: SystemUser[]
  filteredUsers: SystemUser[]
  roles: SystemRole[]
  roleById: Map<string, SystemRole>
  isLoading: boolean

  /* --- Search --- */
  search: string
  onSearchChange: (value: string) => void

  /* --- Table --- */
  columns: AdminListColumn<SystemUser>[]
  actionsFor: (user: SystemUser) => RowActionItem[]

  /* --- Form dialog --- */
  formOpen: boolean
  formUser: SystemUser | null
  openCreate: () => void
  openEdit: (user: SystemUser) => void
  closeForm: () => void
  onFormSubmit: (values: UserFormValues) => void
  isFormSaving: boolean

  /* --- Delete dialog --- */
  pendingDelete: SystemUser | null
  requestDelete: (user: SystemUser) => void
  confirmDelete: () => void
  cancelDelete: () => void

  /* --- Misc --- */
  refreshUsers: () => Promise<void>
  loadOrgUnits?: () => Promise<OrgUnitOption[]>
  saveUserOrgUnit?: (userId: string, organizationalUnitId: string) => Promise<void>
}

export type UsersPageProps = {
  /** Host nạp danh sách đơn vị tổ chức cho form gán user. */
  loadOrgUnits?: () => Promise<OrgUnitOption[]>
  /** Host lưu gán đơn vị sau khi tạo/sửa user thành công. */
  saveUserOrgUnit?: (userId: string, organizationalUnitId: string) => Promise<void>
  /**
   * Custom UI — host truyền JSX hoặc render function nhận `UsersPageContext`.
   * Không truyền → hiển thị giao diện mặc định (AdminList + FeatureDialog).
   */
  content?: ReactNode | ((ctx: UsersPageContext) => ReactNode)
}

/** Trang danh sách người dùng của tenant; thêm/sửa qua dialog. */
export function UsersPage({ loadOrgUnits, saveUserOrgUnit, content }: UsersPageProps) {
  const queryClient = useQueryClient()
  const usersQuery = useQuery({ queryKey: identityQueryKeys.users, queryFn: listSystemUsers, retry: false })
  const rolesQuery = useQuery({ queryKey: identityQueryKeys.roles, queryFn: listSystemRoles, retry: false })

  useEffect(() => {
    if (usersQuery.isError) notifyApiError(usersQuery.error)
  }, [usersQuery.error, usersQuery.isError])
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

  useEffect(() => {
    const onKey = (event: KeyboardEvent) => {
      if (event.key !== 'F4') return
      event.preventDefault()
      setFormUser(null)
      setFormOpen(true)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [])

  const columns = useMemo<AdminListColumn<SystemUser>[]>(() => [
    { key: 'userName', label: 'Tên đăng nhập', sortable: true, sortValue: (user) => user.userName, className: 'font-mono font-medium whitespace-nowrap', value: (user) => user.userName },
    { key: 'fullName', label: 'Họ và tên', sortable: true, sortValue: (user) => user.fullName, className: 'font-medium', value: (user) => user.fullName },
    { key: 'email', label: 'Email', sortable: true, sortValue: (user) => user.email, className: 'text-slate-500', value: (user) => user.email },
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

  /* ---------- content render-prop ---------- */
  if (content) {
    const ctx: UsersPageContext = {
      users,
      filteredUsers,
      roles,
      roleById,
      isLoading: usersQuery.isLoading,
      search,
      onSearchChange: setSearch,
      columns,
      actionsFor,
      formOpen,
      formUser,
      openCreate: () => { setFormUser(null); setFormOpen(true) },
      openEdit: (user: SystemUser) => { setFormUser(user); setFormOpen(true) },
      closeForm: () => { setFormOpen(false); setFormUser(null) },
      onFormSubmit: (values: UserFormValues) => saveMutation.mutate(values),
      isFormSaving: saveMutation.isPending,
      pendingDelete,
      requestDelete: setPendingDelete,
      confirmDelete: () => pendingDelete && deleteMutation.mutate(pendingDelete),
      cancelDelete: () => setPendingDelete(null),
      refreshUsers,
      loadOrgUnits,
      saveUserOrgUnit,
    }
    const resolved = typeof content === 'function' ? content(ctx) : content
    return <>{resolved}</>
  }

  return (
    <section className="flex h-full min-h-0 flex-col overflow-hidden p-4 text-sm text-slate-800 sm:p-6">
      <AdminList
        items={filteredUsers}
        columns={columns}
        loading={usersQuery.isLoading}
        searchValue={search}
        onSearchChange={setSearch}
        searchLabel="Tìm người dùng"
        searchPlaceholder="Tìm tên, email, vai trò..."
        emptyMessage={
          usersQuery.isError
            ? 'Không tải được danh sách người dùng.'
            : usersQuery.isLoading
              ? 'Đang tải danh sách người dùng…'
              : 'Không tìm thấy người dùng phù hợp.'
        }
        actionsFor={actionsFor}
        primaryAction={(
          <button type="button" onClick={() => { setFormUser(null); setFormOpen(true) }} className={btnPrimaryClass}>
            <Plus className="size-4" aria-hidden /> Thêm người dùng (F4)
          </button>
        )}
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
