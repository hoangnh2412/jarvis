export const TENANT_ROUTES = {
  list: '/tenants',
  create: '/tenants/create',
  detail: '/tenants/:id',
  edit: '/tenants/:id/edit',
  connections: '/tenants/:id/connections',
  domains: '/tenants/:id/domains',
} as const

export type TenantRouteKey = keyof typeof TENANT_ROUTES

function withId(pattern: string, id: string) {
  return pattern.replace(':id', id)
}

export function getTenantListPath() {
  return TENANT_ROUTES.list
}

export function getTenantCreatePath() {
  return TENANT_ROUTES.create
}

export function getTenantDetailPath(id: string) {
  return withId(TENANT_ROUTES.detail, id)
}

export function getTenantEditPath(id: string) {
  return withId(TENANT_ROUTES.edit, id)
}

export function getTenantConnectionsPath(id: string) {
  return withId(TENANT_ROUTES.connections, id)
}

export function getTenantDomainsPath(id: string) {
  return withId(TENANT_ROUTES.domains, id)
}
