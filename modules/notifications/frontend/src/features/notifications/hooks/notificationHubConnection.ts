import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import { buildNotificationHubOptions, notificationHubUrl } from '../config'
import type { NotificationItem } from '../types'

export type NotificationHubMessage = Omit<NotificationItem, 'isRead'>

type MessageListener = (message: NotificationHubMessage) => void
type HubErrorListener = (message: string | null) => void

let connection: HubConnection | null = null
let startPromise: Promise<void> | null = null
let pageUnloadHookRegistered = false

const messageListeners = new Set<MessageListener>()
const hubErrorListeners = new Set<HubErrorListener>()

function notifyHubError(message: string | null) {
  for (const listener of hubErrorListeners) {
    listener(message)
  }
}

function registerPageUnloadHook() {
  if (pageUnloadHookRegistered || typeof window === 'undefined') return
  pageUnloadHookRegistered = true
  window.addEventListener('pagehide', () => {
    if (!connection) return
    void connection.stop()
    connection = null
    startPromise = null
  })
}

function ensureConnection() {
  if (connection) return connection

  registerPageUnloadHook()

  connection = new HubConnectionBuilder()
    .withUrl(notificationHubUrl, buildNotificationHubOptions())
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Error)
    .build()

  connection.on('notification', (message: NotificationHubMessage) => {
    for (const listener of messageListeners) {
      listener(message)
    }
  })

  connection.onreconnected(() => {
    notifyHubError(null)
  })

  connection.onclose((cause) => {
    if (cause) {
      console.error('[notifications] SignalR hub disconnected:', cause)
      notifyHubError(
        cause instanceof Error
          ? cause.message
          : 'Kết nối realtime thông báo bị ngắt.',
      )
    }
  })

  return connection
}

async function startConnection() {
  const hub = ensureConnection()
  if (hub.state === HubConnectionState.Connected) return

  if (hub.state === HubConnectionState.Connecting && startPromise) {
    await startPromise
    return
  }

  startPromise = hub
    .start()
    .then(() => {
      notifyHubError(null)
    })
    .catch((cause) => {
      console.error('[notifications] SignalR hub connection failed:', cause)
      notifyHubError(
        cause instanceof Error
          ? cause.message
          : 'Không thể kết nối realtime thông báo.',
      )
      throw cause
    })
    .finally(() => {
      startPromise = null
    })

  await startPromise
}

/**
 * Shared hub for the SPA session. React unmount (incl. StrictMode) only removes
 * listeners — does not stop the connection, avoiding canceled negotiate/poll cycles.
 */
export function subscribeNotificationHub(options: {
  onMessage: MessageListener
  onHubError?: HubErrorListener
}) {
  messageListeners.add(options.onMessage)
  if (options.onHubError) hubErrorListeners.add(options.onHubError)

  void startConnection()

  return () => {
    messageListeners.delete(options.onMessage)
    if (options.onHubError) hubErrorListeners.delete(options.onHubError)
  }
}
