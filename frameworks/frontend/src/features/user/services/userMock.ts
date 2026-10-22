import { mockGetRoleList } from '../../role/services/roleMock'
import type {
  AdminUser,
  CreateUserPayload,
  GetUserListParams,
  UpdateUserPayload,
  UserListResult,
  UserRoleOption,
} from '../types'

const delay = (ms = 120) => new Promise((resolve) => setTimeout(resolve, ms))

let seq = 20
const store: AdminUser[] = [
  {
    id: 'user-admin',
    username: 'admin',
    email: 'admin@example.com',
    isLocked: false,
    roleId: 'role-admin',
    roleName: 'admin',
    tenantId: null,
    orgId: null,
    createdAt: '2026-08-06T06:00:00.000Z',
  },
  {
    id: 'user-director',
    username: 'director',
    email: 'director@example.com',
    isLocked: false,
    roleId: 'role-director',
    roleName: 'DIRECTOR',
    tenantId: 'tenant-a',
    orgId: 'org-a',
    createdAt: '2026-09-01T02:00:00.000Z',
  },
  {
    id: 'user-employee',
    username: 'employee',
    email: 'employee@example.com',
    isLocked: false,
    roleId: 'role-employee',
    roleName: 'EMPLOYEE',
    tenantId: 'tenant-a',
    orgId: 'org-a',
    createdAt: '2026-09-02T02:00:00.000Z',
  },
]

function cloneUser(user: AdminUser): AdminUser {
  return { ...user }
}

export async function mockGetUserRoleOptions(): Promise<UserRoleOption[]> {
  const result = await mockGetRoleList({ page: 1, size: 200 })
  return result.items.map((role) => ({
    id: role.id,
    name: role.name,
    displayName: role.displayName,
    roleLevel: role.roleLevel,
    isSystemRole: role.isSystemRole,
  }))
}

export async function mockGetUserList(
  params: GetUserListParams = {},
): Promise<UserListResult> {
  await delay()
  const search = params.search?.trim().toLowerCase() ?? ''
  let items = store.map(cloneUser)
  if (search) {
    items = items.filter(
      (user) =>
        user.username.toLowerCase().includes(search) ||
        user.email.toLowerCase().includes(search),
    )
  }
  const page = params.page ?? 1
  const size = params.size ?? 10
  const start = (page - 1) * size
  return {
    items: items.slice(start, start + size),
    total: items.length,
    page,
    size,
  }
}

export async function mockGetUser(id: string): Promise<AdminUser | null> {
  await delay(80)
  const user = store.find((item) => item.id === id)
  return user ? cloneUser(user) : null
}

export async function mockCreateUser(
  data: CreateUserPayload,
): Promise<AdminUser> {
  await delay()
  const username = data.username.trim()
  const email = data.email.trim().toLowerCase()
  if (store.some((user) => user.username.toLowerCase() === username.toLowerCase())) {
    throw new Error('Tên đăng nhập đã tồn tại')
  }
  if (store.some((user) => user.email.toLowerCase() === email)) {
    throw new Error('Email đã tồn tại')
  }
  const roles = await mockGetUserRoleOptions()
  const role = roles.find((item) => item.id === data.roleId)
  if (!role) throw new Error('Không tìm thấy vai trò')

  const user: AdminUser = {
    id: `user-${++seq}`,
    username,
    email,
    isLocked: false,
    roleId: role.id,
    roleName: role.name,
    tenantId: data.tenantId?.trim() || null,
    orgId: data.orgId?.trim() || null,
    createdAt: new Date().toISOString(),
  }
  store.push(user)
  return cloneUser(user)
}

export async function mockUpdateUser(
  id: string,
  data: UpdateUserPayload,
): Promise<AdminUser> {
  await delay()
  const user = store.find((item) => item.id === id)
  if (!user) throw new Error('Không tìm thấy người dùng')
  const roles = await mockGetUserRoleOptions()
  const role = roles.find((item) => item.id === data.roleId)
  if (!role) throw new Error('Không tìm thấy vai trò')
  user.isLocked = data.isLocked
  user.roleId = role.id
  user.roleName = role.name
  return cloneUser(user)
}

export async function mockDeleteUser(id: string): Promise<void> {
  await delay()
  const index = store.findIndex((item) => item.id === id)
  if (index < 0) throw new Error('Không tìm thấy người dùng')
  store.splice(index, 1)
}
