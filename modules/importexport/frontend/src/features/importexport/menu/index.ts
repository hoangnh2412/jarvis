import { IMPORT_EXPORT_ROUTES } from '../routes'
import { IMPORT_EXPORT_PERMISSIONS } from '../constants'

export type ImportExportMenuItem = {
  path: string
  label: string
  permission: string
}

export const importExportMenuItems: ImportExportMenuItem[] = [
  {
    path: IMPORT_EXPORT_ROUTES.templates,
    label: 'Excel templates',
    permission: IMPORT_EXPORT_PERMISSIONS.templateView,
  },
]
