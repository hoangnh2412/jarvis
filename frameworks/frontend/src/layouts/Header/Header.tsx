import {
  Bell,
  HelpCircle,
  LayoutGrid,
  LogOut,
  Megaphone,
  Menu,
  MessageCircle,
  ShoppingCart,
  User,
  UserPlus,
} from 'lucide-react'
import type { ChangeEvent, ReactNode } from 'react'
import { Badge } from 'primereact/badge'
import { Button } from 'primereact/button'
import { InputText } from 'primereact/inputtext'
import { OverlayBadge } from 'primereact/overlaybadge'
import { Popover } from 'primereact/popover'
import { Toolbar } from 'primereact/toolbar'

export type HeaderProps = {
  title?: string
  onMenuClick?: () => void
  user?: { fullName?: string; email?: string } | null
  notificationCount?: number
  announcementCount?: number
  cartCount?: number
  /** Thay chuông mặc định (ví dụ `<NotificationBell />`). */
  notificationSlot?: ReactNode
  onNotificationClick?: () => void
  searchPlaceholder?: string
  searchValue?: string
  onSearchChange?: (value: string) => void
  onProfileClick?: () => void
  onLogout?: () => void
  className?: string
}

type HeaderIconButtonProps = {
  label: string
  children: ReactNode
  badge?: number
  onClick?: () => void
}

function HeaderIconButton({ label, children, badge = 0, onClick }: HeaderIconButtonProps) {
  const button = (
    <button
      type="button"
      aria-label={label}
      className="kit-admin-header-action"
      onClick={onClick}
    >
      {children}
    </button>
  )

  if (badge <= 0) return button

  return (
    <OverlayBadge className="kit-admin-header-action-badge">
      {button}
      <Badge shape="circle" severity="danger">
        {badge > 99 ? '99+' : badge}
      </Badge>
    </OverlayBadge>
  )
}

export function Header({
  onMenuClick,
  user = null,
  notificationCount = 0,
  announcementCount = 0,
  cartCount = 0,
  notificationSlot,
  onNotificationClick,
  searchPlaceholder = 'Tìm kiếm...',
  searchValue,
  onSearchChange,
  onProfileClick,
  onLogout,
  className = '',
}: HeaderProps) {
  return (
    <Toolbar.Root className={['kit-admin-header', className].filter(Boolean).join(' ')}>
      <Toolbar.Start className="kit-admin-header-start">
        {onMenuClick ? (
          <Button
            type="button"
            unstyled
            onClick={onMenuClick}
            aria-label="Mở menu"
            className="kit-header-menu-btn pr-btn-text"
          >
            <Menu className="kit-admin-header-menu-icon" aria-hidden />
          </Button>
        ) : null}

        <div className="kit-admin-header-left-tools">
          <button type="button" aria-label="Ứng dụng" className="kit-admin-header-apps-btn">
            <LayoutGrid className="kit-admin-header-apps-icon" aria-hidden />
          </button>
          <div className="kit-admin-header-search-wrap">
            <InputText
              type="search"
              value={searchValue}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                onSearchChange?.(e.target.value)
              }
              placeholder={searchPlaceholder}
              unstyled
              className="kit-admin-header-search"
            />
          </div>
        </div>
      </Toolbar.Start>

      <Toolbar.End className="kit-admin-header-end">
        <div className="kit-admin-header-actions">
          <HeaderIconButton label="Thông báo hệ thống" badge={announcementCount}>
            <Megaphone className="kit-admin-header-action-icon" aria-hidden />
          </HeaderIconButton>

          <HeaderIconButton label="Giỏ hàng" badge={cartCount}>
            <ShoppingCart className="kit-admin-header-action-icon" aria-hidden />
          </HeaderIconButton>

          <HeaderIconButton label="Thêm người dùng">
            <UserPlus className="kit-admin-header-action-icon" aria-hidden />
          </HeaderIconButton>

          <HeaderIconButton label="Tin nhắn">
            <MessageCircle className="kit-admin-header-action-icon" aria-hidden />
          </HeaderIconButton>

          {notificationSlot ?? (
            <HeaderIconButton
              label="Thông báo"
              badge={notificationCount}
              onClick={onNotificationClick}
            >
              <Bell className="kit-admin-header-action-icon" aria-hidden />
            </HeaderIconButton>
          )}

          <HeaderIconButton label="Trợ giúp">
            <HelpCircle className="kit-admin-header-action-icon" aria-hidden />
          </HeaderIconButton>

        </div>

        <Popover.Root>
          <Popover.Trigger className="kit-admin-header-profile-trigger" aria-label="Tài khoản">
            <div className="kit-admin-header-avatar">
              <User className="kit-admin-header-avatar-icon" aria-hidden />
            </div>
            <div className="kit-admin-header-user">
              <p className="kit-admin-header-user-name">
                {user?.fullName ?? 'Quản trị viên'}
              </p>
              <p className="kit-admin-header-user-email">{user?.email ?? 'Admin'}</p>
            </div>
          </Popover.Trigger>
          <Popover.Portal>
            <Popover.Positioner>
              <Popover.Popup className="kit-admin-header-profile-menu">
                <p className="kit-admin-header-profile-menu-label">Tài khoản</p>
                {onProfileClick ? (
                  <button
                    type="button"
                    className="kit-admin-header-profile-menu-item"
                    onClick={onProfileClick}
                  >
                    <User className="kit-admin-header-profile-menu-icon" aria-hidden />
                    Hồ sơ
                  </button>
                ) : null}
                {onLogout ? (
                  <>
                    <div className="kit-admin-header-profile-menu-divider" />
                    <button
                      type="button"
                      className="kit-admin-header-profile-menu-item is-danger"
                      onClick={onLogout}
                    >
                      <LogOut className="kit-admin-header-profile-menu-icon" aria-hidden />
                      Đăng xuất
                    </button>
                  </>
                ) : null}
              </Popover.Popup>
            </Popover.Positioner>
          </Popover.Portal>
        </Popover.Root>
      </Toolbar.End>
    </Toolbar.Root>
  )
}
