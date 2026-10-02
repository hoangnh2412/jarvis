import { DYNAMIC_FORM_PERMISSIONS } from './keys'

export function hasDynamicFormPermission(
  permissions: readonly string[] | undefined,
  key: string,
): boolean {
  if (!permissions?.length) return true
  return (
    permissions.includes(key) ||
    permissions.includes(DYNAMIC_FORM_PERMISSIONS.view)
  )
}

export { DYNAMIC_FORM_PERMISSIONS } from './keys'
export type { DynamicFormPermissionKey } from './keys'
