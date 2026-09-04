import { memo, startTransition, useEffect, useMemo, useRef, useState } from 'react'
import { combine } from '@atlaskit/pragmatic-drag-and-drop/combine'
import {
  draggable,
  dropTargetForElements,
  monitorForElements,
} from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { QueryBuilderDnD } from '@react-querybuilder/dnd'
import { createPragmaticDndAdapter } from '@react-querybuilder/dnd/pragmatic-dnd'
import {
  QueryBuilder as RqbQueryBuilder,
  type RuleGroupType,
} from 'react-querybuilder'
import { EMPLOYEE_ALLOWED_FIELDS, translations } from '../constants'
import { EMPTY_QUERY, type QueryBuilderProps } from '../types'
import {
  toFilterAst,
  toFilterJson,
  toFilterParams,
  validateFilterAst,
} from '../utils'
import { primeReact11ControlElements } from './controlElements'

export type { QueryBuilderProps }

const dndAdapter = createPragmaticDndAdapter({
  draggable,
  dropTargetForElements,
  monitorForElements,
  combine,
})

const rqbShell =
  'box-border w-full rounded-2xl border border-slate-200 bg-gradient-to-b from-slate-50 to-white p-3.5 shadow-[0_1px_2px_rgba(15,23,42,0.04)]'

const rqbGroup =
  'relative flex flex-col gap-3 rounded-[14px] border border-slate-200 bg-white p-3 shadow-[0_1px_2px_rgba(15,23,42,0.03)] before:absolute before:bottom-2.5 before:left-0 before:top-2.5 before:w-[3px] before:rounded-full before:bg-gradient-to-b before:from-teal-400 before:to-teal-700 before:opacity-85 before:content-[""] [&_.ruleGroup]:ml-1 [&_.ruleGroup]:bg-slate-50 [&.dndDragging]:opacity-45 [&.dndOver]:outline [&.dndOver]:outline-2 [&.dndOver]:outline-dashed [&.dndOver]:outline-teal-400 [&.dndOver]:outline-offset-2'

const rqbRule =
  'flex flex-wrap items-center gap-2 rounded-xl border border-[#eef2f7] bg-gradient-to-b from-white to-[#fbfdff] px-3 py-2.5 shadow-[0_1px_1px_rgba(15,23,42,0.02)] transition-[border-color,box-shadow] hover:border-[#dbe3ee] hover:shadow-[0_2px_8px_rgba(15,23,42,0.04)] [&.dndDragging]:opacity-45 [&.dndOver]:outline [&.dndOver]:outline-2 [&.dndOver]:outline-dashed [&.dndOver]:outline-teal-400 [&.dndOver]:outline-offset-2 [&.dropAbove]:before:my-0.5 [&.dropAbove]:before:block [&.dropAbove]:before:h-[3px] [&.dropAbove]:before:rounded-full [&.dropAbove]:before:bg-teal-600 [&.dropAbove]:before:content-[""] [&.dropBelow]:after:my-0.5 [&.dropBelow]:after:block [&.dropBelow]:after:h-[3px] [&.dropBelow]:after:rounded-full [&.dropBelow]:after:bg-teal-600 [&.dropBelow]:after:content-[""]'

const rqbBetween =
  'relative flex w-full items-center justify-start py-0.5 pl-[18px] pr-0 before:absolute before:bottom-[-6px] before:left-2 before:top-[-6px] before:w-px before:bg-gradient-to-b before:from-transparent before:via-slate-300 before:to-transparent before:content-[""] [&>[data-scope=select]]:!w-fit'

/**
 * Query builder UI — DnD theo pattern thư viện (`QueryBuilderDnD` + Pragmatic).
 * UI PrimeReact **11** qua `primeReact11ControlElements` (không dùng
 * `@react-querybuilder/prime` — package đó chỉ tương thích PrimeReact 10).
 *
 * Draft query updates locally first so typing stays snappy; parent sync is
 * deferred via startTransition. AST/params are only built when those callbacks
 * are provided.
 */
function QueryBuilderInner({
  fields = EMPLOYEE_ALLOWED_FIELDS,
  query: controlledQuery,
  defaultQuery = EMPTY_QUERY,
  disabled = false,
  className,
  onQueryChange,
  onAstChange,
  onParamsChange,
  onPagedParamsChange,
}: QueryBuilderProps) {
  const [localQuery, setLocalQuery] = useState<RuleGroupType>(
    () => controlledQuery ?? defaultQuery,
  )

  // Sync when parent resets / applyAst / external replace (not every keystroke).
  const prevControlled = useRef(controlledQuery)
  useEffect(() => {
    if (controlledQuery === undefined) return
    if (controlledQuery === prevControlled.current) return
    prevControlled.current = controlledQuery
    setLocalQuery(controlledQuery)
  }, [controlledQuery])

  const needsDerived =
    onAstChange != null ||
    onParamsChange != null ||
    onPagedParamsChange != null

  const handleChange = (next: RuleGroupType) => {
    prevControlled.current = next
    // Local UI first (sync) so typing / date picks stay snappy.
    setLocalQuery(next)
    // Parent page (table/toolbar) can update at lower priority.
    startTransition(() => {
      onQueryChange?.(next)
      if (!needsDerived) return
      const ast = toFilterAst(next, fields)
      onAstChange?.(ast)
      const check = validateFilterAst(ast)
      const filterJson = check.ok ? toFilterJson(ast) : undefined
      onParamsChange?.(
        filterJson ? { filter: filterJson } : toFilterParams(null),
      )
      onPagedParamsChange?.(filterJson ? { filter: filterJson } : {})
    })
  }

  const controlClassnames = useMemo(
    () => ({
      queryBuilder: 'fe-rqb',
      ruleGroup: rqbGroup,
      rule: rqbRule,
      header: 'flex min-h-10 flex-wrap items-center gap-2 pl-2',
      body: 'flex flex-col gap-2 pl-2',
      combinators: 'fe-rqb-combinator',
      fields: 'fe-rqb-field',
      operators: 'fe-rqb-operator',
      value: 'fe-rqb-value min-w-[17rem] w-[17rem]',
      addRule: 'fe-rqb-add-rule ml-0',
      addGroup: 'fe-rqb-add-group ml-0',
      removeRule: 'fe-rqb-remove',
      removeGroup: 'fe-rqb-remove',
      betweenRules: rqbBetween,
      dragHandle: 'fe-rqb-drag-handle touch-none select-none',
    }),
    [],
  )

  return (
    <div
      className={[
        rqbShell,
        disabled ? 'pointer-events-none opacity-65' : '',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <QueryBuilderDnD dnd={dndAdapter}>
        <RqbQueryBuilder
          fields={fields}
          query={localQuery}
          onQueryChange={handleChange}
          disabled={disabled}
          enableDragAndDrop
          addRuleToNewGroups
          showCombinatorsBetweenRules
          translations={translations}
          controlElements={primeReact11ControlElements}
          controlClassnames={controlClassnames}
        />
      </QueryBuilderDnD>
    </div>
  )
}

export const QueryBuilder = memo(QueryBuilderInner)
