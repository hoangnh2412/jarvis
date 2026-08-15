import { useCallback, useEffect, useRef, useState, startTransition } from 'react'
import { isRuleGroup, type Field, type RuleGroupType } from 'react-querybuilder'
import { getErrorMessage } from '../../../lib'
import { notify } from '../../../common/Toaster'
import {
  callGetQueryBuilderFields,
  type QueryBuilderFieldsMode,
} from '../constants'
import type { FilterAst, FilterParams, PagedListQueryParams } from '../types'
import { EMPTY_QUERY } from '../types'
import {
  fromFilterAst,
  toFilterAst,
  toFilterJson,
  toFilterParams,
  validateFilterAst,
  type FilterValidationResult,
} from '../utils'

/** Keep only rules whose field is still in the active catalog. */
function pruneQueryToFields(
  query: RuleGroupType,
  fields: Field[],
): RuleGroupType {
  if (fields.length === 0) return query
  const allowed = new Set(fields.map((f) => f.name))

  const pruneRules = (group: RuleGroupType): RuleGroupType => {
    const nextRules: RuleGroupType['rules'] = []
    for (const item of group.rules) {
      if (isRuleGroup(item)) {
        const nested = pruneRules(item)
        if (nested.rules.length > 0) nextRules.push(nested)
        continue
      }
      if (item.field && allowed.has(String(item.field))) {
        nextRules.push(item)
      }
    }
    return { ...group, rules: nextRules }
  }

  return pruneRules(query)
}

export type UseQueryBuilderStateOptions = {
  /** Prefill from AST or JSON string (`PagedListRequest.Filter`) */
  initialAst?: FilterAst | string | null
  /** Auto-load field catalog on mount */
  loadFields?: boolean
  /** Whitelist field set — company APIs or demo */
  fieldsMode?: QueryBuilderFieldsMode
  initialSort?: string
  initialColumns?: string
}

/** Snapshot derived from the current draft query — compute only when applying. */
export type QueryBuilderDerived = {
  ast: FilterAst | null
  validation: FilterValidationResult
  filterJson: string | undefined
  params: FilterParams
  filterWire: Pick<PagedListQueryParams, 'filter' | 'sort' | 'columns'>
}

export type QueryBuilderState = {
  fields: Field[]
  loadingFields: boolean
  query: RuleGroupType
  setQuery: (query: RuleGroupType) => void
  /**
   * Convert / validate draft query → AST + wire params.
   * Call on Apply (not on every keystroke).
   */
  deriveFilter: () => QueryBuilderDerived
  /** @deprecated Prefer `deriveFilter()` — kept for compat; same as deriveFilter().ast */
  ast: FilterAst | null
  /** @deprecated Prefer `deriveFilter()` */
  filterJson: string | undefined
  /** @deprecated Prefer `deriveFilter()` */
  params: FilterParams
  /** @deprecated Prefer `deriveFilter()` */
  filterWire: Pick<PagedListQueryParams, 'filter' | 'sort' | 'columns'>
  /** @deprecated Prefer `deriveFilter()` */
  validation: FilterValidationResult
  sort: string
  setSort: (sort: string) => void
  columns: string
  setColumns: (columns: string) => void
  reloadFields: () => Promise<void>
  applyAst: (ast: FilterAst | string | null) => void
  reset: () => void
}

function buildDerived(
  query: RuleGroupType,
  fields: Field[],
  sort: string,
  columns: string,
): QueryBuilderDerived {
  const ast = toFilterAst(query, fields)
  const validation = validateFilterAst(ast)
  let filterJson: string | undefined
  if (validation.ok) {
    try {
      filterJson = toFilterJson(ast)
    } catch {
      filterJson = undefined
    }
  }
  const filterWire: Pick<PagedListQueryParams, 'filter' | 'sort' | 'columns'> =
    {}
  if (filterJson) filterWire.filter = filterJson
  const s = sort.trim()
  if (s) filterWire.sort = s
  const c = columns.trim()
  if (c) filterWire.columns = c
  return {
    ast,
    validation,
    filterJson,
    params: filterJson ? { filter: filterJson } : toFilterParams(null),
    filterWire,
  }
}

export function useQueryBuilderState(
  options: UseQueryBuilderStateOptions = {},
): QueryBuilderState {
  const {
    initialAst = null,
    loadFields = true,
    fieldsMode = 'employees',
    initialSort = '',
    initialColumns = '',
  } = options
  const [fields, setFields] = useState<Field[]>([])
  const [loadingFields, setLoadingFields] = useState(false)
  const [query, setQueryState] = useState<RuleGroupType>(() =>
    fromFilterAst(initialAst),
  )
  const [sort, setSortState] = useState(initialSort)
  const [columns, setColumnsState] = useState(initialColumns)
  /** Always latest draft — safe for Apply while parent re-render is deferred. */
  const queryRef = useRef(query)
  const sortRef = useRef(sort)
  const columnsRef = useRef(columns)
  const fieldsRef = useRef(fields)
  queryRef.current = query
  sortRef.current = sort
  columnsRef.current = columns
  fieldsRef.current = fields

  const reloadFields = useCallback(async () => {
    setLoadingFields(true)
    try {
      const result = await callGetQueryBuilderFields(fieldsMode)
      setFields(result.fields)
      // Drop stale rules (e.g. Department.Name) not in the new catalog.
      setQueryState((prev) => {
        const next = pruneQueryToFields(prev, result.fields)
        queryRef.current = next
        return next
      })
    } catch (error) {
      notify.error(getErrorMessage(error, 'Không tải được danh sách field'))
    } finally {
      setLoadingFields(false)
    }
  }, [fieldsMode])

  useEffect(() => {
    if (loadFields) void reloadFields()
  }, [loadFields, reloadFields])

  const setQuery = useCallback((next: RuleGroupType) => {
    queryRef.current = next
    // Defer page re-render (table/toolbar); Apply still reads queryRef.
    startTransition(() => {
      setQueryState(next)
    })
  }, [])

  const applyAst = useCallback((ast: FilterAst | string | null) => {
    const next = fromFilterAst(ast)
    queryRef.current = next
    setQueryState(next)
  }, [])

  const reset = useCallback(() => {
    const next = { ...EMPTY_QUERY, rules: [] as RuleGroupType['rules'] }
    queryRef.current = next
    setQueryState(next)
    sortRef.current = ''
    columnsRef.current = ''
    setSortState('')
    setColumnsState('')
  }, [])

  const setSort = useCallback((next: string) => {
    sortRef.current = next
    setSortState(next)
  }, [])

  const setColumns = useCallback((next: string) => {
    columnsRef.current = next
    setColumnsState(next)
  }, [])

  const deriveFilter = useCallback(
    () =>
      buildDerived(
        queryRef.current,
        fieldsRef.current,
        sortRef.current,
        columnsRef.current,
      ),
    [],
  )

  // Lazy getters — no AST/JSON work until something reads these (e.g. Apply).
  return {
    fields,
    loadingFields,
    query,
    setQuery,
    deriveFilter,
    get ast() {
      return deriveFilter().ast
    },
    get filterJson() {
      return deriveFilter().filterJson
    },
    get params() {
      return deriveFilter().params
    },
    get filterWire() {
      return deriveFilter().filterWire
    },
    get validation() {
      return deriveFilter().validation
    },
    sort,
    setSort,
    columns,
    setColumns,
    reloadFields,
    applyAst,
    reset,
  }
}
