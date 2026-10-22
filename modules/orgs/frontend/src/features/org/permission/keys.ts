/** Khớp `OrgPermissions` trên backend. */
export const ORG_PERMISSIONS = {
  view: 'Org.View',
  manage: 'Org.Manage',
} as const

export type OrgPermissionKey =
  (typeof ORG_PERMISSIONS)[keyof typeof ORG_PERMISSIONS]
