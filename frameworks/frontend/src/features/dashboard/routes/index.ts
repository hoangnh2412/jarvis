import { DASHBOARD_ROUTES, getDashboardHomePath } from './paths'

export { DASHBOARD_ROUTES, getDashboardHomePath } from './paths'
export type { DashboardRouteKey } from './paths'

export type DashboardNavigateFn = (to: string) => void

let navigateFn: DashboardNavigateFn | null = null

export function configureDashboardNavigate(fn: DashboardNavigateFn) {
  navigateFn = fn
}

export function navigateDashboard(to: string) {
  if (navigateFn) {
    navigateFn(to)
    return
  }
  if (typeof window !== 'undefined') {
    window.location.assign(to)
  }
}

export type DashboardRouteItem = {
  key: string
  path: string
  label: string
}

export function getDashboardRouteList(): DashboardRouteItem[] {
  return [
    { key: 'home', path: DASHBOARD_ROUTES.home, label: 'Dashboard' },
  ]
}

export const dashboardPaths = {
  home: getDashboardHomePath,
}
