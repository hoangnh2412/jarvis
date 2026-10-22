export type UserNavigateFn = (to: string) => void

let navigateImpl: UserNavigateFn | null = null

export function configureUserNavigate(fn: UserNavigateFn) {
  navigateImpl = fn
}

export function navigateUser(to: string) {
  if (navigateImpl) {
    navigateImpl(to)
    return
  }
  if (typeof window === 'undefined') return
  window.history.pushState({}, '', to)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
