import { ORG_PERMISSIONS } from '../permission'
import { ORG_ROUTES } from '../routes'

export type OrgMenuItem = {
  path: string
  label: string
  permission: string
}

export const orgMenuItems: OrgMenuItem[] = [
  {
    path: ORG_ROUTES.list,
    label: 'Organization',
    permission: ORG_PERMISSIONS.view,
  },
]
