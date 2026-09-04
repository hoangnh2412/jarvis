import {
  useEffect,
  useMemo,
  useState,
  type Dispatch,
  type SetStateAction,
} from 'react'
import { Link, useLocation } from 'react-router-dom'
import {
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  ChevronUp,
  LogOut,
  X,
} from 'lucide-react'
import type { AdminNavItem, AdminNavSubItem } from '../types'
import { isNavItemActive, navItemMatches } from '../navMatch'

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

function cx(...parts: Array<string | false | null | undefined>) {
  return parts.filter(Boolean).join(' ')
}

function isActive(pathname: string, path: string, flatPaths: readonly { path: string }[]) {
  return isNavItemActive(pathname, path, flatPaths)
}

function isSectionActive(
  pathname: string,
  item: AdminNavItem,
  flatPaths: readonly { path: string }[],
) {
  if (item.children?.length) {
    return item.children.some((child) => isActive(pathname, child.path, flatPaths))
  }
  return isActive(pathname, item.path, flatPaths)
}

function SubNavLink({
  item,
  onNavigate,
  flatPaths,
}: {
  item: AdminNavSubItem
  onNavigate: () => void
  flatPaths: readonly { path: string }[]
}) {
  const { pathname } = useLocation()
  const active = isActive(pathname, item.path, flatPaths)

  return (
    <Link
      to={item.path}
      onClick={onNavigate}
      className={cx('kit-sidebar-sub-item', active && 'is-active')}
    >
      {item.label}
    </Link>
  )
}

function NavItem({
  item,
  collapsed,
  expanded,
  onToggle,
  onNavigate,
  flatPaths,
}: {
  item: AdminNavItem
  collapsed: boolean
  expanded: boolean
  onToggle: () => void
  onNavigate: () => void
  flatPaths: readonly { path: string }[]
}) {
  const { pathname } = useLocation()
  const Icon = item.icon
  const hasChildren = Boolean(item.children?.length)
  const sectionActive = isSectionActive(pathname, item, flatPaths)
  const leafActive = !hasChildren && isActive(pathname, item.path, flatPaths)

  if (hasChildren) {
    return (
      <div className="kit-sidebar-section">
        {sectionActive && !collapsed ? (
          <span className="kit-sidebar-active-bar" aria-hidden />
        ) : null}

        <button
          type="button"
          onClick={onToggle}
          title={collapsed ? item.label : undefined}
          className={cx(
            'kit-sidebar-item',
            collapsed && 'is-collapsed',
            sectionActive && 'is-active',
          )}
        >
          <Icon className="kit-sidebar-item-icon" />
          {!collapsed ? (
            <>
              <span className="kit-sidebar-item-label">{item.label}</span>
              {expanded ? (
                <ChevronUp className="kit-sidebar-item-chevron" />
              ) : (
                <ChevronDown className="kit-sidebar-item-chevron" />
              )}
            </>
          ) : null}
        </button>

        {!collapsed && expanded && item.children ? (
          <div className="kit-sidebar-sub-list">
            {item.children.map((child) => (
              <SubNavLink
                key={child.id}
                item={child}
                flatPaths={flatPaths}
                onNavigate={onNavigate}
              />
            ))}
          </div>
        ) : null}
      </div>
    )
  }

  return (
    <div className="kit-sidebar-section">
      {leafActive && !collapsed ? (
        <span className="kit-sidebar-active-bar" aria-hidden />
      ) : null}

      <Link
        to={item.path}
        onClick={onNavigate}
        title={collapsed ? item.label : undefined}
        className={cx(
          'kit-sidebar-item',
          collapsed && 'is-collapsed',
          leafActive && 'is-active',
        )}
      >
        <Icon className="kit-sidebar-item-icon" />
        {!collapsed ? (
          <>
            <span className="kit-sidebar-item-label">{item.label}</span>
            {item.badge != null ? (
              <span className="kit-sidebar-badge">{item.badge}</span>
            ) : null}
          </>
        ) : null}
      </Link>
    </div>
  )
}

function NavLinks({
  items,
  collapsed,
  expandedMap,
  onToggle,
  onNavigate,
  flatPaths,
}: {
  items: AdminNavItem[]
  collapsed: boolean
  expandedMap: Record<string, boolean>
  onToggle: (id: string) => void
  onNavigate: () => void
  flatPaths: readonly { path: string }[]
}) {
  return (
    <div className="kit-sidebar-nav-list">
      {items.map((item) => (
        <NavItem
          key={item.id}
          item={item}
          collapsed={collapsed}
          expanded={Boolean(expandedMap[item.id])}
          onToggle={() => onToggle(item.id)}
          onNavigate={onNavigate}
          flatPaths={flatPaths}
        />
      ))}
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
  const { pathname } = useLocation()
  const allItems = useMemo(
    () => [...mainNav, ...secondaryNav],
    [mainNav, secondaryNav],
  )
  const flatPaths = useMemo(() => {
    const paths: { path: string }[] = []
    for (const item of allItems) {
      if (item.children?.length) {
        for (const child of item.children) paths.push(child)
      } else {
        paths.push(item)
      }
    }
    return paths
  }, [allItems])
  const [expandedMap, setExpandedMap] = useState<Record<string, boolean>>({})

  useEffect(() => {
    setExpandedMap((prev) => {
      const next = { ...prev }
      for (const item of allItems) {
        if (item.children?.some((child) => navItemMatches(pathname, child.path))) {
          next[item.id] = true
        }
      }
      return next
    })
  }, [pathname, allItems])

  const toggleExpanded = (id: string) => {
    setExpandedMap((prev) => ({ ...prev, [id]: !prev[id] }))
  }

  return (
    <div className="kit-sidebar">
      <div className="kit-sidebar-brand">
        <div className="kit-sidebar-brand-mark" aria-hidden>
          <span className="kit-sidebar-brand-letter">
            {(logoTitle ?? 'L').charAt(0).toUpperCase()}
          </span>
        </div>
        {!collapsed ? (
          <div className="kit-sidebar-brand-text">
            <div className="kit-sidebar-brand-row">
              <p className="kit-sidebar-brand-title">{logoTitle}</p>
              {logoSubtitle ? (
                <span className="kit-sidebar-brand-tag">{logoSubtitle}</span>
              ) : null}
            </div>
          </div>
        ) : null}
        {showClose ? (
          <button
            type="button"
            onClick={onClose}
            aria-label="Đóng menu"
            className="kit-sidebar-close"
          >
            <X className="h-5 w-5" />
          </button>
        ) : null}
      </div>

      <nav className="kit-sidebar-nav">
        {mainNav.length > 0 ? (
          <NavLinks
            items={mainNav}
            collapsed={collapsed}
            expandedMap={expandedMap}
            onToggle={toggleExpanded}
            onNavigate={onNavigate}
            flatPaths={flatPaths}
          />
        ) : null}

        {mainNav.length > 0 && secondaryNav.length > 0 && !collapsed ? (
          <div className="kit-sidebar-divider" />
        ) : null}

        {secondaryNav.length > 0 ? (
          <NavLinks
            items={secondaryNav}
            collapsed={collapsed}
            expandedMap={expandedMap}
            onToggle={toggleExpanded}
            onNavigate={onNavigate}
            flatPaths={flatPaths}
          />
        ) : null}
      </nav>

      {onLogout ? (
        <div className="kit-sidebar-footer">
          <button
            type="button"
            title={collapsed ? 'Đăng xuất' : undefined}
            onClick={onLogout}
            className={cx('kit-sidebar-item', collapsed && 'is-collapsed')}
          >
            <LogOut className="kit-sidebar-item-icon" />
            {!collapsed ? <span className="kit-sidebar-item-label">Đăng xuất</span> : null}
          </button>
        </div>
      ) : null}

      {onToggleCollapse ? (
        <button
          type="button"
          onClick={onToggleCollapse}
          aria-label={collapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'}
          className="kit-sidebar-toggle"
        >
          {collapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronLeft className="h-4 w-4" />
          )}
        </button>
      ) : null}
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
      <aside
        className={cx(
          'kit-sidebar-desktop',
          collapsed ? 'is-collapsed' : 'is-expanded',
          className,
        )}
      >
        <NavBody
          collapsed={collapsed}
          {...bodyProps}
          onToggleCollapse={() => setCollapsed((v) => !v)}
        />
      </aside>

      <div
        className={cx('kit-sidebar-mobile-root', mobileOpen && 'is-open')}
        aria-hidden={!mobileOpen}
      >
        <div
          className={cx(
            'kit-sidebar-mobile-backdrop',
            mobileOpen && 'is-visible',
          )}
          onClick={closeMobile}
        />
        <aside className="kit-sidebar-mobile-panel">
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
