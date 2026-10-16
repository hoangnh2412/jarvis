import { fetchEventSource } from '@microsoft/fetch-event-source'
import {
  getNotificationAuthHeaders,
  getNotificationRealtimeConfig,
} from '../config'
import type { NotificationHubMessage } from './notificationHubConnection'

type MessageListener = (message: NotificationHubMessage) => void
type ErrorListener = (message: string | null) => void

let abortController: AbortController | null = null
let connectPromise: Promise<void> | null = null
let pageUnloadHookRegistered = false

const messageListeners = new Set<MessageListener>()
const errorListeners = new Set<ErrorListener>()

function notifyError(message: string | null) {
  for (const listener of errorListeners) {
    listener(message)
  }
}

function registerPageUnloadHook() {
  if (pageUnloadHookRegistered || typeof window === 'undefined') return
  pageUnloadHookRegistered = true
  window.addEventListener('pagehide', () => {
    abortController?.abort()
    abortController = null
    connectPromise = null
  })
}

async function startConnection() {
  if (connectPromise) {
    await connectPromise
    return
  }

  registerPageUnloadHook()

  abortController?.abort()
  const controller = new AbortController()
  abortController = controller

  const { sseUrl, eventName } = getNotificationRealtimeConfig()

  connectPromise = fetchEventSource(sseUrl, {
    signal: controller.signal,
    headers: getNotificationAuthHeaders(),
    openWhenHidden: true,
    onopen: async (response) => {
      const contentType = response.headers.get('content-type') ?? ''
      if (
        response.ok &&
        contentType.toLowerCase().includes('text/event-stream')
      ) {
        notifyError(null)
        return
      }

      throw new Error(
        `Không thể mở kết nối SSE (${response.status} ${response.statusText}).`,
      )
    },
    onmessage: (event) => {
      if (event.event && event.event !== eventName) return
      if (!event.data?.trim()) return

      try {
        const message = JSON.parse(event.data) as NotificationHubMessage
        for (const listener of messageListeners) {
          listener(message)
        }
      } catch (cause) {
        console.error('[notifications] Invalid SSE payload:', cause)
      }
    },
    onerror: (cause) => {
      console.error('[notifications] SSE connection error:', cause)
      notifyError(
        cause instanceof Error
          ? cause.message
          : 'Kết nối realtime thông báo bị ngắt.',
      )
    },
  })
    .then(() => {
      if (!controller.signal.aborted) {
        notifyError('Kết nối realtime thông báo bị ngắt.')
      }
    })
    .catch((cause) => {
      if (controller.signal.aborted) return

      console.error('[notifications] SSE connection failed:', cause)
      notifyError(
        cause instanceof Error
          ? cause.message
          : 'Không thể kết nối realtime thông báo.',
      )
      throw cause
    })
    .finally(() => {
      if (abortController === controller) {
        abortController = null
      }
      connectPromise = null
    })

  await connectPromise
}

/**
 * Shared SSE stream for the SPA session. React unmount only removes listeners.
 */
export function subscribeNotificationSse(options: {
  onMessage: MessageListener
  onError?: ErrorListener
}) {
  messageListeners.add(options.onMessage)
  if (options.onError) errorListeners.add(options.onError)

  void startConnection()

  return () => {
    messageListeners.delete(options.onMessage)
    if (options.onError) errorListeners.delete(options.onError)
  }
}
