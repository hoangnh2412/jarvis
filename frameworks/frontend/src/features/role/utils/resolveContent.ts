import type { ReactNode } from 'react'

export type RoleSlotContent<TContext> = ReactNode | ((ctx: TContext) => ReactNode)

export function resolveRoleContent<TContext>(
  content: RoleSlotContent<TContext> | undefined,
  ctx: TContext,
  fallback: ReactNode,
): ReactNode {
  if (content == null) return fallback
  if (typeof content === 'function') {
    return (content as (ctx: TContext) => ReactNode)(ctx)
  }
  return content
}
