export type DynamicFormNavigateFn = (to: string) => void

let navigateImpl: DynamicFormNavigateFn | null = null

export function configureDynamicFormNavigate(fn: DynamicFormNavigateFn) {
  navigateImpl = fn
}

export function navigateDynamicForm(to: string) {
  if (navigateImpl) {
    navigateImpl(to)
    return
  }
  if (typeof window === 'undefined') return
  window.history.pushState({}, '', to)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
