import { accountHttp } from '@jarvis/core'
import { getAccessToken } from './accessToken'

let configured = false

/** Gắn Bearer JWT cho account API — chỉ dùng ở Sample host app. */
export function setupSampleAccountAuth() {
  if (configured) return
  configured = true

  accountHttp.interceptors.request.use((config) => {
    const token = getAccessToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })
}
