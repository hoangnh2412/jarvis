export const ORG_ROUTES = {
  list: '/orgs',
} as const

export type OrgRouteKey = keyof typeof ORG_ROUTES

export function getOrgListPath() {
  return ORG_ROUTES.list
}
