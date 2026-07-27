import type { LucideIcon } from 'lucide-react'

export type AdminNavSubItem = {
  id: string
  label: string
  path: string
}

export type AdminNavItem = {
  id: string
  label: string
  path: string
  icon: LucideIcon
  badge?: string | number
  /** Submenu (PrimeReact Sidebar.MenuSub) */
  children?: AdminNavSubItem[]
}
