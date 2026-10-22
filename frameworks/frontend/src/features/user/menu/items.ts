import { USER_PERMISSIONS } from '../permission/keys'

export type UserMenuItem = {
  path: string
  label: string
  permission: string
}

export const userMenuItems: UserMenuItem[] = [
  {
    path: '/users',
    label: 'Người dùng',
    permission: USER_PERMISSIONS.view,
  },
]
