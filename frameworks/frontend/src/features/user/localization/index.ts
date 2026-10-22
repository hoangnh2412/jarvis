import { enMessages } from './en'
import { viMessages } from './vi'

export type UserLocale = 'vi' | 'en'

export const userMessages = {
  vi: viMessages,
  en: enMessages,
} as const

export function getUserMessages(locale: UserLocale = 'vi') {
  return userMessages[locale]
}

export type UserMessages = ReturnType<typeof getUserMessages>
