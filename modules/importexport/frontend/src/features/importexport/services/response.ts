import type { ApiEnvelope, PlatformBaseResponse } from '../types'

export function isLegacyEnvelope(body: unknown): body is ApiEnvelope<unknown> {
  return (
    !!body &&
    typeof body === 'object' &&
    'status' in body &&
    typeof (body as ApiEnvelope<unknown>).status === 'number' &&
    'message' in body &&
    typeof (body as ApiEnvelope<unknown>).message === 'string'
  )
}

export function isPlatformResponse(
  body: unknown,
): body is PlatformBaseResponse<unknown> {
  return (
    !!body &&
    typeof body === 'object' &&
    'code' in body &&
    !isLegacyEnvelope(body)
  )
}

export function isEmptyResponseBody(body: unknown): boolean {
  return body == null || body === ''
}

export function unwrap<T>(body: unknown, fallback: string): T {
  if (isEmptyResponseBody(body)) {
    throw new Error(fallback)
  }

  if (isPlatformResponse(body)) {
    if (body.data == null) {
      throw new Error(
        body.error?.systemMessage || body.error?.message || fallback,
      )
    }
    return body.data as T
  }

  if (isLegacyEnvelope(body)) {
    if (body.data == null) {
      throw new Error(body.message || fallback)
    }
    return body.data as T
  }

  if (typeof body === 'object') {
    return body as T
  }

  throw new Error(fallback)
}

export function unwrapArray<T>(body: unknown, fallback: string): T[] {
  const data = unwrap<T[]>(body, fallback)
  if (!Array.isArray(data)) {
    throw new Error(fallback)
  }
  return data
}
