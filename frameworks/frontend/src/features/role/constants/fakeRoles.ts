import roleListMock from '../mocks/get-role-list.json'
import type { Role } from '../types'

type RoleJson = {
  id: string
  name: string
  displayName: string
  description?: string | null
  isDefault: boolean
  isPublic: boolean
  permissions: string[]
  createdAt: string
  createdBy?: string | null
  updatedAt?: string | null
  updatedBy?: string | null
  roleLevel?: number
  isSystemRole?: boolean
  policies?: string[]
}

function withGovernance(role: RoleJson): Role {
  const system = role.isSystemRole ?? role.name.toLowerCase() === 'admin'
  const permissions = [...role.permissions]
  if (role.name.toLowerCase() === 'admin' && !permissions.includes('System.All')) {
    permissions.unshift('System.All')
  }
  return {
    ...role,
    permissions,
    policies: [...(role.policies ?? (system ? [] : []))],
    isSystemRole: system,
    roleLevel: role.roleLevel ?? (role.name.toLowerCase() === 'admin' ? 1000 : 100),
  }
}

const platformRoles: Role[] = [
  {
    id: 'role-tenant-admin',
    name: 'TENANT_ADMIN',
    displayName: 'TenantAdmin',
    description: 'Quản trị trong một tenant',
    isDefault: false,
    isPublic: false,
    roleLevel: 900,
    isSystemRole: true,
    permissions: ['User.Manage', 'Role.Manage'],
    policies: ['P_SAME_TENANT'],
    createdAt: '2026-08-06T06:00:00.000Z',
    createdBy: 'system',
  },
  {
    id: 'role-org-admin',
    name: 'ORG_ADMIN',
    displayName: 'OrgAdmin',
    description: 'Quản trị trong một tổ chức',
    isDefault: false,
    isPublic: false,
    roleLevel: 800,
    isSystemRole: true,
    permissions: ['User.Manage', 'Role.Manage', 'Role.AssignPermission', 'Role.AssignPolicy'],
    policies: ['P_SAME_ORG'],
    createdAt: '2026-08-06T06:00:00.000Z',
    createdBy: 'system',
  },
  {
    id: 'role-director',
    name: 'DIRECTOR',
    displayName: 'Director',
    description: 'Duyệt phiếu chi trong tổ chức, trong hạn mức',
    isDefault: false,
    isPublic: true,
    roleLevel: 700,
    isSystemRole: false,
    permissions: ['Expense.View', 'Expense.Approve'],
    policies: ['P_SAME_ORG', 'P_APPROVE_LIMIT'],
    createdAt: '2026-09-01T02:00:00.000Z',
    createdBy: 'system',
  },
  {
    id: 'role-employee',
    name: 'EMPLOYEE',
    displayName: 'Employee',
    description: 'Xem dữ liệu của chính mình',
    isDefault: false,
    isPublic: true,
    roleLevel: 100,
    isSystemRole: false,
    permissions: ['Expense.View'],
    policies: ['P_OWNER_ONLY'],
    createdAt: '2026-09-02T02:00:00.000Z',
    createdBy: 'system',
  },
]

/** Seed fake roles — nguồn: `mocks/get-role-list.json` + role nền tảng. */
export const FAKE_ROLES: Role[] = [
  ...(roleListMock.items as RoleJson[]).map(withGovernance),
  ...platformRoles,
]
