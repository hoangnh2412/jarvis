export type PlannerViewMode = 'kanban' | 'calendar' | 'timeline'

export type PlannerPriority = 'low' | 'medium' | 'high'

export type PlannerColumn = {
  id: string
  name: string
  order: number
  color?: string
}

export type PlannerItem = {
  id: string
  title: string
  description?: string | null
  statusId: string
  start: string
  end?: string | null
  allDay?: boolean
  priority?: PlannerPriority
  assignee?: string | null
  updatedAt?: string
}

export type PlannerBoardResult = {
  columns: PlannerColumn[]
  items: PlannerItem[]
}

/** Query filter board — xử lý ở BE, FE chỉ truyền params. */
export type PlannerBoardQuery = {
  search?: string
  priority?: PlannerPriority | 'all'
}

export type CreatePlannerItemPayload = {
  title: string
  description?: string | null
  statusId: string
  start: string
  end?: string | null
  allDay?: boolean
  priority?: PlannerPriority
  assignee?: string | null
}

export type UpdatePlannerItemPayload = {
  id: string
  title?: string
  description?: string | null
  statusId?: string
  start?: string
  end?: string | null
  allDay?: boolean
  priority?: PlannerPriority
  assignee?: string | null
}

export type MovePlannerItemPayload = {
  id: string
  statusId: string
}

export type ReschedulePlannerItemPayload = {
  id: string
  start: string
  end?: string | null
  allDay?: boolean
}
