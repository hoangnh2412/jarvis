import { enMessages } from './en'
import { viMessages, type QueryBuilderMessages } from './vi'

export type QueryBuilderLocale = 'vi' | 'en'

export type { QueryBuilderMessages }

export const queryBuilderMessages = {
  vi: viMessages,
  en: enMessages,
} as const

export function getQueryBuilderMessages(
  locale: QueryBuilderLocale = 'vi',
): QueryBuilderMessages {
  return queryBuilderMessages[locale] ?? viMessages
}
