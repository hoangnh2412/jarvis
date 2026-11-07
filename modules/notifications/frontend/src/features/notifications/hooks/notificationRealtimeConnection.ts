import type { NotificationRealtimeMessage } from '../types'
import { subscribeNotificationSse } from './notificationSseConnection'

export type { NotificationRealtimeMessage }

type MessageListener = (message: NotificationRealtimeMessage) => void
type ErrorListener = (message: string | null) => void

/** Chuông nhận realtime qua SSE. */
export function subscribeNotificationRealtime(options: {
  onMessage: MessageListener
  onError?: ErrorListener
}) {
  return subscribeNotificationSse({
    onMessage: options.onMessage,
    onError: options.onError,
  })
}
