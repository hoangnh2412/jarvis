import { CRAFT_DOC_ROUTES, getCraftDocBuilderPath } from './paths'

export { CRAFT_DOC_ROUTES, getCraftDocBuilderPath } from './paths'
export type { CraftDocRouteKey } from './paths'

export type CraftDocNavigateFn = (to: string) => void

let navigateFn: CraftDocNavigateFn | null = null

export function configureCraftDocNavigate(fn: CraftDocNavigateFn) {
  navigateFn = fn
}

export function navigateCraftDoc(to: string) {
  if (navigateFn) {
    navigateFn(to)
    return
  }
  if (typeof window !== 'undefined') {
    window.location.assign(to)
  }
}

export type CraftDocRouteItem = {
  key: string
  path: string
  label: string
}

export function getCraftDocRouteList(): CraftDocRouteItem[] {
  return [{ key: 'builder', path: CRAFT_DOC_ROUTES.builder, label: 'DOCX Templates' }]
}

export const craftDocPaths = {
  builder: getCraftDocBuilderPath,
}
