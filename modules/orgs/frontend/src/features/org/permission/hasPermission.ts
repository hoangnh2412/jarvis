import type { OrgPermissionKey } from './keys'

export function hasOrgPermission(
  permissions: readonly string[],
  key: OrgPermissionKey,
): boolean {
  return permissions.includes(key)
}
