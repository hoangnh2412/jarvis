import { enMessages } from './en'
import { viMessages, type CraftPdfMessages } from './vi'

export type CraftPdfLocale = 'vi' | 'en'
export type { CraftPdfMessages }

export const craftPdfMessages = {
  vi: viMessages,
  en: enMessages,
} as const

export function getCraftPdfMessages(locale: CraftPdfLocale = 'vi') {
  return craftPdfMessages[locale] as CraftPdfMessages
}
