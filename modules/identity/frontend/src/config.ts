export type IdentityCompanyProfile = {
  id?: string
  name: string
  taxCode: string
  address: string
  tel?: string
  email?: string
  delegate?: string
  position?: string
  cityName?: string
  province?: string
  taxAuthorityName?: string
  identityCard?: string
  useIdentityCard?: boolean
}

export type IdentityConfig = {
  /** Khóa localStorage giữ access token. Mặc định `platform.accessToken`. */
  tokenStorageKey?: string
  /** Tiền tố khóa phiên (user/company/tenant/permissions). Mặc định rỗng. */
  sessionStoragePrefix?: string
  /** Host nạp hồ sơ đơn vị sau đăng nhập (ánh xạ DTO của host về khuôn chung). */
  loadCompanyProfile?: () => Promise<IdentityCompanyProfile>
  /** Host đồng bộ phạm vi mặc định sau đăng nhập (bỏ qua lỗi phía host). */
  ensureDefaultScope?: () => Promise<void>
  /** Host ghi nhớ tenant/org đã chọn ở form đăng nhập cho các request sau. */
  rememberScope?: (tenantId: string, orgId: string, tenantName?: string) => void
  /** Host dọn phạm vi đã lưu khi đăng xuất. */
  clearScope?: () => void
  /** Header phạm vi host gắn vào request của package (tenant/org đang làm việc). */
  getScopeHeaders?: () => Record<string, string>
  /** Kiểm tra tiền lệ trước request có phạm vi (host có thể chặn khi chưa chọn đơn vị). */
  beforeRequest?: (url: string) => Promise<void>
  /** Đường dẫn đăng nhập khi phiên hết hạn. Mặc định `/app/login`. */
  loginPath?: string
}

const DEFAULT_CONFIG: Required<Omit<IdentityConfig, 'loadCompanyProfile' | 'ensureDefaultScope' | 'rememberScope' | 'clearScope' | 'getScopeHeaders' | 'beforeRequest'>> = {
  tokenStorageKey: 'platform.accessToken',
  sessionStoragePrefix: '',
  loginPath: '/app/login',
}

let current: IdentityConfig = {}

export function configureIdentity(config: IdentityConfig): void {
  current = { ...config }
}

export function getIdentityConfig(): IdentityConfig & typeof DEFAULT_CONFIG {
  return { ...DEFAULT_CONFIG, ...current }
}

export function sessionKey(name: string): string {
  return `${getIdentityConfig().sessionStoragePrefix}${name}`
}
