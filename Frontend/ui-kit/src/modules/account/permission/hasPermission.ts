export function hasPermission(permissions: string[], key: string): boolean {
  return permissions.includes(key)
}
