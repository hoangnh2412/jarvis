import type { SearchableSelectLoadParams } from '../../../common/SearchableSelect'
import instance from './req'
import type {
  TimesheetBoardResult,
  TimesheetFilterOption,
  TimesheetOptionsResult,
  TimesheetQuery,
  TimesheetLog,
  UpdateTimesheetHoursPayload,
} from '../types'

function toBoardParams(query?: TimesheetQuery) {
  if (!query) return undefined
  const params: Record<string, string> = {
    from: query.from,
    to: query.to,
  }
  const search = query.search?.trim()
  if (search) params.search = search
  if (query.projectIds?.length) {
    params.projectIds = query.projectIds.join(',')
  }
  if (query.userIds?.length) {
    params.userIds = query.userIds.join(',')
  }
  if (query.groupBy?.length) {
    params.groupBy = query.groupBy.join(',')
  }
  if (query.grain) params.grain = query.grain
  return params
}

function toOptionsParams(params: SearchableSelectLoadParams) {
  const query: Record<string, string | number> = {
    page: params.page,
    pageSize: params.pageSize,
  }
  const search = params.search?.trim()
  if (search) query.search = search
  return query
}

/** GET v1/timesheet/board — filter/range xử lý ở BE. */
export const callGetTimesheetBoard = async (query: TimesheetQuery) => {
  return await instance.get<TimesheetBoardResult>('v1/timesheet/board', {
    params: toBoardParams(query),
  })
}

/** GET v1/timesheet/options — options cho SearchableMultiSelect (phân trang). */
export const callGetTimesheetOptions = async (
  params: SearchableSelectLoadParams,
) => {
  return await instance.get<TimesheetOptionsResult>('v1/timesheet/options', {
    params: toOptionsParams(params),
    signal: params.signal,
  })
}

export const callUpdateTimesheetHours = async (
  data: UpdateTimesheetHoursPayload,
) => {
  return await instance.put<TimesheetLog>('v1/timesheet/hours', data)
}

export const callSaveTimesheet = async () => {
  return await instance.post<{ saved: number }>('v1/timesheet/save')
}

export type { TimesheetFilterOption }
