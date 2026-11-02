import { identityHttp } from '../http'

export type LoginCatalogItem = {
  id: string
  code: string
  name: string
}

function mapItem(raw: Record<string, unknown>): LoginCatalogItem {
  return {
    id: String(raw.id ?? raw.Id ?? ''),
    code: String(raw.code ?? raw.Code ?? ''),
    name: String(raw.name ?? raw.Name ?? ''),
  }
}

export async function listAppLoginTenants(): Promise<LoginCatalogItem[]> {
  const rows = await identityHttp.get<unknown[]>('/api/v1/platform/account/catalog/tenants', {
    attachScope: false,
  })
  if (!Array.isArray(rows)) return []
  return rows
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapItem)
    .filter((row) => row.id && row.code)
}

export async function listAppLoginOrgs(tenantId: string): Promise<LoginCatalogItem[]> {
  const id = tenantId.trim()
  if (!id) return []
  const rows = await identityHttp.get<unknown[]>(
    `/api/v1/platform/account/catalog/tenants/${encodeURIComponent(id)}/orgs`,
    { attachScope: false },
  )
  if (!Array.isArray(rows)) return []
  return rows
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapItem)
    .filter((row) => row.id && row.code)
}
