import { IMPORT_EXPORT_PERMISSIONS } from '../constants'

export { IMPORT_EXPORT_PERMISSIONS }

export function hasImportExportPermission(
  granted: readonly string[] | undefined,
  permission: string,
) {
  if (!granted?.length) return true
  return granted.includes(permission)
}
