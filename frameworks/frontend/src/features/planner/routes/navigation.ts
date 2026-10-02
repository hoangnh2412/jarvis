export type PlannerNavigateFn = (to: string) => void

let navigateImpl: PlannerNavigateFn | null = null

export function configurePlannerNavigate(fn: PlannerNavigateFn) {
  navigateImpl = fn
}

export function navigatePlanner(to: string) {
  if (navigateImpl) {
    navigateImpl(to)
    return
  }
  if (typeof window === 'undefined') return
  window.history.pushState({}, '', to)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
