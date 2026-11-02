import { getIdentityConfig, sessionKey, type IdentityCompanyProfile } from './config'
import { getAccessToken, identityHttp } from './http'

export interface LoginPayload {
  userNameOrEmailAddress: string
  password?: string
  tenancyName?: string
  orgCode?: string
  tenantId?: string
  orgId?: string
  tenantName?: string
  /** platform = cổng nền tảng (user Master). app = cổng nghiệp vụ (user trên TenantDB). */
  realm?: 'platform' | 'app'
}

export type MeOrgUnit = {
  id: string
  code: string
  name: string
  unitType: string
  path: string
}

export interface CurrentUser {
  id: string
  userName: string
  fullName: string
  email: string
  roles: string[]
  tenantId?: string
  tenantName?: string
  tenantTaxCode?: string
  permissions?: Record<string, boolean>
  orgScope?: {
    organizationalUnit: MeOrgUnit | null
    descendants: MeOrgUnit[]
  }
}

export type MeAccount = {
  id: string
  userName: string
  displayName: string | null
  email: string | null
}

export type MeApiResponse = {
  account?: MeAccount | null
  permissions: string[]
  roles: string[]
  organizationalUnit: MeOrgUnit | null
  descendants: MeOrgUnit[]
}

export type TenantCompany = IdentityCompanyProfile

export interface TenantInfo {
  success: boolean
  tenantId: string
  name: string
  isActive: boolean
}

export interface AuthSession {
  isAuthenticated: boolean
  user: CurrentUser
  company: TenantCompany
  tenant: TenantInfo
  permissions: string[]
}

export const SHELL_ACCESS = 'Platform.Shell.Access'

export function hasPermission(codes: readonly string[], code: string): boolean {
  return codes.includes('*') || codes.includes('System.All') || codes.includes(code)
}

export const STORAGE_KEYS = {
  AUTH_FLAG: 'isAuthenticated',
  USER: 'user',
  COMPANY: 'tenant_company',
  TENANT: 'current_tenant',
  PERMISSIONS: 'tenant_permissions',
} as const

function emptyCompany(): TenantCompany {
  return { name: '', taxCode: '', address: '' }
}

function accountFullName(account: MeAccount | null | undefined, fallbackUserName: string): string {
  const displayName = account?.displayName?.trim()
  if (displayName) return displayName
  const userName = account?.userName?.trim()
  if (userName) return userName
  return fallbackUserName
}

function accountEmail(account: MeAccount | null | undefined): string {
  return account?.email?.trim() ?? ''
}

function persistSession(session: AuthSession): void {
  localStorage.setItem(sessionKey(STORAGE_KEYS.AUTH_FLAG), 'true')
  localStorage.setItem(sessionKey(STORAGE_KEYS.USER), JSON.stringify(session.user))
  localStorage.setItem(sessionKey(STORAGE_KEYS.COMPANY), JSON.stringify(session.company))
  localStorage.setItem(sessionKey(STORAGE_KEYS.TENANT), JSON.stringify(session.tenant))
  localStorage.setItem(sessionKey(STORAGE_KEYS.PERMISSIONS), JSON.stringify(session.permissions))
}

/** Ghi tenant catalog lấy từ hồ sơ đơn vị. Header phạm vi phải là id này, không phải claim cũ trong token. */
function rememberPlatformTenant(company: TenantCompany): void {
  const tenantId = company.id?.trim() ?? ''
  if (!tenantId) return

  const stored = authService.getStoredSession()
  const user = stored?.user
  if (user) {
    localStorage.setItem(
      sessionKey(STORAGE_KEYS.USER),
      JSON.stringify({
        ...user,
        tenantId,
        tenantName: company.name || user.tenantName,
        tenantTaxCode: company.taxCode || user.tenantTaxCode,
      }),
    )
  }

  const tenant: TenantInfo = {
    success: true,
    tenantId,
    name: company.name,
    isActive: stored?.tenant?.isActive ?? true,
  }
  localStorage.setItem(sessionKey(STORAGE_KEYS.TENANT), JSON.stringify(tenant))
  localStorage.setItem(sessionKey(STORAGE_KEYS.COMPANY), JSON.stringify(company))
}

export const authService = {
  async fetchMeProfile(): Promise<MeApiResponse> {
    return identityHttp.get<MeApiResponse>('/api/v1/me')
  },

  /** Làm mới quyền từ `/api/v1/me` để thay đổi gán vai trò được phản ánh mà không chờ lần đăng nhập sau. */
  async getCurrentUser(): Promise<CurrentUser> {
    const user = this.getStoredSession()?.user as CurrentUser | undefined
    if (!user?.userName) {
      throw new Error('Chưa đăng nhập.')
    }

    const me = await this.fetchMeProfile()
    const permissionCodes = Array.isArray(me.permissions)
      ? me.permissions.filter((code): code is string => typeof code === 'string')
      : []
    const currentUser: CurrentUser = {
      ...user,
      id: me.account?.id?.trim() || user.id,
      userName: me.account?.userName?.trim() || user.userName,
      fullName: accountFullName(me.account, user.userName),
      email: accountEmail(me.account),
      roles: Array.isArray(me.roles) ? me.roles : user.roles,
      permissions: Object.fromEntries(permissionCodes.map((code) => [code, true])),
      orgScope: {
        organizationalUnit: me.organizationalUnit ?? null,
        descendants: me.descendants ?? [],
      },
    }

    localStorage.setItem(sessionKey(STORAGE_KEYS.USER), JSON.stringify(currentUser))
    localStorage.setItem(sessionKey(STORAGE_KEYS.PERMISSIONS), JSON.stringify(permissionCodes))
    return currentUser
  },

  async getTenantCompany(): Promise<TenantCompany> {
    const config = getIdentityConfig()
    try {
      if (!config.loadCompanyProfile) {
        const stored = this.getStoredSession()?.company
        if (stored?.name || stored?.taxCode) return stored as TenantCompany
        return emptyCompany()
      }
      const company = await config.loadCompanyProfile()
      rememberPlatformTenant(company)
      return company
    } catch {
      const stored = this.getStoredSession()?.company
      if (stored?.name || stored?.taxCode) {
        return stored as TenantCompany
      }
      throw new Error('Không tải được thông tin đơn vị.')
    }
  },

  /** Đồng bộ tenant catalog trước khi các màn gọi API có header phạm vi. */
  async alignPlatformTenant(): Promise<string> {
    const company = await this.getTenantCompany()
    const tenantId = company.id?.trim() ?? ''
    if (!tenantId) {
      throw new Error('API không trả tenant đang làm việc.')
    }
    return tenantId
  },

  async getPermissions(): Promise<string[]> {
    return (this.getStoredSession()?.permissions as string[]) ?? []
  },

  getAccessToken,

  async executeLogin(payload: LoginPayload): Promise<AuthSession> {
    const config = getIdentityConfig()
    const userName = payload.userNameOrEmailAddress.trim()
    const password = payload.password?.trim() ?? ''
    const realm = payload.realm ?? 'app'
    const tenantId = payload.tenantId?.trim() ?? ''
    const orgId = payload.orgId?.trim() ?? ''
    if (realm === 'app' && tenantId && orgId) {
      config.rememberScope?.(tenantId, orgId, payload.tenantName)
    }

    const loginScopeHeaders =
      realm === 'app' && tenantId && orgId
        ? config.getScopeHeaders?.() ?? {}
        : undefined

    const login = await identityHttp.post<{
      accessToken?: string
      AccessToken?: string
      userId?: string
      UserId?: string
      tenantId?: string
      TenantId?: string
      orgId?: string
      OrgId?: string
    }>(
      realm === 'platform' ? '/api/v1/platform/account/login' : '/api/v1/app/account/login',
      {
        userName,
        password,
        tenantCode: realm === 'platform' ? undefined : payload.tenancyName?.trim(),
        orgCode: realm === 'platform' ? undefined : payload.orgCode?.trim(),
      },
      loginScopeHeaders ? { scopeHeadersOverride: loginScopeHeaders } : undefined,
    )
    const accessToken = login.accessToken ?? login.AccessToken
    if (!accessToken) {
      throw new Error('Máy chủ không trả access token.')
    }
    localStorage.setItem(config.tokenStorageKey, accessToken)

    const orgIdFromLogin = (login.orgId ?? login.OrgId ?? orgId).trim()
    const tenantIdFromLogin = (login.tenantId ?? login.TenantId ?? tenantId).trim()
    if (realm === 'app' && orgIdFromLogin && tenantIdFromLogin) {
      config.rememberScope?.(tenantIdFromLogin, orgIdFromLogin, payload.tenantName)
    }

    const me = await this.fetchMeProfile()
    const codes = Array.isArray(me.permissions) ? me.permissions : []

    let company = emptyCompany()
    try {
      company = await this.getTenantCompany()
    } catch {
      company = emptyCompany()
    }
    const platformTenantId = company.id?.trim() || undefined

    const activeUser: CurrentUser = {
      id: me.account?.id?.trim() || login.userId || login.UserId || userName,
      userName: me.account?.userName?.trim() || userName,
      fullName: accountFullName(me.account, userName),
      email: accountEmail(me.account),
      roles: Array.isArray(me.roles) ? me.roles : [],
      tenantId: platformTenantId,
      tenantName: company.name || undefined,
      tenantTaxCode: company.taxCode || undefined,
      permissions: Object.fromEntries(codes.map((code) => [code, true])),
      orgScope: {
        organizationalUnit: me.organizationalUnit ?? null,
        descendants: me.descendants ?? [],
      },
    }

    const session: AuthSession = {
      isAuthenticated: true,
      user: activeUser,
      company,
      tenant: {
        success: true,
        tenantId: platformTenantId ?? '',
        name: company.name,
        isActive: true,
      },
      permissions: codes,
    }

    persistSession(session)

    try {
      await config.ensureDefaultScope?.()
    } catch {
      /* catalog phạm vi có thể chưa seed — resolver backend vẫn fallback */
    }

    return session
  },

  logout(): void {
    const config = getIdentityConfig()
    localStorage.removeItem(config.tokenStorageKey)
    localStorage.removeItem(sessionKey(STORAGE_KEYS.AUTH_FLAG))
    localStorage.removeItem(sessionKey(STORAGE_KEYS.USER))
    localStorage.removeItem(sessionKey(STORAGE_KEYS.COMPANY))
    localStorage.removeItem(sessionKey(STORAGE_KEYS.TENANT))
    localStorage.removeItem(sessionKey(STORAGE_KEYS.PERMISSIONS))
    config.clearScope?.()
  },

  getStoredSession(): Partial<AuthSession> | null {
    const isAuth = localStorage.getItem(sessionKey(STORAGE_KEYS.AUTH_FLAG)) === 'true'
    if (!isAuth) return null

    try {
      const user = JSON.parse(localStorage.getItem(sessionKey(STORAGE_KEYS.USER)) || '{}')
      const company = JSON.parse(localStorage.getItem(sessionKey(STORAGE_KEYS.COMPANY)) || '{}')
      const tenant = JSON.parse(localStorage.getItem(sessionKey(STORAGE_KEYS.TENANT)) || '{}')
      const permissions = JSON.parse(localStorage.getItem(sessionKey(STORAGE_KEYS.PERMISSIONS)) || '[]')

      return {
        isAuthenticated: true,
        user,
        company,
        tenant,
        permissions,
      }
    } catch {
      return null
    }
  },
}
