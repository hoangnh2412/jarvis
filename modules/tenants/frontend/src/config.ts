export type TenantsConfig = {
  /** Khóa localStorage giữ access token. Mặc định `platform.accessToken`. */
  tokenStorageKey?: string
  /** Header phạm vi host gắn vào request của package (tenant/org đang làm việc). */
  getScopeHeaders?: () => Record<string, string>
  /** Kiểm tra tiền lệ trước request có phạm vi (host có thể chặn khi chưa chọn đơn vị). */
  beforeRequest?: (url: string) => Promise<void>
}

const DEFAULT_CONFIG: Required<Omit<TenantsConfig, 'getScopeHeaders' | 'beforeRequest'>> = {
  tokenStorageKey: 'platform.accessToken',
}

let current: TenantsConfig = {}

export function configureTenants(config: TenantsConfig): void {
  current = { ...config }
}

export function getTenantsConfig(): TenantsConfig & typeof DEFAULT_CONFIG {
  return { ...DEFAULT_CONFIG, ...current }
}
