import type { LoginResult } from '@platform/core'

/** Tenant seed của BasicAuthUser (`SampleAuthTenants.DevTenantId`). */
const DEV_TENANT_ID = 'dd348565-5d31-46bb-aef7-aa7f0c4a1866'

type BearerTokenBody = {
  accessToken?: string
  AccessToken?: string
  message?: string
  title?: string
}

/**
 * Đăng nhập Sample: verify user rồi nhận JWT từ `POST /api/authentication/bearer-token`.
 * Trường form `email` là username (hoặc email nếu tài khoản dùng email làm tên).
 */
export async function loginWithJwt(payload: {
  email: string
  password: string
}): Promise<LoginResult> {
  const username = payload.email.trim()
  const tenantId = import.meta.env.VITE_TENANT_ID?.trim() || DEV_TENANT_ID

  const response = await fetch('/api/authentication/bearer-token', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Tenant-Id': tenantId,
    },
    body: JSON.stringify({ username, password: payload.password }),
  })

  const body = (await response.json().catch(() => null)) as BearerTokenBody | null
  if (!response.ok) {
    throw new Error(
      body?.message ||
        body?.title ||
        (response.status === 401
          ? 'Sai tên đăng nhập hoặc mật khẩu.'
          : 'Đăng nhập thất bại.'),
    )
  }

  const accessToken = body?.accessToken || body?.AccessToken
  if (!accessToken) {
    throw new Error('Server không trả access token.')
  }

  return {
    user: {
      id: username,
      email: username.includes('@') ? username : `${username}@local`,
      fullName: username,
    },
    tokens: { accessToken },
  }
}
