import { ACCOUNT_ROUTES, type AccountRouteKey } from './paths'

export { ACCOUNT_ROUTES } from './paths'
export type { AccountRouteKey } from './paths'

export type AccountRouteItem = {
  id: AccountRouteKey
  path: string
  titleKey: string
}

export function getAccountRouteList(): AccountRouteItem[] {
  return [
    { id: 'login', path: ACCOUNT_ROUTES.login, titleKey: 'account.routes.login' },
    {
      id: 'register',
      path: ACCOUNT_ROUTES.register,
      titleKey: 'account.routes.register',
    },
    {
      id: 'forgotPassword',
      path: ACCOUNT_ROUTES.forgotPassword,
      titleKey: 'account.routes.forgotPassword',
    },
    { id: 'profile', path: ACCOUNT_ROUTES.profile, titleKey: 'account.routes.profile' },
    {
      id: 'changePassword',
      path: ACCOUNT_ROUTES.changePassword,
      titleKey: 'account.routes.changePassword',
    },
  ]
}
