// Components
export { NotificationBell } from './components/NotificationBell'
export type { NotificationBellProps } from './components/NotificationBell'

export { NotificationCenter } from './components/NotificationCenter'
export type { NotificationCenterProps } from './components/NotificationCenter'

// Hooks
export { useNotifications } from './hooks'
export type { UseNotificationsResult } from './hooks'

// Services
export {
  getNotifications,
  markNotificationsRead,
  markNotificationsUnread,
  markAllNotificationsRead,
  toNotificationItem,
} from './services'

// Types
export type {
  NotificationData,
  NotificationFilter,
  NotificationItem,
  NotificationListResult,
} from './types'

// Config
export {
  notificationApiBase,
  notificationAuthHeaders,
  notificationHubOptions,
  buildNotificationHubOptions,
  configureNotificationAuth,
  configureNotificationTenant,
  getNotificationAuthHeaders,
  notificationHubUrl,
} from './config'
