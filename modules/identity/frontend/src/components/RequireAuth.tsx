import type { ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Navigate } from 'react-router-dom'
import { Loading } from '@platform/core'
import { getIdentityConfig } from '../config'
import { useCurrentUser } from '../hooks/useAuth'
import { authService, hasPermission, SHELL_ACCESS } from '../session'

export type RequireAuthProps = {
  children: ReactNode
  loginTo?: string
  /** Nội dung khi thiếu quyền truy cập shell; host tự render trang lỗi theo theme. */
  renderForbidden?: () => ReactNode
  /** Nội dung khi không nối được phiên với API. */
  renderSessionError?: () => ReactNode
}

/**
 * Giữ cổng xác thực của SPA ở mức shell; API vẫn phải tự kiểm tra quyền vì client chạy trên máy người dùng.
 */
export function RequireAuth({ children, loginTo, renderForbidden, renderSessionError }: RequireAuthProps) {
  const token = authService.getAccessToken()
  const session = authService.getStoredSession()
  const currentUserQuery = useCurrentUser()
  const platformTenantQuery = useQuery({
    queryKey: ['auth', 'platform-tenant'],
    queryFn: () => authService.alignPlatformTenant(),
    enabled: Boolean(token && session?.isAuthenticated),
    staleTime: 60_000,
  })
  if (!token || !session?.isAuthenticated) {
    return <Navigate to={loginTo ?? getIdentityConfig().loginPath} replace />
  }

  if (platformTenantQuery.isPending) {
    return <Loading label="Đang đồng bộ phiên làm việc..." fullScreen size="lg" />
  }

  if (platformTenantQuery.isError) {
    return renderSessionError ? (
      <>{renderSessionError()}</>
    ) : (
      <p className="p-6 text-sm text-red-600" role="alert">
        Không nối được phiên làm việc với API. Đăng xuất rồi đăng nhập lại.
      </p>
    )
  }

  const codes = Object.entries(currentUserQuery.data?.permissions ?? {})
    .filter(([, granted]) => granted)
    .map(([permission]) => permission)
  if (!hasPermission(codes, SHELL_ACCESS)) {
    if (currentUserQuery.isFetching) {
      return <p className="p-6 text-sm" role="status">Đang kiểm tra quyền truy cập ứng dụng.</p>
    }
    return renderForbidden ? (
      <>{renderForbidden()}</>
    ) : (
      <p className="p-6 text-sm text-red-600" role="alert">
        Quyền truy cập bị từ chối.
      </p>
    )
  }

  return <>{children}</>
}

/**
 * Chặn route được map rõ với catalog hiện tại; route chưa map tiếp tục theo quyết định phạm vi đã chốt.
 */
export function RequirePermission({
  children,
  code,
  renderForbidden,
}: {
  children: ReactNode
  code: string | readonly string[]
  renderForbidden?: () => ReactNode
}) {
  const currentUserQuery = useCurrentUser()
  const codes = Object.entries(currentUserQuery.data?.permissions ?? {})
    .filter(([, granted]) => granted)
    .map(([permission]) => permission)
  const requiredCodes: readonly string[] = typeof code === 'string' ? [code] : code

  if (!requiredCodes.some((requiredCode) => hasPermission(codes, requiredCode))) {
    if (currentUserQuery.isFetching) {
      return <p className="p-6 text-sm" role="status">Đang kiểm tra quyền truy cập chức năng.</p>
    }
    return renderForbidden ? (
      <>{renderForbidden()}</>
    ) : (
      <p className="p-6 text-sm text-red-600" role="alert">
        Quyền truy cập bị từ chối.
      </p>
    )
  }

  return <>{children}</>
}
