import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import type { GridApi } from 'ag-grid-community'
import type { AxiosResponse } from 'axios'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import {
  TimesheetDayLogPopover,
  TimesheetGrid,
  TimesheetPageShell,
  TimesheetSkeleton,
  TimesheetToolbar,
} from '../../components'
import { getTimesheetMessages, type TimesheetLocale } from '../../localization'
import {
  callGetTimesheetBoard,
  callSaveTimesheet,
} from '../../services'
import type {
  TimesheetBoardResult,
  TimesheetDayCellContext,
  TimesheetGrain,
  TimesheetGroupField,
  TimesheetGridRow,
  TimesheetLog,
  TimesheetQuery,
  UpdateTimesheetHoursPayload,
} from '../../types'
import {
  buildDayColumns,
  buildTimesheetRows,
  buildTotalRow,
  collectWorkLogEntriesForCell,
  endOfMonth,
  flattenTimesheetRows,
  formatYmd,
  normalizeGroupFields,
  resolveTimesheetContent,
  startOfMonth,
  type TimesheetSlotContent,
} from '../../utils'

const FILTER_DEBOUNCE_MS = 300

function filterIdsEqual(a: readonly string[], b: readonly string[]) {
  if (a.length !== b.length) return false
  return a.every((id, index) => id === b[index])
}

function parseFilterIds(filterIds: string[]) {
  const projectIds: string[] = []
  const userIds: string[] = []
  for (const id of filterIds) {
    if (id.startsWith('project:')) projectIds.push(id.slice(8))
    else if (id.startsWith('user:')) userIds.push(id.slice(5))
  }
  return { projectIds, userIds }
}

export type TimesheetPageContentContext = {
  logs: TimesheetLog[]
  rows: TimesheetGridRow[]
  loading: boolean
  from: string
  to: string
  filterIds: string[]
  groupBy: TimesheetGroupField[]
  grain: TimesheetGrain
  reload: () => Promise<void>
  DefaultLayout: ReactNode
}

export type TimesheetPageProps = {
  locale?: TimesheetLocale
  className?: string
  title?: string
  description?: string
  logs?: TimesheetLog[]
  loading?: boolean
  withShell?: boolean
  callback?: {
    list?: ActionProps<
      TimesheetQuery,
      TimesheetQuery,
      AxiosResponse<TimesheetBoardResult>
    >
    updateHours?: ActionProps<
      UpdateTimesheetHoursPayload,
      UpdateTimesheetHoursPayload,
      AxiosResponse<TimesheetLog>
    >
    save?: ActionProps<
      Record<string, never>,
      void,
      AxiosResponse<{ saved: number }>
    >
  }
  content?: TimesheetSlotContent<TimesheetPageContentContext>
  /** Gọi khi bấm Log Time trong popover ô ngày. */
  onLogTime?: (ctx: TimesheetDayCellContext) => void
  /**
   * Tuỳ biến click ô ngày. Return `true` để bỏ popover mặc định.
   */
  onDayCellClick?: (ctx: TimesheetDayCellContext) => boolean | void
}

export function TimesheetPage({
  locale = 'vi',
  className,
  title,
  description,
  logs: logsProp,
  loading: loadingProp,
  withShell = true,
  callback,
  content,
  onLogTime,
  onDayCellClick,
}: TimesheetPageProps) {
  const messages = getTimesheetMessages(locale)
  const controlled = logsProp != null
  const [logs, setLogs] = useState<TimesheetLog[]>(logsProp ?? [])
  const [loading, setLoading] = useState(!controlled)
  const [saving, setSaving] = useState(false)
  const [filterIds, setFilterIds] = useState<string[]>([])
  const [debouncedFilterIds, setDebouncedFilterIds] = useState<string[]>([])
  const [groupBy, setGroupBy] = useState<TimesheetGroupField[]>([
    'project',
    'user',
  ])
  const [grain, setGrain] = useState<TimesheetGrain>('day')
  const [from, setFrom] = useState(() => formatYmd(startOfMonth()))
  const [to, setTo] = useState(() => formatYmd(endOfMonth()))
  const [expanded, setExpanded] = useState<Set<string>>(new Set())
  const [dayDetail, setDayDetail] = useState<TimesheetDayCellContext | null>(null)
  const gridApiRef = useRef<GridApi<TimesheetGridRow> | null>(null)
  const logsRef = useRef(logs)
  logsRef.current = logs

  useEffect(() => {
    if (!controlled) return
    setLogs(logsProp)
  }, [controlled, logsProp])

  useEffect(() => {
    if (filterIdsEqual(filterIds, debouncedFilterIds)) return
    const timer = window.setTimeout(() => {
      setDebouncedFilterIds(filterIds)
    }, FILTER_DEBOUNCE_MS)
    return () => window.clearTimeout(timer)
  }, [debouncedFilterIds, filterIds])

  const boardQuery = useMemo((): TimesheetQuery => {
    const { projectIds, userIds } = parseFilterIds(debouncedFilterIds)
    return {
      from,
      to,
      projectIds: projectIds.length ? projectIds : undefined,
      userIds: userIds.length ? userIds : undefined,
    }
  }, [debouncedFilterIds, from, to])

  const reload = useCallback(async () => {
    if (controlled) return
    const showSkeleton = logsRef.current.length === 0
    if (showSkeleton) setLoading(true)
    try {
      const outcome = await handleAction({
        ctx: boardQuery,
        callback: callback?.list,
        defaultSubmit: callGetTimesheetBoard,
        getPayload: (ctx) => ctx,
        onSuccess: async (_ctx, res) => {
          setLogs(res.data.logs)
        },
      })
      if (outcome.status === 'cancelled') return
    } catch (error) {
      notify.error(getErrorMessage(error, messages.page.error))
    } finally {
      setLoading(false)
    }
  }, [boardQuery, callback?.list, controlled, messages.page.error])

  useEffect(() => {
    if (controlled) {
      setLoading(Boolean(loadingProp))
      return
    }

    let cancelled = false
    const showSkeleton = logsRef.current.length === 0
    if (showSkeleton) setLoading(true)

    void (async () => {
      try {
        const outcome = await handleAction({
          ctx: boardQuery,
          callback: callback?.list,
          defaultSubmit: callGetTimesheetBoard,
          getPayload: (ctx) => ctx,
          onSuccess: async (_ctx, res) => {
            if (!cancelled) setLogs(res.data.logs)
          },
        })
        if (cancelled || outcome.status === 'cancelled') return
      } catch (error) {
        if (!cancelled) {
          notify.error(getErrorMessage(error, messages.page.error))
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()

    return () => {
      cancelled = true
    }
  }, [boardQuery, callback?.list, controlled, loadingProp, messages.page.error])

  const dayColumns = useMemo(
    () =>
      buildDayColumns(
        new Date(`${from}T00:00:00`),
        new Date(`${to}T00:00:00`),
        grain,
        locale,
      ),
    [from, grain, locale, to],
  )

  const treeRows = useMemo(
    () =>
      buildTimesheetRows(logs, dayColumns, {
        from,
        to,
        groupBy,
        grain,
      }),
    [dayColumns, from, grain, groupBy, logs, to],
  )

  useEffect(() => {
    const groupIds = treeRows
      .filter((row) => row.rowType === 'group')
      .map((row) => row.id)
    setExpanded((current) => {
      if (current.size === 0) return new Set(groupIds)
      const next = new Set<string>()
      for (const id of groupIds) {
        if (current.has(id)) next.add(id)
      }
      return next.size === 0 ? new Set(groupIds) : next
    })
  }, [treeRows])

  const visibleRows = useMemo(
    () => flattenTimesheetRows(treeRows, expanded),
    [expanded, treeRows],
  )
  const totalRow = useMemo(
    () => buildTotalRow(treeRows, dayColumns, messages.page.total),
    [dayColumns, messages.page.total, treeRows],
  )

  const toggleExpand = useCallback((id: string) => {
    setExpanded((current) => {
      const next = new Set(current)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }, [])

  const dayLogEntries = useMemo(
    () => (dayDetail ? collectWorkLogEntriesForCell(logs, dayDetail) : []),
    [dayDetail, logs],
  )

  const onSave = useCallback(async () => {
    setSaving(true)
    try {
      await handleAction({
        ctx: {},
        callback: callback?.save,
        defaultSubmit: callSaveTimesheet,
        getPayload: () => undefined,
      })
      notify.success(messages.page.saved)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.page.error))
    } finally {
      setSaving(false)
    }
  }, [callback?.save, messages.page.error, messages.page.saved])

  const onExport = useCallback(() => {
    gridApiRef.current?.exportDataAsCsv({
      fileName: `timesheet-${from}-to-${to}.csv`,
    })
    notify.success(messages.page.exported)
  }, [from, messages.page.exported, to])

  const isLoading = loadingProp ?? loading
  const showInitialLoading = isLoading && logs.length === 0
  const [primaryGroupBy, secondaryGroupBy] = useMemo(
    () => normalizeGroupFields(groupBy),
    [groupBy],
  )

  const defaultLayout = (
    <>
      <TimesheetToolbar
        locale={locale}
        filterIds={filterIds}
        onFilterChange={setFilterIds}
        groupBy={groupBy}
        onGroupByChange={setGroupBy}
        grain={grain}
        onGrainChange={setGrain}
        from={from}
        to={to}
        onRangeChange={(nextFrom, nextTo) => {
          setFrom(nextFrom)
          setTo(nextTo)
        }}
        onExport={onExport}
        onSave={() => void onSave()}
        saving={saving}
      />
      <div className="flex min-h-0 flex-1 flex-col overflow-hidden">
        {showInitialLoading ? (
          <TimesheetSkeleton />
        ) : visibleRows.length === 0 ? (
          <div className="kit-timesheet-empty">
            <p className="kit-timesheet-empty__title">{messages.page.empty}</p>
            <p className="kit-timesheet-empty__hint">{messages.page.emptyHint}</p>
          </div>
        ) : (
          <TimesheetGrid
            rows={visibleRows}
            totalRow={totalRow}
            columns={dayColumns}
            grain={grain}
            groupBy={groupBy}
            expanded={expanded}
            onToggleExpand={toggleExpand}
            nameHeader={
              primaryGroupBy === 'user' && secondaryGroupBy !== 'none'
                ? 'User / Project'
                : primaryGroupBy === 'user'
                  ? 'User'
                  : primaryGroupBy === 'key'
                    ? 'Key'
                  : primaryGroupBy === 'project' && secondaryGroupBy === 'none'
                    ? 'Project'
                    : messages.columns.name
            }
            keyHeader={messages.columns.key}
            loggedHeader={messages.columns.logged}
            onDayCellClick={(payload) => {
              if (onDayCellClick?.(payload) === true) return
              setDayDetail(payload)
            }}
            onGridReady={(api) => {
              gridApiRef.current = api
            }}
          />
        )}
      </div>
      <TimesheetDayLogPopover
        open={Boolean(dayDetail)}
        anchor={dayDetail?.anchor ?? null}
        entries={dayLogEntries}
        locale={locale}
        onClose={() => setDayDetail(null)}
        onLogTime={
          dayDetail && onLogTime
            ? () => {
                onLogTime(dayDetail)
                setDayDetail(null)
              }
            : undefined
        }
      />
    </>
  )

  const pageContent = resolveTimesheetContent(
    content,
    {
      logs,
      rows: visibleRows,
      loading: isLoading,
      from,
      to,
      filterIds,
      groupBy,
      grain,
      reload,
      DefaultLayout: defaultLayout,
    },
    defaultLayout,
  )

  const body = <div className="flex min-h-0 flex-1 flex-col">{pageContent}</div>

  if (!withShell) {
    return (
      <div
        className={[className, 'flex h-full min-h-0 flex-col']
          .filter(Boolean)
          .join(' ')}
      >
        {body}
      </div>
    )
  }

  return (
    <TimesheetPageShell
      title={title}
      description={description}
      className={className}
    >
      {body}
    </TimesheetPageShell>
  )
}
