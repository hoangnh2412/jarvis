import { Bell } from 'lucide-react'
import { Popover } from 'primereact/popover'
import { useNotifications } from '../../hooks'
import { NotificationCenter } from '../NotificationCenter'

export type NotificationBellProps = {
  className?: string
}

/** Chuông header + dropdown thông báo (kiểu Facebook). */
export function NotificationBell({ className = '' }: NotificationBellProps) {
  const notifications = useNotifications()
  const { unreadCount, reload } = notifications
  const badgeLabel =
    unreadCount > 99 ? '99+' : unreadCount > 0 ? String(unreadCount) : null

  const handleBellClick = () => {
    void reload()
  }

  return (
    <Popover.Root>
      <Popover.Trigger
        className={['notification-bell-trigger', className]
          .filter(Boolean)
          .join(' ')}
        aria-label={
          unreadCount > 0
            ? `Thông báo, ${unreadCount} chưa đọc`
            : 'Thông báo'
        }
        title="Thông báo"
        onClick={handleBellClick}
      >
        <span className="notification-bell-icon">
          <Bell className="size-6" aria-hidden />
          {badgeLabel && (
            <span className="notification-bell-badge">{badgeLabel}</span>
          )}
        </span>
      </Popover.Trigger>
      <Popover.Portal>
        <Popover.Positioner side="bottom" align="end" sideOffset={10}>
          <Popover.Popup className="notification-bell-popup">
            <NotificationCenter
              variant="popover"
              controller={notifications}
            />
          </Popover.Popup>
        </Popover.Positioner>
      </Popover.Portal>
    </Popover.Root>
  )
}
