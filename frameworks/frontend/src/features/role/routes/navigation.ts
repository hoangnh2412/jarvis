export type RoleNavigateFn = (to: string) => void

let navigateImpl: RoleNavigateFn | null = null

export function configureRoleNavigate(fn: RoleNavigateFn) {
  navigateImpl = fn
}

export function navigateRole(to: string) {
  if (navigateImpl) {
    navigateImpl(to)
    return
  }
  if (typeof window === 'undefined') return
  window.history.pushState({}, '', to)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
