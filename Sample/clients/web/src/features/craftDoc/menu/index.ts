import { CRAFT_DOC_ROUTES } from '../routes/paths'
import { CRAFT_DOC_PERMISSIONS } from '../permission'

export type CraftDocMenuItem = {
  path: string
  label: string
  permission: string
}

export const craftDocMenuItems: CraftDocMenuItem[] = [
  {
    path: CRAFT_DOC_ROUTES.builder,
    label: 'Craft DOCX',
    permission: CRAFT_DOC_PERMISSIONS.templateView,
  },
]
