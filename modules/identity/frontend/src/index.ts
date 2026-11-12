export {
  configureIdentity,
  getIdentityConfig,
  sessionKey,
  type IdentityCompanyProfile,
  type IdentityConfig,
} from './config'
export { identityHttp, identityRequest, getAccessToken, type IdentityRequestOptions } from './http'
export {
  authService,
  hasPermission,
  SHELL_ACCESS,
  STORAGE_KEYS,
  type AuthSession,
  type CurrentUser,
  type LoginPayload,
  type MeAccount,
  type MeApiResponse,
  type MeOrgUnit,
  type TenantCompany,
  type TenantInfo,
} from './session'
export {
  AUTH_QUERY_KEYS,
  useCurrentUser,
  useHasPermission,
  useLogin,
  useLogout,
  useTenantCompany,
} from './hooks/useAuth'
export { RequireAuth, RequirePermission, type RequireAuthProps } from './components/RequireAuth'
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
export { notifyApiError } from './notify'
export { catalogSearchMatches, catalogSearchNormalize, searchNormalize } from './utils'
export {
  identityQueryKeys,
  createSystemUser,
  deleteSystemUser,
  getSystemUser,
  listSystemUsers,
  mapIdentityUser,
  updateSystemUser,
  type SystemUser,
  type SystemUserWrite,
} from './services/users'
export {
  createSystemRole,
  deleteSystemRole,
  fetchPermissionCatalog,
  listPermissionCatalog,
  listPermissionCatalogItems,
  listSystemRoles,
  mapIdentityRole,
  setSystemRolePermissions,
  setSystemRolePolicies,
  updateSystemRole,
  type PermissionCatalogItem,
  type PermissionChoice,
  type PermissionChoiceGroup,
  type SystemRole,
} from './services/roles'
export {
  createAuthorizationPolicy,
  deleteAuthorizationPolicy,
  getAuthorizationPolicy,
  listAuthorizationOperands,
  listAuthorizationOperators,
  listAuthorizationPolicies,
  listPolicyLimits,
  updateAuthorizationPolicy,
  type AuthorizationOperand,
  type AuthorizationPolicyDetail,
  type AuthorizationPolicyRule,
  type AuthorizationPolicyWrite,
  type PolicyLimit,
} from './services/policies'
export {
  createPlatformUser,
  listPlatformUsers,
  platformUserQueryKeys,
  updatePlatformUser,
  type PlatformUser,
  type PlatformUserAdminInput,
} from './services/platformUsers'
export {
  listAppLoginOrgs,
  listAppLoginTenants,
  type LoginCatalogItem,
} from './services/loginCatalog'
export { LoginPage, type LoginBranding, type LoginPageProps, type LoginPageContext } from './pages/LoginPage'
export { UsersPage, type UsersPageContext, type UsersPageProps } from './pages/UsersPage'
export { UserFormPage, type UserFormPageContext, type UserFormPageProps } from './pages/UserFormPage'
export { UserForm, type UserFormContext, type OrgUnitOption, type UserFormProps, type UserFormValues } from './pages/UserForm'
export { RolesPage, type RolesPageProps } from './pages/RolesPage'
export { RoleAuthorizationDialog } from './pages/RoleAuthorizationDialog'
export { RolePolicyDialog, type RolePolicyDialogProps } from './pages/RolePolicyDialog'
export { RolePolicyPanel, type RolePolicyPanelProps } from './pages/RolePolicyPanel'
export { PolicyScopesPage } from './pages/PolicyScopesPage'
export { CreatePolicyLimitDialog, type CreatePolicyLimitDialogProps } from './pages/CreatePolicyLimitDialog'
export { PlatformUsersPage, type PlatformUsersPageProps } from './pages/PlatformUsersPage'
export {
  canMutatePolicyCatalog,
  matchesPolicySearch,
  operandDisplayLabel,
  operatorDisplayLabel,
  policyConditionLines,
  policyHumanSummary,
  policyLimitMeta,
  type PolicyLimitMeta,
} from './pages/policyLimitUi'
export {
  DEFAULT_IDENTITY_ROUTES,
  identityMenuItems,
  type IdentityMenuItem,
  type IdentityRouteSet,
} from './routes'
