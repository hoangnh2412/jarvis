import { useState, type ReactNode } from 'react'
import {
  Building2,
  FileStack,
  FileText,
  FolderOpen,
  HelpCircle,
  LayoutDashboard,
  Settings,
  Shield,
  Upload,
  Users,
} from 'lucide-react'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { ACCOUNT_ROUTES } from '../../features/account/routes/paths'
import { FILE_MANAGER_ROUTES } from '../../features/fileManager/routes/paths'
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
  { id: 'import', label: 'Import', icon: Upload, path: IMPORT_ROUTES.page },
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
