import { enMessages } from './en'
import { viMessages } from './vi'

export type TenantLocale = 'vi' | 'en'

export const tenantMessages = {
  vi: viMessages,
  en: enMessages,
} as const

export function getTenantMessages(locale: TenantLocale = 'vi') {
  return tenantMessages[locale]
}
