import { configureJarvisHttp } from '@jarvis/core'

/** Shared API base — same-origin `/api/` → Vite proxy → BE */
export const BASE_URL = import.meta.env.VITE_API_URL

export const API_KEY = import.meta.env.VITE_API_KEY

export const API_KEY_HEADER = import.meta.env.VITE_API_KEY_NAME

/**
 * Tài khoản demo — login không cần BE.
 * Email: admin@gmail.com / Password: Admin@123
 */
export const MOCK_ACCOUNT = {
  email: 'admin@gmail.com',
  password: 'Admin@123',
  fullName: 'Admin',
  user: {
    id: 'user-001',
    email: 'admin@gmail.com',
    fullName: 'Admin',
    roles: ['admin'],
  },
} as const

export type MockAccount = typeof MOCK_ACCOUNT

export type MockLoginResult = {
  user: typeof MOCK_ACCOUNT.user
  token: string
}

/** Khớp credentials với `MOCK_ACCOUNT`? */
export function isMockAccountCredentials(payload: {
  email: string
  password: string
}) {
  return (
    payload.email.trim().toLowerCase() === MOCK_ACCOUNT.email.toLowerCase() &&
    payload.password === MOCK_ACCOUNT.password
  )
}

/**
 * Login mock — không gọi API.
 * Sai thông tin thì throw để LoginPage hiện toast lỗi.
 */
export async function mockLogin(payload: {
  email: string
  password: string
}): Promise<MockLoginResult> {
  await new Promise((resolve) => setTimeout(resolve, 250))

  if (!isMockAccountCredentials(payload)) {
    throw new Error('Email hoặc mật khẩu không đúng')
  }

  return {
    user: { ...MOCK_ACCOUNT.user },
    token: 'mock-demo-token',
  }
}

/** Wire kit HTTP from Sample env (call once at bootstrap). */
export function configureSampleHttp() {
  configureJarvisHttp({
    baseURL: BASE_URL,
    apiKey: API_KEY,
    apiKeyHeader: API_KEY_HEADER,
  })
}
