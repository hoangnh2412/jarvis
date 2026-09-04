import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { ACCOUNT_ROUTES } from '@jarvis/core'
import { isAuthenticated } from './accessToken'

export function RequireAuth({ children }: { children: ReactNode }) {
  const location = useLocation()

  if (!isAuthenticated()) {
    return (
      <Navigate
        to={ACCOUNT_ROUTES.login}
        replace
        state={{ from: location.pathname }}
      />
    )
  }

  return children
}
