import type { ReactNode } from 'react'
import { useAccess } from './accessContext'
import { matchPermission } from './matchPermission'

export type CanProps = {
  /** RBAC. Bỏ trống thì chỉ xét `allowed`. */
  permission?: string
  /**
   * ABAC. Màn đã tính từ dòng dữ liệu (cùng org, owner, hạn mức…).
   * Mặc định cho qua — chỉ chặn khi truyền `false`.
   */
  allowed?: boolean
  fallback?: ReactNode
  children: ReactNode
}

export function Can({
  permission,
  allowed = true,
  fallback = null,
  children,
}: CanProps) {
  const { permissions } = useAccess()
  const permissionOk = permission
    ? matchPermission(permissions, permission)
    : true
  if (permissionOk && allowed) return children
  return fallback
}
