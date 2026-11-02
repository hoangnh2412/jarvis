import { platformHttp } from '@platform/core'
import { getOrgStructureConfig } from './config'

export type OrgStructureRequestOptions = {
  /** false khi request phải để host tự lấy tenant của user, không gửi header phạm vi đang lưu. */
  attachScope?: boolean
  /** Gửi header phạm vi cố định (vd. host chọn sẵn phạm vi cho request này). */
  scopeHeadersOverride?: Record<string, string>
  params?: Record<string, unknown>
  headers?: Record<string, string>
}

function unwrapApiBody<T>(body: unknown): T {
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

export function getAccessToken(): string | null {
  return localStorage.getItem(getOrgStructureConfig().tokenStorageKey)
}

export async function orgRequest<T = unknown>(
  method: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE',
  url: string,
  data?: unknown,
  options?: OrgStructureRequestOptions,
): Promise<T> {
  const config = getOrgStructureConfig()
  const attachScope = options?.attachScope ?? true
  const accessToken = getAccessToken()
  if (attachScope && accessToken && config.beforeRequest) {
    await config.beforeRequest(url)
  }

  const scopeHeaders = options?.scopeHeadersOverride
    ?? (attachScope ? config.getScopeHeaders?.() ?? {} : {})

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

export const orgHttp = {
  get: <T = unknown>(url: string, options?: OrgStructureRequestOptions) =>
    orgRequest<T>('GET', url, undefined, options),
  post: <T = unknown>(url: string, data?: unknown, options?: OrgStructureRequestOptions) =>
    orgRequest<T>('POST', url, data, options),
  put: <T = unknown>(url: string, data?: unknown, options?: OrgStructureRequestOptions) =>
    orgRequest<T>('PUT', url, data, options),
  patch: <T = unknown>(url: string, data?: unknown, options?: OrgStructureRequestOptions) =>
    orgRequest<T>('PATCH', url, data, options),
  delete: <T = unknown>(url: string, options?: OrgStructureRequestOptions) =>
    orgRequest<T>('DELETE', url, undefined, options),
}
