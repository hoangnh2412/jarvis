export const ROLE_ROUTES = {
  list: '/roles',
} as const

export type RoleRouteKey = keyof typeof ROLE_ROUTES

export function getRoleListPath() {
  return ROLE_ROUTES.list
}
