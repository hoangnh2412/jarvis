/** Id form seed demo (localStorage / mock JSON). */
export const DYNAMIC_FORM_DEMO_ID = 'form-001'

export const DYNAMIC_FORM_ROUTES = {
  page: '/dynamic-forms',
} as const

export type DynamicFormRouteKey = keyof typeof DYNAMIC_FORM_ROUTES

export function getDynamicFormPagePath() {
  return DYNAMIC_FORM_ROUTES.page
}

/** @deprecated Dùng `getDynamicFormPagePath` — URL không còn `:id` */
export function getDynamicFormBuilderPath(_id?: string) {
  return DYNAMIC_FORM_ROUTES.page
}

export function getDynamicFormDemoPath() {
  return DYNAMIC_FORM_ROUTES.page
}

export function getDynamicFormRouteList() {
  return [DYNAMIC_FORM_ROUTES.page]
}

export const dynamicFormPaths = DYNAMIC_FORM_ROUTES
