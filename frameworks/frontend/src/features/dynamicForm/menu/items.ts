import { DYNAMIC_FORM_PERMISSIONS } from '../permission/keys'
import { DYNAMIC_FORM_ROUTES } from '../routes/paths'

export type DynamicFormMenuItem = {
  path: string
  label: string
  permission: string
}

export const dynamicFormMenuItems: DynamicFormMenuItem[] = [
  {
    path: DYNAMIC_FORM_ROUTES.page,
    label: 'Form động (RJSF)',
    permission: DYNAMIC_FORM_PERMISSIONS.view,
  },
]
