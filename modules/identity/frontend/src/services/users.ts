import { identityHttp } from '../http'

export const identityQueryKeys = {
  users: ['identity', 'users'] as const,
  roles: ['identity', 'roles'] as const,
  permissionCatalog: ['authorization', 'catalog'] as const,
  permissionCatalogItems: ['authorization', 'catalog', 'items'] as const,
  policyLimits: ['authorization', 'catalog', 'policies'] as const,
  authorizationPolicies: ['authorization', 'policies'] as const,
  authorizationOperands: ['authorization', 'operands'] as const,
  authorizationOperators: ['authorization', 'operators'] as const,
  platformUsers: ['platform', 'users'] as const,
}

export type SystemUser = {
  id: string
  userName: string
  fullName: string
  email: string
  phoneNumber: string
  roleIds: string[]
  isActive: boolean
}

export type SystemUserWrite = {
  userName: string
  fullName: string
  email: string
  phoneNumber?: string
  roleIds: string[]
  isActive: boolean
  password?: string
}

type IdentityUserResponse = {
  userId: string
  username: string
  email?: string | null
  phoneNumber?: string | null
  displayName?: string | null
  isLocked?: boolean
  roleIds?: string[]
}

function text(value: string | null | undefined): string {
  return value ?? ''
}

export function mapIdentityUser(user: IdentityUserResponse): SystemUser {
  return {
    id: user.userId,
    userName: text(user.username),
    fullName: text(user.displayName),
    email: text(user.email),
    phoneNumber: text(user.phoneNumber),
    roleIds: user.roleIds ?? [],
    isActive: !user.isLocked,
  }
}

function isStatusOnly(input: Partial<SystemUserWrite>): boolean {
  return input.isActive !== undefined && input.userName === undefined && input.roleIds === undefined
}

export async function listSystemUsers(): Promise<SystemUser[]> {
  const users = await identityHttp.get<IdentityUserResponse[]>('/api/v1/app/users')
  return (users ?? []).map(mapIdentityUser)
}

export function getSystemUser(id: string): Promise<SystemUser> {
  return identityHttp.get<IdentityUserResponse>(`/api/v1/app/users/${id}`).then(mapIdentityUser)
}

export async function createSystemUser(input: SystemUserWrite): Promise<SystemUser> {
  const created = await identityHttp.post<{ userId: string; username: string }>('/api/v1/app/users', {
    username: input.userName,
    email: input.email,
    password: input.password,
    phoneNumber: input.phoneNumber,
    displayName: input.fullName,
    roleIds: input.roleIds,
  })
  return {
    id: created.userId,
    userName: text(created.username),
    fullName: input.fullName,
    email: input.email,
    phoneNumber: input.phoneNumber ?? '',
    roleIds: input.roleIds,
    isActive: input.isActive,
  }
}

export async function updateSystemUser(id: string, input: Partial<SystemUserWrite>): Promise<SystemUser> {
  if (isStatusOnly(input)) {
    await identityHttp.put<void>(`/api/v1/app/users/${id}/lock`, { isLocked: !input.isActive })
  } else {
    await identityHttp.put<void>(`/api/v1/app/users/${id}`, {
      username: input.userName,
      email: input.email,
      phoneNumber: input.phoneNumber,
      displayName: input.fullName,
      password: input.password?.trim() ? input.password : undefined,
      isLocked: !input.isActive,
      roleIds: input.roleIds,
    })
  }

  return {
    id,
    userName: input.userName ?? '',
    fullName: input.fullName ?? '',
    email: input.email ?? '',
    phoneNumber: input.phoneNumber ?? '',
    roleIds: input.roleIds ?? [],
    isActive: input.isActive ?? true,
  }
}

export function deleteSystemUser(id: string): Promise<void> {
  return identityHttp.delete<void>(`/api/v1/app/users/${id}`)
}
