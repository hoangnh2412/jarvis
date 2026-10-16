import { getImportExportHttp } from '@platform/importexport'
import { IMPORT_EXPORT_SETTING_GROUP } from '../constants/importExportSettingKeys'
import type { SettingFormItem, SettingGroup } from '../types/settings'

type PlatformResponse<T> = {
  code?: string
  data?: T
  error?: { message?: string; systemMessage?: string }
}

function unwrap<T>(body: unknown, fallback: string): T {
  if (body == null || body === '') throw new Error(fallback)
  if (typeof body === 'object' && body !== null && 'code' in body) {
    const platform = body as PlatformResponse<T>
    if (platform.data == null) {
      throw new Error(
        platform.error?.systemMessage || platform.error?.message || fallback,
      )
    }
    return platform.data
  }
  return body as T
}

function mapFormItem(raw: Record<string, unknown>): SettingFormItem {
  return {
    group: String(raw.group ?? raw.Group ?? ''),
    key: String(raw.key ?? raw.Key ?? ''),
    name: String(raw.name ?? raw.Name ?? ''),
    type: String(raw.type ?? raw.Type ?? 'Text'),
    options: (raw.options ?? raw.Options) as string | null | undefined,
    description: (raw.description ?? raw.Description) as string | null | undefined,
    defaultValue: (raw.defaultValue ?? raw.DefaultValue) as string | null | undefined,
    isReadOnly: Boolean(raw.isReadOnly ?? raw.IsReadOnly),
    isEncrypted: Boolean(raw.isEncrypted ?? raw.IsEncrypted),
    value: String(raw.value ?? raw.Value ?? ''),
    isPersisted: Boolean(raw.isPersisted ?? raw.IsPersisted),
  }
}

function mapGroup(raw: Record<string, unknown>): SettingGroup {
  return {
    name: String(raw.name ?? raw.Name ?? ''),
    displayName: String(raw.displayName ?? raw.DisplayName ?? raw.name ?? raw.Name ?? ''),
    description: (raw.description ?? raw.Description) as string | null | undefined,
    order: Number(raw.order ?? raw.Order ?? 0),
  }
}

const http = () => getImportExportHttp()

export async function getSettingGroups(): Promise<SettingGroup[]> {
  const { data } = await http().get('v1/settings/groups')
  const groups = unwrap<Record<string, unknown>[]>(data, 'Không tải được danh sách nhóm Setting')
  return groups.map(mapGroup).sort((a, b) => a.order - b.order || a.name.localeCompare(b.name))
}

export async function getSettingForm(
  group: string = IMPORT_EXPORT_SETTING_GROUP,
): Promise<SettingFormItem[]> {
  const { data } = await http().get('v1/settings/form', { params: { group } })
  const items = unwrap<Record<string, unknown>[]>(data, 'Không tải được form Setting')
  return items.map(mapFormItem)
}

export async function saveSettingGroup(
  group: string,
  values: Record<string, string>,
): Promise<void> {
  await http().put(`v1/settings/group/${encodeURIComponent(group)}`, {
    values,
  })
}

export async function getSettingValue(key: string): Promise<string | null> {
  try {
    const { data } = await http().get(
      `v1/settings/${encodeURIComponent(key)}`,
    )
    const item = unwrap<Record<string, unknown>>(data, 'Không tải được Setting')
    const value = String(item.value ?? item.Value ?? '').trim()
    return value || null
  } catch {
    return null
  }
}

export type { SettingFormItem, SettingGroup } from '../types/settings'
