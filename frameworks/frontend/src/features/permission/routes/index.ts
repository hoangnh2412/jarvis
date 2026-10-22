import { PERMISSION_ROUTES } from './paths'

export {
  PERMISSION_ROUTES,
  getPermissionListPath,
} from './paths'
export { configurePermissionNavigate, navigatePermission } from './navigation'

export const permissionPaths = Object.values(PERMISSION_ROUTES)

export function getPermissionRouteList() {
  return permissionPaths
}
