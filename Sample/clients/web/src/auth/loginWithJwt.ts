import type { LoginResult } from '@platform/core'

type TokenPayload = {
  accessToken?: string
  refreshToken?: string
  AccessToken?: string
  RefreshToken?: string
}

type TokenBody = TokenPayload & {
  data?: TokenPayload
  message?: string
  title?: string
  error?: { message?: string; systemMessage?: string }
}

function readTokens(body: TokenBody | null) {
  const payload = body?.data ?? body
  return {
    accessToken: payload?.accessToken || payload?.AccessToken || '',
    refreshToken: payload?.refreshToken || payload?.RefreshToken || '',
  }
}

/**
 * Đăng nhập app: `POST /api/v1/app/account/login`.
 * Trường form `email` là username.
 */
export async function loginWithJwt(payload: {
  email: string
  password: string
  tenantCode: string
  orgCode: string
  tenantId: string
  orgId: string
}): Promise<LoginResult> {
  const response = await fetch('/api/v1/app/account/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Tenant-Id': payload.tenantId,
      'X-Org-Id': payload.orgId,
    },
    body: JSON.stringify({
      userName: payload.email.trim(),
      password: payload.password,
      tenantCode: payload.tenantCode.trim(),
      orgCode: payload.orgCode.trim(),
    }),
  })

  const body = (await response.json().catch(() => null)) as TokenBody | null
  if (!response.ok) {
    throw new Error(
      body?.error?.message ||
        body?.message ||
        body?.title ||
        (response.status === 401
          ? 'Sai tên đăng nhập hoặc mật khẩu.'
          : 'Đăng nhập thất bại.'),
    )
  }

  const tokens = readTokens(body)
  if (!tokens.accessToken || !tokens.refreshToken) {
    throw new Error('Server không trả access token.')
  }

  const username = payload.email.trim()
  return {
    user: {
      id: username,
      email: username.includes('@') ? username : `${username}@local`,
      fullName: username,
    },
    tokens,
  }
}
