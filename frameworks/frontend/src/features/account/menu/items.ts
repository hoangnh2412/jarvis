import { ACCOUNT_ROUTES } from '../routes/paths'
import { ACCOUNT_PERMISSIONS } from '../permission/keys'

export type AccountMenuItem = {
  path: string
  label: string
  permission: string
}

export const accountMenuItems: AccountMenuItem[] = [
  {
    path: ACCOUNT_ROUTES.profile,
    label: 'Hồ sơ',
    permission: ACCOUNT_PERMISSIONS.profileView,
  },
  {
    path: ACCOUNT_ROUTES.changePassword,
    label: 'Đổi mật khẩu',
    permission: ACCOUNT_PERMISSIONS.changePassword,
  },
]
