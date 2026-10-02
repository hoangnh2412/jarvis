import axios from 'axios'
import { API_KEY, API_KEY_HEADER, BASE_URL, normalizeApiBaseUrl } from './constants'

export type PlatformHttpConfig = {
  baseURL?: string
  apiKey?: string
  apiKeyHeader?: string
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

instance.interceptors.request.use(
  (config) => {
    config.baseURL = resolveBaseURL()
    const apiKey = resolveApiKey()
    if (apiKey) {
      config.headers = config.headers ?? {}
      config.headers[resolveApiKeyHeader()] = apiKey
    }
    return config
  },
  (error) => Promise.reject(error),
)

instance.interceptors.response.use(
  (response) => response,
  (error) => {
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
