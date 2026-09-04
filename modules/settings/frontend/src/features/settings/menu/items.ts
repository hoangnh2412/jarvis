import { Settings } from 'lucide-react'
import { SETTING_ROUTES } from '../routes/paths'

export const settingMenuItems = [
  {
    id: 'settings',
    label: 'Cài đặt',
    icon: Settings,
    path: SETTING_ROUTES.home,
  },
] as const
