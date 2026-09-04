import type {
  Tenant,
  TenantConnection,
  TenantDomain,
  TenantListResult,
} from '../types'

/** Lấy body từ AxiosResponse hoặc raw data */
export function unwrapData<T>(payload: unknown): T {
  if (
    payload &&
    typeof payload === 'object' &&
    'data' in payload &&
    (payload as { data: unknown }).data !== undefined
  ) {
    return (payload as { data: T }).data
  }
  return payload as T
}

export function unwrapTenantListResult(payload: unknown): TenantListResult {
  const body = unwrapData<unknown>(payload)

  if (Array.isArray(body)) {
    return { items: body, total: body.length }
  }

  if (body && typeof body === 'object') {
    const record = body as Record<string, unknown>
    const items = Array.isArray(record.items)
      ? (record.items as Tenant[])
      : Array.isArray(record.data)
        ? (record.data as Tenant[])
        : []
    const total =
      typeof record.total === 'number'
        ? record.total
        : typeof record.totalCount === 'number'
          ? record.totalCount
          : items.length
    const page = typeof record.page === 'number' ? record.page : undefined
    const size =
      typeof record.size === 'number'
        ? record.size
        : typeof record.pageSize === 'number'
          ? record.pageSize
          : undefined

    return { items, total, page, size }
  }

  return { items: [], total: 0 }
}

/** @deprecated Dùng `unwrapTenantListResult` để lấy cả total */
export function unwrapTenantList(payload: unknown): Tenant[] {
  return unwrapTenantListResult(payload).items
}

export function unwrapTenant(payload: unknown): Tenant {
  return unwrapData<Tenant>(payload)
}

export function unwrapConnectionList(payload: unknown): TenantConnection[] {
  const body = unwrapData<unknown>(payload)
  if (Array.isArray(body)) return body
  if (
    body &&
    typeof body === 'object' &&
    'items' in body &&
    Array.isArray((body as { items: unknown }).items)
  ) {
    return (body as { items: TenantConnection[] }).items
  }
  return []
}

export function unwrapDomainList(payload: unknown): TenantDomain[] {
  const body = unwrapData<unknown>(payload)
  if (Array.isArray(body)) return body
  if (
    body &&
    typeof body === 'object' &&
    'items' in body &&
    Array.isArray((body as { items: unknown }).items)
  ) {
    return (body as { items: TenantDomain[] }).items
  }
  return []
}
