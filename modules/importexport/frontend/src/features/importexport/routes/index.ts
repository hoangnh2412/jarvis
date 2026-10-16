export const IMPORT_EXPORT_ROUTES = {
  templates: '/import-export/templates',
  templateCreate: '/import-export/templates/create',
  templateEdit: '/import-export/templates/:code/edit',
  templateConfiguration: '/import-export/templates/:code/configuration',
  templateImport: '/import-export/templates/:code/import',
} as const

export type ImportExportRouteKey = keyof typeof IMPORT_EXPORT_ROUTES

export function getImportExportTemplateListPath() {
  return IMPORT_EXPORT_ROUTES.templates
}

export function getImportExportTemplateCreatePath() {
  return IMPORT_EXPORT_ROUTES.templateCreate
}

export function getImportExportTemplateEditPath(code: string) {
  return IMPORT_EXPORT_ROUTES.templateEdit.replace(
    ':code',
    encodeURIComponent(code),
  )
}

export function getImportExportTemplateConfigurationPath(code: string) {
  return IMPORT_EXPORT_ROUTES.templateConfiguration.replace(
    ':code',
    encodeURIComponent(code),
  )
}

export function getImportExportTemplateImportPath(code: string) {
  return IMPORT_EXPORT_ROUTES.templateImport.replace(
    ':code',
    encodeURIComponent(code),
  )
}

export type ImportExportNavigateFn = (to: string) => void

let navigateFn: ImportExportNavigateFn | null = null

export function configureImportExportNavigate(fn: ImportExportNavigateFn) {
  navigateFn = fn
}

export function navigateImportExport(to: string) {
  if (navigateFn) {
    navigateFn(to)
    return
  }
  if (typeof window !== 'undefined') {
    window.location.assign(to)
  }
}

export const importExportPaths = {
  list: getImportExportTemplateListPath,
  create: getImportExportTemplateCreatePath,
  edit: getImportExportTemplateEditPath,
  configuration: getImportExportTemplateConfigurationPath,
  import: getImportExportTemplateImportPath,
}
