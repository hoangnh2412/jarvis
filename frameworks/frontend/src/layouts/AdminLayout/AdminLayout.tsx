import { useState, type ReactNode } from 'react'
import { Building2, FileStack, FileText, FolderOpen, HelpCircle, KeyRound, Settings, Shield, Users } from 'lucide-react'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { ACCOUNT_ROUTES } from '../../features/account/routes/paths'
import { FILE_MANAGER_ROUTES } from '../../features/fileManager/routes/paths'
import { Header } from '../Header'
import { SideBar } from '../SideBar'
import { findBestNavMatch } from '../navMatch'
import type { AdminNavItem } from '../types'
import { IMPORT_ROUTES } from '../../features'

function resolveNavTitle(pathname: string, items: readonly AdminNavItem[], fallback: string) {
  const flat: { path: string; label: string }[] = []
  for (const item of items) {
    flat.push({ path: item.path, label: item.label })
    for (const child of item.children ?? []) {
      flat.push({ path: child.path, label: child.label })
    }
  }
  return findBestNavMatch(pathname, flat)?.label ?? fallback
}

export const DEFAULT_ADMIN_MAIN_NAV: AdminNavItem[] = [
  { id: 'users', label: 'Người dùng', icon: Users, path: '/users' },
  { id: 'templates', label: 'Biểu mẫu', icon: FileStack, path: '/templates' },
  { id: 'tenants', label: 'Tenant', icon: Building2, path: '/tenants' },
  { id: 'documents', label: 'Tài liệu', icon: FileText, path: '/documents' },
  { id: 'roles', label: 'Vai trò', icon: Shield, path: '/roles' },
  { id: 'permissions', label: 'Permission', icon: KeyRound, path: '/permissions' },
  { id: 'files', label: 'Files', icon: FolderOpen, path: FILE_MANAGER_ROUTES.list },
  // { id: 'import', label: 'Import', icon: Upload, path: IMPORT_ROUTES.page },
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
  /** Path cần vùng nội dung full height, ví dụ trang demo của host. */
  fillPaths?: string[]
  /** Ô phạm vi làm việc trên header (tenant, org). */
  scopeSlot?: ReactNode
  /** Đổi key để mount lại trang đang mở sau khi đổi phạm vi. */
  contentKey?: string
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
  fillPaths = [],
  scopeSlot,
  contentKey,
  className = '',
  children,
}: AdminLayoutProps = {}) {
  const [collapsed, setCollapsed] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const { pathname } = useLocation()
  const navigate = useNavigate()

  const allNav = [...mainNav, ...secondaryNav]
  const title = resolveNavTitle(pathname, allNav, defaultTitle)

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
  const isFillPage = fillPaths.some(
    (path) => pathname === path || pathname.startsWith(`${path}/`),
  )

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
          scopeSlot={scopeSlot}
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
            key={contentKey}
            className={[
              'min-h-0 flex-1',
              isImportPage
                ? 'kit-admin-main--import kit-scrollbar-hidden overflow-y-auto'
                : isFillPage
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
