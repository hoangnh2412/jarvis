// Pages
export { RoleListPage } from './pages/RoleList'
export type {
  RoleListPageProps,
  RoleListPageContentContext,
} from './pages/RoleList'

// Components
export {
  PermissionEditor,
  PermissionTreeNode,
  RoleDetailView,
  RoleForm,
  RolePageShell,
  RoleTable,
} from './components'
export type {
  PermissionEditorProps,
  PermissionTreeNodeProps,
  RoleDetailViewProps,
  RoleFormProps,
  RolePageShellProps,
  RoleTableProps,
} from './components'

// Types
export type {
  Role,
  PermissionNode,
  PermissionGroup,
  CreateRolePayload,
  UpdateRolePayload,
  UpdateRolePermissionsPayload,
  GetRoleListParams,
  RoleListResult,
} from './types'
export type { ActionProps } from './types'

// Services (mock — demo mặc định)
export {
  mockCreateRole,
  mockDeleteRole,
  mockGetPermissionCatalog,
  mockGetRole,
  mockGetRoleList,
  mockResetRoles,
  mockUpdateRole,
  mockUpdateRolePermissions,
} from './services'

// Services (axios API — gắn qua callback.onSubmit)
export {
  roleHttp,
  configureRoleHttp,
  callGetRoleList,
  callGetRole,
  callCreateRole,
  callUpdateRole,
  callDeleteRole,
  callUpdateRolePermissions,
} from './services'

// Validation
export {
  roleFormSchema,
  roleFormDefaultValues,
} from './validation'
export type { RoleFormData } from './validation'

// Hooks
export { useRoleForm } from './hooks'
export type { UseRoleFormOptions } from './hooks'

// Routes
export {
  ROLE_ROUTES,
  getRoleListPath,
  getRoleRouteList,
  rolePaths,
  configureRoleNavigate,
  navigateRole,
} from './routes'

// Permission
export { ROLE_PERMISSIONS, hasRolePermission } from './permission'
export type { RolePermissionKey } from './permission'

// Menu
export { roleMenuItems } from './menu'
export type { RoleMenuItem } from './menu'

// Localization
export { roleMessages, getRoleMessages } from './localization'
export type { RoleLocale } from './localization'

// Constants
export { PERMISSION_CATALOG, FAKE_ROLES, ROLE_STORAGE_KEY } from './constants'

// Utils
export {
  collectAllPermissionIds,
  filterPermissionGroups,
  filterPermissionNodes,
  getDescendantIds,
  getNodeCheckState,
  setGrantAll,
  togglePermissionNode,
  resolveRoleContent,
} from './utils'
export type { CheckState, RoleSlotContent } from './utils'
