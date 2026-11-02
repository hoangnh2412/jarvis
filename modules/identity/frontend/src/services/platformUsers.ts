import { identityHttp } from '../http'

const USERS_API = '/api/v1/platform/users'

export const platformUserQueryKeys = {
  all: ['platform', 'users'] as const,
  list: (search?: string) => ['platform', 'users', 'list', search ?? ''] as const,
}

export type PlatformUser = {
  id: string
  username: string
  email: string
  displayName: string
  phoneNumber: string
  isLocked: boolean
  roleNames: string[]
}

export type PlatformUserAdminInput = {
  username: string
  email: string
  password?: string
  displayName: string
}

function mapPlatformUser(raw: Record<string, unknown>): PlatformUser {
  const roles = raw.roleNames ?? raw.RoleNames
  return {
    id: String(raw.id ?? raw.Id ?? ''),
    username: String(raw.username ?? raw.Username ?? ''),
    email: String(raw.email ?? raw.Email ?? ''),
    displayName: String(raw.displayName ?? raw.DisplayName ?? ''),
    phoneNumber: String(raw.phoneNumber ?? raw.PhoneNumber ?? ''),
    isLocked: Boolean(raw.isLocked ?? raw.IsLocked ?? false),
    roleNames: Array.isArray(roles) ? roles.map((role) => String(role)) : [],
  }
}

export async function listPlatformUsers(params?: { search?: string }): Promise<PlatformUser[]> {
  const payload = await identityHttp.get<unknown[]>(USERS_API, {
    attachScope: false,
    params: { search: params?.search?.trim() || undefined },
  })
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapPlatformUser)
    .filter((user) => user.id)
}

export async function createPlatformUser(input: { admin: PlatformUserAdminInput }): Promise<PlatformUser> {
  const payload = await identityHttp.post<Record<string, unknown>>(USERS_API, {
    username: input.admin.username,
    email: input.admin.email,
    password: input.admin.password,
    displayName: input.admin.displayName,
  }, { attachScope: false })
  return mapPlatformUser(payload)
}

export async function updatePlatformUser(
  id: string,
  input: {
    email?: string
    displayName?: string
    phoneNumber?: string
    password?: string
    isLocked?: boolean
  },
): Promise<PlatformUser> {
  const payload = await identityHttp.put<Record<string, unknown>>(
    `${USERS_API}/${encodeURIComponent(id)}`,
    input,
    { attachScope: false },
  )
  return mapPlatformUser(payload)
}
