import type { ReactNode } from 'react'

/** Slot: UI mặc định, hoặc ReactNode / render prop để custom */
export type AccountSlotContent<TContext> =
  | ReactNode
  | ((ctx: TContext) => ReactNode)

export function resolveAccountContent<TContext>(
  content: AccountSlotContent<TContext> | undefined,
  ctx: TContext,
  fallback: ReactNode,
): ReactNode {
  if (content == null) return fallback
  if (typeof content === 'function') {
    return (content as (ctx: TContext) => ReactNode)(ctx)
  }
  return content
}
