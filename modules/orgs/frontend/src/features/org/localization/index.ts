import { enMessages } from './en'
import { viMessages } from './vi'

export type OrgLocale = 'vi' | 'en'

export const orgMessages = {
  vi: viMessages,
  en: enMessages,
} as const

export function getOrgMessages(locale: OrgLocale = 'vi') {
  return orgMessages[locale]
}
