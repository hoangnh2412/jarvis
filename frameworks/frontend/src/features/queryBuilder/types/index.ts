import type { Field, RuleGroupType } from 'react-querybuilder'

/** Leaf: [field, operator, value] — operator is BE name when serialized */
export type FilterAstRule = [string, string, unknown]

/** Binary tree: [left, "and"|"or", right] — BE accepts case-insensitive */
export type FilterAstNode = [FilterAst, 'and' | 'or', FilterAst]

/**
 * Nested filter AST (DevExtreme / Jarvis FilterParser).
 * Wire format: JSON.stringify(ast) → PagedListRequest.Filter
 */
export type FilterAst = FilterAstRule | FilterAstNode

/** @deprecated Prefer PagedListQueryParams.filter as JSON string */
export type FilterParams = {
  filter?: FilterAst | string
}

/** Matches Jarvis.DDD.Domain.Repositories.PagedListRequest */
export type PagedListQueryParams = {
  page: number
  size: number
  /** JSON nested-array string for FilterParser */
  filter?: string
  /** e.g. "UpdatedAt:desc,CreatedAt:asc" */
  sort?: string
  /** Comma-separated property names */
  columns?: string
}

export type QueryBuilderFieldsResult = {
  fields: Field[]
}

export const EMPTY_QUERY: RuleGroupType = {
  combinator: 'and',
  rules: [],
}

export const FILTER_MAX_DEPTH = 8
export const FILTER_MAX_CONDITIONS = 50

export type CompanyEmployeeListResult = {
  total: number
  items: unknown[]
}

export type QueryBuilderProps = {
  /** Field catalog for selects — default EMPLOYEE_ALLOWED_FIELDS */
  fields?: Field[]
  query?: RuleGroupType
  defaultQuery?: RuleGroupType
  disabled?: boolean
  className?: string
  onQueryChange?: (query: RuleGroupType) => void
  onAstChange?: (ast: FilterAst | null) => void
  onParamsChange?: (params: FilterParams) => void
  onPagedParamsChange?: (
    params: Omit<PagedListQueryParams, 'page' | 'size'>,
  ) => void
}
 