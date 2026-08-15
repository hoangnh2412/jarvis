export {
  mockCreateRole,
  mockDeleteRole,
  mockGetPermissionCatalog,
  mockGetRole,
  mockGetRoleList,
  mockResetRoles,
  mockUpdateRole,
  mockUpdateRolePermissions,
} from './roleMock'

export {
  callCreateRole,
  callDeleteRole,
  callGetRole,
  callGetRoleList,
  callUpdateRole,
  callUpdateRolePermissions,
} from './role'

export { default as roleHttp, configureRoleHttp } from './req'
