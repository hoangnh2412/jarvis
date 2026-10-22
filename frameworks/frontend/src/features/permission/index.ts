export { PermissionCatalogPage } from './pages/PermissionCatalog'
export type { PermissionCatalogPageProps } from './pages/PermissionCatalog'

export {
  PERMISSION_ROUTES,
  configurePermissionNavigate,
  getPermissionListPath,
  getPermissionRouteList,
  navigatePermission,
  permissionPaths,
} from './routes'

export { permissionMenuItems, PERMISSION_MENU_KEY } from './menu'
export type { PermissionMenuItem } from './menu'

export { getPermissionMessages, permissionMessages } from './localization'
export type { PermissionLocale, PermissionMessages } from './localization'

export { mockGetAuthorizationCatalog } from './services'
export type { AuthorizationCatalog } from './services'
