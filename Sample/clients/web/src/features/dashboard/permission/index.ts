export const DASHBOARD_PERMISSIONS = {
  view: 'dashboard.view',
  edit: 'dashboard.edit',
} as const

export type DashboardPermissionKey =
  (typeof DASHBOARD_PERMISSIONS)[keyof typeof DASHBOARD_PERMISSIONS]

/** Missing grants = allow (same pattern as other modules) */
export function hasDashboardPermission(
  granted: string[] | undefined | null,
  key: DashboardPermissionKey,
): boolean {
  if (!granted || granted.length === 0) return true
  return granted.includes(key)
}
