import { Button, notify } from '@jarvis/core'
import {
  Bell,
  BellOff,
  Check,
  CheckCheck,
  ChevronDown,
  ExternalLink,
  MessageCircle,
  MoreHorizontal,
  RefreshCw,
  ShieldCheck,
  UserPlus,
} from 'lucide-react'
import { useCallback, useEffect, useRef, useState } from 'react'
import type { ComponentType } from 'react'
import {
  useNotifications,
  type UseNotificationsResult,
} from '../../hooks'
import type { NotificationItem } from '../../types'

const iconByType: Record<string, ComponentType<{ size?: number }>> = {
  comment: MessageCircle,
  friend: UserPlus,
  security: ShieldCheck,
}

const rtf = new Intl.RelativeTimeFormat('vi', { numeric: 'auto' })

function relativeTime(value: string) {
  const milliseconds = new Date(value).getTime() - Date.now()
  const ranges: Array<[Intl.RelativeTimeFormatUnit, number]> = [
    ['year', 365 * 24 * 60 * 60 * 1000],
    ['month', 30 * 24 * 60 * 60 * 1000],
    ['day', 24 * 60 * 60 * 1000],
    ['hour', 60 * 60 * 1000],
    ['minute', 60 * 1000],
  ]
  for (const [unit, size] of ranges) {
    if (Math.abs(milliseconds) >= size) {
      return rtf.format(Math.round(milliseconds / size), unit)
    }
  }
  return 'vừa xong'
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat('vi-VN', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

function getInitials(item: NotificationItem) {
  const name = item.data?.actorName || item.title
  return String(name)
    .split(/\s+/)
    .slice(-2)
    .map((part) => part[0])
    .join('')
    .toUpperCase()
}

function resolveActionUrl(raw: string) {
  const url = raw.trim()
  if (/^https?:\/\//i.test(url)) return url
  if (url.startsWith('/')) {
    return new URL(url, window.location.origin).href
  }
  return new URL(`/${url}`, window.location.origin).href
}

function openActionUrl(raw: string) {
  window.open(resolveActionUrl(raw), '_blank', 'noopener,noreferrer')
}

export type NotificationCenterProps = {
  /**
   * `page` — full page standalone
   * `embedded` — trong layout content
   * `popover` — dropdown nhỏ kiểu Facebook
   */
  variant?: 'page' | 'embedded' | 'popover'
  /** @deprecated dùng `variant="embedded"` */
  embedded?: boolean
  /** Dùng chung state với NotificationBell (tránh double fetch). */
  controller?: UseNotificationsResult
}

export function NotificationCenter({
  variant,
  embedded = false,
  controller,
}: NotificationCenterProps = {}) {
  if (controller) {
    return (
      <NotificationCenterView
        variant={variant}
        embedded={embedded}
        notifications={controller}
      />
    )
  }

  return (
    <NotificationCenterConnected variant={variant} embedded={embedded} />
  )
}

function NotificationCenterConnected({
  variant,
  embedded = false,
}: Omit<NotificationCenterProps, 'controller'>) {
  const notifications = useNotifications()
  return (
    <NotificationCenterView
      variant={variant}
      embedded={embedded}
      notifications={notifications}
    />
  )
}

const LONG_PRESS_MS = 480
const SELECT_ALL_PROMPT_THRESHOLD = 5

function NotificationCenterView({
  variant,
  embedded = false,
  notifications,
}: Omit<NotificationCenterProps, 'controller'> & {
  notifications: UseNotificationsResult
}) {
  const {
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
    reload,
    loadMore,
    markRead,
    markUnread,
    markManyRead,
    markManyUnread,
    markAllRead,
    dismissFromUnreadList,
  } = notifications
  const resolvedVariant = variant ?? (embedded ? 'embedded' : 'page')
  const [selectionMode, setSelectionMode] = useState(false)
  const [selectedIds, setSelectedIds] = useState<Set<string>>(() => new Set())
  const [expandedIds, setExpandedIds] = useState<Set<string>>(() => new Set())
  const contentRef = useRef<HTMLDivElement>(null)
  const loadMoreSentinelRef = useRef<HTMLDivElement>(null)

  const run = async (action: () => Promise<void>, message: string) => {
    try {
      await action()
    } catch (error) {
      notify.error(error instanceof Error ? error.message : message)
    }
  }

  const isPopover = resolvedVariant === 'popover'

  const exitSelection = useCallback(() => {
    setSelectionMode(false)
    setSelectedIds(new Set())
  }, [])

  const toggleSelected = useCallback((notificationId: string) => {
    setSelectedIds((current) => {
      const next = new Set(current)
      if (next.has(notificationId)) next.delete(notificationId)
      else next.add(notificationId)
      return next
    })
  }, [])

  const enterSelectionWith = useCallback((notificationId: string) => {
    setSelectionMode(true)
    setSelectedIds(new Set([notificationId]))
    setExpandedIds(new Set())
  }, [])

  const selectAllLoaded = useCallback(() => {
    setSelectedIds(new Set(items.map((item) => item.notificationId)))
  }, [items])

  const handleFilterChange = (next: 'all' | 'unread') => {
    exitSelection()
    setExpandedIds(new Set())
    setFilter(next)
  }

  const handleExpandToggle = (item: NotificationItem) => {
    const isExpanded = expandedIds.has(item.notificationId)
    if (isExpanded) {
      setExpandedIds((current) => {
        const next = new Set(current)
        next.delete(item.notificationId)
        return next
      })
      if (filter === 'unread' && item.isRead) {
        dismissFromUnreadList(item.notificationId)
      }
      return
    }

    setExpandedIds((current) => new Set(current).add(item.notificationId))
    if (!item.isRead) {
      void run(
        () => markRead(item, { retainInList: true }),
        'Không thể cập nhật thông báo.',
      )
    }
  }

  const selectedItems = items.filter((item) =>
    selectedIds.has(item.notificationId),
  )
  const hasUnreadSelected = selectedItems.some((item) => !item.isRead)
  const hasReadSelected = selectedItems.some((item) => item.isRead)
  const allLoadedSelected =
    items.length > 0 &&
    items.every((item) => selectedIds.has(item.notificationId))
  const showSelectAll =
    selectionMode &&
    selectedIds.size >= SELECT_ALL_PROMPT_THRESHOLD &&
    !allLoadedSelected

  useEffect(() => {
    if (!selectionMode) return
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') exitSelection()
    }
    document.addEventListener('keydown', onKeyDown)
    return () => document.removeEventListener('keydown', onKeyDown)
  }, [exitSelection, selectionMode])

  useEffect(() => {
    const sentinel = loadMoreSentinelRef.current
    if (!sentinel || !hasMore) return

    const scrollRoot = contentRef.current
    const root =
      scrollRoot && scrollRoot.scrollHeight > scrollRoot.clientHeight
        ? scrollRoot
        : null

    const observer = new IntersectionObserver(
      (entries) => {
        const entry = entries[0]
        if (
          entry?.isIntersecting &&
          hasMore &&
          !loadingMore &&
          !loading
        ) {
          void loadMore()
        }
      },
      { root, rootMargin: '160px', threshold: 0 },
    )

    observer.observe(sentinel)
    return () => observer.disconnect()
  }, [hasMore, items.length, loadMore, loading, loadingMore])

  return (
    <div
      className={[
        'notification-page',
        resolvedVariant === 'embedded' ? 'notification-page--embedded' : '',
        isPopover ? 'notification-page--popover' : '',
        selectionMode ? 'notification-page--selecting' : '',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <section
        className="notification-panel"
        aria-labelledby="notification-title"
      >
        <header className="notification-header">
          <div>
            {resolvedVariant === 'page' && (
              <p className="notification-eyebrow">
                <Bell size={15} aria-hidden />
                Trung tâm cập nhật
              </p>
            )}
            <h1 id="notification-title">Thông báo</h1>
          </div>
          <div className="notification-header__actions">
            {!selectionMode && unreadCount > 0 && (
              <Button
                unstyled
                type="button"
                className="notification-text-button"
                aria-label="Đánh dấu tất cả là đã đọc"
                title="Đánh dấu tất cả là đã đọc"
                onClick={() =>
                  void run(
                    markAllRead,
                    'Không thể đánh dấu tất cả là đã đọc.',
                  )
                }
              >
                <CheckCheck size={isPopover ? 16 : 18} aria-hidden />
                <span>
                  {isPopover
                    ? 'Đánh dấu tất cả'
                    : 'Đánh dấu tất cả là đã đọc'}
                </span>
              </Button>
            )}
          </div>
        </header>

        <nav className="notification-tabs" aria-label="Bộ lọc thông báo">
          <button
            type="button"
            className={filter === 'all' ? 'is-active' : ''}
            aria-current={filter === 'all' ? 'page' : undefined}
            onClick={() => handleFilterChange('all')}
          >
            Tất cả
          </button>
          <button
            type="button"
            className={filter === 'unread' ? 'is-active' : ''}
            aria-current={
              filter === 'unread' ? 'page' : undefined
            }
            onClick={() => handleFilterChange('unread')}
          >
            Chưa đọc
            {unreadCount > 0 && (
              <span>{Math.min(unreadCount, 99)}</span>
            )}
          </button>
        </nav>

        {selectionMode && (
          <div
            className="notification-selection-bar"
            role="toolbar"
            aria-label="Thao tác chọn nhiều"
          >
            <span className="notification-selection-bar__count">
              Đã chọn {selectedIds.size}
              {showSelectAll && (
                <Button
                  unstyled
                  type="button"
                  className="notification-selection-bar__select-all"
                  aria-label={`Chọn tất cả ${items.length} thông báo đang hiển thị`}
                  onClick={selectAllLoaded}
                >
                  Chọn tất cả
                </Button>
              )}
            </span>
            <div className="notification-selection-bar__actions">
              <Button
                unstyled
                type="button"
                className="notification-selection-bar__button"
                disabled={!hasUnreadSelected}
                onClick={() =>
                  void run(async () => {
                    await markManyRead([...selectedIds])
                    exitSelection()
                  }, 'Không thể đánh dấu đã đọc.')
                }
              >
                <Check size={16} aria-hidden />
                Đã đọc
              </Button>
              <Button
                unstyled
                type="button"
                className="notification-selection-bar__button"
                disabled={!hasReadSelected}
                onClick={() =>
                  void run(async () => {
                    await markManyUnread([...selectedIds])
                    exitSelection()
                  }, 'Không thể đánh dấu chưa đọc.')
                }
              >
                <BellOff size={16} aria-hidden />
                Chưa đọc
              </Button>
              <Button
                unstyled
                type="button"
                className="notification-selection-bar__button notification-selection-bar__button--ghost"
                onClick={exitSelection}
              >
                Hủy
              </Button>
            </div>
          </div>
        )}

        {!selectionMode && (
          <p className="notification-hint">
            Giữ để chọn nhiều thông báo
          </p>
        )}

        <div
          ref={contentRef}
          className="notification-content"
          aria-live="polite"
        >
          {loading ? (
            <NotificationSkeleton compact={isPopover} />
          ) : error ? (
            <NotificationError
              message={error}
              onRetry={reload}
            />
          ) : items.length ? (
            <div className="notification-list">
              {!isPopover && (
                <p className="notification-section-title">
                  Mới nhất
                  {totalItems > 0 && (
                    <span className="notification-section-title__meta">
                      {items.length} / {totalItems}
                    </span>
                  )}
                </p>
              )}
              {items.map((item) => (
                <NotificationRow
                  key={item.notificationId}
                  item={item}
                  compact={isPopover}
                  selectionMode={selectionMode}
                  selected={selectedIds.has(item.notificationId)}
                  expanded={expandedIds.has(item.notificationId)}
                  onEnterSelection={() => enterSelectionWith(item.notificationId)}
                  onSelectToggle={() => toggleSelected(item.notificationId)}
                  onExpandToggle={() => handleExpandToggle(item)}
                  onMarkRead={() =>
                    void run(
                      () => markRead(item),
                      'Không thể cập nhật thông báo.',
                    )
                  }
                  onToggleRead={() =>
                    void run(
                      async () => {
                        if (item.isRead) {
                          await markUnread(item)
                        } else {
                          await markRead(item, {
                            retainInList: expandedIds.has(item.notificationId),
                          })
                        }
                      },
                      'Không thể cập nhật thông báo.',
                    )
                  }
                />
              ))}
              {hasMore && (
                <div
                  ref={loadMoreSentinelRef}
                  className="notification-load-more-sentinel"
                  aria-hidden
                >
                  {loadingMore && (
                    <p className="notification-load-more-status">
                      <RefreshCw size={16} className="is-spinning" aria-hidden />
                      Đang tải thêm…
                    </p>
                  )}
                </div>
              )}
              {!isPopover &&
                totalPages > 1 &&
                !hasMore && (
                  <p className="notification-pagination-meta">
                    Đã xem hết · trang {page} /{' '}
                    {totalPages}
                  </p>
                )}
            </div>
          ) : (
            <NotificationEmpty filter={filter} />
          )}
        </div>
      </section>
    </div>
  )
}

function NotificationRow({
  item,
  selectionMode,
  selected,
  expanded,
  onEnterSelection,
  onSelectToggle,
  onExpandToggle,
  onMarkRead,
  onToggleRead,
  compact = false,
}: {
  item: NotificationItem
  selectionMode: boolean
  selected: boolean
  expanded: boolean
  onEnterSelection: () => void
  onSelectToggle: () => void
  onExpandToggle: () => void
  onMarkRead: () => void
  onToggleRead: () => void
  compact?: boolean
}) {
  const [menuOpen, setMenuOpen] = useState(false)
  const menuRef = useRef<HTMLDivElement>(null)
  const longPressTimerRef = useRef<number | null>(null)
  const longPressTriggeredRef = useRef(false)
  const TypeIcon = iconByType[item.type.toLowerCase()] || Bell
  const actionUrl = item.data?.actionUrl
    ? String(item.data.actionUrl)
    : undefined

  const handleOpenLink = () => {
    if (!item.isRead) onMarkRead()
    if (actionUrl) openActionUrl(actionUrl)
  }

  useEffect(() => {
    if (!menuOpen) return
    const close = (event: MouseEvent) => {
      if (!menuRef.current?.contains(event.target as Node)) setMenuOpen(false)
    }
    document.addEventListener('mousedown', close)
    return () => document.removeEventListener('mousedown', close)
  }, [menuOpen])

  const clearLongPressTimer = () => {
    if (longPressTimerRef.current !== null) {
      window.clearTimeout(longPressTimerRef.current)
      longPressTimerRef.current = null
    }
  }

  const startLongPress = () => {
    if (selectionMode) return
    longPressTriggeredRef.current = false
    clearLongPressTimer()
    longPressTimerRef.current = window.setTimeout(() => {
      longPressTriggeredRef.current = true
      onEnterSelection()
    }, LONG_PRESS_MS)
  }

  const handleMainClick = () => {
    if (longPressTriggeredRef.current) {
      longPressTriggeredRef.current = false
      return
    }
    if (selectionMode) {
      onSelectToggle()
      return
    }
    onExpandToggle()
  }

  return (
    <article
      className={[
        'notification-row',
        item.isRead ? '' : 'is-unread',
        expanded ? 'is-expanded' : '',
        selected ? 'is-selected' : '',
        selectionMode ? 'notification-row--selecting' : '',
        compact ? 'notification-row--compact' : '',
        actionUrl ? 'notification-row--has-link' : '',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      {!selectionMode && !item.isRead && (
        <span className="notification-unread-dot" title="Chưa đọc" />
      )}

      <div className="notification-row__header">
        {selectionMode && (
          <label className="notification-row__checkbox">
            <input
              type="checkbox"
              checked={selected}
              onChange={onSelectToggle}
              aria-label={`Chọn ${item.title}`}
            />
            <span aria-hidden />
          </label>
        )}
        <button
          type="button"
          className="notification-row__main"
          aria-expanded={expanded}
          onClick={handleMainClick}
          onPointerDown={startLongPress}
          onPointerUp={clearLongPressTimer}
          onPointerLeave={clearLongPressTimer}
          onPointerCancel={clearLongPressTimer}
          onContextMenu={(event) => event.preventDefault()}
        >
          <span className="notification-avatar">
            <span className="notification-avatar__fallback">
              {getInitials(item)}
            </span>
            <span className="notification-avatar__type" aria-hidden>
              <TypeIcon size={compact ? 14 : 16} />
            </span>
          </span>
          <span className="notification-row__copy">
            <span className="notification-row__title">{item.title}</span>
            {item.body && (
              <span
                className={[
                  'notification-row__body',
                  expanded ? 'notification-row__body--collapsed-hidden' : '',
                ]
                  .filter(Boolean)
                  .join(' ')}
              >
                {item.body}
              </span>
            )}
            <time dateTime={item.createdAtUtc}>
              {relativeTime(item.createdAtUtc)}
            </time>
          </span>
          <ChevronDown
            size={compact ? 16 : 18}
            className={[
              'notification-row__chevron',
              expanded ? 'is-expanded' : '',
            ]
              .filter(Boolean)
              .join(' ')}
            aria-hidden
          />
        </button>

        <div className="notification-row__side" ref={menuRef}>
          {!selectionMode && (
            <div className="notification-row__actions">
              {actionUrl && (
                <button
                  type="button"
                  className="notification-action notification-action--link"
                  aria-label={`Mở liên kết: ${item.title}`}
                  title="Mở liên kết"
                  onClick={(event) => {
                    event.stopPropagation()
                    handleOpenLink()
                  }}
                >
                  <ExternalLink size={compact ? 16 : 18} aria-hidden />
                </button>
              )}
              <button
                type="button"
                className="notification-action"
                aria-label={
                  item.isRead
                    ? `Đánh dấu ${item.title} là chưa đọc`
                    : `Đánh dấu ${item.title} là đã đọc`
                }
                onClick={(event) => {
                  event.stopPropagation()
                  onToggleRead()
                }}
              >
                {item.isRead ? (
                  <BellOff size={compact ? 16 : 18} aria-hidden />
                ) : (
                  <Check size={compact ? 16 : 18} aria-hidden />
                )}
              </button>
              <button
                type="button"
                className="notification-more"
                aria-label={`Tùy chọn cho ${item.title}`}
                aria-expanded={menuOpen}
                onClick={(event) => {
                  event.stopPropagation()
                  setMenuOpen((open) => !open)
                }}
              >
                <MoreHorizontal size={compact ? 18 : 20} aria-hidden />
              </button>
            </div>
          )}
          {menuOpen && (
            <div className="notification-menu" role="menu">
              {actionUrl && (
                <button
                  type="button"
                  role="menuitem"
                  onClick={() => {
                    setMenuOpen(false)
                    handleOpenLink()
                  }}
                >
                  <ExternalLink size={18} aria-hidden />
                  Mở liên kết
                </button>
              )}
              <button
                type="button"
                role="menuitem"
                onClick={() => {
                  setMenuOpen(false)
                  onToggleRead()
                }}
              >
                {item.isRead ? (
                  <BellOff size={18} aria-hidden />
                ) : (
                  <Check size={18} aria-hidden />
                )}
                {item.isRead
                  ? 'Đánh dấu là chưa đọc'
                  : 'Đánh dấu là đã đọc'}
              </button>
            </div>
          )}
        </div>
      </div>

      {expanded && (
        <div className="notification-row__detail">
          {item.data?.actorName && (
            <p className="notification-row__detail-actor">
              {String(item.data.actorName)}
            </p>
          )}
          {item.body ? (
            <p className="notification-row__detail-body">{item.body}</p>
          ) : (
            <p className="notification-row__detail-body notification-row__detail-body--muted">
              Không có nội dung chi tiết.
            </p>
          )}
          <div className="notification-row__detail-meta">
            <span>{formatDateTime(item.createdAtUtc)}</span>
            <span className="notification-row__detail-type">{item.type}</span>
          </div>
          {actionUrl && (
            <button
              type="button"
              className="notification-row__detail-link"
              onClick={(event) => {
                event.stopPropagation()
                handleOpenLink()
              }}
            >
              Mở liên kết
              <ExternalLink size={14} aria-hidden />
            </button>
          )}
        </div>
      )}
    </article>
  )
}

function NotificationEmpty({ filter }: { filter: 'all' | 'unread' }) {
  return (
    <div className="notification-state">
      <span className="notification-state__icon">
        <BellOff size={30} aria-hidden />
      </span>
      <h2>
        {filter === 'unread'
          ? 'Bạn đã xem hết thông báo'
          : 'Chưa có thông báo nào'}
      </h2>
      <p>
        {filter === 'unread'
          ? 'Thông báo mới sẽ xuất hiện tại đây.'
          : 'Khi có cập nhật mới, bạn sẽ nhìn thấy chúng ở đây.'}
      </p>
    </div>
  )
}

function NotificationError({
  message,
  onRetry,
}: {
  message: string
  onRetry: () => void
}) {
  return (
    <div className="notification-state">
      <span className="notification-state__icon is-error">
        <BellOff size={30} aria-hidden />
      </span>
      <h2>Không thể tải thông báo</h2>
      <p>{message}</p>
      <Button
        unstyled
        type="button"
        className="notification-retry"
        onClick={onRetry}
      >
        Thử lại
      </Button>
    </div>
  )
}

function NotificationSkeleton({ compact = false }: { compact?: boolean }) {
  return (
    <div className="notification-skeleton" aria-label="Đang tải thông báo">
      {Array.from({ length: compact ? 4 : 6 }, (_, index) => (
        <div className="notification-skeleton__row" key={index}>
          <span className="notification-skeleton__avatar" />
          <span className="notification-skeleton__copy">
            <span />
            <span />
            <span />
          </span>
        </div>
      ))}
    </div>
  )
}
