export const ACCOUNT_PERMISSIONS = {
  profileView: 'account.profile.view',
  profileEdit: 'account.profile.edit',
  changePassword: 'account.password.change',
} as const

export type AccountPermissionKey = keyof typeof ACCOUNT_PERMISSIONS
