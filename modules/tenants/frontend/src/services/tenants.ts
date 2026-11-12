import { tenantsHttp } from '../http'

const TENANTS_API = '/api/v1/platform/tenants'

export const tenantQueryKeys = {
  all: ['platform', 'tenants'] as const,
  list: (search?: string) => ['platform', 'tenants', 'list', search ?? ''] as const,
}

export type PlatformTenant = {
  id: string
  code: string
  name: string
  isActive: boolean
  parentTenantId: string | null
}

export type TenantAdminInput = {
  username: string
  email: string
  password?: string
  displayName: string
}

export type CatalogAdminProvision = {
  userId: string
  username: string
}

export type CreatePlatformTenantResult = {
  tenant: PlatformTenant
  initialAdmin?: CatalogAdminProvision | null
}

function mapCatalogAdmin(raw: Record<string, unknown>): CatalogAdminProvision {
  return {
    userId: String(raw.userId ?? raw.UserId ?? ''),
    username: String(raw.username ?? raw.Username ?? ''),
  }
}

function unwrapTenantPayload(payload: Record<string, unknown>): CreatePlatformTenantResult {
  const nested = payload.tenant ?? payload.Tenant
  if (nested && typeof nested === 'object') {
    const adminRaw = payload.initialAdmin ?? payload.InitialAdmin
    return {
      tenant: mapTenant(nested as Record<string, unknown>),
      initialAdmin:
        adminRaw && typeof adminRaw === 'object'
          ? mapCatalogAdmin(adminRaw as Record<string, unknown>)
          : null,
    }
  }
  return { tenant: mapTenant(payload) }
}

function mapTenant(raw: Record<string, unknown>): PlatformTenant {
  const parent = raw.parentTenantId ?? raw.ParentTenantId
  return {
    id: String(raw.id ?? raw.Id ?? ''),
    code: String(raw.code ?? raw.Code ?? ''),
    name: String(raw.name ?? raw.Name ?? ''),
    isActive: Boolean(raw.isActive ?? raw.IsActive ?? true),
    parentTenantId: parent ? String(parent) : null,
  }
}

export async function listPlatformTenants(params?: { search?: string }): Promise<PlatformTenant[]> {
  const payload = await tenantsHttp.get<unknown[]>(TENANTS_API, {
    attachScope: false,
    params: { search: params?.search?.trim() || undefined },
  })
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapTenant)
    .filter((tenant) => tenant.id)
}

export async function createPlatformTenant(input: {
  code: string
  name: string
  connectionString: string
  parentTenantId?: string | null
  admin?: TenantAdminInput
}): Promise<CreatePlatformTenantResult> {
  const body: Record<string, unknown> = {
    code: input.code.trim(),
    name: input.name.trim(),
    connectionString: input.connectionString.trim(),
    parentTenantId: input.parentTenantId || null,
  }
  if (input.admin) {
    body.initialAdmin = {
      username: input.admin.username,
      email: input.admin.email,
      password: input.admin.password,
      displayName: input.admin.displayName,
    }
  }
  const payload = await tenantsHttp.post<Record<string, unknown>>(TENANTS_API, body, { attachScope: false })
  return unwrapTenantPayload(payload)
}

export async function updatePlatformTenant(
  id: string,
  input: { code: string; name: string; connectionString: string },
): Promise<PlatformTenant> {
  const payload = await tenantsHttp.put<Record<string, unknown>>(`${TENANTS_API}/${encodeURIComponent(id)}`, {
    code: input.code.trim(),
    name: input.name.trim(),
    connectionString: input.connectionString.trim(),
  }, { attachScope: false })
  return mapTenant(payload)
}

export async function setPlatformTenantActive(id: string, isActive: boolean): Promise<PlatformTenant> {
  const payload = await tenantsHttp.patch<Record<string, unknown>>(
    `${TENANTS_API}/${encodeURIComponent(id)}/active`,
    { isActive },
    { attachScope: false },
  )
  return mapTenant(payload)
}

export async function getPlatformTenantConnection(id: string): Promise<string> {
  const payload = await tenantsHttp.get<Record<string, unknown>>(
    `${TENANTS_API}/${encodeURIComponent(id)}/connection`,
    { attachScope: false },
  )
  return String(payload.connectionString ?? payload.ConnectionString ?? '')
}
