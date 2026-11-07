import { TIMESHEET_PERMISSIONS } from '../permission'
import { getTimesheetPath } from '../routes/paths'

export type TimesheetMenuItem = {
  path: string
  label: string
  permission: string
}

export const timesheetMenuItems: TimesheetMenuItem[] = [
  {
    path: getTimesheetPath(),
    label: 'Timesheet',
    permission: TIMESHEET_PERMISSIONS.view,
  },
]
