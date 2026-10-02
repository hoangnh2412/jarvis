import { useState, type ReactNode } from 'react'
import {
  Building2,
  CalendarClock,
  CalendarDays,
  ClipboardList,
  FileStack,
  FileText,
  FileType,
  FolderOpen,
  HelpCircle,
  LayoutDashboard,
  Settings,
  Shield,
  Users,
} from 'lucide-react'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { ACCOUNT_ROUTES } from '../../features/account/routes/paths'
import { FILE_MANAGER_ROUTES } from '../../features/fileManager/routes/paths'
import { PLANNER_ROUTES } from '../../features/planner/routes/paths'
import { TIMESHEET_ROUTES } from '../../features/timesheet/routes/paths'
import { DYNAMIC_FORM_ROUTES } from '../../features/dynamicForm/routes/paths'
import { CRAFT_DOC_ROUTES } from '../../features/craftDoc/routes/paths'
import { Header } from '../Header'
import { SideBar } from '../SideBar'
import { findBestNavMatch } from '../navMatch'
import type { AdminNavItem } from '../types'
import { IMPORT_ROUTES } from '../../features'

export const DEFAULT_ADMIN_MAIN_NAV: AdminNavItem[] = [
  { id: 'dashboard', label: 'Tổng quan', icon: LayoutDashboard, path: '/dashboard' },
  { id: 'users', label: 'Người dùng', icon: Users, path: '/users' },
  { id: 'templates', label: 'Biểu mẫu', icon: FileStack, path: '/templates' },
  { id: 'tenants', label: 'Tenant', icon: Building2, path: '/tenants' },
  { id: 'documents', label: 'Tài liệu', icon: FileText, path: '/documents' },
  { id: 'roles', label: 'Vai trò', icon: Shield, path: '/roles' },
  { id: 'files', label: 'Files', icon: FolderOpen, path: FILE_MANAGER_ROUTES.list },
  // { id: 'import', label: 'Import', icon: Upload, path: IMPORT_ROUTES.page },
  { id: 'planner', label: 'Planner', icon: CalendarDays, path: PLANNER_ROUTES.page },
  { id: 'timesheet', label: 'Timesheet', icon: CalendarClock, path: TIMESHEET_ROUTES.page },
  { id: 'dynamic-forms', label: 'Form', icon: ClipboardList, path: DYNAMIC_FORM_ROUTES.page },
  { id: 'craft-doc', label: 'Craft DOCX', icon: FileType, path: CRAFT_DOC_ROUTES.builder },
]

export const DEFAULT_ADMIN_SECONDARY_NAV: AdminNavItem[] = [
  { id: 'settings', label: 'Cài đặt', icon: Settings, path: '/settings' },
  { id: 'help', label: 'Trợ giúp', icon: HelpCircle, path: '/help' },
]

export type AdminLayoutProps = {
  /** Override nav chính — mặc định `DEFAULT_ADMIN_MAIN_NAV` */
  mainNav?: AdminNavItem[]
  /** Override nav phụ — mặc định `DEFAULT_ADMIN_SECONDARY_NAV` */
  secondaryNav?: AdminNavItem[]
  user?: { fullName?: string; email?: string } | null
  notificationCount?: number
  /** Thay chuông mặc định trên header (ví dụ dropdown notifications). */
  notificationSlot?: ReactNode
  onNotificationClick?: () => void
  searchPlaceholder?: string
  searchValue?: string
  onSearchChange?: (value: string) => void
  onLogout?: () => void | boolean | Promise<void | boolean>
  profilePath?: string
  onProfileClick?: () => void
  logoTitle?: string
  logoSubtitle?: string
  defaultTitle?: string
  className?: string
  children?: ReactNode
}

export function AdminLayout({
  mainNav = DEFAULT_ADMIN_MAIN_NAV,
  secondaryNav = DEFAULT_ADMIN_SECONDARY_NAV,
  user = null,
  notificationCount = 0,
  notificationSlot,
  onNotificationClick,
  searchPlaceholder,
  searchValue,
  onSearchChange,
  onLogout,
  profilePath = '/profile',
  onProfileClick,
  logoTitle = 'FE Admin',
  logoSubtitle = 'Console',
  defaultTitle = 'Quản lý',
  className = '',
  children,
}: AdminLayoutProps = {}) {
  const [collapsed, setCollapsed] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const { pathname } = useLocation()
  const navigate = useNavigate()

  const allNav = [...mainNav, ...secondaryNav]
  const currentNav = findBestNavMatch(pathname, allNav)
  const title = currentNav?.label ?? defaultTitle

  const handleProfileClick =
    onProfileClick ?? (() => navigate(profilePath))

  const handleLogout = async () => {
    if (onLogout) {
      const result = await onLogout()
      if (result === false) return
    }
    navigate(ACCOUNT_ROUTES.login, { replace: true })
  }

  const isImportPage =
    pathname === IMPORT_ROUTES.page ||
    pathname.startsWith(`${IMPORT_ROUTES.page}/`)
  const isPlannerPage =
    pathname === PLANNER_ROUTES.page ||
    pathname.startsWith(`${PLANNER_ROUTES.page}/`)
  const isTimesheetPage =
    pathname === TIMESHEET_ROUTES.page ||
    pathname.startsWith(`${TIMESHEET_ROUTES.page}/`)
  const isDynamicFormPage =
    pathname === DYNAMIC_FORM_ROUTES.page ||
    pathname.startsWith(`${DYNAMIC_FORM_ROUTES.page}/`)
  const isCraftDocPage =
    pathname === CRAFT_DOC_ROUTES.builder ||
    pathname.startsWith(`${CRAFT_DOC_ROUTES.builder}/`)

  return (
    <div
      className={['flex h-svh overflow-hidden bg-slate-50', className]
        .filter(Boolean)
        .join(' ')}
    >
      <SideBar
        collapsed={collapsed}
        setCollapsed={setCollapsed}
        mobileOpen={mobileOpen}
        setMobileOpen={setMobileOpen}
        mainNav={mainNav}
        secondaryNav={secondaryNav}
        logoTitle={logoTitle}
        logoSubtitle={logoSubtitle}
        onLogout={handleLogout}
      />

      <div className="flex min-w-0 flex-1 flex-col overflow-hidden">
        <Header
          title={title}
          onMenuClick={() => setMobileOpen(true)}
          user={user}
          notificationCount={notificationCount}
          notificationSlot={notificationSlot}
          onNotificationClick={onNotificationClick}
          searchPlaceholder={searchPlaceholder}
          searchValue={searchValue}
          onSearchChange={onSearchChange}
          onProfileClick={handleProfileClick}
          onLogout={handleLogout}
        />
        <main className="flex min-h-0 flex-1 flex-col overflow-hidden p-3 lg:p-4">
          <div
            className={[
              'min-h-0 flex-1',
              isImportPage
                ? 'kit-admin-main--import kit-scrollbar-hidden overflow-y-auto'
                : isPlannerPage || isTimesheetPage || isDynamicFormPage || isCraftDocPage
                  ? 'flex flex-col overflow-hidden'
                  : 'overflow-y-auto',
            ]
              .filter(Boolean)
              .join(' ')}
          >
            {children ?? <Outlet />}
          </div>
        </main>
      </div>
    </div>
  )
}
