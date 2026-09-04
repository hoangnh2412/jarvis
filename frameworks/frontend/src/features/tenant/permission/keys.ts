export const TENANT_PERMISSIONS = {
  tenantView: 'tenant.view',
  tenantCreate: 'tenant.create',
  tenantEdit: 'tenant.edit',
  tenantDelete: 'tenant.delete',
  tenantStatus: 'tenant.status',
  connectionView: 'tenant.connection.view',
  connectionManage: 'tenant.connection.manage',
  domainView: 'tenant.domain.view',
  domainManage: 'tenant.domain.manage',
} as const

export type TenantPermissionKey = keyof typeof TENANT_PERMISSIONS
