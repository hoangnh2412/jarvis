import {
  useEffect,
  type Dispatch,
  type SetStateAction,
} from 'react'
import { Link, useLocation } from 'react-router-dom'
import { ChevronLeft, ChevronRight, LogOut, X } from 'lucide-react'
import Logo from '../../common/Logo'
import type { AdminNavItem } from '../types'

export type SideBarProps = {
  collapsed: boolean
  setCollapsed: Dispatch<SetStateAction<boolean>>
  mobileOpen: boolean
  setMobileOpen: Dispatch<SetStateAction<boolean>>
  mainNav?: AdminNavItem[]
  secondaryNav?: AdminNavItem[]
  onLogout?: () => void
  logoTitle?: string
  logoSubtitle?: string
  className?: string
}

const shell =
  'flex h-full flex-col border-r border-slate-200/80 bg-white shadow-[1px_0_12px_rgba(15,23,42,0.04)]'

function cx(...parts: Array<string | false | null | undefined>) {
  return parts.filter(Boolean).join(' ')
}

function isActive(pathname: string, path: string) {
  return path === '/' ? pathname === '/' : pathname.startsWith(path)
}

function NavLinks({
  items,
  collapsed,
  onNavigate,
}: {
  items: AdminNavItem[]
  collapsed: boolean
  onNavigate: () => void
}) {
  const { pathname } = useLocation()

  return (
    <div className="space-y-0.5">
      {items.map(({ id, label, icon: Icon, path, badge }) => {
        const active = isActive(pathname, path)
        return (
          <Link
            key={id}
            to={path}
            onClick={onNavigate}
            title={collapsed ? label : undefined}
            className={cx(
              'group flex items-center gap-3 rounded-lg text-sm font-medium transition-all duration-150',
              collapsed ? 'justify-center px-2 py-2.5' : 'px-3 py-2.5',
              active
                ? 'bg-blue-50 text-blue-700 shadow-sm shadow-blue-600/5 ring-1 ring-blue-100'
                : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900',
            )}
          >
            <Icon
              className={cx(
                'h-[18px] w-[18px] shrink-0',
                active
                  ? 'text-blue-600'
                  : 'text-slate-400 group-hover:text-slate-600',
              )}
            />
            {!collapsed && <span className="truncate">{label}</span>}
            {!collapsed && badge != null && (
              <span
                className={cx(
                  'ml-auto inline-flex h-5 min-w-5 items-center justify-center rounded-md px-1.5 text-[11px] font-medium tabular-nums',
                  active
                    ? 'bg-blue-100 text-blue-700'
                    : 'bg-slate-100 text-slate-600',
                )}
              >
                {badge}
              </span>
            )}
          </Link>
        )
      })}
    </div>
  )
}

function NavBody({
  collapsed,
  mainNav,
  secondaryNav,
  logoTitle,
  logoSubtitle,
  onNavigate,
  onLogout,
  showClose,
  onClose,
  onToggleCollapse,
}: {
  collapsed: boolean
  mainNav: AdminNavItem[]
  secondaryNav: AdminNavItem[]
  logoTitle?: string
  logoSubtitle?: string
  onNavigate: () => void
  onLogout?: () => void
  showClose?: boolean
  onClose?: () => void
  onToggleCollapse?: () => void
}) {
  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="relative flex h-16 shrink-0 items-center overflow-visible border-b border-slate-100 px-3">
        <Logo
          collapsed={collapsed}
          showText={false}
          size="md"
          className="shrink-0"
        />
        {!collapsed && (
          <div className="min-w-0 overflow-hidden">
            <p className="m-0 truncate text-[15px] font-semibold leading-tight tracking-tight text-slate-900">
              {logoTitle}
            </p>
          </div>
        )}
        {showClose && (
          <button
            type="button"
            onClick={onClose}
            aria-label="Đóng menu"
            className="absolute right-3 top-1/2 inline-flex -translate-y-1/2 cursor-pointer appearance-none items-center justify-center rounded-lg border-0 bg-transparent p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-700"
          >
            <X className="h-5 w-5" />
          </button>
        )}
      </div>

      <nav className="min-h-0 flex-1 overflow-y-auto px-3 py-4">
        {mainNav.length > 0 && (
          <div className="mb-4">
            {!collapsed && (
              <p className="mb-2 px-3 text-[10px] font-semibold uppercase tracking-wider text-slate-400">
                Menu chính
              </p>
            )}
            <NavLinks
              items={mainNav}
              collapsed={collapsed}
              onNavigate={onNavigate}
            />
          </div>
        )}

        {secondaryNav.length > 0 && (
          <div>
            {!collapsed && (
              <p className="mb-2 px-3 text-[10px] font-semibold uppercase tracking-wider text-slate-400">
                Hệ thống
              </p>
            )}
            <NavLinks
              items={secondaryNav}
              collapsed={collapsed}
              onNavigate={onNavigate}
            />
          </div>
        )}
      </nav>

      {onLogout && (
        <div className="shrink-0 border-t border-slate-100 p-2">
          <button
            type="button"
            title={collapsed ? 'Đăng xuất' : undefined}
            onClick={onLogout}
            className={cx(
              'group flex w-full cursor-pointer appearance-none items-center gap-3 rounded-lg border-0 bg-transparent text-sm font-medium text-slate-600 transition-colors',
              'hover:bg-red-50 hover:text-red-600',
              collapsed ? 'justify-center px-2 py-2.5' : 'px-3 py-2.5',
            )}
          >
            <LogOut className="h-[18px] w-[18px] shrink-0 text-slate-400 group-hover:text-red-500" />
            {!collapsed && <span>Đăng xuất</span>}
          </button>
        </div>
      )}

      {onToggleCollapse && (
        <button
          type="button"
          onClick={onToggleCollapse}
          aria-label={collapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'}
          className="hidden h-11 w-full shrink-0 cursor-pointer appearance-none items-center justify-center border-0 border-t border-solid border-slate-100 bg-transparent text-slate-400 transition-colors hover:bg-slate-50 hover:text-slate-700 lg:flex"
        >
          {collapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronLeft className="h-4 w-4" />
          )}
        </button>
      )}
    </div>
  )
}

export function SideBar({
  collapsed,
  setCollapsed,
  mobileOpen,
  setMobileOpen,
  mainNav = [],
  secondaryNav = [],
  onLogout,
  logoTitle = 'FE Admin',
  logoSubtitle = 'Console',
  className = '',
}: SideBarProps) {
  const closeMobile = () => setMobileOpen(false)

  useEffect(() => {
    if (!mobileOpen) return
    const prev = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') closeMobile()
    }
    window.addEventListener('keydown', onKey)
    return () => {
      document.body.style.overflow = prev
      window.removeEventListener('keydown', onKey)
    }
  }, [mobileOpen])

  const bodyProps = {
    mainNav,
    secondaryNav,
    logoTitle,
    logoSubtitle,
    onLogout,
    onNavigate: closeMobile,
  }

  return (
    <>
      {/* Desktop */}
      <aside
        className={cx(
          'hidden h-svh shrink-0 transition-[width] duration-300 ease-out lg:flex lg:flex-col',
          shell,
          collapsed ? 'w-[72px]' : 'w-64',
          className,
        )}
      >
        <NavBody
          collapsed={collapsed}
          {...bodyProps}
          onToggleCollapse={() => setCollapsed((v) => !v)}
        />
      </aside>

      {/* Mobile */}
      <div
        className={cx(
          'fixed inset-0 z-50 lg:hidden',
          mobileOpen ? 'pointer-events-auto' : 'pointer-events-none',
        )}
        aria-hidden={!mobileOpen}
      >
        <div
          className={cx(
            'absolute inset-0 bg-slate-900/20 backdrop-blur-[2px] transition-opacity duration-300',
            mobileOpen ? 'opacity-100' : 'opacity-0',
          )}
          onClick={closeMobile}
        />
        <aside
          className={cx(
            'absolute bottom-0 left-0 top-0 w-64 transition-transform duration-300 ease-out',
            shell,
            mobileOpen ? 'translate-x-0' : '-translate-x-full',
          )}
        >
          <NavBody
            collapsed={false}
            {...bodyProps}
            showClose
            onClose={closeMobile}
          />
        </aside>
      </div>
    </>
  )
}
