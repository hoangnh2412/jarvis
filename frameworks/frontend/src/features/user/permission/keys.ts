export const USER_PERMISSIONS = {
  view: 'User.Manage',
  create: 'User.Manage',
  edit: 'User.Manage',
  delete: 'User.Manage',
} as const

export type UserPermissionKey =
  (typeof USER_PERMISSIONS)[keyof typeof USER_PERMISSIONS]
