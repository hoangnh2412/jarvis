export const TENANT_ROUTES = {
  tenants: '/platform/tenants',
  companyProfile: '/company-profile',
} as const

export type TenantMenuItem = {
  path: string
  label: string
  permission: string | string[]
}

export const tenantMenuItems: TenantMenuItem[] = [
  { path: TENANT_ROUTES.tenants, label: 'Tenant', permission: 'Platform.Tenant.Manage' },
  { path: TENANT_ROUTES.companyProfile, label: 'Thông tin doanh nghiệp', permission: ['System.Company.View', 'System.Company.Manage'] },
]
