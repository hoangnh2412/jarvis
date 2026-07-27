export const ACCOUNT_ROUTES = {
  login: '/login',
  register: '/register',
  forgotPassword: '/forgot-password',
  profile: '/profile',
  changePassword: '/change-password',
} as const

export type AccountRouteKey = keyof typeof ACCOUNT_ROUTES
