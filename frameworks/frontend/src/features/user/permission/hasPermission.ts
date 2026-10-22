import { matchPermission } from '../../authorization/matchPermission'
import type { UserPermissionKey } from './keys'

export function hasUserPermission(
  permissions: readonly string[] | undefined,
  key: UserPermissionKey,
): boolean {
  return matchPermission(permissions, key)
}
