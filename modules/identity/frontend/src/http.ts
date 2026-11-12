import { platformHttp } from '@platform/core'
import { getIdentityConfig, sessionKey } from './config'

export type IdentityRequestOptions = {
  /** false khi request phải để host tự lấy tenant của user, không gửi header phạm vi đang lưu. */
  attachScope?: boolean
  /** Gửi header phạm vi cố định (vd. đăng nhập app sau khi chọn dropdown). */
  scopeHeadersOverride?: Record<string, string>
  params?: Record<string, unknown>
  headers?: Record<string, string>
}

function unwrapApiBody<T>(body: unknown): T {
  if (typeof body === 'string' && body.trimStart().startsWith('<')) {
    throw new Error('API người dùng không trả JSON. Host chưa đăng ký dịch vụ identity.')
  }
  if (body && typeof body === 'object' && 'code' in body && 'data' in body) {
    return (body as { data: T }).data
  }
  return body as T
}

function toApiError(error: unknown): Error {
  const payload = error as {
    status?: number
    code?: unknown
    response?: { status?: number; data?: { code?: unknown; message?: unknown; error?: { code?: unknown; message?: unknown } } }
    error?: { code?: unknown; message?: unknown; systemMessage?: unknown }
    message?: unknown
    Message?: unknown
  }
  const body = payload?.response?.data as
    | { code?: unknown; error?: { code?: unknown; message?: unknown; systemMessage?: unknown } }
    | undefined
  const status = payload?.response?.status ?? payload?.status
  const message =
    body?.error?.message
    ?? payload?.response?.data?.message
    ?? payload?.error?.message
    ?? payload?.message
    ?? payload?.Message
  const systemMessage = body?.error?.systemMessage ?? payload?.error?.systemMessage
  let text =
    typeof message === 'string' && message.trim()
      ? message
      : 'Có lỗi xảy ra khi gọi API.'

  if (text === 'Network Error' || text === 'Failed to fetch') {
    text = 'Không kết nối được API.'
  } else if (status === 403) {
    text = `${text} (403 — thiếu quyền hoặc không phải user root nền tảng.)`
  }

  const err = new Error(text) as Error & { status?: number; code?: string; systemMessage?: string }
  if (typeof status === 'number') err.status = status
  const nestedCode = body?.error?.code ?? payload?.error?.code
  const rawCode = body?.code ?? nestedCode ?? payload?.code
  if (typeof rawCode === 'string' && rawCode.trim()) err.code = rawCode.trim()
  if (typeof systemMessage === 'string' && systemMessage.trim()) {
    err.systemMessage = systemMessage.trim()
  }
  return err
}

function requestPath(url: string): string {
  if (url.startsWith('http')) {
    try {
      return new URL(url).pathname
    } catch {
      return url
    }
  }
  return url
}

function shouldAttachScopeHeaders(url: string): boolean {
  const path = requestPath(url)
  if (path.includes('/platform/')) return false
  if (path.includes('/account/login')) return false
  if (path.includes('/account/catalog')) return false
  if (path.includes('/system/install')) return false
  return true
}

export function getAccessToken(): string | null {
  return localStorage.getItem(getIdentityConfig().tokenStorageKey)
}

export async function identityRequest<T = unknown>(
  method: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE',
  url: string,
  data?: unknown,
  options?: IdentityRequestOptions,
): Promise<T> {
  const config = getIdentityConfig()
  const attachScope = options?.attachScope ?? true
  const accessToken = getAccessToken()
  if (attachScope && accessToken && config.beforeRequest) {
    await config.beforeRequest(url)
  }

  const scopeHeaders = options?.scopeHeadersOverride
    ?? (attachScope && shouldAttachScopeHeaders(url) ? config.getScopeHeaders?.() ?? {} : {})

  try {
    const response = (await platformHttp.request({
      method,
      url,
      data,
      params: options?.params,
      headers: {
        ...scopeHeaders,
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        ...options?.headers,
      },
    })) as { data: unknown }
    return unwrapApiBody<T>(response.data)
  } catch (error) {
    throw toApiError(error)
  }
}

export const identityHttp = {
  get: <T = unknown>(url: string, options?: IdentityRequestOptions) =>
    identityRequest<T>('GET', url, undefined, options),
  post: <T = unknown>(url: string, data?: unknown, options?: IdentityRequestOptions) =>
    identityRequest<T>('POST', url, data, options),
  put: <T = unknown>(url: string, data?: unknown, options?: IdentityRequestOptions) =>
    identityRequest<T>('PUT', url, data, options),
  patch: <T = unknown>(url: string, data?: unknown, options?: IdentityRequestOptions) =>
    identityRequest<T>('PATCH', url, data, options),
  delete: <T = unknown>(url: string, options?: IdentityRequestOptions) =>
    identityRequest<T>('DELETE', url, undefined, options),
}

export { sessionKey }
