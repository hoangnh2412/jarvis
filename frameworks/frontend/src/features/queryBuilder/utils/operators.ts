/** Operators accepted by Jarvis.DDD.Domain FilterParser (lowercase). */
export const BE_FILTER_OPERATORS = [
  '=',
  '!=',
  '<>',
  '>',
  '<',
  '>=',
  '<=',
  'contains',
  'notcontains',
  'startswith',
  'endswith',
  'between',
  'in',
  'isnull',
  'isnotnull',
] as const

export type BeFilterOperator = (typeof BE_FILTER_OPERATORS)[number]

/** RQB UI operator → BE FilterParser operator */
const RQB_TO_BE: Record<string, BeFilterOperator> = {
  '=': '=',
  '!=': '!=',
  '<>': '<>',
  '>': '>',
  '<': '<',
  '>=': '>=',
  '<=': '<=',
  contains: 'contains',
  doesNotContain: 'notcontains',
  notcontains: 'notcontains',
  beginsWith: 'startswith',
  startswith: 'startswith',
  endsWith: 'endswith',
  endswith: 'endswith',
  between: 'between',
  in: 'in',
  null: 'isnull',
  isnull: 'isnull',
  notNull: 'isnotnull',
  isnotnull: 'isnotnull',
}

/** BE operator → RQB UI operator (for hydrate) */
const BE_TO_RQB: Record<string, string> = {
  '=': '=',
  '!=': '!=',
  '<>': '!=',
  '>': '>',
  '<': '<',
  '>=': '>=',
  '<=': '<=',
  contains: 'contains',
  notcontains: 'doesNotContain',
  startswith: 'beginsWith',
  endswith: 'endsWith',
  between: 'between',
  in: 'in',
  isnull: 'null',
  isnotnull: 'notNull',
}

export function toBeOperator(rqbOperator: string): BeFilterOperator {
  const key = rqbOperator.trim()
  const mapped = RQB_TO_BE[key] ?? RQB_TO_BE[key.toLowerCase()]
  if (!mapped) {
    throw new Error(`Unsupported filter operator '${rqbOperator}'.`)
  }
  return mapped
}

export function toRqbOperator(beOperator: string): string {
  const key = beOperator.trim().toLowerCase()
  return BE_TO_RQB[key] ?? beOperator
}

export function isUnaryBeOperator(op: string): boolean {
  const be = op.toLowerCase()
  return be === 'isnull' || be === 'isnotnull'
}

export function isArrayBeOperator(op: string): boolean {
  const be = op.toLowerCase()
  return be === 'between' || be === 'in'
}

/** Default operator options for string fields (RQB names). */
export const STRING_OPERATORS = [
  { name: '=', label: '=' },
  { name: '!=', label: '!=' },
  { name: 'contains', label: 'contains' },
  { name: 'doesNotContain', label: 'not contains' },
  { name: 'beginsWith', label: 'starts with' },
  { name: 'endsWith', label: 'ends with' },
  { name: 'in', label: 'in' },
  { name: 'null', label: 'is null' },
  { name: 'notNull', label: 'is not null' },
]

export const NUMBER_OPERATORS = [
  { name: '=', label: '=' },
  { name: '!=', label: '!=' },
  { name: '>', label: '>' },
  { name: '<', label: '<' },
  { name: '>=', label: '>=' },
  { name: '<=', label: '<=' },
  { name: 'between', label: 'between' },
  { name: 'in', label: 'in' },
  { name: 'null', label: 'is null' },
  { name: 'notNull', label: 'is not null' },
]

export const BOOLEAN_OPERATORS = [
  { name: '=', label: '=' },
  { name: '!=', label: '!=' },
  { name: 'null', label: 'is null' },
  { name: 'notNull', label: 'is not null' },
]

/** Date / DateTime fields — comparison + between (ISO strings for FilterParser). */
export const DATE_OPERATORS = [
  { name: '=', label: '=' },
  { name: '!=', label: '!=' },
  { name: '>', label: '>' },
  { name: '<', label: '<' },
  { name: '>=', label: '>=' },
  { name: '<=', label: '<=' },
  { name: 'between', label: 'between' },
  { name: 'null', label: 'is null' },
  { name: 'notNull', label: 'is not null' },
]
