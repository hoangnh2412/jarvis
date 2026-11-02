export type IdentityRouteSet = {
  users: string
  userCreate: string
  userEdit: (id: string) => string
  roles: string
  policyScopes: string
  platformUsers: string
  appLogin: string
  platformLogin: string
}

export const DEFAULT_IDENTITY_ROUTES: IdentityRouteSet = {
  users: '/system/users',
  userCreate: '/system/users/create',
  userEdit: (id: string) => `/system/users/edit/${id}`,
  roles: '/system/roles',
  policyScopes: '/system/policy-scopes',
  platformUsers: '/platform/users',
  appLogin: '/app/login',
  platformLogin: '/platform/login',
}

export type IdentityMenuItem = {
  path: string
  label: string
  permission: string | string[]
}

export function identityMenuItems(routes: IdentityRouteSet = DEFAULT_IDENTITY_ROUTES): IdentityMenuItem[] {
  return [
    { path: routes.users, label: 'Người dùng', permission: 'System.User.Manage' },
    { path: routes.roles, label: 'Vai trò', permission: 'System.Role.Manage' },
    { path: routes.policyScopes, label: 'Phạm vi truy cập', permission: 'System.Role.Manage' },
    { path: routes.platformUsers, label: 'Người dùng nền tảng', permission: 'Platform.Tenant.Manage' },
  ]
}
