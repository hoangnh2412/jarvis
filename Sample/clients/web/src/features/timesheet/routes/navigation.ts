export type TimesheetNavigateFn = (to: string) => void

let navigateImpl: TimesheetNavigateFn | null = null

export function configureTimesheetNavigate(fn: TimesheetNavigateFn) {
  navigateImpl = fn
}

export function navigateTimesheet(to: string) {
  if (navigateImpl) {
    navigateImpl(to)
    return
  }
  if (typeof window === 'undefined') return
  window.history.pushState({}, '', to)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
