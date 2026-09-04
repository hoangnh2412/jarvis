/** Audit fields dùng chung */
export type AuditedFields = {
  createdAt: string
  createdBy?: string | null
  updatedAt?: string | null
  updatedBy?: string | null
}

export type Role = AuditedFields & {
  id: string
  name: string
  displayName: string
  description?: string | null
  isDefault: boolean
  isPublic: boolean
  permissions: string[]
}

export type PermissionNode = {
  id: string
  label: string
  children?: PermissionNode[]
}

export type PermissionGroup = {
  id: string
  label: string
  permissions: PermissionNode[]
}

export type CreateRolePayload = {
  name: string
  displayName: string
  description?: string | null
  isDefault?: boolean
  isPublic?: boolean
}

export type UpdateRolePayload = CreateRolePayload

export type UpdateRolePermissionsPayload = {
  permissions: string[]
}

export type GetRoleListParams = {
  search?: string
  page?: number
  size?: number
}

export type RoleListResult = {
  items: Role[]
  total: number
  page?: number
  size?: number
}

export type { ActionProps } from './mutation'
