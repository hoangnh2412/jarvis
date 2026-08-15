import type { RolePermissionKey } from './keys'

export function hasRolePermission(
  permissions: string[],
  key: RolePermissionKey,
): boolean {
  return permissions.includes(key)
}
