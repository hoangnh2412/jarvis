import { POLICY_CATALOG } from '../../authorization/catalog'
import { FAKE_ROLES, PERMISSION_CATALOG } from '../constants'
import type {
  CreateRolePayload,
  GetRoleListParams,
  PermissionGroup,
  Role,
  RoleListResult,
  UpdateRolePayload,
} from '../types'
import { collectAllPermissionIds } from '../utils/permissionTree'

let seq = 100
const store: Role[] = FAKE_ROLES.map((r) => ({
  ...r,
  permissions: [...r.permissions],
  policies: [...r.policies],
}))

const delay = (ms = 120) => new Promise((r) => setTimeout(r, ms))

function cloneRole(role: Role): Role {
  return {
    ...role,
    permissions: [...role.permissions],
    policies: [...role.policies],
  }
}

function assertMutable(role: Role) {
  if (role.isSystemRole) {
    throw new Error('Không sửa vai trò hệ thống')
  }
}

export async function mockGetRoleList(
  params: GetRoleListParams = {},
): Promise<RoleListResult> {
  await delay()
  const search = params.search?.trim().toLowerCase() ?? ''
  let items = store.map(cloneRole)

  if (search) {
    items = items.filter(
      (r) =>
        r.name.toLowerCase().includes(search) ||
        r.displayName.toLowerCase().includes(search),
    )
  }

  const page = params.page ?? 1
  const size = params.size ?? 10
  const start = (page - 1) * size
  const paged = items.slice(start, start + size)

  return { items: paged, total: items.length, page, size }
}

export async function mockGetRole(id: string): Promise<Role | null> {
  await delay(80)
  const role = store.find((r) => r.id === id)
  return role ? cloneRole(role) : null
}

export async function mockCreateRole(data: CreateRolePayload): Promise<Role> {
  await delay()
  if (store.some((r) => r.name.toLowerCase() === data.name.trim().toLowerCase())) {
    throw new Error('Tên vai trò đã tồn tại')
  }
  if (data.isDefault) {
    for (const r of store) r.isDefault = false
  }
  const role: Role = {
    id: `role-${++seq}`,
    name: data.name.trim(),
    displayName: data.displayName.trim(),
    description: data.description?.trim() || null,
    isDefault: Boolean(data.isDefault),
    isPublic: Boolean(data.isPublic),
    roleLevel: data.roleLevel,
    isSystemRole: false,
    permissions: [],
    policies: [],
    createdAt: new Date().toISOString(),
  }
  store.push(role)
  return cloneRole(role)
}

export async function mockUpdateRole(
  id: string,
  data: UpdateRolePayload,
): Promise<Role> {
  await delay()
  const role = store.find((r) => r.id === id)
  if (!role) throw new Error('Không tìm thấy vai trò')
  assertMutable(role)
  if (
    store.some(
      (r) =>
        r.id !== id && r.name.toLowerCase() === data.name.trim().toLowerCase(),
    )
  ) {
    throw new Error('Tên vai trò đã tồn tại')
  }
  if (data.isDefault) {
    for (const r of store) r.isDefault = r.id === id
  }
  role.name = data.name.trim()
  role.displayName = data.displayName.trim()
  role.description = data.description?.trim() || null
  role.isDefault = Boolean(data.isDefault)
  role.isPublic = Boolean(data.isPublic)
  role.roleLevel = data.roleLevel
  role.updatedAt = new Date().toISOString()
  return cloneRole(role)
}

export async function mockDeleteRole(id: string): Promise<void> {
  await delay()
  const index = store.findIndex((r) => r.id === id)
  if (index < 0) throw new Error('Không tìm thấy vai trò')
  if (store[index]!.isSystemRole) throw new Error('Không xoá vai trò hệ thống')
  if (store[index]!.isDefault) throw new Error('Không thể xoá vai trò mặc định')
  store.splice(index, 1)
}

export async function mockUpdateRolePermissions(
  id: string,
  permissions: string[],
): Promise<Role> {
  await delay()
  const role = store.find((r) => r.id === id)
  if (!role) throw new Error('Không tìm thấy vai trò')
  assertMutable(role)
  if (permissions.some((permission) => permission === '*' || permission === 'System.All')) {
    throw new Error('Không gán wildcard cho vai trò nghiệp vụ')
  }

  const valid = new Set(collectAllPermissionIds(PERMISSION_CATALOG))
  role.permissions = permissions.filter((p) => valid.has(p))
  role.updatedAt = new Date().toISOString()
  return cloneRole(role)
}

export async function mockGetPermissionCatalog(): Promise<PermissionGroup[]> {
  await delay(60)
  return PERMISSION_CATALOG
}

export async function mockUpdateRolePolicies(
  id: string,
  policies: string[],
): Promise<Role> {
  await delay()
  const role = store.find((item) => item.id === id)
  if (!role) throw new Error('Không tìm thấy vai trò')
  assertMutable(role)
  const valid = new Set(POLICY_CATALOG.map((policy) => policy.code))
  role.policies = policies.filter((code) => valid.has(code))
  role.updatedAt = new Date().toISOString()
  return cloneRole(role)
}

/** Reset store — tiện cho demo / test */
export function mockResetRoles(): void {
  store.length = 0
  store.push(...FAKE_ROLES.map((r) => ({
    ...r,
    permissions: [...r.permissions],
    policies: [...r.policies],
  })))
}
