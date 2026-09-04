export const IMPORT_PERMISSIONS = {
  view: 'import.view',
  validate: 'import.validate',
  commit: 'import.commit',
} as const

export type ImportPermissionKey =
  (typeof IMPORT_PERMISSIONS)[keyof typeof IMPORT_PERMISSIONS]
