export type OrgNavigateFn = (to: string) => void

let navigateImpl: OrgNavigateFn | null = null

export function configureOrgNavigate(fn: OrgNavigateFn) {
  navigateImpl = fn
}

export function navigateOrg(to: string) {
  if (navigateImpl) {
    navigateImpl(to)
    return
  }
  if (typeof window === 'undefined') return
  window.history.pushState({}, '', to)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
