import {
  getNotificationAuthHeaders,
  notificationApiBase,
} from '../config'
import type {
  NotificationFilter,
  NotificationItem,
  NotificationListResult,
} from '../types'

type ApiEnvelope<T> = {
  data?: T
  message?: string
  title?: string
  error?: { message?: string; systemMessage?: string }
}

const readStatusByFilter: Record<NotificationFilter, 'All' | 'Unread'> = {
  all: 'All',
  unread: 'Unread',
}

async function request<T>(path = '', init?: RequestInit): Promise<T> {
  const response = await fetch(`${notificationApiBase}${path}`, {
    ...init,
    headers: {
      Accept: 'application/json',
      ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
      ...getNotificationAuthHeaders(),
      ...init?.headers,
    },
  })
  const text = await response.text()
  let payload: ApiEnvelope<T> | T | string | null = null

  if (text) {
    try {
      payload = JSON.parse(text) as ApiEnvelope<T> | T
    } catch {
      payload = text
    }
  }

  if (!response.ok) {
    const envelope =
      payload && typeof payload === 'object'
        ? (payload as ApiEnvelope<T>)
        : undefined
    throw new Error(
      envelope?.error?.message ||
        envelope?.error?.systemMessage ||
        envelope?.message ||
        envelope?.title ||
        (typeof payload === 'string' ? payload : '') ||
        `Không thể tải thông báo (HTTP ${response.status}).`,
    )
  }

  if (
    payload &&
    typeof payload === 'object' &&
    Object.prototype.hasOwnProperty.call(payload, 'data') &&
    !Object.prototype.hasOwnProperty.call(payload, 'totalPages')
  ) {
    return (payload as ApiEnvelope<T>).data as T
  }
  return payload as T
}

export function getNotifications(
  page: number,
  size: number,
  filter: NotificationFilter,
) {
  const params = new URLSearchParams({
    page: String(page),
    size: String(size),
    readStatus: readStatusByFilter[filter],
  })
  return request<NotificationListResult>(`?${params}`)
}

export function markNotificationsRead(notificationIds: string[]) {
  return request<void>('/read', {
    method: 'PUT',
    body: JSON.stringify({ notificationIds }),
  })
}

export function markNotificationsUnread(notificationIds: string[]) {
  return request<void>('/unread', {
    method: 'PUT',
    body: JSON.stringify({ notificationIds }),
  })
}

export function markAllNotificationsRead() {
  return request<void>('/read-all', { method: 'PUT' })
}

export function toNotificationItem(
  message: Omit<NotificationItem, 'isRead'>,
): NotificationItem {
  return { ...message, isRead: false }
}
