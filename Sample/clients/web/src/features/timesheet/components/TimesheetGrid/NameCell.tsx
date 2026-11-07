import { ChevronDown, ChevronRight, FolderKanban } from 'lucide-react'
import type { ICellRendererParams } from 'ag-grid-community'
import type { TimesheetGridRow, TimesheetGroupField } from '../../types'
import { getAvatarTone, getInitials } from '../../utils'

export type TimesheetGridContext = {
  expanded: Set<string>
  onToggle: (id: string) => void
  groupBy: TimesheetGroupField[]
}

export function NameCell(
  params: ICellRendererParams<TimesheetGridRow, unknown, TimesheetGridContext>,
) {
  const row = params.data
  if (!row) return null
  const pinned = Boolean(params.node.rowPinned)
  const isGroup = row.rowType === 'group' && !pinned
  const expanded = params.context.expanded.has(row.id)
  const primaryField = params.context.groupBy[0] ?? 'project'
  const secondaryField = params.context.groupBy[1] ?? 'user'
  const showUserAvatar =
    Boolean(row.userName) &&
    ((primaryField === 'user' && row.rowType === 'group') ||
      (secondaryField === 'user' && row.rowType === 'leaf') ||
      (primaryField === 'user' && secondaryField === 'none' && row.rowType === 'leaf'))
  const initials = getInitials(row.userName)
  const tone = getAvatarTone(row.userName)

  return (
    <div
      className="kit-timesheet-name"
      style={{ paddingLeft: `${row.depth * 1.1}rem` }}
    >
      {isGroup ? (
        <button
          type="button"
          className="kit-timesheet-name__toggle"
          aria-expanded={expanded}
          onClick={() => params.context.onToggle(row.id)}
        >
          {expanded ? (
            <ChevronDown className="h-4 w-4" />
          ) : (
            <ChevronRight className="h-4 w-4" />
          )}
        </button>
      ) : (
        <span className="kit-timesheet-name__spacer" />
      )}
      {showUserAvatar ? (
        <span
          className={`kit-timesheet-avatar kit-timesheet-avatar--${tone}`}
          aria-hidden
        >
          {initials}
        </span>
      ) : (
        <span className="kit-timesheet-folder" aria-hidden>
          <FolderKanban className="h-3.5 w-3.5" />
        </span>
      )}
      <span className="kit-timesheet-name__label" title={row.label}>
        {row.label}
      </span>
    </div>
  )
}

export function KeyCell(
  params: ICellRendererParams<TimesheetGridRow>,
) {
  const key = params.data?.key
  if (!key) return null
  return <span className="kit-timesheet-key">{key}</span>
}
