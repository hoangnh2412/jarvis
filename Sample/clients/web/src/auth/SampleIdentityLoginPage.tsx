import { Navigate } from 'react-router-dom'
import { ACCOUNT_ROUTES } from '@platform/core'
import { DEFAULT_IDENTITY_ROUTES, LoginPage } from '@platform/identity'
import { DASHBOARD_ROUTES } from '../features/dashboard/routes/paths'
import { isAuthenticated } from './accessToken'

export type SampleIdentityLoginPageProps = {
  realm: 'app' | 'platform'
}

/** Cổng đăng nhập identity (app tenant/org hoặc platform Master). */
export function SampleIdentityLoginPage({ realm }: SampleIdentityLoginPageProps) {
  if (isAuthenticated()) {
    return <Navigate to={DASHBOARD_ROUTES.home} replace />
  }

  return (
    <LoginPage
      realm={realm}
      redirectTo={DASHBOARD_ROUTES.home}
      appLoginPath={DEFAULT_IDENTITY_ROUTES.appLogin}
      platformLoginPath={DEFAULT_IDENTITY_ROUTES.platformLogin}
      forgotPasswordPath={ACCOUNT_ROUTES.forgotPassword}
    />
  )
}

export function SampleAppLoginPage() {
  return <SampleIdentityLoginPage realm="app" />
}

export function SamplePlatformLoginPage() {
  return <SampleIdentityLoginPage realm="platform" />
}
