import { useCallback, useMemo } from 'react'
import { AgGridReact } from 'ag-grid-react'
import {
  AllCommunityModule,
  ModuleRegistry,
  themeQuartz,
  type CellClassParams,
  type CellClickedEvent,
  type ColDef,
  type GridApi,
  type ValueFormatterParams,
  type ValueGetterParams,
} from 'ag-grid-community'
import type {
  TimesheetDayCellContext,
  TimesheetDayColumn,
  TimesheetGrain,
  TimesheetGroupField,
  TimesheetGridRow,
} from '../../types'
import { formatHours } from '../../utils'
import { DayHeader } from './DayHeader'
import { KeyCell, NameCell, type TimesheetGridContext } from './NameCell'

ModuleRegistry.registerModules([AllCommunityModule])

const timesheetTheme = themeQuartz.withParams({
  accentColor: '#0f766e',
  backgroundColor: '#ffffff',
  borderColor: '#e8edf3',
  headerBackgroundColor: '#f8fafc',
  headerTextColor: '#64748b',
  headerFontWeight: 700,
  headerHeight: 52,
  rowHeight: 42,
  fontFamily: 'inherit',
  fontSize: 13,
  foregroundColor: '#0f172a',
  rowHoverColor: '#f8fafc',
  selectedRowBackgroundColor: '#f0fdfa',
  cellHorizontalPadding: 10,
  wrapperBorderRadius: 0,
  columnBorder: true,
  oddRowBackgroundColor: '#ffffff',
})

export type TimesheetGridProps = {
  rows: TimesheetGridRow[]
  totalRow: TimesheetGridRow
  columns: TimesheetDayColumn[]
  grain: TimesheetGrain
  groupBy: TimesheetGroupField[]
  expanded: Set<string>
  onToggleExpand: (id: string) => void
  nameHeader: string
  keyHeader: string
  loggedHeader: string
  onDayCellClick?: (payload: TimesheetDayCellContext) => void
  onGridReady?: (api: GridApi<TimesheetGridRow>) => void
}

function hoursValue(
  params: ValueGetterParams<TimesheetGridRow>,
  dayId: string,
) {
  return params.data?.hoursByDay?.[dayId] ?? 0
}

function hoursFormatter(params: ValueFormatterParams<TimesheetGridRow>) {
  return formatHours(Number(params.value) || 0)
}

function readCellAnchor(event: CellClickedEvent<TimesheetGridRow>) {
  const target = event.event?.target
  if (!(target instanceof Element)) return null
  const cell = target.closest('.ag-cell')
  if (!cell) return null
  const rect = cell.getBoundingClientRect()
  return {
    top: rect.top,
    left: rect.left,
    width: rect.width,
    height: rect.height,
  }
}

export function TimesheetGrid({
  rows,
  totalRow,
  columns,
  grain,
  groupBy,
  expanded,
  onToggleExpand,
  nameHeader,
  keyHeader,
  loggedHeader,
  onDayCellClick,
  onGridReady,
}: TimesheetGridProps) {
  const context = useMemo<TimesheetGridContext>(
    () => ({ expanded, onToggle: onToggleExpand, groupBy }),
    [expanded, groupBy, onToggleExpand],
  )

  const columnDefs = useMemo<ColDef<TimesheetGridRow>[]>(() => {
    const dayCols: ColDef<TimesheetGridRow>[] = columns.map((column) => ({
      colId: column.field,
      headerName: column.dayNum,
      headerComponent: DayHeader,
      context: column,
      width: 56,
      minWidth: 52,
      maxWidth: 72,
      editable: false,
      valueGetter: (params) => hoursValue(params, column.id),
      valueFormatter: hoursFormatter,
      cellClassRules: {
        'kit-timesheet-cell--weekend': (params: CellClassParams<TimesheetGridRow>) =>
          Boolean((params.colDef.context as TimesheetDayColumn | undefined)?.isWeekend),
        'kit-timesheet-cell--today': (params: CellClassParams<TimesheetGridRow>) =>
          Boolean((params.colDef.context as TimesheetDayColumn | undefined)?.isToday),
        'kit-timesheet-cell--hours': (params: CellClassParams<TimesheetGridRow>) =>
          Number(params.value) > 0,
        'kit-timesheet-cell--clickable': (params: CellClassParams<TimesheetGridRow>) =>
          params.data?.id !== '__total__',
      },
      headerClass: [
        column.isWeekend ? 'kit-timesheet-header--weekend' : '',
        column.isToday ? 'kit-timesheet-header--today' : '',
      ]
        .filter(Boolean)
        .join(' '),
    }))

    return [
      {
        field: 'label',
        headerName: nameHeader,
        pinned: 'left',
        lockPinned: true,
        minWidth: 280,
        width: 300,
        cellRenderer: NameCell,
        sortable: false,
        filter: false,
      },
      {
        field: 'key',
        headerName: keyHeader,
        pinned: 'left',
        lockPinned: true,
        width: 92,
        sortable: false,
        cellRenderer: KeyCell,
        cellClass: 'kit-timesheet-cell--key',
      },
      {
        field: 'logged',
        headerName: loggedHeader,
        pinned: 'left',
        lockPinned: true,
        width: 92,
        sortable: false,
        valueFormatter: hoursFormatter,
        cellClass: 'kit-timesheet-cell--logged',
      },
      ...dayCols,
    ]
  }, [columns, keyHeader, loggedHeader, nameHeader])

  const defaultColDef = useMemo<ColDef<TimesheetGridRow>>(
    () => ({
      resizable: true,
      sortable: false,
      suppressMovable: true,
      cellClass: 'kit-timesheet-cell',
    }),
    [],
  )

  const handleCellClicked = useCallback(
    (event: CellClickedEvent<TimesheetGridRow>) => {
      const row = event.data
      const column = event.colDef.context as TimesheetDayColumn | undefined
      const anchor = readCellAnchor(event)
      if (!row || !column || !anchor || row.id === '__total__') return
      onDayCellClick?.({
        date: column.id,
        hours: row.hoursByDay?.[column.id] ?? 0,
        grain,
        row,
        anchor,
      })
    },
    [grain, onDayCellClick],
  )

  const getRowId = useCallback(
    (params: { data: TimesheetGridRow }) => params.data.id,
    [],
  )

  const getRowClass = useCallback((params: { data?: TimesheetGridRow }) => {
    if (!params.data) return ''
    if (params.data.id === '__total__') return 'kit-timesheet-row--total'
    return params.data.rowType === 'group'
      ? 'kit-timesheet-row--group'
      : 'kit-timesheet-row--leaf'
  }, [])

  return (
    <div className="kit-timesheet-grid">
      <AgGridReact<TimesheetGridRow>
        theme={timesheetTheme}
        rowData={rows}
        pinnedBottomRowData={[totalRow]}
        columnDefs={columnDefs}
        defaultColDef={defaultColDef}
        context={context}
        getRowId={getRowId}
        getRowClass={getRowClass}
        headerHeight={52}
        rowHeight={42}
        suppressCellFocus
        animateRows={false}
        onCellClicked={handleCellClicked}
        onGridReady={(event) => {
          onGridReady?.(event.api)
        }}
      />
    </div>
  )
}
