import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { isAuthenticated } from './accessToken'
import { SAMPLE_APP_LOGIN_PATH } from './loginPaths'

export function RequireAuth({ children }: { children: ReactNode }) {
  const location = useLocation()

  if (!isAuthenticated()) {
    return (
      <Navigate
        to={SAMPLE_APP_LOGIN_PATH}
        replace
        state={{ from: location.pathname }}
      />
    )
  }

  return children
}
