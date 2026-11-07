export type TimesheetGrain = 'day' | 'week'

export type TimesheetGroupField = 'project' | 'user' | 'key' | 'none'

export type TimesheetWorkItem = {
  id: string
  title: string
  issueKey?: string
  hours: number
}

export type TimesheetWorkLogEntry = {
  id: string
  date: string
  userId: string
  userName: string
  issueKey?: string
  issueTitle: string
  description: string
  hours: number
}

export type TimesheetLog = {
  id: string
  projectId: string
  projectName: string
  projectKey: string
  userId: string
  userName: string
  date: string
  hours: number
  workItems: TimesheetWorkItem[]
}

export type TimesheetRange = {
  from: string
  to: string
}

export type TimesheetQuery = {
  from: string
  to: string
  search?: string
  /** Project ids — lọc ở BE. */
  projectIds?: string[]
  /** User ids — lọc ở BE. */
  userIds?: string[]
  groupBy?: TimesheetGroupField[]
  grain?: TimesheetGrain
}

export type TimesheetBoardResult = {
  logs: TimesheetLog[]
  range: TimesheetRange
}

/** Option filter (value dạng `project:{id}` / `user:{id}`). */
export type TimesheetFilterOption = {
  label: string
  value: string
  group?: string
}

export type TimesheetOptionsResult =
  | TimesheetFilterOption[]
  | {
      options: TimesheetFilterOption[]
      hasMore?: boolean
    }

export type UpdateTimesheetHoursPayload = {
  projectId: string
  userId: string
  date: string
  hours: number
}

export type TimesheetGridRow = {
  id: string
  rowType: 'group' | 'leaf'
  depth: number
  label: string
  key?: string
  projectId?: string
  projectName?: string
  userId?: string
  userName?: string
  hoursByDay: Record<string, number>
  logged: number
  parentId?: string
  childIds?: string[]
}

export type TimesheetDayColumn = {
  id: string
  date: Date
  field: string
  dayNum: string
  weekday: string
  isWeekend: boolean
  isToday: boolean
}

export type TimesheetDayCellAnchor = {
  top: number
  left: number
  width: number
  height: number
}

export type TimesheetDayCellContext = {
  date: string
  hours: number
  grain: TimesheetGrain
  row: TimesheetGridRow
  anchor: TimesheetDayCellAnchor
}
