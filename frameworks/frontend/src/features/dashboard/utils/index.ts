import type { ReactNode } from 'react'

export type DashboardSlotContent<TContext> =
  | ReactNode
  | ((ctx: TContext) => ReactNode)

export function resolveDashboardContent<TContext>(
  content: DashboardSlotContent<TContext> | undefined,
  ctx: TContext,
  fallback: ReactNode,
): ReactNode {
  if (content == null) return fallback
  if (typeof content === 'function') return content(ctx)
  return content
}

export {
  createInstanceId,
  cloneChartData,
  applyColorToData,
  createChartInstanceFromCatalog,
  patchInstanceSettings,
  loadDashboardLayout,
  saveDashboardLayout,
  clearDashboardLayout,
  toGridStackWidget,
} from './charts'

export { ensureGridstackStyles } from './ensureGridstackStyles'
export { ensureTwAnimateStyles } from './ensureTwAnimateStyles'
