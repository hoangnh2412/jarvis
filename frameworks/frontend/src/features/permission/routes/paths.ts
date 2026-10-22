export const PERMISSION_ROUTES = {
  list: '/permissions',
} as const

export function getPermissionListPath() {
  return PERMISSION_ROUTES.list
}
