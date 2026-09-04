import type {
  SettingConnection,
  SettingFormItem,
  SettingGroup,
} from '../types'

type ApiEnvelope<T> = {
  data?: T
  error?: {
    message?: string
    systemMessage?: string
  }
  message?: string
  title?: string
}

type RequestOptions = {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE'
  body?: unknown
  requiresTenant?: boolean
}

export const DEFAULT_SETTING_CONNECTION: SettingConnection = {
  // Same-origin với Sample (wwwroot) hoặc Vite proxy /api → Sample
  apiBase: '/api/v1/settings',
  tenantId: '11111111-1111-1111-1111-111111111111',
  apiKey: '',
}

export async function getSettingGroups(connection: SettingConnection) {
  return request<SettingGroup[]>(connection, '/groups', {
    requiresTenant: false,
  })
}

export async function getSettingForm(
  connection: SettingConnection,
  group: string,
) {
  return request<SettingFormItem[]>(
    connection,
    `/form?group=${encodeURIComponent(group)}`,
  )
}

export async function saveSettingGroup(
  connection: SettingConnection,
  group: string,
  values: Record<string, string>,
) {
  return request(connection, `/group/${encodeURIComponent(group)}`, {
    method: 'PUT',
    body: { values },
  })
}

export async function deleteSetting(
  connection: SettingConnection,
  key: string,
) {
  return request<void>(connection, `/${encodeURIComponent(key)}`, {
    method: 'DELETE',
  })
}

async function request<T>(
  connection: SettingConnection,
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const requiresTenant = options.requiresTenant !== false
  const tenantId = connection.tenantId.trim()
  if (requiresTenant && !tenantId) {
    throw new Error('Nhập Tenant ID trước khi đọc hoặc thay đổi Setting.')
  }

  const headers = new Headers({ Accept: 'application/json' })
  if (tenantId) headers.set('X-Tenant-Id', tenantId)
  if (connection.apiKey.trim()) {
    headers.set('X-API-KEY', connection.apiKey.trim())
  }
  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }

  const base = connection.apiBase.trim().replace(/\/+$/, '')
  if (!base) throw new Error('API base không được để trống.')

  const response = await fetch(`${base}${path}`, {
    method: options.method ?? 'GET',
    headers,
    body:
      options.body === undefined ? undefined : JSON.stringify(options.body),
  })

  const text = await response.text()
  let payload: ApiEnvelope<T> | T | string | null = null
  if (text) {
    try {
      payload = JSON.parse(text) as ApiEnvelope<T> | T
    } catch {
      payload = text
    }
  }

  if (!response.ok) {
    const envelope =
      payload && typeof payload === 'object'
        ? (payload as ApiEnvelope<T>)
        : undefined
    throw new Error(
      envelope?.error?.message ||
        envelope?.error?.systemMessage ||
        envelope?.message ||
        envelope?.title ||
        (typeof payload === 'string' ? payload : '') ||
        `API trả về HTTP ${response.status}.`,
    )
  }

  if (
    payload &&
    typeof payload === 'object' &&
    Object.prototype.hasOwnProperty.call(payload, 'data')
  ) {
    return (payload as ApiEnvelope<T>).data as T
  }

  return payload as T
}
