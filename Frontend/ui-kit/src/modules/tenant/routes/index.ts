import {
  TENANT_ROUTES,
  getTenantListPath,
  getTenantCreatePath,
  getTenantDetailPath,
  getTenantEditPath,
  getTenantConnectionsPath,
  getTenantDomainsPath,
  type TenantRouteKey,
} from './paths'
import {
  configureTenantNavigate,
  navigateTenant,
} from './navigation'

export { TENANT_ROUTES } from './paths'
export type { TenantRouteKey } from './paths'
export {
  getTenantListPath,
  getTenantCreatePath,
  getTenantDetailPath,
  getTenantEditPath,
  getTenantConnectionsPath,
  getTenantDomainsPath,
} from './paths'
export {
  configureTenantNavigate,
  navigateTenant,
} from './navigation'
export type { TenantNavigateFn } from './navigation'

export type TenantRouteItem = {
  id: TenantRouteKey
  path: string
  titleKey: string
}

export function getTenantRouteList(): TenantRouteItem[] {
  return [
    { id: 'list', path: TENANT_ROUTES.list, titleKey: 'tenant.routes.list' },
    {
      id: 'create',
      path: TENANT_ROUTES.create,
      titleKey: 'tenant.routes.create',
    },
    {
      id: 'detail',
      path: TENANT_ROUTES.detail,
      titleKey: 'tenant.routes.detail',
    },
    { id: 'edit', path: TENANT_ROUTES.edit, titleKey: 'tenant.routes.edit' },
    {
      id: 'connections',
      path: TENANT_ROUTES.connections,
      titleKey: 'tenant.routes.connections',
    },
    {
      id: 'domains',
      path: TENANT_ROUTES.domains,
      titleKey: 'tenant.routes.domains',
    },
  ]
}

export const tenantPaths = {
  list: getTenantListPath,
  create: getTenantCreatePath,
  detail: getTenantDetailPath,
  edit: getTenantEditPath,
  connections: getTenantConnectionsPath,
  domains: getTenantDomainsPath,
  navigate: navigateTenant,
  configure: configureTenantNavigate,
} as const
