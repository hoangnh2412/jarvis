import type { PermissionGroup } from '../types'

const crud = (prefix: string, _label: string) => [
  { id: `${prefix}.create`, label: 'Create' },
  { id: `${prefix}.edit`, label: 'Edit' },
  { id: `${prefix}.delete`, label: 'Delete' },
  { id: `${prefix}.change_permissions`, label: 'Change permissions' },
  { id: `${prefix}.view_change_history`, label: 'View change history' },
]

/** Catalog quyền — cấu trúc nhóm + cây cha-con (demo, khớp UX ABP). */
export const PERMISSION_CATALOG: PermissionGroup[] = [
  {
    id: 'identity',
    label: 'Identity management',
    permissions: [
      {
        id: 'identity.roles',
        label: 'Role management',
        children: crud('identity.roles', 'Role management'),
      },
      {
        id: 'identity.users',
        label: 'User management',
        children: [
          ...crud('identity.users', 'User management'),
          {
            id: 'identity.users.edit.managing_roles',
            label: 'Managing roles',
          },
          {
            id: 'identity.users.edit.managing_organization_tree',
            label: 'Managing organization tree',
          },
        ],
      },
    ],
  },
  {
    id: 'feature',
    label: 'Feature management',
    permissions: [
      {
        id: 'feature.manage',
        label: 'Manage host features',
        children: [
          { id: 'feature.manage.enable', label: 'Enable' },
          { id: 'feature.manage.disable', label: 'Disable' },
        ],
      },
    ],
  },
  {
    id: 'setting',
    label: 'Setting management',
    permissions: [
      {
        id: 'setting.manage',
        label: 'Manage settings',
        children: [
          { id: 'setting.manage.view', label: 'View' },
          { id: 'setting.manage.edit', label: 'Edit' },
        ],
      },
    ],
  },
  {
    id: 'saas',
    label: 'Saas',
    permissions: [
      {
        id: 'saas.tenants',
        label: 'Tenants',
        children: crud('saas.tenants', 'Tenants'),
      },
      {
        id: 'saas.editions',
        label: 'Editions',
        children: crud('saas.editions', 'Editions'),
      },
    ],
  },
  {
    id: 'chat',
    label: 'Chat',
    permissions: [
      { id: 'chat.messaging', label: 'Messaging' },
      { id: 'chat.settings', label: 'Settings' },
    ],
  },
  {
    id: 'audit',
    label: 'Audit logging',
    permissions: [
      { id: 'audit.view', label: 'View audit logs' },
      { id: 'audit.export', label: 'Export audit logs' },
    ],
  },
  {
    id: 'openid',
    label: 'OpenId',
    permissions: [
      {
        id: 'openid.clients',
        label: 'Clients',
        children: crud('openid.clients', 'Clients'),
      },
    ],
  },
  {
    id: 'account',
    label: 'Account',
    permissions: [
      { id: 'account.profile.view', label: 'View profile' },
      { id: 'account.profile.edit', label: 'Edit profile' },
      { id: 'account.security.change_password', label: 'Change password' },
    ],
  },
  {
    id: 'language',
    label: 'Language management',
    permissions: [
      {
        id: 'language.texts',
        label: 'Language texts',
        children: crud('language.texts', 'Language texts'),
      },
    ],
  },
  {
    id: 'text_template',
    label: 'Text template management',
    permissions: [
      {
        id: 'text_template.templates',
        label: 'Templates',
        children: crud('text_template.templates', 'Templates'),
      },
    ],
  },
]
