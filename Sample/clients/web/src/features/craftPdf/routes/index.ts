import {
  CRAFT_PDF_ROUTES,
  getCraftPdfCreatePath,
  getCraftPdfEditorPath,
  getCraftPdfListPath,
  getCraftPdfPreviewPath,
} from './paths'

export { CRAFT_PDF_ROUTES } from './paths'
export type { CraftPdfRouteKey } from './paths'
export {
  getCraftPdfListPath,
  getCraftPdfCreatePath,
  getCraftPdfEditorPath,
  getCraftPdfPreviewPath,
} from './paths'

export type CraftPdfNavigateFn = (to: string) => void

let navigateFn: CraftPdfNavigateFn | null = null

export function configureCraftPdfNavigate(fn: CraftPdfNavigateFn) {
  navigateFn = fn
}

export function navigateCraftPdf(to: string) {
  if (navigateFn) {
    navigateFn(to)
    return
  }
  if (typeof window !== 'undefined') {
    window.location.assign(to)
  }
}

export type CraftPdfRouteItem = {
  key: string
  path: string
  label: string
}

export function getCraftPdfRouteList(): CraftPdfRouteItem[] {
  return [
    { key: 'list', path: CRAFT_PDF_ROUTES.list, label: 'PDF Templates' },
    { key: 'create', path: CRAFT_PDF_ROUTES.create, label: 'Create template' },
  ]
}

export const craftPdfPaths = {
  list: getCraftPdfListPath,
  create: getCraftPdfCreatePath,
  editor: getCraftPdfEditorPath,
  preview: getCraftPdfPreviewPath,
}
