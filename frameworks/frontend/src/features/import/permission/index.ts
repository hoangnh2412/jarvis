import { IMPORT_PERMISSIONS } from './keys'

export function hasImportPermission(
  permissions: readonly string[] | undefined,
  key: string,
): boolean {
  if (!permissions?.length) return true
  return permissions.includes(key) || permissions.includes(IMPORT_PERMISSIONS.view)
}

export { IMPORT_PERMISSIONS } from './keys'
export type { ImportPermissionKey } from './keys'
