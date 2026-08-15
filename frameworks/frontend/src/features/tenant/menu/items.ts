import { TENANT_ROUTES } from '../routes/paths'
import { TENANT_PERMISSIONS } from '../permission/keys'

export type TenantMenuItem = {
  path: string
  label: string
  permission: string
}

export const tenantMenuItems: TenantMenuItem[] = [
  {
    path: TENANT_ROUTES.list,
    label: 'Tenant',
    permission: TENANT_PERMISSIONS.tenantView,
  },
]
