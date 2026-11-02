import { useMemo } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, notify } from '@platform/core'
import { ArrowLeft } from 'lucide-react'
import {
  createSystemUser,
  getSystemUser,
  identityQueryKeys,
  updateSystemUser,
} from '../services/users'
import { listSystemRoles } from '../services/roles'
import { notifyApiError } from '../notify'
import { btnSecondaryClass } from '../components/fieldStyles'
import { UserForm, type OrgUnitOption, type UserFormValues } from './UserForm'

export type UserFormPageProps = {
  mode: 'create' | 'edit'
  /** Đường dẫn về danh sách người dùng của host. */
  listPath: string
  loadOrgUnits?: () => Promise<OrgUnitOption[]>
  saveUserOrgUnit?: (userId: string, organizationalUnitId: string) => Promise<void>
}

/** Màn hình thêm hoặc sửa người dùng theo route được chọn. */
export function UserFormPage({ mode, listPath, loadOrgUnits, saveUserOrgUnit }: UserFormPageProps) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { id } = useParams<{ id: string }>()
  const rolesQuery = useQuery({ queryKey: identityQueryKeys.roles, queryFn: listSystemRoles })
  const userQuery = useQuery({
    queryKey: [...identityQueryKeys.users, id],
    queryFn: () => getSystemUser(id ?? ''),
    enabled: mode === 'edit' && Boolean(id),
  })
  const roles = useMemo(() => {
    const loaded = rolesQuery.data ?? []
    if (mode !== 'edit' || !userQuery.data) return loaded.filter((role) => role.isActive)
    return loaded.filter((role) => role.isActive || userQuery.data.roleIds.includes(role.id))
  }, [mode, rolesQuery.data, userQuery.data])

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
      const saved = mode === 'edit' && id
        ? await updateSystemUser(id, input)
        : await createSystemUser(input)
      if (values.organizationalUnitId && saveUserOrgUnit) {
        await saveUserOrgUnit(saved.id, values.organizationalUnitId)
      }
      return saved
    },
    onError: notifyApiError,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: identityQueryKeys.users })
      notify.success(mode === 'edit' ? 'Đã cập nhật người dùng.' : 'Đã thêm người dùng.')
      navigate(listPath)
    },
  })

  const title = mode === 'edit' ? 'Sửa người dùng' : 'Thêm người dùng'
  const isLoading = rolesQuery.isLoading || (mode === 'edit' && userQuery.isLoading)
  const missingUser = mode === 'edit' && !userQuery.isLoading && (userQuery.isError || !userQuery.data)

  return (
    <section className="h-full min-h-0 overflow-y-auto text-sm text-slate-800">
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-5 px-4 py-5 sm:px-6 sm:py-6">
        <header className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <p className="mb-1 text-xs font-semibold uppercase tracking-[0.08em] text-teal-700">Quản lý người dùng</p>
            <h1 className="m-0 text-xl font-bold text-slate-900">{title}</h1>
            <p className="mb-0 mt-1 text-sm text-slate-500">Quản lý thông tin tài khoản và quyền truy cập.</p>
          </div>
          <Button
            type="button"
            onClick={() => navigate(listPath)}
            className={btnSecondaryClass}
          >
            <ArrowLeft className="size-4" aria-hidden /> Quay lại danh sách
          </Button>
        </header>
        {isLoading ? (
          <p role="status" className="text-sm text-slate-500">Đang tải thông tin người dùng…</p>
        ) : missingUser ? (
          <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
            <h2 className="m-0 text-base font-semibold">Không tìm thấy người dùng</h2>
            <p className="mb-4 mt-2 text-sm text-slate-500">Tài khoản có thể đã bị xóa.</p>
            <Button
              type="button"
              className={btnSecondaryClass}
              onClick={() => navigate(listPath)}
            >
              Về danh sách
            </Button>
          </div>
        ) : (
          <UserForm
            item={mode === 'edit' ? userQuery.data ?? null : null}
            roles={roles}
            submitLabel={mode === 'edit' ? 'Lưu thay đổi' : 'Tạo người dùng'}
            onCancel={() => navigate(listPath)}
            onSubmit={(values) => saveMutation.mutate(values)}
            loadOrgUnits={loadOrgUnits}
          />
        )}
      </div>
    </section>
  )
}
