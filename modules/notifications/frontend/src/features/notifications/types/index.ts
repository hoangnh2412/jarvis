export type NotificationFilter = 'all' | 'unread'

export type NotificationData = {
  actionUrl?: string
  actorName?: string
  actorAvatarUrl?: string
  imageUrl?: string
  icon?: string
  [key: string]: unknown
}

export type NotificationItem = {
  notificationId: string
  type: string
  title: string
  body?: string | null
  data?: NotificationData | null
  /** Computed by API from ZSET membership; not stored in Redis item JSON. */
  isRead: boolean
  createdAtUtc: string
}

/** Matches backend `NotificationListResult` : `IPagedDto<NotificationItem>` + unreadCount. */
export type NotificationListResult = {
  data: NotificationItem[]
  unreadCount: number
  page: number
  size: number
  totalItems: number
  totalPages: number
}
