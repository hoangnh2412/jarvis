export const TIMESHEET_ROUTES = {
  page: '/timesheet',
} as const

export type TimesheetRouteKey = keyof typeof TIMESHEET_ROUTES

export function getTimesheetPath() {
  return TIMESHEET_ROUTES.page
}

export function getTimesheetRouteList() {
  return Object.values(TIMESHEET_ROUTES)
}

export const timesheetPaths = TIMESHEET_ROUTES
