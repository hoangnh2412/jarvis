export const PLANNER_PERMISSIONS = {
  view: 'planner.view',
  edit: 'planner.edit',
} as const

export type PlannerPermissionKey =
  (typeof PLANNER_PERMISSIONS)[keyof typeof PLANNER_PERMISSIONS]

export function hasPlannerPermission(
  granted: string[] | undefined,
  key: PlannerPermissionKey,
) {
  return Boolean(granted?.includes(key))
}
