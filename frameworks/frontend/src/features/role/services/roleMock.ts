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
const store: Role[] = FAKE_ROLES.map((r) => ({ ...r, permissions: [...r.permissions] }))

const delay = (ms = 120) => new Promise((r) => setTimeout(r, ms))

function cloneRole(role: Role): Role {
  return { ...role, permissions: [...role.permissions] }
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
    permissions: [],
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
  role.updatedAt = new Date().toISOString()
  return cloneRole(role)
}

export async function mockDeleteRole(id: string): Promise<void> {
  await delay()
  const index = store.findIndex((r) => r.id === id)
  if (index < 0) throw new Error('Không tìm thấy vai trò')
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

  const valid = new Set(collectAllPermissionIds(PERMISSION_CATALOG))
  role.permissions = permissions.filter((p) => valid.has(p))
  role.updatedAt = new Date().toISOString()
  return cloneRole(role)
}

export async function mockGetPermissionCatalog(): Promise<PermissionGroup[]> {
  await delay(60)
  return PERMISSION_CATALOG
}

/** Reset store — tiện cho demo / test */
export function mockResetRoles(): void {
  store.length = 0
  store.push(...FAKE_ROLES.map((r) => ({ ...r, permissions: [...r.permissions] })))
}
