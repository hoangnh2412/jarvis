import { useEffect, useMemo, useState, type FormEvent, type ReactNode } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Eye, EyeOff, Lock, User } from 'lucide-react'
import { useLogin } from '../hooks/useAuth'
import {
  listAppLoginOrgs,
  listAppLoginTenants,
  type LoginCatalogItem,
} from '../services/loginCatalog'

export type LoginBranding = {
  logoUrl?: string
  logoAlt?: string
  backgroundImageUrl?: string
  backgroundLabel?: string
  title?: string
  subtitle?: string
  loadingIndicator?: ReactNode
}

const selectClass = (hasError: boolean) =>
  `h-10 w-full rounded-md border bg-white px-3 text-sm text-slate-800 outline-none transition-colors disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-400 ${
    hasError ? 'border-red-500 focus:border-red-500' : 'border-slate-300 focus:border-teal-600'
  }`

function formatCatalogLabel(item: LoginCatalogItem): string {
  const name = item.name.trim()
  return name ? `${item.code} — ${name}` : item.code
}

/**
 * Context object passed to the `content` render-prop.
 * Contains all login logic; consumer only provides UI.
 */
export type LoginPageContext = {
  /** Pre-rendered tenant <select> (null when realm='platform') */
  TenantSelect: ReactNode
  /** Pre-rendered org <select> (null when realm='platform') */
  OrgSelect: ReactNode

  /** Spread-able props to build your OWN tenant <select> with custom styling */
  tenantSelectProps: {
    value: string
    onChange: (e: React.ChangeEvent<HTMLSelectElement>) => void
    disabled: boolean
    options: LoginCatalogItem[]
    loading: boolean
  }
  /** Spread-able props to build your OWN org <select> with custom styling */
  orgSelectProps: {
    value: string
    onChange: (e: React.ChangeEvent<HTMLSelectElement>) => void
    disabled: boolean
    options: LoginCatalogItem[]
    loading: boolean
  }

  /** Spread-able props for a custom username input */
  usernameProps: {
    value: string
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => void
    disabled: boolean
  }
  /** Spread-able props for a custom password input */
  passwordProps: {
    value: string
    onChange: (val: string) => void
    disabled: boolean
  }
  /** True while the login mutation is in-flight */
  isLoading: boolean
  /** Validation / server errors */
  errors: {
    username?: string
    password?: string
    tenantCode?: string
    orgCode?: string
    general?: string
  }
  /** Attach to <form onSubmit={ctx.handleSubmit}> */
  handleSubmit: (e: FormEvent) => void

  /** Current realm */
  realm: 'platform' | 'app'
  /** Path to app login page */
  appLoginPath: string
  /** Path to platform login page */
  platformLoginPath: string
  /** Path to forgot password page */
  forgotPasswordPath: string
}

export type LoginPageProps = {
  redirectTo?: string
  realm?: 'platform' | 'app'
  branding?: LoginBranding
  appLoginPath?: string
  platformLoginPath?: string
  forgotPasswordPath?: string
  /** Custom UI render-prop. When provided, LoginPage renders this instead of the default form. */
  content?: ReactNode | ((ctx: LoginPageContext) => ReactNode)
  /** Override tenant loader — khi truyền, LoginPage gọi hàm này thay vì API mặc định. */
  loadTenants?: () => Promise<LoginCatalogItem[]>
  /** Override org loader — khi truyền, LoginPage gọi hàm này thay vì API mặc định. */
  loadOrgs?: (tenantId: string) => Promise<LoginCatalogItem[]>
}

export function LoginPage({
  redirectTo = '/dashboard',
  realm = 'app',
  branding,
  appLoginPath = '/app/login',
  platformLoginPath = '/platform/login',
  forgotPasswordPath = '/forgot-password',
  content,
  loadTenants,
  loadOrgs,
}: LoginPageProps) {
  const navigate = useNavigate()
  const { mutate: login, isPending: isLoading } = useLogin()

  const [username, setUsername] = useState('')
  const [tenantId, setTenantId] = useState('')
  const [orgId, setOrgId] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [rememberMe, setRememberMe] = useState(false)
  const [errors, setErrors] = useState<{
    username?: string
    password?: string
    tenantCode?: string
    orgCode?: string
    general?: string
  }>({})

  const tenantsQuery = useQuery({
    queryKey: ['app-login-catalog', 'tenants'],
    queryFn: loadTenants ?? listAppLoginTenants,
    enabled: realm === 'app',
    staleTime: 60_000,
  })

  const orgsQuery = useQuery({
    queryKey: ['app-login-catalog', 'orgs', tenantId],
    queryFn: () => (loadOrgs ?? listAppLoginOrgs)(tenantId),
    enabled: realm === 'app' && Boolean(tenantId),
    staleTime: 60_000,
  })

  const selectedTenant = useMemo(
    () => tenantsQuery.data?.find((row) => row.id === tenantId),
    [tenantsQuery.data, tenantId],
  )

  const selectedOrg = useMemo(
    () => orgsQuery.data?.find((row) => row.id === orgId),
    [orgsQuery.data, orgId],
  )

  useEffect(() => {
    if (realm !== 'app' || !tenantsQuery.data?.length || tenantId) return
    if (tenantsQuery.data.length === 1) {
      setTenantId(tenantsQuery.data[0].id)
    }
  }, [realm, tenantId, tenantsQuery.data])

  useEffect(() => {
    if (realm !== 'app' || !tenantId) {
      setOrgId('')
      return
    }
    if (!orgsQuery.data?.length) {
      setOrgId('')
      return
    }
    if (orgId && orgsQuery.data.some((row) => row.id === orgId)) return
    if (orgsQuery.data.length === 1) {
      setOrgId(orgsQuery.data[0].id)
    } else {
      setOrgId('')
    }
  }, [realm, tenantId, orgId, orgsQuery.data])

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()

    const newErrors: { username?: string; password?: string; tenantCode?: string; orgCode?: string } = {}
    if (realm === 'app' && !tenantId) {
      newErrors.tenantCode = 'Vui lòng chọn tenant'
    }
    if (realm === 'app' && !orgId) {
      newErrors.orgCode = 'Vui lòng chọn organization'
    }
    if (!username.trim()) {
      newErrors.username = 'Vui lòng nhập tên đăng nhập'
    }
    if (!password) {
      newErrors.password = 'Vui lòng nhập mật khẩu'
    }

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors)
      return
    }

    setErrors({})

    login(
      {
        userNameOrEmailAddress: username,
        password,
        realm,
        tenancyName: selectedTenant?.code,
        orgCode: selectedOrg?.code,
        tenantId: selectedTenant?.id,
        orgId: selectedOrg?.id,
        tenantName: selectedTenant?.name,
      },
      {
        onSuccess: () => {
          navigate(redirectTo)
        },
        onError: (err: Error) => {
          setErrors({
            general: err.message || 'Đăng nhập không thành công, vui lòng thử lại',
          })
        },
      },
    )
  }

  /* ── content slot: build context, render custom UI if provided ── */
  if (content) {
    const tenantSelectNode: ReactNode =
      realm === 'app' ? (
        <select
          id="login-tenant"
          value={tenantId}
          onChange={(e) => {
            setTenantId(e.target.value)
            setOrgId('')
            if (errors.tenantCode) setErrors((prev) => ({ ...prev, tenantCode: undefined }))
          }}
          className={selectClass(Boolean(errors.tenantCode))}
          disabled={isLoading || tenantsQuery.isLoading}
        >
          <option value="">
            {tenantsQuery.isLoading ? 'Đang tải…' : '— Chọn đơn vị —'}
          </option>
          {(tenantsQuery.data ?? []).map((tenant) => (
            <option key={tenant.id} value={tenant.id}>
              {formatCatalogLabel(tenant)}
            </option>
          ))}
        </select>
      ) : null

    const orgSelectNode: ReactNode =
      realm === 'app' ? (
        <select
          id="login-org"
          value={orgId}
          onChange={(e) => {
            setOrgId(e.target.value)
            if (errors.orgCode) setErrors((prev) => ({ ...prev, orgCode: undefined }))
          }}
          className={selectClass(Boolean(errors.orgCode))}
          disabled={isLoading || !tenantId || orgsQuery.isLoading}
        >
          <option value="">
            {!tenantId
              ? '— Chọn đơn vị trước —'
              : orgsQuery.isLoading
                ? 'Đang tải…'
                : '— Chọn tổ chức —'}
          </option>
          {(orgsQuery.data ?? []).map((org) => (
            <option key={org.id} value={org.id}>
              {formatCatalogLabel(org)}
            </option>
          ))}
        </select>
      ) : null

    const ctx: LoginPageContext = {
      TenantSelect: tenantSelectNode,
      OrgSelect: orgSelectNode,
      tenantSelectProps: {
        value: tenantId,
        onChange: (e: React.ChangeEvent<HTMLSelectElement>) => {
          setTenantId(e.target.value)
          setOrgId('')
          if (errors.tenantCode) setErrors((prev) => ({ ...prev, tenantCode: undefined }))
        },
        disabled: isLoading || tenantsQuery.isLoading,
        options: tenantsQuery.data ?? [],
        loading: tenantsQuery.isLoading,
      },
      orgSelectProps: {
        value: orgId,
        onChange: (e: React.ChangeEvent<HTMLSelectElement>) => {
          setOrgId(e.target.value)
          if (errors.orgCode) setErrors((prev) => ({ ...prev, orgCode: undefined }))
        },
        disabled: isLoading || !tenantId || orgsQuery.isLoading,
        options: orgsQuery.data ?? [],
        loading: !tenantId ? false : orgsQuery.isLoading,
      },
      usernameProps: {
        value: username,
        onChange: (e: React.ChangeEvent<HTMLInputElement>) => {
          setUsername(e.target.value)
          if (errors.username) setErrors((prev) => ({ ...prev, username: undefined }))
        },
        disabled: isLoading,
      },
      passwordProps: {
        value: password,
        onChange: (val: string) => {
          setPassword(val)
          if (errors.password) setErrors((prev) => ({ ...prev, password: undefined }))
        },
        disabled: isLoading,
      },
      isLoading,
      errors,
      handleSubmit,
      realm,
      appLoginPath,
      platformLoginPath,
      forgotPasswordPath,
    }

    const resolved = typeof content === 'function' ? content(ctx) : content
    return <>{resolved}</>
  }

  const title = branding?.title ?? (realm === 'platform' ? 'Đăng nhập nền tảng' : 'Đăng nhập nghiệp vụ')
  const subtitle =
    branding?.subtitle ??
    (realm === 'platform' ? 'Tài khoản quản trị trên Master' : 'Hệ thống quản lý hóa đơn điện tử')

  return (
    <div className="h-full min-h-0 overflow-y-auto bg-slate-100 font-sans text-sm text-slate-800">
      <main className="h-screen min-h-0 overflow-hidden">
        <div className="grid h-full min-h-0 grid-cols-[minmax(0,2fr)_minmax(360px,1fr)] overflow-hidden bg-white shadow-lg max-[820px]:grid-cols-1">
          <aside
            className="relative flex min-h-full items-start bg-slate-200 bg-cover bg-center p-12 before:absolute before:inset-0 before:bg-black/20 before:content-[''] max-[820px]:min-h-[210px] max-[820px]:p-8"
            style={branding?.backgroundImageUrl ? { backgroundImage: `url("${branding.backgroundImageUrl}")` } : undefined}
            aria-label={branding?.backgroundLabel ?? branding?.logoAlt ?? 'Đăng nhập'}
          >
            {branding?.logoUrl ? (
              <img
                className="relative z-[1] block h-auto w-[min(230px,70%)]"
                src={branding.logoUrl}
                alt={branding.logoAlt ?? ''}
              />
            ) : null}
          </aside>

          <section className="flex h-full min-h-0 flex-col overflow-y-auto overscroll-contain px-8 py-10 sm:px-12 max-[820px]:h-auto max-[820px]:py-8">
            <div className="mx-auto my-auto w-full max-w-[400px]">
              {branding?.logoUrl ? (
                <div className="mb-8 flex flex-col items-center text-center">
                  <img
                    src={branding.logoUrl}
                    alt={branding.logoAlt ?? ''}
                    className="h-20 w-auto max-w-[320px] object-contain sm:h-24"
                  />
                </div>
              ) : null}

              <div className="mb-5 flex flex-col items-center text-center">
                <h2 className="text-xl font-bold text-slate-900">{title}</h2>
                <p className="mt-1 text-sm text-slate-500">{subtitle}</p>
              </div>

              {errors.general && (
                <div
                  role="alert"
                  className="mb-4 rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600 shadow-sm"
                >
                  {errors.general}
                </div>
              )}

              <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                {realm === 'app' && (
                  <div className="flex flex-col gap-1.5">
                    <label htmlFor="login-tenant" className="text-sm font-semibold text-slate-800">
                      Tenant
                    </label>
                    <select
                      id="login-tenant"
                      value={tenantId}
                      onChange={(e) => {
                        setTenantId(e.target.value)
                        setOrgId('')
                        if (errors.tenantCode) setErrors((prev) => ({ ...prev, tenantCode: undefined }))
                      }}
                      className={selectClass(Boolean(errors.tenantCode))}
                      disabled={isLoading || tenantsQuery.isLoading}
                    >
                      <option value="">
                        {tenantsQuery.isLoading ? 'Đang tải tenant…' : 'Chọn tenant'}
                      </option>
                      {(tenantsQuery.data ?? []).map((tenant) => (
                        <option key={tenant.id} value={tenant.id}>
                          {formatCatalogLabel(tenant)}
                        </option>
                      ))}
                    </select>
                    {errors.tenantCode && (
                      <p className="text-sm text-red-600">{errors.tenantCode}</p>
                    )}
                  </div>
                )}
                {realm === 'app' && (
                  <div className="flex flex-col gap-1.5">
                    <label htmlFor="login-org" className="text-sm font-semibold text-slate-800">
                      Organization
                    </label>
                    <select
                      id="login-org"
                      value={orgId}
                      onChange={(e) => {
                        setOrgId(e.target.value)
                        if (errors.orgCode) setErrors((prev) => ({ ...prev, orgCode: undefined }))
                      }}
                      className={selectClass(Boolean(errors.orgCode))}
                      disabled={isLoading || !tenantId || orgsQuery.isLoading}
                    >
                      <option value="">
                        {!tenantId
                          ? 'Chọn tenant trước'
                          : orgsQuery.isLoading
                            ? 'Đang tải organization…'
                            : 'Chọn organization'}
                      </option>
                      {(orgsQuery.data ?? []).map((org) => (
                        <option key={org.id} value={org.id}>
                          {formatCatalogLabel(org)}
                        </option>
                      ))}
                    </select>
                    {errors.orgCode && (
                      <p className="text-sm text-red-600">{errors.orgCode}</p>
                    )}
                  </div>
                )}
                <div className="flex flex-col gap-1.5">
                  <label htmlFor="login-username" className="text-sm font-semibold text-slate-800">
                    Tên đăng nhập
                  </label>
                  <div className="relative flex items-center">
                    <span className="pointer-events-none absolute left-3 text-slate-400">
                      <User className="size-4" aria-hidden="true" />
                    </span>
                    <input
                      id="login-username"
                      type="text"
                      value={username}
                      onChange={(e) => {
                        setUsername(e.target.value)
                        if (errors.username) setErrors((prev) => ({ ...prev, username: undefined }))
                      }}
                      placeholder="Nhập tên đăng nhập hoặc email"
                      className={`h-10 w-full rounded-md border bg-white pl-9 pr-3 text-sm text-slate-800 outline-none transition-colors placeholder:text-slate-400 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-400 ${errors.username
                          ? 'border-red-500 focus:border-red-500'
                          : 'border-slate-300 focus:border-teal-600'
                        }`}
                      disabled={isLoading}
                      autoComplete="username"
                    />
                  </div>
                  {errors.username && (
                    <p className="text-sm text-red-600">{errors.username}</p>
                  )}
                </div>

                <div className="flex flex-col gap-1.5">
                  <div className="flex items-center justify-between">
                    <label htmlFor="login-password" className="text-sm font-semibold text-slate-800">
                      Mật khẩu
                    </label>
                    <Link
                      to={forgotPasswordPath}
                      className="text-sm text-teal-700 transition-colors hover:text-teal-800 hover:underline"
                    >
                      Quên mật khẩu?
                    </Link>
                  </div>
                  <div className="relative flex items-center">
                    <span className="pointer-events-none absolute left-3 text-slate-400">
                      <Lock className="size-4" aria-hidden="true" />
                    </span>
                    <input
                      id="login-password"
                      type={showPassword ? 'text' : 'password'}
                      value={password}
                      onChange={(e) => {
                        setPassword(e.target.value)
                        if (errors.password) setErrors((prev) => ({ ...prev, password: undefined }))
                      }}
                      placeholder="Nhập mật khẩu"
                      className={`h-10 w-full rounded-md border bg-white pl-9 pr-10 text-sm text-slate-800 outline-none transition-colors placeholder:text-slate-400 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-400 ${errors.password
                          ? 'border-red-500 focus:border-red-500'
                          : 'border-slate-300 focus:border-teal-600'
                        }`}
                      disabled={isLoading}
                      autoComplete="current-password"
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword((prev) => !prev)}
                      className="absolute right-3 text-slate-400 transition-colors hover:text-slate-700 focus:outline-none"
                      aria-label={showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                    >
                      {showPassword ? (
                        <EyeOff className="size-4" aria-hidden="true" />
                      ) : (
                        <Eye className="size-4" aria-hidden="true" />
                      )}
                    </button>
                  </div>
                  {errors.password && (
                    <p className="text-sm text-red-600">{errors.password}</p>
                  )}
                </div>

                <div className="flex items-center justify-between">
                  <label className="flex cursor-pointer items-center gap-2 select-none">
                    <input
                      type="checkbox"
                      checked={rememberMe}
                      onChange={(e) => setRememberMe(e.target.checked)}
                      className="size-4 rounded border-slate-300 accent-teal-700"
                    />
                    <span className="text-sm text-slate-500">Ghi nhớ đăng nhập</span>
                  </label>
                </div>

                <button
                  type="submit"
                  disabled={isLoading}
                  className="mt-5 flex h-11 w-full items-center justify-center gap-2 rounded-lg bg-teal-700 text-base font-semibold text-white shadow-sm transition-all hover:bg-teal-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-teal-600 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-85"
                >
                  {isLoading ? (branding?.loadingIndicator ?? 'Đang đăng nhập…') : 'Đăng nhập'}
                </button>
              </form>

              <p className="mt-6 text-center text-sm text-slate-500">
                {realm === 'platform' ? (
                  <>
                    Đăng nhập nghiệp vụ tenant/org?{' '}
                    <Link to={appLoginPath} className="text-teal-700 hover:underline">
                      Vào cổng app
                    </Link>
                  </>
                ) : (
                  <>
                    Quản trị tenant trên Master?{' '}
                    <Link to={platformLoginPath} className="text-teal-700 hover:underline">
                      Đăng nhập nền tảng
                    </Link>
                  </>
                )}
              </p>
            </div>
          </section>
        </div>
      </main>
    </div>
  )
}

export default LoginPage
