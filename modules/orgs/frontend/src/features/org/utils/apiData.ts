type OrgEnvelope<T> = {
  data?: T
  error?: {
    message?: string
    systemMessage?: string
  }
}

export function unwrapOrgBody<T>(body: unknown, fallback: string): T {
  if (body == null || body === '') {
    throw new Error(fallback)
  }

  if (typeof body === 'object' && body !== null && 'data' in body) {
    const envelope = body as OrgEnvelope<T>
    if (envelope.data == null) {
      throw new Error(
        envelope.error?.systemMessage || envelope.error?.message || fallback,
      )
    }
    return envelope.data
  }

  return body as T
}
