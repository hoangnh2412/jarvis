export const CRAFT_PDF_PERMISSIONS = {
  templateView: 'craftPdf.template.view',
  templateCreate: 'craftPdf.template.create',
  templateEdit: 'craftPdf.template.edit',
  templateDelete: 'craftPdf.template.delete',
  templateGenerate: 'craftPdf.template.generate',
} as const

export type CraftPdfPermissionKey = keyof typeof CRAFT_PDF_PERMISSIONS

export function hasCraftPdfPermission(
  granted: string[] | Set<string> | undefined,
  key: (typeof CRAFT_PDF_PERMISSIONS)[CraftPdfPermissionKey],
) {
  if (!granted) return true
  if (granted instanceof Set) return granted.has(key)
  return granted.includes(key)
}
