import type { AccountAuthVariant, AuthPreset } from '../types'
import { AUTH_PRESETS as enAuthPresets, enMessages } from './en'
import { AUTH_PRESETS as viAuthPresets, viMessages } from './vi'

export { AUTH_PRESETS } from './vi'

export type AccountLocale = 'vi' | 'en'

export const accountMessages = {
  vi: viMessages,
  en: enMessages,
} as const

const authPresetsByLocale: Record<AccountLocale, Record<AccountAuthVariant, AuthPreset>> = {
  vi: viAuthPresets,
  en: enAuthPresets,
}

export function getAuthPreset(
  locale: AccountLocale,
  variant: AccountAuthVariant,
): AuthPreset {
  return authPresetsByLocale[locale][variant]
}

export function getAccountMessages(locale: AccountLocale = 'vi') {
  return accountMessages[locale]
}
