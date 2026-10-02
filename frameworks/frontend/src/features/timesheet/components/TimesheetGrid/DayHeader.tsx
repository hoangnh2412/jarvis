import type { IHeaderParams } from 'ag-grid-community'
import type { TimesheetDayColumn } from '../../types'

export function DayHeader(params: IHeaderParams) {
  const column = params.column.getColDef().context as TimesheetDayColumn | undefined
  if (!column) return <span>{params.displayName}</span>
  return (
    <div
      className={[
        'kit-timesheet-day-header',
        column.isWeekend ? 'is-weekend' : '',
        column.isToday ? 'is-today' : '',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <span className="kit-timesheet-day-header__num">{column.dayNum}</span>
      <span className="kit-timesheet-day-header__wk">{column.weekday}</span>
    </div>
  )
}
