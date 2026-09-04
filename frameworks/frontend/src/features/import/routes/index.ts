import { IMPORT_ROUTES } from './paths'

export type ImportNavigateFn = (to: string) => void

let navigateImpl: ImportNavigateFn | null = null

export function configureImportNavigate(fn: ImportNavigateFn) {
  navigateImpl = fn
}

export function navigateImport(to: string) {
  if (navigateImpl) {
    navigateImpl(to)
    return
  }
  if (typeof window === 'undefined') return
  window.history.pushState({}, '', to)
  window.dispatchEvent(new PopStateEvent('popstate'))
}

export {
  IMPORT_ROUTES,
  getImportPath,
  getImportListFallbackPath,
  getImportStudentPath,
  getImportStudentListFallbackPath,
} from './paths'

export function getImportRouteList() {
  return [IMPORT_ROUTES.page]
}

export const importPaths = IMPORT_ROUTES

export type { ImportRouteKey } from './paths'
