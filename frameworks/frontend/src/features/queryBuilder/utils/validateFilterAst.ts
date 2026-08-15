import type { FilterAst, FilterAstNode, FilterAstRule } from '../types'
import { FILTER_MAX_CONDITIONS, FILTER_MAX_DEPTH } from '../types'

export type FilterValidationResult =
  | { ok: true }
  | { ok: false; message: string }

function isLogic(v: unknown): v is 'and' | 'or' {
  return typeof v === 'string' && /^(and|or)$/i.test(v)
}

function isRule(ast: FilterAst): ast is FilterAstRule {
  return (
    Array.isArray(ast) &&
    ast.length === 3 &&
    typeof ast[0] === 'string' &&
    typeof ast[1] === 'string' &&
    !isLogic(ast[1])
  )
}

function isNode(ast: FilterAst): ast is FilterAstNode {
  return (
    Array.isArray(ast) &&
    ast.length === 3 &&
    isLogic(ast[1]) &&
    Array.isArray(ast[0])
  )
}

function walk(
  ast: FilterAst,
  depth: number,
  state: { conditions: number; error?: string },
): void {
  if (state.error) return
  if (depth > FILTER_MAX_DEPTH) {
    state.error = `Filter depth exceeds maximum of ${FILTER_MAX_DEPTH}.`
    return
  }
  if (isRule(ast)) {
    state.conditions += 1
    if (state.conditions > FILTER_MAX_CONDITIONS) {
      state.error = `Number of filter conditions exceeds maximum of ${FILTER_MAX_CONDITIONS}.`
    }
    return
  }
  if (!isNode(ast)) {
    state.error = 'Invalid filter AST shape.'
    return
  }
  walk(ast[0], depth + 1, state)
  walk(ast[2], depth + 1, state)
}

/** Client-side checks mirroring Jarvis FilterParser limits. */
export function validateFilterAst(
  ast: FilterAst | null | undefined,
): FilterValidationResult {
  if (ast == null) return { ok: true }
  const state: { conditions: number; error?: string } = { conditions: 0 }
  walk(ast, 0, state)
  if (state.error) return { ok: false, message: state.error }
  return { ok: true }
}
