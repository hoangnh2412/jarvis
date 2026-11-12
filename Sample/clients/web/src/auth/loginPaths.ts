import { ACCOUNT_ROUTES } from '@platform/core'
import { DEFAULT_IDENTITY_ROUTES } from '@platform/identity'

const LOGIN_PATHS = [
  ACCOUNT_ROUTES.login,
  DEFAULT_IDENTITY_ROUTES.appLogin,
  DEFAULT_IDENTITY_ROUTES.platformLogin,
] as const

export function isPublicLoginPath(pathname: string): boolean {
  return LOGIN_PATHS.some((path) => pathname === path || pathname.startsWith(`${path}/`))
}

export const SAMPLE_APP_LOGIN_PATH = DEFAULT_IDENTITY_ROUTES.appLogin
