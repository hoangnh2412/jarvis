import { PLANNER_PERMISSIONS } from '../permission'
import { getPlannerPath } from '../routes/paths'

export type PlannerMenuItem = {
  path: string
  label: string
  permission: string
}

export const plannerMenuItems: PlannerMenuItem[] = [
  {
    path: getPlannerPath(),
    label: 'Planner',
    permission: PLANNER_PERMISSIONS.view,
  },
]
