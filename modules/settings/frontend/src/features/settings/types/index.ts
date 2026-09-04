export type SettingGroup = {
  name: string
  displayName: string
  description?: string | null
  order: number
}

export type SettingFormItem = {
  group: string
  key: string
  name: string
  type:
    | 'Text'
    | 'Textarea'
    | 'Combobox'
    | 'Checkbox'
    | 'Switch'
    | 'Password'
    | 'Number'
    | 'Email'
    | 'Radio'
    | 'MultiSelect'
    | 'DateTime'
    | 'Date'
    | 'Image'
    | string
  options?: string | null
  description?: string | null
  defaultValue?: string | null
  isReadOnly: boolean
  isEncrypted: boolean
  value: string
  isPersisted: boolean
}

export type SettingConnection = {
  apiBase: string
  tenantId: string
  apiKey: string
}

export type SettingOption = {
  label: string
  value: string
}

export function parseSettingOptions(raw?: string | null): SettingOption[] {
  if (!raw) return []

  return raw
    .split('|')
    .filter(Boolean)
    .map((entry) => {
      const separator = entry.indexOf(':')
      if (separator < 0) {
        const value = decodeOptionPart(entry)
        return { value, label: value }
      }

      return {
        value: decodeOptionPart(entry.slice(0, separator)),
        label: decodeOptionPart(entry.slice(separator + 1)),
      }
    })
}

function decodeOptionPart(value: string) {
  try {
    return decodeURIComponent(value)
  } catch {
    return value
  }
}
