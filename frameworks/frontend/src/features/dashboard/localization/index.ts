import { enMessages } from './en'
import { viMessages, type DashboardMessages } from './vi'

export type DashboardLocale = 'vi' | 'en'

export type { DashboardMessages }

export const dashboardMessages = {
  vi: viMessages,
  en: enMessages,
} as const

export function getDashboardMessages(
  locale: DashboardLocale = 'vi',
): DashboardMessages {
  return dashboardMessages[locale] ?? viMessages
}
