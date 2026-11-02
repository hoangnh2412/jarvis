export {
  configureTenants,
  getTenantsConfig,
  type TenantsConfig,
} from './config'
export { tenantsHttp, tenantsRequest, type TenantsRequestOptions } from './http'
export { notifyApiError } from './notify'
export { catalogSearchMatches, catalogSearchNormalize, searchNormalize } from './utils'
export { SimpleTable, type SimpleColumn, type SimpleTableProps } from './components/SimpleTable'
export {
  btnAuxClass,
  btnPrimaryClass,
  btnSecondaryClass,
  codeBadgeClass,
  fieldInputClass,
  fieldInputInvalidClass,
  labelClass,
  systemBadgeClass,
} from './components/fieldStyles'
export {
  createPlatformTenant,
  getPlatformTenantConnection,
  listPlatformTenants,
  setPlatformTenantActive,
  tenantQueryKeys,
  updatePlatformTenant,
  type CatalogAdminProvision,
  type CreatePlatformTenantResult,
  type PlatformTenant,
  type TenantAdminInput,
} from './services/tenants'
export {
  companyProfileQueryKeys,
  emptyCompanyProfileCore,
  getCompanyProfile,
  saveCompanyProfile,
  type CompanyProfileCore,
  type CompanyProfileDto,
} from './services/companyProfile'
export { TenantCatalogPage, TenantForm, type TenantFormData } from './pages/TenantCatalogPage'
export {
  CompanyProfilePage,
  type CompanyProfileExtensionField,
  type CompanyProfileExtensionSlot,
  type CompanyProfilePageProps,
} from './pages/CompanyProfilePage'
export { TENANT_ROUTES, tenantMenuItems, type TenantMenuItem } from './routes'
