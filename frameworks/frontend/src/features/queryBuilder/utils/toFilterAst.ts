import {
  isRuleGroup,
  type Field,
  type RuleGroupType,
  type RuleType,
} from 'react-querybuilder'
import type {
  FilterAst,
  FilterAstRule,
  FilterParams,
  PagedListQueryParams,
} from '../types'
import {
  isArrayBeOperator,
  isUnaryBeOperator,
  toBeOperator,
} from './operators'
import { validateFilterAst } from './validateFilterAst'

function findField(fields: Field[] | undefined, name: string): Field | undefined {
  return fields?.find((f) => f.name === name)
}

/** Normalize RightOpt for BE (strings / string arrays / null). */
function toBeValue(value: unknown, beOperator: string): unknown {
  if (isUnaryBeOperator(beOperator)) return null

  if (isArrayBeOperator(beOperator)) {
    const raw = Array.isArray(value)
      ? value
      : typeof value === 'string'
        ? value.split(',').map((s) => s.trim())
        : value == null
          ? []
          : [value]
    if (beOperator === 'between' && raw.length !== 2) {
      // Keep as-is; BE will reject invalid between
      return raw.map((v) => (v == null ? '' : String(v)))
    }
    return raw.map((v) => (v == null ? '' : String(v)))
  }

  if (value == null) return null
  if (typeof value === 'boolean' || typeof value === 'number') return value
  return String(value)
}

function ruleToAst(rule: RuleType, fields?: Field[]): FilterAstRule | null {
  const fieldName = String(rule.field)
  // Drop rules for fields not in the active catalog (stale UI / mode switch).
  if (fields && fields.length > 0 && !findField(fields, fieldName)) {
    return null
  }
  const beOp = toBeOperator(String(rule.operator))
  return [fieldName, beOp, toBeValue(rule.value, beOp)]
}

/**
 * Convert react-querybuilder query → nested AST for Jarvis FilterParser.
 * Empty rules → `null`.
 */
export function toFilterAst(
  query: RuleGroupType,
  fields?: Field[],
): FilterAst | null {
  const parts: FilterAst[] = []

  for (const item of query.rules) {
    if (isRuleGroup(item)) {
      const nested = toFilterAst(item, fields)
      if (nested != null) parts.push(nested)
      continue
    }
    if (!item.field) continue
    const ruleAst = ruleToAst(item, fields)
    if (ruleAst != null) parts.push(ruleAst)
  }

  if (parts.length === 0) return null
  if (parts.length === 1) return parts[0]!

  const combinator: 'and' | 'or' =
    query.combinator === 'or' ? 'or' : 'and'

  let tree: FilterAst = parts[0]!
  for (let i = 1; i < parts.length; i += 1) {
    tree = [tree, combinator, parts[i]!]
  }
  return tree
}

/** JSON string for PagedListRequest.Filter */
export function toFilterJson(ast: FilterAst | null): string | undefined {
  if (ast == null) return undefined
  const check = validateFilterAst(ast)
  if (!check.ok) throw new Error(check.message)
  return JSON.stringify(ast)
}

/** @deprecated Prefer toPagedListParams — kept for compatibility */
export function toFilterParams(ast: FilterAst | null): FilterParams {
  if (ast == null) return {}
  return { filter: ast }
}

export function queryToFilterParams(
  query: RuleGroupType,
  fields?: Field[],
): FilterParams {
  return toFilterParams(toFilterAst(query, fields))
}

export type ToPagedListParamsInput = {
  page: number
  size: number
  query?: RuleGroupType
  fields?: Field[]
  /** Precomputed AST (skips query convert when set) */
  ast?: FilterAst | null
  sort?: string | null
  columns?: string | null
}

/** Build params matching Jarvis PagedListRequest. */
export function toPagedListParams(
  input: ToPagedListParamsInput,
): PagedListQueryParams {
  const ast =
    input.ast !== undefined
      ? input.ast
      : input.query
        ? toFilterAst(input.query, input.fields)
        : null

  const params: PagedListQueryParams = {
    page: input.page,
    size: input.size,
  }

  const filter = toFilterJson(ast)
  if (filter) params.filter = filter

  const sort = input.sort?.trim()
  if (sort) params.sort = sort

  const columns = input.columns?.trim()
  if (columns) params.columns = columns

  return params
}
