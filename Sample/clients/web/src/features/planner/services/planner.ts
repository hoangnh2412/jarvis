import instance from './req'
import type {
  CreatePlannerItemPayload,
  MovePlannerItemPayload,
  PlannerBoardQuery,
  PlannerBoardResult,
  PlannerItem,
  ReschedulePlannerItemPayload,
  UpdatePlannerItemPayload,
} from '../types'

function toBoardParams(query?: PlannerBoardQuery) {
  if (!query) return undefined
  const params: Record<string, string> = {}
  const search = query.search?.trim()
  if (search) params.search = search
  if (query.priority && query.priority !== 'all') {
    params.priority = query.priority
  }
  return Object.keys(params).length > 0 ? params : undefined
}

/** GET /api/v1/planner/board — Sample MSW trả JSON khi `VITE_USE_MOCK` ≠ false */
export const callGetPlannerBoard = async (query?: PlannerBoardQuery) => {
  return await instance.get<PlannerBoardResult>('v1/planner/board', {
    params: toBoardParams(query),
  })
}

export const callCreatePlannerItem = async (data: CreatePlannerItemPayload) => {
  return await instance.post<PlannerItem>('v1/planner/items', data)
}

export const callUpdatePlannerItem = async (data: UpdatePlannerItemPayload) => {
  const { id, ...body } = data
  return await instance.put<PlannerItem>(`v1/planner/items/${id}`, body)
}

export const callMovePlannerItem = async (data: MovePlannerItemPayload) => {
  return await instance.put<PlannerItem>(`v1/planner/items/${data.id}/move`, {
    statusId: data.statusId,
  })
}

export const callReschedulePlannerItem = async (
  data: ReschedulePlannerItemPayload,
) => {
  return await instance.put<PlannerItem>(
    `v1/planner/items/${data.id}/reschedule`,
    {
      start: data.start,
      end: data.end,
      allDay: data.allDay,
    },
  )
}

export const callDeletePlannerItem = async (id: string) => {
  return await instance.delete(`v1/planner/items/${id}`)
}
