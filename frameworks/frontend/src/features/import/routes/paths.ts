export const IMPORT_ROUTES = {
  page: '/import',
  /** @deprecated Dùng `IMPORT_ROUTES.page` */
  student: '/import',
  listFallback: '/',
} as const

export type ImportRouteKey = keyof typeof IMPORT_ROUTES

export function getImportPath() {
  return IMPORT_ROUTES.page
}

/** @deprecated Dùng `getImportPath` */
export const getImportStudentPath = getImportPath

export function getImportListFallbackPath() {
  return IMPORT_ROUTES.listFallback
}

/** @deprecated Dùng `getImportListFallbackPath` */
export const getImportStudentListFallbackPath = getImportListFallbackPath
