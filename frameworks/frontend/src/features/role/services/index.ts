export {
  mockCreateRole,
  mockDeleteRole,
  mockGetPermissionCatalog,
  mockGetRole,
  mockGetRoleList,
  mockResetRoles,
  mockUpdateRole,
  mockUpdateRolePermissions,
  mockUpdateRolePolicies,
} from './roleMock'

export {
  callCreateRole,
  callDeleteRole,
  callGetRole,
  callGetRoleList,
  callUpdateRole,
  callUpdateRolePermissions,
  callUpdateRolePolicies,
} from './role'

export { default as roleHttp, configureRoleHttp } from './req'
