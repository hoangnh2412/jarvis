export const PERMISSION_MENU_KEY = 'Role.AssignPermission'

export type PermissionMenuItem = {
  path: string
  label: string
  permission: string
}

export const permissionMenuItems: PermissionMenuItem[] = [
  {
    path: '/permissions',
    label: 'Permission',
    permission: PERMISSION_MENU_KEY,
  },
]
