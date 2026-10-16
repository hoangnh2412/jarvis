import type { NotificationHubMessage } from './notificationHubConnection'
import { subscribeNotificationHub } from './notificationHubConnection'
import { subscribeNotificationSse } from './notificationSseConnection'
import { getNotificationRealtimeConfig } from '../config'

export type NotificationRealtimeMessage = NotificationHubMessage

type MessageListener = (message: NotificationRealtimeMessage) => void
type ErrorListener = (message: string | null) => void

/** Chọn SignalR hoặc SSE theo `configureNotificationRealtime` / env. */
export function subscribeNotificationRealtime(options: {
  onMessage: MessageListener
  onError?: ErrorListener
}) {
  const { transport } = getNotificationRealtimeConfig()

  if (transport === 'sse') {
    return subscribeNotificationSse({
      onMessage: options.onMessage,
      onError: options.onError,
    })
  }

  return subscribeNotificationHub({
    onMessage: options.onMessage,
    onHubError: options.onError,
  })
}
