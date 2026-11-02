import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  authService,
  hasPermission,
  type AuthSession,
  type CurrentUser,
  type LoginPayload,
  type TenantCompany,
} from '../session'

export const AUTH_QUERY_KEYS = {
  CURRENT_USER: ['auth', 'currentUser'] as const,
  COMPANY: ['auth', 'company'] as const,
  PERMISSIONS: ['auth', 'permissions'] as const,
  SESSION: ['auth', 'session'] as const,
}

export function useLogin() {
  const queryClient = useQueryClient()

  return useMutation<AuthSession, Error, LoginPayload>({
    mutationFn: (payload: LoginPayload) => authService.executeLogin(payload),
    onSuccess: (session) => {
      queryClient.setQueryData(AUTH_QUERY_KEYS.SESSION, session)
      queryClient.setQueryData(AUTH_QUERY_KEYS.CURRENT_USER, session.user)
      queryClient.setQueryData(AUTH_QUERY_KEYS.COMPANY, session.company)
      queryClient.setQueryData(AUTH_QUERY_KEYS.PERMISSIONS, session.permissions)
    },
  })
}

/** Làm mới snapshot từ `/api/v1/me` khi mount hoặc focus sau thời hạn tươi 60 giây. */
export function useCurrentUser() {
  return useQuery<CurrentUser>({
    queryKey: AUTH_QUERY_KEYS.CURRENT_USER,
    queryFn: () => authService.getCurrentUser(),
    enabled: Boolean(authService.getAccessToken()),
    initialData: () => authService.getStoredSession()?.user as CurrentUser | undefined,
    initialDataUpdatedAt: () => (authService.getStoredSession() ? 0 : undefined),
    staleTime: 60 * 1000,
  })
}

/**
 * Dùng permission snapshot từ `/api/v1/me` cho trạng thái UI; API vẫn phải enforce quyền ở server.
 */
export function useHasPermission(code: string): boolean {
  const currentUserQuery = useCurrentUser()
  const permissionCodes = Object.entries(currentUserQuery.data?.permissions ?? {})
    .filter(([, granted]) => granted)
    .map(([permissionCode]) => permissionCode)

  return hasPermission(permissionCodes, code)
}

export function useTenantCompany() {
  return useQuery<TenantCompany>({
    queryKey: AUTH_QUERY_KEYS.COMPANY,
    queryFn: () => authService.getTenantCompany(),
    initialData: () => authService.getStoredSession()?.company as TenantCompany | undefined,
    staleTime: 10 * 60 * 1000,
  })
}

export function useLogout() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async () => {
      authService.logout()
    },
    onSuccess: () => {
      queryClient.clear()
    },
  })
}
