function pickString(...candidates: unknown[]): string | undefined {
  for (const value of candidates) {
    if (typeof value === 'string' && value.trim()) return value.trim()
  }
  return undefined
}

function messageFromData(data: unknown, depth = 0): string | undefined {
  if (data == null || depth > 4) return undefined
  if (typeof data === 'string' && data.trim()) return data.trim()
  if (typeof data !== 'object') return undefined

  const obj = data as Record<string, unknown>

  // Jarvis envelope: { error: { message: "..." }, code, traceId, ... }
  const nested =
    messageFromData(obj.error, depth + 1) ??
    messageFromData(obj.Error, depth + 1) ??
    messageFromData(obj.errors, depth + 1) ??
    messageFromData(obj.Errors, depth + 1)

  return (
    pickString(obj.message, obj.Message, obj.title, obj.Title, obj.detail, obj.Detail) ??
    nested ??
    pickString(
      typeof obj.error === 'string' ? obj.error : undefined,
      typeof obj.Error === 'string' ? obj.Error : undefined,
    )
  )
}

/** Prefer API body message (axios / interceptor / plain object), then Error.message. */
export function getErrorMessage(error: unknown, fallback: string) {
  if (error && typeof error === 'object') {
    const anyErr = error as {
      response?: { data?: unknown }
      data?: unknown
      message?: unknown
      Message?: unknown
    }

    const fromBody = messageFromData(
      anyErr.response?.data ?? anyErr.data ?? error,
    )
    if (fromBody) return fromBody

    // AxiosError.message is often "Request failed with status code 500" — use last.
    const fromProps = pickString(anyErr.message, anyErr.Message)
    if (fromProps && !/^Request failed with status code \d+/i.test(fromProps)) {
      return fromProps
    }
  }

  if (typeof error === 'string' && error.trim()) return error.trim()
  return fallback
}
