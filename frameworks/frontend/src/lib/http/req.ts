import axios, { type InternalAxiosRequestConfig } from 'axios'
import { API_KEY, API_KEY_HEADER, BASE_URL, normalizeApiBaseUrl } from './constants'

export type PlatformHttpConfig = {
  baseURL?: string
  apiKey?: string
  apiKeyHeader?: string
  /** Host tiêm header phạm vi (tenant/org) cho mỗi request; package không tự biết khóa lưu trữ của host. */
  getScopeHeaders?: () => Record<string, string>
  /** Nhận biết token hết hạn để host dọn phiên; gọi khi response 401 và không đổi được token. */
  onUnauthorized?: () => void
  /** 401: trả access token mới để gửi lại request. `null` thì coi là hết phiên. */
  tryRefresh?: () => Promise<string | null>
}

const runtime: PlatformHttpConfig = {}

/**
 * Wire shared axios from host app (Sample `.env`).
 * Call once at bootstrap — overrides bake-time empty env in kit `dist`.
 */
export function configurePlatformHttp(config: PlatformHttpConfig) {
  Object.assign(runtime, config)
  if (config.baseURL != null) {
    instance.defaults.baseURL = normalizeApiBaseUrl(config.baseURL)
  }
}

function resolveBaseURL(): string {
  if (runtime.baseURL != null && runtime.baseURL !== '') {
    return normalizeApiBaseUrl(runtime.baseURL)
  }
  return BASE_URL
}

function resolveApiKey(): string | undefined {
  return runtime.apiKey || API_KEY || undefined
}

function resolveApiKeyHeader(): string {
  return runtime.apiKeyHeader || API_KEY_HEADER
}

const instance = axios.create({
  baseURL: BASE_URL,
  withCredentials: true,
})

function urlBesideApiBase(baseURL: string, url: string): string {
  const basePath = baseURL.replace(/\/+$/, '')
  if (basePath.endsWith('/api') && url.startsWith('/api/')) {
    return url.slice('/api/'.length)
  }
  return url
}

instance.interceptors.request.use(
  (config) => {
    config.baseURL = resolveBaseURL()
    if (typeof config.url === 'string') {
      config.url = urlBesideApiBase(config.baseURL, config.url)
    }
    const apiKey = resolveApiKey()
    if (apiKey) {
      config.headers = config.headers ?? {}
      config.headers[resolveApiKeyHeader()] = apiKey
    }
    const scopeHeaders = runtime.getScopeHeaders?.()
    if (scopeHeaders) {
      config.headers = config.headers ?? {}
      for (const [key, value] of Object.entries(scopeHeaders)) {
        if (value && config.headers[key] == null) {
          config.headers[key] = value
        }
      }
    }
    return config
  },
  (error) => Promise.reject(error),
)

instance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const config = error?.config as (InternalAxiosRequestConfig & { __retried?: boolean }) | undefined
    const url = String(config?.url ?? '')
    const isAuthCall = url.includes('account/login') || url.includes('account/refresh')
    if (error?.response?.status === 401 && config && !config.__retried && !isAuthCall && runtime.tryRefresh) {
      config.__retried = true
      const nextToken = await runtime.tryRefresh().catch(() => null)
      if (nextToken) {
        config.headers.Authorization = `Bearer ${nextToken}`
        return instance.request(config)
      }
    }

    if (error?.response?.status === 401) {
      runtime.onUnauthorized?.()
    }
    const normalizeError = error?.response?.data ?? {
      Status: error?.response?.status ?? 0,
      Message: 'Không thể kết nối tới máy chủ',
    }
    return Promise.reject(normalizeError)
  },
)

export default instance

/** @deprecated Use configurePlatformHttp */
export const configureQueryBuilderHttp = configurePlatformHttp
/** @deprecated Use configurePlatformHttp */
export const configureTenantHttp = configurePlatformHttp
