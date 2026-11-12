/** Same-origin Sample host (`SpaProxy` / publish). Override via VITE_* khi tách API. */
export const notificationApiBase = (
  import.meta.env.VITE_NOTIFICATION_API_URL || '/api/v1/notifications'
).replace(/\/+$/, '')

export const notificationSseUrl =
  import.meta.env.VITE_NOTIFICATION_SSE_URL || '/sse/notifications'

export type NotificationRealtimeConfig = {
  sseUrl: string
  eventName: string
  /** false thì không mở SSE. Mặc định bật. */
  enabled: boolean
}

let realtimeConfig: NotificationRealtimeConfig = {
  sseUrl: notificationSseUrl,
  eventName: 'notification',
  enabled: true,
}

/** Host đổi URL SSE hoặc tên event. */
export function configureNotificationRealtime(
  options: Partial<NotificationRealtimeConfig>,
) {
  realtimeConfig = { ...realtimeConfig, ...options }
}

export function getNotificationRealtimeConfig(): Readonly<NotificationRealtimeConfig> {
  return realtimeConfig
}

let accessTokenGetter: (() => string | null) | null = null
let tenantIdGetter: (() => string | null) | null = null
let orgIdGetter: (() => string | null) | null = null

/** Host app gắn nguồn JWT (ví dụ `getAccessToken` từ `@platform/core`). */
export function configureNotificationAuth(getter: () => string | null) {
  accessTokenGetter = getter
}

/** Host app gắn tenant đang làm việc. */
export function configureNotificationTenant(getter: () => string | null) {
  tenantIdGetter = getter
}

/** Host app gắn org đang làm việc. */
export function configureNotificationOrg(getter: () => string | null) {
  orgIdGetter = getter
}

function resolveAccessToken() {
  return accessTokenGetter?.() ?? null
}

function resolveTenantId() {
  return tenantIdGetter?.() ?? null
}

function resolveOrgId() {
  return orgIdGetter?.() ?? null
}

/**
 * Header REST. SSE fetch đang gửi cùng header cho đến khi realtime có cơ chế riêng.
 * `X-Tenant-Id` chỉ khi host gọi `configureNotificationTenant`.
 */
export function getNotificationAuthHeaders(): Record<string, string> {
  const headers: Record<string, string> = {}
  const token = resolveAccessToken()
  if (token) headers.Authorization = `Bearer ${token}`

  // CROSS-MODULE 2026-11-04 · slice đăng nhập Sample (ô chọn Org/Tenant)
  // Nguyên nhân: /api/v1/notifications đi qua MultiOrg, thiếu X-Org-Id thì 403.
  // Lý do: thêm header org từ scope đăng nhập. Không đổi payload notification.
  const tenantId = resolveTenantId()
  if (tenantId) headers['X-Tenant-Id'] = tenantId

  const orgId = resolveOrgId()
  if (orgId) headers['X-Org-Id'] = orgId

  return headers
}

/** @deprecated dùng `getNotificationAuthHeaders()` */
export const notificationAuthHeaders: Record<string, string> = {}
