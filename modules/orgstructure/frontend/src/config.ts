export type OrgStructureConfig = {
  /** Khóa localStorage giữ access token. Mặc định `platform.accessToken`. */
  tokenStorageKey?: string
  /** Header phạm vi host gắn vào request của package (tenant/org đang làm việc). */
  getScopeHeaders?: () => Record<string, string>
  /** Kiểm tra tiền lệ trước request có phạm vi (host có thể chặn khi chưa chọn đơn vị). */
  beforeRequest?: (url: string) => Promise<void>
}

const DEFAULT_CONFIG: Required<Omit<OrgStructureConfig, 'getScopeHeaders' | 'beforeRequest'>> = {
  tokenStorageKey: 'platform.accessToken',
}

let current: OrgStructureConfig = {}

export function configureOrgStructure(config: OrgStructureConfig): void {
  current = { ...config }
}

export function getOrgStructureConfig(): OrgStructureConfig & typeof DEFAULT_CONFIG {
  return { ...DEFAULT_CONFIG, ...current }
}
