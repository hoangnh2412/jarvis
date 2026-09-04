import { ROLE_ROUTES } from './paths'

export { ROLE_ROUTES, getRoleListPath } from './paths'
export { configureRoleNavigate, navigateRole } from './navigation'

export const rolePaths = Object.values(ROLE_ROUTES)

export function getRoleRouteList() {
  return rolePaths
}
