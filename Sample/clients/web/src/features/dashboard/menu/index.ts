import { DASHBOARD_ROUTES } from '../routes/paths'
import { DASHBOARD_PERMISSIONS } from '../permission'

export type DashboardMenuItem = {
  path: string
  label: string
  permission: string
}

export const dashboardMenuItems: DashboardMenuItem[] = [
  {
    path: DASHBOARD_ROUTES.home,
    label: 'Dashboard',
    permission: DASHBOARD_PERMISSIONS.view,
  },
]
