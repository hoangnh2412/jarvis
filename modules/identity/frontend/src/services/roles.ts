import { identityHttp } from '../http'

export type SystemRole = {
  id: string
  name: string
  description: string
  isActive: boolean
  permissionKeys: string[]
  policyCodes: string[]
  roleLevel: number
  isSystemRole: boolean
}

export type PermissionChoice = {
  id: string
  label: string
  usageHint?: string | null
  isSystemPermission?: boolean
}

export type PermissionChoiceGroup = {
  id: string
  label: string
  permissions: PermissionChoice[]
}

export type PermissionCatalogItem = {
  code: string
  module: string
  group: string
  displayName: string
}

type IdentityRoleResponse = {
  roleId: string
  name: string
  displayName?: string | null
  roleLevel?: number
  isSystemRole?: boolean
  isActive?: boolean
  permissions?: string[]
  policies?: string[]
}

type PermissionCatalogResponse = {
  permissions?: { code: string; module?: string; group: string; displayName: string }[]
  policies?: { code: string; name: string; isSystemPolicy?: boolean }[]
}

function text(value: string | null | undefined): string {
  return value ?? ''
}

export function mapIdentityRole(role: IdentityRoleResponse): SystemRole {
  return {
    id: role.roleId,
    name: text(role.name),
    description: text(role.displayName),
    isActive: role.isActive !== false,
    permissionKeys: role.permissions ?? [],
    policyCodes: role.policies ?? [],
    roleLevel: role.roleLevel ?? 100,
    isSystemRole: Boolean(role.isSystemRole),
  }
}

export async function listSystemRoles(): Promise<SystemRole[]> {
  const roles = await identityHttp.get<IdentityRoleResponse[]>('/api/v1/app/roles')
  return (roles ?? []).map(mapIdentityRole)
}

export async function createSystemRole(input: Pick<SystemRole, 'name' | 'description' | 'isActive'>): Promise<SystemRole> {
  const created = await identityHttp.post<IdentityRoleResponse>('/api/v1/app/roles', {
    name: input.name,
    displayName: input.description,
    isActive: input.isActive,
  })
  return mapIdentityRole({ ...created, displayName: input.description, isActive: input.isActive })
}

export async function updateSystemRole(
  role: Pick<SystemRole, 'id' | 'roleLevel'>,
  input: Pick<SystemRole, 'name' | 'description' | 'isActive'>,
): Promise<SystemRole> {
  await identityHttp.put<void>(`/api/v1/app/roles/${role.id}`, {
    name: input.name,
    displayName: input.description,
    roleLevel: role.roleLevel,
    isActive: input.isActive,
  })
  return {
    id: role.id,
    name: input.name,
    description: input.description,
    isActive: input.isActive,
    permissionKeys: [],
    policyCodes: [],
    roleLevel: role.roleLevel,
    isSystemRole: false,
  }
}

export function deleteSystemRole(id: string): Promise<void> {
  return identityHttp.delete<void>(`/api/v1/app/roles/${id}`)
}

export async function setSystemRolePermissions(id: string, permissionKeys: string[]): Promise<void> {
  await identityHttp.put<void>(`/api/v1/app/roles/${id}/permissions`, { permissions: permissionKeys })
}

export async function setSystemRolePolicies(id: string, policyCodes: string[]): Promise<void> {
  await identityHttp.put<void>(`/api/v1/app/roles/${id}/policies`, { policies: policyCodes })
}

export async function fetchPermissionCatalog(): Promise<PermissionCatalogResponse> {
  return identityHttp.get<PermissionCatalogResponse>('/api/v1/app/authorization/catalog')
}

export async function listPermissionCatalogItems(): Promise<PermissionCatalogItem[]> {
  const catalog = await fetchPermissionCatalog()
  return (catalog.permissions ?? []).map((permission) => ({
    code: permission.code,
    module: permission.module?.trim() || permission.group,
    group: permission.group,
    displayName: permission.displayName,
  }))
}

export async function listPermissionCatalog(
  usageHintFor?: (code: string) => string | null | undefined,
): Promise<PermissionChoiceGroup[]> {
  const items = await listPermissionCatalogItems()
  const groups = new Map<string, PermissionChoice[]>()
  for (const permission of items) {
    const current = groups.get(permission.group) ?? []
    current.push({
      id: permission.code,
      label: permission.displayName,
      usageHint: usageHintFor?.(permission.code),
      isSystemPermission: permission.module === 'System',
    })
    groups.set(permission.group, current)
  }

  return [...groups.entries()]
    .map(([group, permissions]) => ({
      id: group,
      label: group,
      permissions,
    }))
    .filter((group) => group.permissions.length > 0)
}
