import { enMessages, viMessages } from './messages'

export type PermissionLocale = 'vi' | 'en'

export const permissionMessages = {
  vi: viMessages,
  en: enMessages,
} as const

export function getPermissionMessages(locale: PermissionLocale = 'vi') {
  return permissionMessages[locale]
}

export type PermissionMessages = ReturnType<typeof getPermissionMessages>
