export function hasTenantPermission(
  permissions: string[],
  key: string,
): boolean {
  return permissions.includes(key)
}
