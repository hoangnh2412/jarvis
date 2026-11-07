export const DASHBOARD_ROUTES = {
  home: '/dashboard',
} as const

export type DashboardRouteKey = keyof typeof DASHBOARD_ROUTES

export function getDashboardHomePath() {
  return DASHBOARD_ROUTES.home
}
