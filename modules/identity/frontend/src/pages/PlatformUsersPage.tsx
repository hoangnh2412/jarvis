import { useCallback, useEffect, useMemo, useState } from 'react'
import { FeatureDialog, PageHeader, Tag, notify, type RowActionItem } from '@platform/core'
import { Lock, Pencil, Plus, RefreshCw, Search, Unlock } from 'lucide-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useHasPermission } from '../hooks/useAuth'
import { notifyApiError } from '../notify'
import {
  createPlatformUser,
  listPlatformUsers,
  platformUserQueryKeys,
  updatePlatformUser,
  type PlatformUser,
  type PlatformUserAdminInput,
} from '../services/platformUsers'
import { SimpleTable, type SimpleColumn } from '../components/SimpleTable'
import { btnPrimaryClass, btnSecondaryClass, fieldInputClass, labelClass } from '../components/fieldStyles'

const emptyAdmin: PlatformUserAdminInput = {
  username: '',
  email: '',
  password: '',
  displayName: '',
}

export type PlatformUsersPageProps = {
  /** Mã quyền quản trị user nền tảng. Mặc định `Platform.Tenant.Manage`. */
  managePermission?: string
}

/** Quản lý tài khoản quản trị trên Master (realm platform). */
export function PlatformUsersPage({ managePermission = 'Platform.Tenant.Manage' }: PlatformUsersPageProps) {
  const queryClient = useQueryClient()
  const canManage = useHasPermission(managePermission)
  const [search, setSearch] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [editUser, setEditUser] = useState<PlatformUser | null>(null)
  const [admin, setAdmin] = useState<PlatformUserAdminInput>(emptyAdmin)
  const [editEmail, setEditEmail] = useState('')
  const [editDisplayName, setEditDisplayName] = useState('')
  const [editPhone, setEditPhone] = useState('')
  const [editPassword, setEditPassword] = useState('')

  const usersQuery = useQuery({
    queryKey: platformUserQueryKeys.list(search),
    queryFn: () => listPlatformUsers({ search: search.trim() || undefined }),
  })

  useEffect(() => {
    if (usersQuery.isError) notifyApiError(usersQuery.error)
  }, [usersQuery.error, usersQuery.isError])

  const rows = usersQuery.data ?? []

  const refresh = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: platformUserQueryKeys.all })
  }, [queryClient])

  const createMutation = useMutation({
    mutationFn: () => createPlatformUser({ admin }),
    onError: notifyApiError,
    onSuccess: async (created) => {
      await refresh()
      setCreateOpen(false)
      setAdmin(emptyAdmin)
      notify.success(`Đã tạo tài khoản nền tảng ${created.username}.`)
    },
  })

  const updateMutation = useMutation({
    mutationFn: (user: PlatformUser) =>
      updatePlatformUser(user.id, {
        email: editEmail.trim(),
        displayName: editDisplayName.trim(),
        phoneNumber: editPhone.trim(),
        password: editPassword.trim() || undefined,
      }),
    onError: notifyApiError,
    onSuccess: async () => {
      await refresh()
      setEditUser(null)
      notify.success('Đã cập nhật tài khoản.')
    },
  })

  const lockMutation = useMutation({
    mutationFn: ({ user, isLocked }: { user: PlatformUser; isLocked: boolean }) =>
      updatePlatformUser(user.id, { isLocked }),
    onError: notifyApiError,
    onSuccess: async (_result, { isLocked }) => {
      await refresh()
      notify.success(isLocked ? 'Đã khóa tài khoản.' : 'Đã mở khóa tài khoản.')
    },
  })

  const openEdit = useCallback((user: PlatformUser) => {
    setEditUser(user)
    setEditEmail(user.email)
    setEditDisplayName(user.displayName)
    setEditPhone(user.phoneNumber)
    setEditPassword('')
  }, [])

  const columns = useMemo<SimpleColumn<PlatformUser>[]>(() => [
    { key: 'username', label: 'Tên đăng nhập', className: 'font-mono font-semibold', value: (row) => row.username },
    { key: 'displayName', label: 'Họ tên', value: (row) => row.displayName || '—' },
    { key: 'email', label: 'Email', className: 'text-slate-500', value: (row) => row.email || '—' },
    { key: 'roleNames', label: 'Vai trò', value: (row) => row.roleNames.join(', ') || '—' },
    {
      key: 'isLocked',
      label: 'Trạng thái',
      render: (row) => row.isLocked
        ? <Tag severity="secondary">Đã khóa</Tag>
        : <Tag severity="success">Hoạt động</Tag>,
    },
  ], [])

  const actionsFor = useCallback((user: PlatformUser): RowActionItem[] => {
    if (!canManage) return []
    return [
      { id: 'edit', label: 'Chỉnh sửa', icon: Pencil, onClick: () => openEdit(user) },
      user.isLocked
        ? { id: 'unlock', label: 'Mở khóa', icon: Unlock, onClick: () => lockMutation.mutate({ user, isLocked: false }) }
        : { id: 'lock', label: 'Khóa tài khoản', icon: Lock, danger: true, separatorBefore: true, onClick: () => lockMutation.mutate({ user, isLocked: true }) },
    ]
  }, [canManage, lockMutation, openEdit])

  const canCreate = admin.username.trim().length >= 3 && /.+@.+\..+/.test(admin.email.trim()) && (admin.password?.length ?? 0) >= 8

  return (
    <section className="flex h-full min-h-0 flex-col gap-4 overflow-hidden p-4 text-sm text-slate-800 sm:p-6">
      <PageHeader
        title="Người dùng nền tảng"
        description="Tài khoản quản trị trên Master."
        actions={(
          <>
            <label className="relative min-w-56 flex-1 sm:max-w-sm">
              <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
              <input aria-label="Tìm người dùng nền tảng" value={search} onChange={(event) => setSearch(event.currentTarget.value)} placeholder="Tìm tên đăng nhập, họ tên, email…" className="h-10 w-full rounded-xl border border-slate-200 bg-white pl-9 pr-3 text-sm outline-none focus:border-teal-600" />
            </label>
            <button type="button" onClick={() => void refresh()} className={btnSecondaryClass}>
              <RefreshCw className="size-4" aria-hidden /> Tải lại
            </button>
            {canManage ? (
              <button type="button" onClick={() => setCreateOpen(true)} className={btnPrimaryClass}>
                <Plus className="size-4" aria-hidden /> Thêm tài khoản
              </button>
            ) : null}
          </>
        )}
      />
      <SimpleTable
        items={rows}
        columns={columns}
        loading={usersQuery.isLoading}
        emptyMessage={usersQuery.isLoading ? 'Đang tải tài khoản nền tảng…' : 'Không tìm thấy tài khoản phù hợp.'}
        actionsFor={actionsFor}
      />

      <FeatureDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="Thêm tài khoản nền tảng"
        description="Tài khoản đăng nhập cổng nền tảng (Master)."
        size="lg"
        footer={(
          <>
            <button type="button" className={btnSecondaryClass} onClick={() => setCreateOpen(false)}>Hủy</button>
            <button type="button" className={btnPrimaryClass} disabled={!canCreate || createMutation.isPending} onClick={() => createMutation.mutate()}>
              Tạo tài khoản
            </button>
          </>
        )}
      >
        <div className="grid gap-4 sm:grid-cols-2">
          <label className="block">
            <span className={labelClass}>Tên đăng nhập <span className="text-red-600">*</span></span>
            <input className={fieldInputClass} value={admin.username} onChange={(e) => setAdmin((c) => ({ ...c, username: e.target.value }))} autoComplete="username" />
          </label>
          <label className="block">
            <span className={labelClass}>Họ tên</span>
            <input className={fieldInputClass} value={admin.displayName} onChange={(e) => setAdmin((c) => ({ ...c, displayName: e.target.value }))} autoComplete="name" />
          </label>
          <label className="block">
            <span className={labelClass}>Email <span className="text-red-600">*</span></span>
            <input className={fieldInputClass} type="email" value={admin.email} onChange={(e) => setAdmin((c) => ({ ...c, email: e.target.value }))} autoComplete="email" />
          </label>
          <label className="block">
            <span className={labelClass}>Mật khẩu <span className="text-red-600">*</span></span>
            <input className={fieldInputClass} type="password" value={admin.password ?? ''} onChange={(e) => setAdmin((c) => ({ ...c, password: e.target.value }))} autoComplete="new-password" />
          </label>
        </div>
      </FeatureDialog>

      <FeatureDialog
        open={Boolean(editUser)}
        onClose={() => setEditUser(null)}
        title="Cập nhật tài khoản nền tảng"
        description={editUser?.username}
        size="lg"
        footer={(
          <>
            <button type="button" className={btnSecondaryClass} onClick={() => setEditUser(null)}>Hủy</button>
            <button type="button" className={btnPrimaryClass} disabled={!editUser || updateMutation.isPending} onClick={() => editUser && updateMutation.mutate(editUser)}>
              Lưu thay đổi
            </button>
          </>
        )}
      >
        <div className="grid gap-4 sm:grid-cols-2">
          <label className="block">
            <span className={labelClass}>Email</span>
            <input className={fieldInputClass} type="email" value={editEmail} onChange={(e) => setEditEmail(e.target.value)} />
          </label>
          <label className="block">
            <span className={labelClass}>Họ tên</span>
            <input className={fieldInputClass} value={editDisplayName} onChange={(e) => setEditDisplayName(e.target.value)} />
          </label>
          <label className="block">
            <span className={labelClass}>Điện thoại</span>
            <input className={fieldInputClass} type="tel" value={editPhone} onChange={(e) => setEditPhone(e.target.value)} />
          </label>
          <label className="block">
            <span className={labelClass}>Mật khẩu mới (để trống nếu giữ nguyên)</span>
            <input className={fieldInputClass} type="password" value={editPassword} onChange={(e) => setEditPassword(e.target.value)} autoComplete="new-password" />
          </label>
        </div>
      </FeatureDialog>
    </section>
  )
}
