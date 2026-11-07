import type { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { accountHttp, configurePlatformHttp, platformHttp } from '@platform/core'
import { clearAccessToken, getAccessToken, isAccessTokenExpired } from './accessToken'
import { refreshAccessToken } from './refreshAccessToken'

let configured = false

type RetriableConfig = InternalAxiosRequestConfig & { __retried?: boolean }

/** Gắn Bearer JWT và đổi token khi access token hết hạn. */
function redirectToLogin() {
  clearAccessToken()
  if (!window.location.pathname.startsWith('/login')) {
    window.location.assign('/login')
  }
}

export function setupSampleAccountAuth() {
  if (configured) return
  configured = true

  configurePlatformHttp({
    tryRefresh: refreshAccessToken,
    onUnauthorized: redirectToLogin,
  })

  for (const instance of [platformHttp, accountHttp]) {
    instance.interceptors.request.use(async (config) => {
      let token = getAccessToken()
      if (token && isAccessTokenExpired(token)) {
        token = (await refreshAccessToken()) ?? null
      }
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
      return config
    })

    instance.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        const config = error.config as RetriableConfig | undefined
        const url = String(config?.url ?? '')
        const isAuthCall = url.includes('account/login') || url.includes('account/refresh')
        if (error.response?.status !== 401 || !config || config.__retried || isAuthCall) {
          return Promise.reject(error)
        }

        config.__retried = true
        const nextToken = await refreshAccessToken()
        if (!nextToken) {
          redirectToLogin()
          return Promise.reject(error.response?.data ?? error)
        }

        config.headers.Authorization = `Bearer ${nextToken}`
        return instance.request(config)
      },
    )
  }
}
