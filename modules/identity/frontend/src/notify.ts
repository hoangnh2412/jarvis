import { getErrorMessage, notify } from '@platform/core'

export function notifyApiError(error: unknown): void {
  notify.error(getErrorMessage(error, 'Có lỗi xảy ra khi gọi API.'))
}
