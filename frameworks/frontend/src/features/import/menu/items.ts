import { IMPORT_PERMISSIONS } from '../permission/keys'

export type ImportMenuItem = {
  path: string
  label: string
  permission: string
}

export const importMenuItems: ImportMenuItem[] = [
  {
    path: '/import',
    label: 'Import dữ liệu',
    permission: IMPORT_PERMISSIONS.view,
  },
]
