export type PermissionNavigateFn = (to: string) => void

let navigateImpl: PermissionNavigateFn | null = null

export function configurePermissionNavigate(fn: PermissionNavigateFn) {
  navigateImpl = fn
}

export function navigatePermission(to: string) {
  if (navigateImpl) {
    navigateImpl(to)
    return
  }
  if (typeof window === 'undefined') return
  window.history.pushState({}, '', to)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
