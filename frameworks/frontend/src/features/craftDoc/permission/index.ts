export const CRAFT_DOC_PERMISSIONS = {
  templateView: 'craft-doc.template.view',
  templateEdit: 'craft-doc.template.edit',
} as const

export type CraftDocPermissionKey = keyof typeof CRAFT_DOC_PERMISSIONS

export function hasCraftDocPermission(
  permissions: string[] | undefined,
  key: CraftDocPermissionKey,
): boolean {
  if (!permissions?.length) return true
  return permissions.includes(CRAFT_DOC_PERMISSIONS[key])
}
