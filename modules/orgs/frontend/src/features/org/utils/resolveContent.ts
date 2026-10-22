import type { ReactNode } from 'react'

export type OrgSlotContent<TContext> =
  | ReactNode
  | ((ctx: TContext) => ReactNode)

export function resolveOrgContent<TContext>(
  content: OrgSlotContent<TContext> | undefined,
  ctx: TContext,
  fallback: ReactNode,
): ReactNode {
  if (content == null) return fallback
  if (typeof content === 'function') {
    return (content as (ctx: TContext) => ReactNode)(ctx)
  }
  return content
}
