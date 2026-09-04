import type { FilterAst, FilterAstNode, FilterAstRule } from '../types'
import { prepareEmployeesApiFilter, prepareEmployeesFilterRequest } from './employeeApiFilter'

/** Nested display fields — BE employees whitelist cannot filter these as strings. */
export const NAV_TITLE_FILTER_FIELDS = [
  'Department.Name',
  'Position.Title',
] as const

/**
 * Date fields shown in QueryBuilder filter JSON (FE).
 * Stripped from company employees API wire AST until BE supports them.
 */
export const FE_ONLY_DATE_FIELDS = ['HireDate', 'CreatedAt'] as const

export type NavTitleFilterField = (typeof NAV_TITLE_FILTER_FIELDS)[number]
export type FeOnlyDateField = (typeof FE_ONLY_DATE_FIELDS)[number]

function isNavTitleField(field: string): field is NavTitleFilterField {
  return (NAV_TITLE_FILTER_FIELDS as readonly string[]).includes(field)
}

function isFeOnlyDateField(field: string): field is FeOnlyDateField {
  return (FE_ONLY_DATE_FIELDS as readonly string[]).includes(field)
}

function isFilterRule(ast: FilterAst): ast is FilterAstRule {
  return (
    Array.isArray(ast) &&
    ast.length === 3 &&
    typeof ast[0] === 'string' &&
    typeof ast[1] === 'string' &&
    ast[1] !== 'and' &&
    ast[1] !== 'or'
  )
}

function isFilterNode(ast: FilterAst): ast is FilterAstNode {
  return (
    Array.isArray(ast) &&
    ast.length === 3 &&
    (ast[1] === 'and' || ast[1] === 'or')
  )
}

function collectNavRules(ast: FilterAst | null, out: FilterAstRule[]): void {
  if (ast == null) return
  if (isFilterRule(ast)) {
    if (isNavTitleField(ast[0])) out.push(ast)
    return
  }
  if (isFilterNode(ast)) {
    collectNavRules(ast[0], out)
    collectNavRules(ast[2], out)
  }
}

function collectDateRules(ast: FilterAst | null, out: FilterAstRule[]): void {
  if (ast == null) return
  if (isFilterRule(ast)) {
    if (isFeOnlyDateField(ast[0])) out.push(ast)
    return
  }
  if (isFilterNode(ast)) {
    collectDateRules(ast[0], out)
    collectDateRules(ast[2], out)
  }
}

/** Remove Department.Name / Position.Title leaves; keep the rest for BE. */
export function stripNavTitleFilters(ast: FilterAst | null): FilterAst | null {
  if (ast == null) return null

  if (isFilterRule(ast)) {
    return isNavTitleField(ast[0]) ? null : ast
  }

  if (!isFilterNode(ast)) return ast

  const left = stripNavTitleFilters(ast[0])
  const right = stripNavTitleFilters(ast[2])
  if (left == null) return right
  if (right == null) return left
  return [left, ast[1], right]
}

/** Remove HireDate / CreatedAt leaves — FE demo only, not on BE yet. */
export function stripFeOnlyDateFilters(
  ast: FilterAst | null,
): FilterAst | null {
  if (ast == null) return null

  if (isFilterRule(ast)) {
    return isFeOnlyDateField(ast[0]) ? null : ast
  }

  if (!isFilterNode(ast)) return ast

  const left = stripFeOnlyDateFilters(ast[0])
  const right = stripFeOnlyDateFilters(ast[2])
  if (left == null) return right
  if (right == null) return left
  return [left, ast[1], right]
}

export type PreparedEmployeeFilter = {
  /** Full UI filter AST (all QueryBuilder rules). */
  filterAst: FilterAst | null
  /** Serialized `Filter` query param — includes HireDate / CreatedAt. */
  requestAst: FilterAst | null
  /** BE-safe subset (dates stripped) — for logging / future server-side use. */
  serverAst: FilterAst | null
  /** Nav title rules (Department.Name / Position.Title) — metadata only. */
  clientRules: FilterAstRule[]
  /** Date rules (HireDate / CreatedAt) — metadata only. */
  demoDateRules: FilterAstRule[]
  /**
   * Plain text for list-custom CustomFilter (searches Department.Name /
   * Position.Title / FullName on BE without JSON AST).
   */
  freeText: string | null
}

/** Split UI filters for metadata / list-custom free-text. */
export function prepareEmployeeFilter(
  ast: FilterAst | null,
): PreparedEmployeeFilter {
  const clientRules: FilterAstRule[] = []
  const demoDateRules: FilterAstRule[] = []
  collectNavRules(ast, clientRules)
  collectDateRules(ast, demoDateRules)

  const freeTextParts = clientRules
    .map(([, , value]) => (value == null ? '' : String(value).trim()))
    .filter(Boolean)
  const freeText =
    freeTextParts.length > 0 ? freeTextParts.join(' ') : null

  return {
    filterAst: ast,
    requestAst: prepareEmployeesFilterRequest(ast),
    serverAst: prepareEmployeesApiFilter(ast),
    clientRules,
    demoDateRules,
    freeText,
  }
}

function readNavTitle(
  row: Record<string, unknown>,
  field: NavTitleFilterField,
): string {
  if (field === 'Department.Name') {
    const dept = (row.Department ?? row.department) as
      | Record<string, unknown>
      | string
      | null
      | undefined
    if (dept == null) return ''
    if (typeof dept === 'string') return dept
    return String(dept.Name ?? dept.name ?? '')
  }

  const pos = (row.Position ?? row.position) as
    | Record<string, unknown>
    | string
    | null
    | undefined
  if (pos == null) return ''
  if (typeof pos === 'string') return pos
  return String(pos.Title ?? pos.title ?? '')
}

function matchRule(row: Record<string, unknown>, rule: FilterAstRule): boolean {
  const [field, op, rawValue] = rule
  if (!isNavTitleField(field)) return true

  const left = readNavTitle(row, field)
  const leftLower = left.toLowerCase()
  const value = rawValue == null ? '' : String(rawValue)
  const valueLower = value.toLowerCase()

  switch (op.toLowerCase()) {
    case '=':
    case 'eq':
      return leftLower === valueLower
    case '!=':
    case '<>':
    case 'neq':
      return leftLower !== valueLower
    case 'contains':
      return leftLower.includes(valueLower)
    case 'notcontains':
      return !leftLower.includes(valueLower)
    case 'startswith':
      return leftLower.startsWith(valueLower)
    case 'endswith':
      return leftLower.endsWith(valueLower)
    case 'isnull':
      return left.trim() === ''
    case 'isnotnull':
      return left.trim() !== ''
    case 'in': {
      const list = Array.isArray(rawValue)
        ? rawValue.map((v) => String(v).toLowerCase())
        : valueLower.split(',').map((s) => s.trim())
      return list.includes(leftLower)
    }
    default:
      return leftLower.includes(valueLower)
  }
}

/** Apply Department.Name / Position.Title rules on employee rows (FE-only). */
export function filterEmployeesByNavTitle(
  items: unknown[],
  clientRules: FilterAstRule[],
): unknown[] {
  if (clientRules.length === 0) return items
  return items.filter((item) => {
    if (item == null || typeof item !== 'object') return false
    const row = item as Record<string, unknown>
    return clientRules.every((rule) => matchRule(row, rule))
  })
}
