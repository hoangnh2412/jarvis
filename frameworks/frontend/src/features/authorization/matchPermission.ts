const WILDCARDS = new Set(['*', 'System.All'])

/** RBAC trên UI: khớp mã, hoặc actor đang giữ wildcard. */
export function matchPermission(
  permissions: readonly string[] | undefined,
  key: string,
): boolean {
  if (!key || !permissions?.length) return false
  if (permissions.some((permission) => WILDCARDS.has(permission))) return true
  return permissions.includes(key)
}
