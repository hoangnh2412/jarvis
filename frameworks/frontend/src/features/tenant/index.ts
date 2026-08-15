// Pages
export { TenantListPage } from './pages/TenantList'
export type {
  TenantListPageProps,
  TenantListPageContentContext,
} from './pages/TenantList'

export { TenantFormPage } from './pages/TenantForm'
export type {
  TenantFormPageProps,
  TenantFormPageContentContext,
} from './pages/TenantForm'

export { TenantDetailPage } from './pages/TenantDetail'
export type {
  TenantDetailPageProps,
  TenantDetailPageContentContext,
} from './pages/TenantDetail'

export { TenantConnectionsPage } from './pages/TenantConnections'
export type {
  TenantConnectionsPageProps,
  TenantConnectionsPageContentContext,
} from './pages/TenantConnections'

export { TenantDomainsPage } from './pages/TenantDomains'
export type {
  TenantDomainsPageProps,
  TenantDomainsPageContentContext,
} from './pages/TenantDomains'

// Components
export { TenantPageShell } from './components/TenantPageShell'
export type { TenantPageShellProps } from './components/TenantPageShell'

export { TenantForm } from './components/TenantForm'
export type { TenantFormProps } from './components/TenantForm'

export { TenantTable } from './components/TenantTable'
export type { TenantTableProps } from './components/TenantTable'

export { TenantStatusBadge } from './components/TenantStatusBadge'
export type { TenantStatusBadgeProps } from './components/TenantStatusBadge'

export { ConnectionForm } from './components/ConnectionForm'
export type { ConnectionFormProps } from './components/ConnectionForm'

export { ConnectionTable } from './components/ConnectionTable'
export type { ConnectionTableProps } from './components/ConnectionTable'

export { DomainForm } from './components/DomainForm'
export type { DomainFormProps } from './components/DomainForm'

export { DomainTable } from './components/DomainTable'
export type { DomainTableProps } from './components/DomainTable'

export { FieldSelect } from './components/FieldSelect'
export type {
  FieldSelectOption,
  FieldSelectProps,
} from './components/FieldSelect'

// Types
export {
  TenantStatus,
  TENANT_STATUS_LABEL,
  TENANT_STATUS_OPTIONS,
  DbProviderType,
  DB_PROVIDER_LABEL,
  DB_PROVIDER_OPTIONS,
} from './types'
export type {
  TenantStatusValue,
  DbProviderTypeValue,
  AuditedFields,
  Tenant,
  TenantConnection,
  TenantDomain,
  CreateTenantPayload,
  UpdateTenantPayload,
  SetTenantStatusPayload,
  CreateTenantConnectionPayload,
  UpdateTenantConnectionPayload,
  CreateTenantDomainPayload,
  UpdateTenantDomainPayload,
  GetTenantListParams,
  TenantListResult,
  TenantSubmitHandler,
  ActionProps,
} from './types'

// Services (axios API)
export {
  tenantHttp,
  configureTenantHttp,
  callGetTenantList,
  callGetTenant,
  callCreateTenant,
  callUpdateTenant,
  callDeleteTenant,
  callSetTenantStatus,
  callGetTenantConnectionList,
  callAddTenantConnection,
  callUpdateTenantConnection,
  callDeleteTenantConnection,
  callGetTenantDomainList,
  callAddTenantDomain,
  callUpdateTenantDomain,
  callDeleteTenantDomain,
} from './services'

// Validation
export {
  tenantFormSchema,
  tenantFormDefaultValues,
  tenantConnectionFormSchema,
  tenantConnectionFormDefaultValues,
  tenantDomainFormSchema,
  tenantDomainFormDefaultValues,
} from './validation'
export type {
  TenantFormData,
  TenantConnectionFormData,
  TenantDomainFormData,
} from './validation'

// Hooks
export {
  useTenantForm,
  useTenantConnectionForm,
  useTenantDomainForm,
} from './hooks'
export type {
  UseTenantFormOptions,
  UseTenantConnectionFormOptions,
  UseTenantDomainFormOptions,
} from './hooks'

// Routes
export {
  TENANT_ROUTES,
  getTenantRouteList,
  getTenantListPath,
  getTenantCreatePath,
  getTenantDetailPath,
  getTenantEditPath,
  getTenantConnectionsPath,
  getTenantDomainsPath,
  configureTenantNavigate,
  navigateTenant,
  tenantPaths,
} from './routes'
export type {
  TenantRouteKey,
  TenantRouteItem,
  TenantNavigateFn,
} from './routes'

// Permission
export { TENANT_PERMISSIONS, hasTenantPermission } from './permission'
export type { TenantPermissionKey } from './permission'

// Menu
export { tenantMenuItems } from './menu'
export type { TenantMenuItem } from './menu'

// Localization
export { tenantMessages, getTenantMessages } from './localization'
export type { TenantLocale } from './localization'

// Constants
export {
  TENANT_ROUTES as TENANT_ROUTE_PATHS,
  BASE_URL_TENANT,
} from './constants'

// Theme
export { defaultTenantTheme } from './theme'
export type { TenantTheme } from './theme'

// Content slot helpers
export { resolveTenantContent } from './utils'
export type { TenantSlotContent } from './utils'
