export type AdminUser = {
  id: string
  username: string
  email: string
  isLocked: boolean
  roleId: string
  roleName: string
  tenantId?: string | null
  orgId?: string | null
  createdAt: string
}

export type UserRoleOption = {
  id: string
  name: string
  displayName: string
  roleLevel: number
  isSystemRole: boolean
}

export type CreateUserPayload = {
  username: string
  email: string
  password: string
  roleId: string
  tenantId?: string | null
  orgId?: string | null
}

export type UpdateUserPayload = {
  isLocked: boolean
  roleId: string
}

export type GetUserListParams = {
  search?: string
  page?: number
  size?: number
}

export type UserListResult = {
  items: AdminUser[]
  total: number
  page: number
  size: number
}
