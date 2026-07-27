import { useState, type ReactNode } from 'react'
import {
  Building2,
  FileStack,
  FileText,
  HelpCircle,
  LayoutDashboard,
  Settings,
  Users,
} from 'lucide-react'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { ACCOUNT_ROUTES } from '../../modules/account/routes/paths'
import { Header } from '../Header'
import { SideBar } from '../SideBar'
import type { AdminNavItem } from '../types'

export const DEFAULT_ADMIN_MAIN_NAV: AdminNavItem[] = [
  { id: 'dashboard', label: 'Tổng quan', icon: LayoutDashboard, path: '/' },
  { id: 'users', label: 'Người dùng', icon: Users, path: '/users' },
  { id: 'templates', label: 'Biểu mẫu', icon: FileStack, path: '/templates' },
  { id: 'tenants', label: 'Tenant', icon: Building2, path: '/tenants' },
  { id: 'documents', label: 'Tài liệu', icon: FileText, path: '/documents' },
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
  const currentNav = allNav.find((item) =>
    item.path === '/' ? pathname === '/' : pathname.startsWith(item.path),
  )
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
          searchPlaceholder={searchPlaceholder}
          searchValue={searchValue}
          onSearchChange={onSearchChange}
          onProfileClick={handleProfileClick}
          onLogout={handleLogout}
        />
        <main className="flex min-h-0 flex-1 flex-col overflow-hidden p-3 lg:p-4">
          <div className="min-h-0 flex-1 overflow-y-auto">
            {children ?? <Outlet />}
          </div>
        </main>
      </div>
    </div>
  )
}
