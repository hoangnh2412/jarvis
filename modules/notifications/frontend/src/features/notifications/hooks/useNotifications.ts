import { useCallback, useEffect, useRef, useState } from 'react'
import { subscribeNotificationHub } from './notificationHubConnection'
import {
  getNotifications,
  markAllNotificationsRead,
  markNotificationsRead,
  markNotificationsUnread,
  toNotificationItem,
} from '../services'
import type {
  NotificationFilter,
  NotificationItem,
  NotificationListResult,
} from '../types'

const DEFAULT_PAGE_SIZE = 20

function uniqueItems(items: NotificationItem[]) {
  return Array.from(
    new Map(items.map((item) => [item.notificationId, item])).values(),
  )
}

export function useNotifications() {
  const [items, setItems] = useState<NotificationItem[]>([])
  const [filter, setFilter] = useState<NotificationFilter>('all')
  const [unreadCount, setUnreadCount] = useState(0)
  const [page, setPage] = useState(1)
  const [totalItems, setTotalItems] = useState(0)
  const [totalPages, setTotalPages] = useState(0)
  const [hasMore, setHasMore] = useState(false)
  const [loading, setLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [hubError, setHubError] = useState<string | null>(null)
  const requestId = useRef(0)
  const filterRef = useRef(filter)

  useEffect(() => {
    filterRef.current = filter
  }, [filter])

  const applyResult = useCallback(
    (result: NotificationListResult, append: boolean) => {
      const nextItems = result.data ?? []
      setItems((current) =>
        uniqueItems(append ? [...current, ...nextItems] : nextItems),
      )
      setUnreadCount(result.unreadCount)
      setPage(result.page)
      setTotalItems(result.totalItems)
      setTotalPages(result.totalPages)
      setHasMore(result.page < result.totalPages)
    },
    [],
  )

  const load = useCallback(
    async (nextPage = 1, append = false) => {
      const id = ++requestId.current
      if (append) {
        setLoadingMore(true)
      } else {
        setLoading(true)
      }
      setError(null)
      try {
        const result = await getNotifications(
          nextPage,
          DEFAULT_PAGE_SIZE,
          filter,
        )
        if (requestId.current === id) applyResult(result, append)
      } catch (cause) {
        if (requestId.current === id) {
          setError(
            cause instanceof Error
              ? cause.message
              : 'Không thể tải danh sách thông báo.',
          )
        }
      } finally {
        if (requestId.current === id) {
          setLoading(false)
          setLoadingMore(false)
        }
      }
    },
    [applyResult, filter],
  )

  useEffect(() => {
    // Fetching is the external synchronization performed by this effect.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
  }, [load])

  useEffect(() => {
    return subscribeNotificationHub({
      onMessage: (message) => {
        const item = toNotificationItem(message)
        setUnreadCount((count) => count + 1)
        setTotalItems((count) => count + 1)
        setItems((current) =>
          filterRef.current === 'all' || !item.isRead
            ? uniqueItems([item, ...current])
            : current,
        )
      },
      onHubError: setHubError,
    })
  }, [])

  const markRead = async (
    item: NotificationItem,
    options?: { retainInList?: boolean },
  ) => {
    if (item.isRead) return
    setItems((current) =>
      current.map((entry) =>
        entry.notificationId === item.notificationId
          ? { ...entry, isRead: true }
          : entry,
      ),
    )
    setUnreadCount((count) => Math.max(0, count - 1))
    try {
      await markNotificationsRead([item.notificationId])
      if (filter === 'unread' && !options?.retainInList) {
        setItems((current) =>
          current.filter(
            (entry) => entry.notificationId !== item.notificationId,
          ),
        )
        setTotalItems((count) => Math.max(0, count - 1))
      }
    } catch (cause) {
      void load()
      throw cause
    }
  }

  const dismissFromUnreadList = (notificationId: string) => {
    if (filter !== 'unread') return
    setItems((current) =>
      current.filter((entry) => entry.notificationId !== notificationId),
    )
    setTotalItems((count) => Math.max(0, count - 1))
  }

  const markManyRead = async (notificationIds: string[]) => {
    if (!notificationIds.length) return
    const idSet = new Set(notificationIds)
    const unreadSelected = items.filter(
      (entry) => idSet.has(entry.notificationId) && !entry.isRead,
    ).length
    const previous = items
    setItems((current) =>
      current.map((entry) =>
        idSet.has(entry.notificationId) ? { ...entry, isRead: true } : entry,
      ),
    )
    setUnreadCount((count) => Math.max(0, count - unreadSelected))
    try {
      await markNotificationsRead(notificationIds)
      if (filter === 'unread') {
        setItems((current) =>
          current.filter((entry) => !idSet.has(entry.notificationId)),
        )
        setTotalItems((count) => Math.max(0, count - unreadSelected))
      }
    } catch (cause) {
      setItems(previous)
      void load()
      throw cause
    }
  }

  const markManyUnread = async (notificationIds: string[]) => {
    if (!notificationIds.length) return
    const idSet = new Set(notificationIds)
    const readSelected = items.filter(
      (entry) => idSet.has(entry.notificationId) && entry.isRead,
    ).length
    const previous = items
    setItems((current) =>
      current.map((entry) =>
        idSet.has(entry.notificationId) ? { ...entry, isRead: false } : entry,
      ),
    )
    setUnreadCount((count) => count + readSelected)
    try {
      await markNotificationsUnread(notificationIds)
      if (filter === 'unread') {
        void load()
      }
    } catch (cause) {
      setItems(previous)
      void load()
      throw cause
    }
  }

  const markUnread = async (item: NotificationItem) => {
    if (!item.isRead) return
    setItems((current) =>
      current.map((entry) =>
        entry.notificationId === item.notificationId
          ? { ...entry, isRead: false }
          : entry,
      ),
    )
    setUnreadCount((count) => count + 1)
    try {
      await markNotificationsUnread([item.notificationId])
    } catch (cause) {
      void load()
      throw cause
    }
  }

  const markAllRead = async () => {
    const previous = items
    setItems((current) =>
      filter === 'unread'
        ? []
        : current.map((item) => ({ ...item, isRead: true })),
    )
    setUnreadCount(0)
    if (filter === 'unread') {
      setTotalItems(0)
      setTotalPages(0)
      setHasMore(false)
    }
    try {
      await markAllNotificationsRead()
    } catch (cause) {
      setItems(previous)
      void load()
      throw cause
    }
  }

  return {
    items,
    filter,
    setFilter,
    unreadCount,
    page,
    totalItems,
    totalPages,
    loading,
    loadingMore,
    hasMore,
    error,
    hubError,
    reload: () => load(),
    loadMore: () => load(page + 1, true),
    markRead,
    markUnread,
    markManyRead,
    markManyUnread,
    markAllRead,
    dismissFromUnreadList,
  }
}

export type UseNotificationsResult = ReturnType<typeof useNotifications>
