export const PLANNER_ROUTES = {
  page: '/planner',
} as const

export type PlannerRouteKey = keyof typeof PLANNER_ROUTES

export function getPlannerPath() {
  return PLANNER_ROUTES.page
}

export function getPlannerRouteList() {
  return Object.values(PLANNER_ROUTES)
}

export const plannerPaths = PLANNER_ROUTES
