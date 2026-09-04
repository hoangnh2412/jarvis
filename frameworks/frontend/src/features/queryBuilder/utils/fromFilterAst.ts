import { generateID, type RuleGroupType, type RuleType } from 'react-querybuilder'
import type { FilterAst, FilterAstNode, FilterAstRule } from '../types'
import { EMPTY_QUERY } from '../types'
import { isUnaryBeOperator, toRqbOperator } from './operators'

function isLogic(v: unknown): v is 'and' | 'or' {
  return typeof v === 'string' && /^(and|or)$/i.test(v)
}

function isRuleTuple(ast: FilterAst): ast is FilterAstRule {
  return (
    Array.isArray(ast) &&
    ast.length === 3 &&
    typeof ast[0] === 'string' &&
    typeof ast[1] === 'string' &&
    !isLogic(ast[1])
  )
}

function isNodeTuple(ast: FilterAst): ast is FilterAstNode {
  return (
    Array.isArray(ast) &&
    ast.length === 3 &&
    isLogic(ast[1]) &&
    Array.isArray(ast[0])
  )
}

function normalizeLogic(op: string): 'and' | 'or' {
  return op.toLowerCase() === 'or' ? 'or' : 'and'
}

function astToRule(ast: FilterAstRule): RuleType {
  const beOp = String(ast[1])
  const rqbOp = toRqbOperator(beOp)
  let value: unknown = ast[2]
  if (isUnaryBeOperator(beOp)) {
    value = null
  } else if (Array.isArray(value)) {
    // between / in — RQB often uses comma-separated or array
    value =
      beOp.toLowerCase() === 'between' && value.length === 2
        ? value
        : beOp.toLowerCase() === 'in'
          ? value.map((v) => String(v ?? ''))
          : value
  }

  return {
    id: generateID(),
    field: ast[0],
    operator: rqbOp,
    value,
  }
}

function collectSameCombinator(
  ast: FilterAst,
  combinator: 'and' | 'or',
): RuleGroupType['rules'] {
  if (isRuleTuple(ast)) return [astToRule(ast)]
  if (!isNodeTuple(ast)) return []
  const nodeLogic = normalizeLogic(ast[1])
  if (nodeLogic !== combinator) {
    return [fromFilterAst(ast)]
  }
  return [
    ...collectSameCombinator(ast[0], combinator),
    ...collectSameCombinator(ast[2], combinator),
  ]
}

/**
 * Hydrate RQB query from filter AST or JSON string (PagedListRequest.Filter).
 */
export function fromFilterAst(
  ast: FilterAst | string | null | undefined,
): RuleGroupType {
  const parsed = parseFilterInput(ast)
  if (parsed == null) return { ...EMPTY_QUERY, rules: [] }

  if (isRuleTuple(parsed)) {
    return {
      id: generateID(),
      combinator: 'and',
      rules: [astToRule(parsed)],
    }
  }

  if (!isNodeTuple(parsed)) {
    return { ...EMPTY_QUERY, rules: [] }
  }

  const combinator = normalizeLogic(parsed[1])
  return {
    id: generateID(),
    combinator,
    rules: collectSameCombinator(parsed, combinator),
  }
}

/** Parse Filter wire value (JSON string or already-parsed AST). */
export function parseFilterInput(
  input: FilterAst | string | null | undefined,
): FilterAst | null {
  if (input == null || input === '') return null
  if (typeof input === 'string') {
    try {
      const parsed = JSON.parse(input) as unknown
      if (!Array.isArray(parsed)) return null
      return parsed as FilterAst
    } catch {
      return null
    }
  }
  if (!Array.isArray(input)) return null
  return input
}
