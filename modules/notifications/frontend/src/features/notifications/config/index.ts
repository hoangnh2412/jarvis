import { HttpTransportType, type IHttpConnectionOptions } from '@microsoft/signalr'

/** Same-origin Sample host (`SpaProxy` / publish). Override via VITE_* khi tách API. */
export const notificationApiBase = (
  import.meta.env.VITE_NOTIFICATION_API_URL || '/api/notifications'
).replace(/\/+$/, '')

export const notificationHubUrl =
  import.meta.env.VITE_NOTIFICATION_HUB_URL || '/hubs/notifications'

export const notificationSseUrl =
  import.meta.env.VITE_NOTIFICATION_SSE_URL || '/sse/notifications'

export type NotificationTransport = 'signalr' | 'sse'

export type NotificationRealtimeConfig = {
  transport: NotificationTransport
  hubUrl: string
  sseUrl: string
  eventName: string
}

function resolveDefaultTransport(): NotificationTransport {
  const raw = import.meta.env.VITE_NOTIFICATION_TRANSPORT?.trim().toLowerCase()
  return raw === 'sse' ? 'sse' : 'signalr'
}

let realtimeConfig: NotificationRealtimeConfig = {
  transport: resolveDefaultTransport(),
  hubUrl: notificationHubUrl,
  sseUrl: notificationSseUrl,
  eventName: 'notification',
}

/** Host chọn transport khớp BE: SignalR → hubUrl, SSE → sseUrl. */
export function configureNotificationRealtime(
  options: Partial<NotificationRealtimeConfig>,
) {
  realtimeConfig = { ...realtimeConfig, ...options }
}

export function getNotificationRealtimeConfig(): Readonly<NotificationRealtimeConfig> {
  return realtimeConfig
}

const apiKey = import.meta.env.VITE_NOTIFICATION_API_KEY ?? ''

let accessTokenGetter: (() => string | null) | null = null
let tenantIdGetter: (() => string | null) | null = null

/** Host app gắn nguồn JWT (ví dụ `getAccessToken` từ `@platform/core`). */
export function configureNotificationAuth(getter: () => string | null) {
  accessTokenGetter = getter
}

/** Host app gắn tenant đang làm việc (ưu tiên sau `VITE_NOTIFICATION_TENANT_ID`). */
export function configureNotificationTenant(getter: () => string | null) {
  tenantIdGetter = getter
}

function resolveAccessToken() {
  return accessTokenGetter?.() ?? null
}

function resolveTenantId() {
  const fromEnv = import.meta.env.VITE_NOTIFICATION_TENANT_ID?.trim()
  if (fromEnv) return fromEnv
  return tenantIdGetter?.() ?? null
}

/**
 * Header REST + SignalR + SSE — ưu tiên API key (demo), không thì Bearer JWT.
 * Gửi kèm `X-Tenant-Id` khi host/env cung cấp tenant.
 */
export function getNotificationAuthHeaders(): Record<string, string> {
  const headers: Record<string, string> = {}

  if (apiKey) {
    headers['X-API-KEY'] = apiKey
  } else {
    const token = resolveAccessToken()
    if (token) headers.Authorization = `Bearer ${token}`
  }

  const tenantId = resolveTenantId()
  if (tenantId) headers['X-Tenant-Id'] = tenantId

  return headers
}

/** Hub options — JWT qua accessTokenFactory; API key dùng Long Polling. */
export function buildNotificationHubOptions(): IHttpConnectionOptions {
  const headers = getNotificationAuthHeaders()

  if (apiKey) {
    return {
      withCredentials: false,
      headers,
      transport: HttpTransportType.LongPolling,
    }
  }

  return {
    accessTokenFactory: async () => resolveAccessToken() ?? '',
    headers: Object.keys(headers).length ? headers : undefined,
  }
}

/** @deprecated dùng `getNotificationAuthHeaders()` */
export const notificationAuthHeaders: Record<string, string> = {}

/** @deprecated dùng `buildNotificationHubOptions()` */
export const notificationHubOptions = buildNotificationHubOptions()
