import { enMessages } from './en'
import { viMessages } from './vi'

export type RoleLocale = 'vi' | 'en'

export const roleMessages = {
  vi: viMessages,
  en: enMessages,
} as const

export function getRoleMessages(locale: RoleLocale = 'vi') {
  return roleMessages[locale]
}
