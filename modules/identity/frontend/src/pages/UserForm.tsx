import { zodResolver } from '@hookform/resolvers/zod'
import { Button, InputText } from '@platform/core'
import { type ReactNode, useEffect } from 'react'
import { useForm, type UseFormRegisterReturn } from 'react-hook-form'
import { useQuery } from '@tanstack/react-query'
import { z } from 'zod'
import type { SystemUser } from '../services/users'
import type { SystemRole } from '../services/roles'
import { btnPrimaryClass, btnSecondaryClass, fieldInputClass, fieldInputInvalidClass } from '../components/fieldStyles'

export type OrgUnitOption = {
  id: string
  name: string
  unitType: string
  path: string
}

function buildUserFormSchema(requirePassword: boolean) {
  return z.object({
    userName: z.string().trim().min(3, 'Tên đăng nhập cần có ít nhất 3 ký tự.'),
    fullName: z.string().trim().min(1, 'Vui lòng nhập họ và tên.'),
    email: z.string().trim().email('Email chưa đúng định dạng.'),
    phoneNumber: z.string().trim().optional(),
    password: requirePassword
      ? z.string().min(8, 'Mật khẩu cần ít nhất 8 ký tự.')
      : z.string().optional(),
    roleIds: z.array(z.string()).min(1, 'Chọn ít nhất một vai trò.'),
    isActive: z.boolean(),
    organizationalUnitId: z.string().optional(),
  })
}

export type UserFormValues = z.infer<ReturnType<typeof buildUserFormSchema>>

const userFormDefaults: UserFormValues = {
  userName: '',
  fullName: '',
  email: '',
  phoneNumber: '',
  password: '',
  roleIds: [],
  isActive: true,
  organizationalUnitId: '',
}

/* ------------------------------------------------------------------ */
/*  Context — tất cả logic & state của form user                       */
/* ------------------------------------------------------------------ */
export type UserFormContext = {
  item: SystemUser | null
  roles: SystemRole[]
  submitLabel: string

  /* Spread-able register props cho từng field (giống LoginPage pattern) */
  userNameProps: UseFormRegisterReturn<'userName'>
  fullNameProps: UseFormRegisterReturn<'fullName'>
  emailProps: UseFormRegisterReturn<'email'>
  phoneProps: UseFormRegisterReturn<'phoneNumber'>
  passwordProps: UseFormRegisterReturn<'password'>
  orgUnitProps: UseFormRegisterReturn<'organizationalUnitId'>
  isActiveProps: UseFormRegisterReturn<'isActive'>

  /* Errors per field */
  errors: {
    userName?: string
    fullName?: string
    email?: string
    phoneNumber?: string
    password?: string
    roleIds?: string
    organizationalUnitId?: string
  }

  /* Role selection */
  selectedRoleIds: string[]
  toggleRole: (roleId: string, checked: boolean) => void

  /* Org unit */
  orgUnits: OrgUnitOption[]
  orgUnitsLoading: boolean
  selectedUnit: OrgUnitOption | undefined
  hasOrgUnits: boolean

  /* Form state */
  isEditMode: boolean
  handleSubmit: (e?: React.BaseSyntheticEvent) => void
  onCancel: () => void
}

export type UserFormProps = {
  item: SystemUser | null
  roles: SystemRole[]
  submitLabel: string
  onCancel: () => void
  onSubmit: (values: UserFormValues) => void
  /** Host nạp danh sách đơn vị tổ chức để gán user; không truyền thì ẩn trường đơn vị. */
  loadOrgUnits?: () => Promise<OrgUnitOption[]>
  /**
   * Custom UI — host truyền JSX hoặc render function nhận `UserFormContext`.
   * Không truyền → hiển thị giao diện mặc định (card form).
   */
  content?: ReactNode | ((ctx: UserFormContext) => ReactNode)
}

/** Form dùng chung cho thêm mới và cập nhật người dùng. */
export function UserForm({ item, roles, submitLabel, onCancel, onSubmit, loadOrgUnits, content }: UserFormProps) {
  const {
    register,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors },
  } = useForm<UserFormValues>({
    resolver: zodResolver(buildUserFormSchema(!item)),
    defaultValues: userFormDefaults,
  })
  const selectedRoleIds = watch('roleIds')
  const selectedUnitId = watch('organizationalUnitId')
  const unitsQuery = useQuery({
    queryKey: ['identity', 'user-form', 'org-units'],
    queryFn: () => loadOrgUnits!(),
    enabled: Boolean(loadOrgUnits),
  })
  const selectedUnit = (unitsQuery.data ?? []).find((unit) => unit.id === selectedUnitId)

  useEffect(() => {
    reset(item ? { ...item, password: '' } : userFormDefaults)
  }, [item, reset])

  const userToggleRole = (roleId: string, checked: boolean) => {
    const nextRoleIds = checked
      ? Array.from(new Set([...selectedRoleIds, roleId]))
      : selectedRoleIds.filter((selectedId) => selectedId !== roleId)
    setValue('roleIds', nextRoleIds, { shouldDirty: true, shouldValidate: true })
  }

  /* ---------- content render-prop ---------- */
  if (content) {
    const ctx: UserFormContext = {
      item,
      roles,
      submitLabel,
      userNameProps: register('userName'),
      fullNameProps: register('fullName'),
      emailProps: register('email'),
      phoneProps: register('phoneNumber'),
      passwordProps: register('password'),
      orgUnitProps: register('organizationalUnitId'),
      isActiveProps: register('isActive'),
      errors: {
        userName: errors.userName?.message,
        fullName: errors.fullName?.message,
        email: errors.email?.message,
        phoneNumber: errors.phoneNumber?.message,
        password: errors.password?.message,
        roleIds: errors.roleIds?.message,
        organizationalUnitId: errors.organizationalUnitId?.message,
      },
      selectedRoleIds,
      toggleRole: userToggleRole,
      orgUnits: unitsQuery.data ?? [],
      orgUnitsLoading: unitsQuery.isLoading,
      selectedUnit,
      hasOrgUnits: Boolean(loadOrgUnits),
      isEditMode: Boolean(item),
      handleSubmit: handleSubmit(onSubmit),
      onCancel,
    }
    const resolved = typeof content === 'function' ? content(ctx) : content
    return <>{resolved}</>
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="w-full overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <div className="space-y-6 p-4 sm:p-6">
        <section className="space-y-4">
          <div className="flex items-start justify-between gap-3 border-b border-slate-200 pb-3">
            <div>
              <h2 className="m-0 text-base font-semibold text-slate-900">Thông tin tài khoản</h2>
              <p className="mb-0 mt-1 text-sm text-slate-500">Nhập thông tin đăng nhập và liên hệ của người dùng.</p>
            </div>
            <span className="shrink-0 text-xs text-slate-500"><span className="text-red-600">*</span> Bắt buộc</span>
          </div>
          <div className="grid grid-cols-1 gap-x-5 gap-y-4 md:grid-cols-2">
            <div>
              <label htmlFor="user-name" className="mb-1.5 block text-sm font-medium text-slate-800">Tên đăng nhập <span className="text-red-600">*</span></label>
              <InputText
                id="user-name"
                autoComplete="username"
                aria-invalid={Boolean(errors.userName)}
                aria-describedby={errors.userName ? 'user-name-error' : undefined}
                className={`${fieldInputClass} ${errors.userName ? fieldInputInvalidClass : ''}`}
                readOnly={Boolean(item)}
                {...register('userName')}
              />
              {errors.userName && <p id="user-name-error" className="mb-0 mt-1.5 text-xs text-red-600">{errors.userName.message}</p>}
            </div>
            <div>
              <label htmlFor="user-full-name" className="mb-1.5 block text-sm font-medium text-slate-800">Họ và tên <span className="text-red-600">*</span></label>
              <InputText
                id="user-full-name"
                autoComplete="name"
                aria-invalid={Boolean(errors.fullName)}
                aria-describedby={errors.fullName ? 'user-full-name-error' : undefined}
                className={`${fieldInputClass} ${errors.fullName ? fieldInputInvalidClass : ''}`}
                {...register('fullName')}
              />
              {errors.fullName && <p id="user-full-name-error" className="mb-0 mt-1.5 text-xs text-red-600">{errors.fullName.message}</p>}
            </div>
            <div>
              <label htmlFor="user-email" className="mb-1.5 block text-sm font-medium text-slate-800">Email <span className="text-red-600">*</span></label>
              <InputText
                id="user-email"
                type="email"
                autoComplete="email"
                aria-invalid={Boolean(errors.email)}
                aria-describedby={errors.email ? 'user-email-error' : undefined}
                className={`${fieldInputClass} ${errors.email ? fieldInputInvalidClass : ''}`}
                {...register('email')}
              />
              {errors.email && <p id="user-email-error" className="mb-0 mt-1.5 text-xs text-red-600">{errors.email.message}</p>}
            </div>
            <div>
              <label htmlFor="user-phone" className="mb-1.5 block text-sm font-medium text-slate-800">Điện thoại</label>
              <InputText
                id="user-phone"
                type="tel"
                autoComplete="tel"
                className={fieldInputClass}
                {...register('phoneNumber')}
              />
            </div>
            {!item && (
              <div className="md:col-span-2">
                <label htmlFor="user-password" className="mb-1.5 block text-sm font-medium text-slate-800">Mật khẩu <span className="text-red-600">*</span></label>
                <InputText
                  id="user-password"
                  type="password"
                  autoComplete="new-password"
                  aria-invalid={Boolean(errors.password)}
                  aria-describedby={errors.password ? 'user-password-error' : undefined}
                  className={`${fieldInputClass} ${errors.password ? fieldInputInvalidClass : ''}`}
                  {...register('password')}
                />
                {errors.password && <p id="user-password-error" className="mb-0 mt-1.5 text-xs text-red-600">{errors.password.message}</p>}
              </div>
            )}
            {loadOrgUnits ? (
              <div className="md:col-span-2">
                <label htmlFor="user-ou" className="mb-1.5 block text-sm font-medium text-slate-800">Đơn vị tổ chức</label>
                <select id="user-ou" className={fieldInputClass} {...register('organizationalUnitId')}>
                  <option value="">Chưa gán</option>
                  {(unitsQuery.data ?? []).map((unit) => (
                    <option key={unit.id} value={unit.id}>{unit.name} ({unit.unitType})</option>
                  ))}
                </select>
                {selectedUnit ? (
                  <p className="mb-0 mt-1.5 text-xs text-slate-500">Loại đơn vị: {selectedUnit.unitType}. Path: {selectedUnit.path}</p>
                ) : null}
              </div>
            ) : null}
          </div>
        </section>

        <section className="space-y-4 border-t border-slate-200 pt-5">
          <div>
            <h2 className="m-0 text-base font-semibold text-slate-900">Phân quyền</h2>
            <p className="mb-0 mt-1 text-sm text-slate-500">Chọn vai trò để xác định quyền truy cập của người dùng.</p>
          </div>
          <fieldset className="m-0 grid min-w-0 grid-cols-1 gap-3 border-0 p-0 sm:grid-cols-2">
            <legend className="sr-only">Vai trò người dùng</legend>
            {roles.map((role) => {
              const selected = selectedRoleIds.includes(role.id)
              return (
                <label key={role.id} className={`flex min-h-20 cursor-pointer items-start gap-3 rounded-xl border p-3 transition-colors ${selected ? 'border-teal-600 bg-teal-50' : 'border-slate-200 bg-white hover:bg-slate-50'}`}>
                  <input type="checkbox" checked={selected} onChange={(event) => userToggleRole(role.id, event.currentTarget.checked)} className="mt-0.5 size-4 shrink-0 accent-teal-700" />
                  <span className="min-w-0">
                    <span className="block text-sm font-semibold text-slate-800">{role.name}</span>
                    <span className="mt-1 block text-xs leading-5 text-slate-500">{role.description}</span>
                  </span>
                </label>
              )
            })}
          </fieldset>
          {errors.roleIds && <p className="mb-0 text-sm text-red-600">{errors.roleIds.message}</p>}
          <label className="flex items-center gap-3 rounded-xl bg-slate-50 px-3 py-3 text-sm text-slate-800">
            <input type="checkbox" className="size-4 accent-teal-700" {...register('isActive')} />
            <span>
              <span className="block font-medium">Tài khoản đang hoạt động</span>
              <span className="mt-0.5 block text-xs text-slate-500">Cho phép người dùng đăng nhập vào hệ thống.</span>
            </span>
          </label>
        </section>
      </div>
      <footer className="flex justify-end gap-2 border-t border-slate-200 bg-slate-50 px-4 py-3 sm:px-6">
        <Button type="button" onClick={onCancel} className={btnSecondaryClass}>Hủy</Button>
        <Button type="submit" className={btnPrimaryClass}>{submitLabel}</Button>
      </footer>
    </form>
  )
}
