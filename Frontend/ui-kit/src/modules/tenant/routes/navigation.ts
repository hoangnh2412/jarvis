export type TenantNavigateFn = (to: string) => void

let navigateImpl: TenantNavigateFn | null = null

/**
 * Gắn navigate của app (vd. react-router `useNavigate`) một lần ở root.
 * Không gọi thì dùng `history.pushState` + `popstate`.
 */
export function configureTenantNavigate(fn: TenantNavigateFn) {
  navigateImpl = fn
}

export function navigateTenant(to: string) {
  if (navigateImpl) {
    navigateImpl(to)
    return
  }
  if (typeof window === 'undefined') return
  window.history.pushState({}, '', to)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
