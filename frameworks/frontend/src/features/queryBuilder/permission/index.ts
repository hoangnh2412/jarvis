export const QUERY_BUILDER_PERMISSIONS = {
  view: 'queryBuilder.view',
  edit: 'queryBuilder.edit',
} as const

export type QueryBuilderPermissionKey =
  (typeof QUERY_BUILDER_PERMISSIONS)[keyof typeof QUERY_BUILDER_PERMISSIONS]

/** Missing grants = allow (same pattern as other modules) */
export function hasQueryBuilderPermission(
  granted: string[] | undefined | null,
  key: QueryBuilderPermissionKey,
): boolean {
  if (!granted || granted.length === 0) return true
  return granted.includes(key)
}
