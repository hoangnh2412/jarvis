export { default as tenantHttp, configureTenantHttp } from './req'

export {
  callGetTenantList,
  callGetTenant,
  callCreateTenant,
  callUpdateTenant,
  callDeleteTenant,
  callSetTenantStatus,
} from './tenant'

export {
  callGetTenantConnectionList,
  callAddTenantConnection,
  callUpdateTenantConnection,
  callDeleteTenantConnection,
} from './connection'

export {
  callGetTenantDomainList,
  callAddTenantDomain,
  callUpdateTenantDomain,
  callDeleteTenantDomain,
} from './domain'
