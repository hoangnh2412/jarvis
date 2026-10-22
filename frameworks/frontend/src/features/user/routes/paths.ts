export const USER_ROUTES = {
  list: '/users',
  create: '/users/create',
  detail: '/users/:id',
  edit: '/users/:id/edit',
} as const

export type UserRouteKey = keyof typeof USER_ROUTES

export function getUserListPath() {
  return USER_ROUTES.list
}

export function getUserCreatePath() {
  return USER_ROUTES.create
}

export function getUserDetailPath(id: string) {
  return `/users/${id}`
}

export function getUserEditPath(id: string) {
  return `/users/${id}/edit`
}
