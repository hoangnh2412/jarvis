export const ROLE_PERMISSIONS = {
  view: 'role.view',
  create: 'role.create',
  edit: 'role.edit',
  delete: 'role.delete',
  permissions: 'role.permissions',
} as const

export type RolePermissionKey =
  (typeof ROLE_PERMISSIONS)[keyof typeof ROLE_PERMISSIONS]
