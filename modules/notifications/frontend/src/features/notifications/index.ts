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
  NotificationRealtimeMessage,
} from './types'

// Config
export {
  notificationApiBase,
  notificationAuthHeaders,
  configureNotificationAuth,
  configureNotificationRealtime,
  configureNotificationTenant,
  configureNotificationOrg,
  getNotificationAuthHeaders,
  getNotificationRealtimeConfig,
  notificationSseUrl,
} from './config'
export type { NotificationRealtimeConfig } from './config'
