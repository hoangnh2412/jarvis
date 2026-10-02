export const TIMESHEET_PERMISSIONS = {
  view: 'timesheet.view',
  edit: 'timesheet.edit',
  export: 'timesheet.export',
} as const

export type TimesheetPermissionKey =
  (typeof TIMESHEET_PERMISSIONS)[keyof typeof TIMESHEET_PERMISSIONS]

export function hasTimesheetPermission(
  granted: string[] | undefined,
  key: TimesheetPermissionKey,
) {
  return Boolean(granted?.includes(key))
}
