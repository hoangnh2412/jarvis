import type { Role } from '../types'
import { collectAllPermissionIds } from '../utils/permissionTree'
import { PERMISSION_CATALOG } from './permissionCatalog'

const now = '2026-08-06T06:00:00.000Z'

const allPermissions = collectAllPermissionIds(PERMISSION_CATALOG)

/** Seed fake roles — dùng cho mock service, không gọi BE. */
export const FAKE_ROLES: Role[] = [
  {
    id: 'role-admin',
    name: 'admin',
    displayName: 'Administrator',
    description: 'Toàn quyền hệ thống',
    isDefault: false,
    isPublic: false,
    permissions: allPermissions,
    createdAt: now,
    createdBy: 'system',
  },
  {
    id: 'role-supporter',
    name: 'supporter',
    displayName: 'Supporter',
    description: 'Hỗ trợ người dùng, quản lý identity cơ bản',
    isDefault: false,
    isPublic: true,
    permissions: [
      'identity.users.create',
      'identity.users.edit',
      'identity.users.delete',
      'identity.users.change_permissions',
      'identity.users.view_change_history',
      'identity.users.edit.managing_roles',
      'identity.roles.create',
      'identity.roles.edit',
      'identity.roles.view_change_history',
      'account.profile.view',
      'account.profile.edit',
    ],
    createdAt: now,
    createdBy: 'system',
  },
  {
    id: 'role-user',
    name: 'user',
    displayName: 'User',
    description: 'Người dùng mặc định',
    isDefault: true,
    isPublic: true,
    permissions: ['account.profile.view', 'account.profile.edit'],
    createdAt: now,
    createdBy: 'system',
  },
]
