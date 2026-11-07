import { CRAFT_PDF_ROUTES } from '../routes/paths'
import { CRAFT_PDF_PERMISSIONS } from '../permission'

export type CraftPdfMenuItem = {
  path: string
  label: string
  permission: string
}

export const craftPdfMenuItems: CraftPdfMenuItem[] = [
  {
    path: CRAFT_PDF_ROUTES.list,
    label: 'Craft PDF',
    permission: CRAFT_PDF_PERMISSIONS.templateView,
  },
]
