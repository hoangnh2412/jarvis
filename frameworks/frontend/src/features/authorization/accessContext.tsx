import { createContext, useContext, type ReactNode } from 'react'
import { matchPermission } from './matchPermission'

export type AccessSnapshot = {
  permissions: readonly string[]
  userId?: string
  tenantId?: string
  orgId?: string
  approveLimit?: number
}

const EMPTY_ACCESS: AccessSnapshot = { permissions: [] }

const AccessContext = createContext<AccessSnapshot>(EMPTY_ACCESS)

export type AccessProviderProps = {
  value: AccessSnapshot
  children: ReactNode
}

export function AccessProvider({ value, children }: AccessProviderProps) {
  return <AccessContext.Provider value={value}>{children}</AccessContext.Provider>
}

export function useAccess(): AccessSnapshot {
  return useContext(AccessContext)
}

export function useCan(permission: string): boolean {
  const { permissions } = useAccess()
  return matchPermission(permissions, permission)
}
