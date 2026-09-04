import { ROLE_PERMISSIONS } from '../permission/keys'

export type RoleMenuItem = {
  path: string
  label: string
  permission: string
}

export const roleMenuItems: RoleMenuItem[] = [
  {
    path: '/roles',
    label: 'Vai trò',
    permission: ROLE_PERMISSIONS.view,
  },
]
