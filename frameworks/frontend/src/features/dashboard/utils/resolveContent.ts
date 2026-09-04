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
