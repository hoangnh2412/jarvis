export const DYNAMIC_FORM_PERMISSIONS = {
  view: 'dynamicForm.view',
  edit: 'dynamicForm.edit',
  submit: 'dynamicForm.submit',
} as const

export type DynamicFormPermissionKey = keyof typeof DYNAMIC_FORM_PERMISSIONS
