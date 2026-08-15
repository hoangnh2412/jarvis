import type { FilterAst, FilterAstNode, FilterAstRule } from '../types'

/** Date fields in QueryBuilder filter JSON — not on BE Employee / API AllowedFields yet. */
export const FE_ONLY_DATE_FIELDS = ['HireDate', 'CreatedAt'] as const

/**
 * Whitelist for API 1 — `GET .../company/employees`
 * (see Sample `CompanyController.GetEmployees` AllowedFields).
 */
export const EMPLOYEES_API_ALLOWED_FIELDS = [
  'Id',
  'FullName',
  'IsActive',
  'BaseSalary',
  'Department',
  'Position',
  'DepartmentId',
  'PositionId',
] as const

export type EmployeesApiAllowedField =
  (typeof EMPLOYEES_API_ALLOWED_FIELDS)[number]

const ALLOWED_SET = new Set<string>(
  EMPLOYEES_API_ALLOWED_FIELDS.map((f) => f.toLowerCase()),
)

const FE_ONLY_DATE_SET = new Set<string>(
  FE_ONLY_DATE_FIELDS.map((f) => f.toLowerCase()),
)

/** Legacy QueryBuilder names → wire field for API 1. */
const LEGACY_FIELD_MAP: Record<string, EmployeesApiAllowedField> = {
  'Department.Name': 'Department',
  'Position.Title': 'Position',
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

function isFeOnlyDateField(field: string): boolean {
  return FE_ONLY_DATE_SET.has(field.toLowerCase())
}

function isAllowedWireField(field: string): boolean {
  return ALLOWED_SET.has(field.toLowerCase())
}

function remapFieldName(field: string): string {
  return LEGACY_FIELD_MAP[field] ?? field
}

function remapAstFields(ast: FilterAst | null): FilterAst | null {
  if (ast == null) return null

  if (isFilterRule(ast)) {
    return [remapFieldName(ast[0]), ast[1], ast[2]]
  }

  if (!isFilterNode(ast)) return ast

  const left = remapAstFields(ast[0])
  const right = remapAstFields(ast[2])
  if (left == null) return right
  if (right == null) return left
  return [left, ast[1], right]
}

function pruneAst(
  ast: FilterAst | null,
  keep: (field: string) => boolean,
): FilterAst | null {
  if (ast == null) return null

  if (isFilterRule(ast)) {
    return keep(ast[0]) ? ast : null
  }

  if (!isFilterNode(ast)) return ast

  const left = pruneAst(ast[0], keep)
  const right = pruneAst(ast[2], keep)
  if (left == null) return right
  if (right == null) return left
  return [left, ast[1], right]
}

/**
 * AST for PagedListRequest.Filter on company `/employees`:
 * - map legacy field names
 * - keep HireDate / CreatedAt (FE filter JSON; BE may ignore until supported)
 * - drop anything outside allowed + date fields
 */
export function prepareEmployeesFilterRequest(
  ast: FilterAst | null,
): FilterAst | null {
  const remapped = remapAstFields(ast)
  return pruneAst(
    remapped,
    (field) => isAllowedWireField(field) || isFeOnlyDateField(field),
  )
}

/**
 * AST for company `/employees` API call (BE-safe subset):
 * - map legacy field names
 * - drop FE-only date fields
 * - drop anything outside BE AllowedFields
 */
export function prepareEmployeesApiFilter(ast: FilterAst | null): FilterAst | null {
  const remapped = remapAstFields(ast)
  return pruneAst(
    remapped,
    (field) => !isFeOnlyDateField(field) && isAllowedWireField(field),
  )
}
