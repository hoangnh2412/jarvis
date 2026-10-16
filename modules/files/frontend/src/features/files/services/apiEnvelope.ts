import type { AxiosResponse } from 'axios'

type ApiEnvelope<T> = {
  data?: T
  error?: {
    message?: string
    systemMessage?: string
  }
  message?: string
  title?: string
  code?: string
}

/** Unwrap Sample `BaseResponse` (`{ data, traceId, code, ... }`). */
export function unwrapApiPayload<T>(payload: unknown): T {
  if (
    payload &&
    typeof payload === 'object' &&
    Object.prototype.hasOwnProperty.call(payload, 'data')
  ) {
    return (payload as ApiEnvelope<T>).data as T
  }

  return payload as T
}

export function unwrapAxiosData<T>(response: AxiosResponse): T {
  return unwrapApiPayload<T>(response.data)
}
