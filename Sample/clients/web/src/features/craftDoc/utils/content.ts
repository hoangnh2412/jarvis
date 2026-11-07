import type { ReactNode } from 'react'

export type CraftDocSlotContent<TContext> = ReactNode | ((ctx: TContext) => ReactNode)

export function resolveCraftDocContent<TContext>(
  content: CraftDocSlotContent<TContext> | undefined,
  ctx: TContext,
  fallback: ReactNode,
): ReactNode {
  if (content == null) return fallback
  if (typeof content === 'function') {
    return (content as (ctx: TContext) => ReactNode)(ctx)
  }
  return content
}
