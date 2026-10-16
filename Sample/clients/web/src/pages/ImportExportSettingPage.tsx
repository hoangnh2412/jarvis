import { useCallback, useEffect, useState, type ChangeEvent } from 'react'
import {
  Button,
  InputText,
  getErrorMessage,
  notify,
} from '@platform/core'
import { ImportExportPageShell } from '@platform/importexport'
import {
  btnPrimaryClass,
  fieldInputClass,
} from '../components/fieldStyles'
import {
  IMPORT_EXPORT_EXPORT_SETTING_GROUP,
  IMPORT_EXPORT_IMPORT_SETTING_GROUP,
} from '../constants/importExportSettingKeys'
import { getSettingForm, saveSettingGroup } from '../services/settings'
import type { SettingFormItem } from '../types/settings'

type SettingSection = {
  groupName: string
  title: string
  description: string
}

const SECTIONS: SettingSection[] = [
  {
    groupName: IMPORT_EXPORT_IMPORT_SETTING_GROUP,
    title: 'Import',
    description: 'Template code dùng khi import dữ liệu từ Excel/CSV.',
  },
  {
    groupName: IMPORT_EXPORT_EXPORT_SETTING_GROUP,
    title: 'Export',
    description: 'Template code dùng khi export dữ liệu ra Excel/CSV.',
  },
]

function SettingFields({
  items,
  values,
  onChange,
}: {
  items: SettingFormItem[]
  values: Record<string, string>
  onChange: (key: string, value: string) => void
}) {
  return (
    <>
      {items.map((item) => (
        <label key={item.key} className="block text-sm">
          <span className="mb-1 block font-medium text-slate-700">
            {item.name}
          </span>
          {item.description ? (
            <span className="mb-1 block text-xs text-slate-500">
              {item.description}
            </span>
          ) : null}
          <InputText
            unstyled
            value={values[item.key] ?? ''}
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              onChange(item.key, e.target.value)
            }
            className={fieldInputClass}
            placeholder="template-code"
            disabled={item.isReadOnly}
          />
          <span className="mt-1 block font-mono text-[11px] text-slate-400">
            {item.key}
          </span>
        </label>
      ))}
    </>
  )
}

export function ImportExportSettingPage() {
  const [itemsByGroup, setItemsByGroup] = useState<
    Record<string, SettingFormItem[]>
  >({})
  const [values, setValues] = useState<Record<string, string>>({})
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)

  const fetchForm = useCallback(async () => {
    setLoading(true)
    try {
      const forms = await Promise.all(
        SECTIONS.map(async (section) => ({
          groupName: section.groupName,
          form: await getSettingForm(section.groupName),
        })),
      )

      const nextItems: Record<string, SettingFormItem[]> = {}
      const nextValues: Record<string, string> = {}

      for (const { groupName, form } of forms) {
        nextItems[groupName] = form
        for (const item of form) {
          nextValues[item.key] = item.value ?? ''
        }
      }

      setItemsByGroup(nextItems)
      setValues(nextValues)
    } catch (err) {
      setItemsByGroup({})
      notify.error(getErrorMessage(err, 'Không tải được form Setting'))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void fetchForm()
  }, [fetchForm])

  const onSave = async () => {
    setSaving(true)
    try {
      for (const section of SECTIONS) {
        const items = itemsByGroup[section.groupName] ?? []
        const groupValues = Object.fromEntries(
          items.map((item) => [item.key, values[item.key] ?? '']),
        )
        await saveSettingGroup(section.groupName, groupValues)
      }
      notify.success('Đã lưu cấu hình Import/Export')
      await fetchForm()
    } catch (err) {
      notify.error(getErrorMessage(err, 'Lưu Setting thất bại'))
    } finally {
      setSaving(false)
    }
  }

  const totalItems = SECTIONS.reduce(
    (count, section) => count + (itemsByGroup[section.groupName]?.length ?? 0),
    0,
  )

  return (
    <ImportExportPageShell
      title="Cài đặt Import / Export"
      description="Gán template code cho từng module nghiệp vụ. Tạo template trước ở menu Excel templates."
    >
      {loading ? (
        <p className="text-sm text-slate-500">Đang tải…</p>
      ) : (
        <form
          className="max-w-xl space-y-6"
          onSubmit={(e) => {
            e.preventDefault()
            void onSave()
          }}
        >
          {SECTIONS.map((section) => {
            const items = itemsByGroup[section.groupName] ?? []
            return (
              <section
                key={section.groupName}
                className="space-y-4 rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
              >
                <div>
                  <h2 className="m-0 text-base font-semibold text-slate-900">
                    {section.title}
                  </h2>
                  <p className="mt-1 m-0 text-sm text-slate-500">
                    {section.description}
                  </p>
                </div>
                <SettingFields
                  items={items}
                  values={values}
                  onChange={(key, value) =>
                    setValues((prev) => ({ ...prev, [key]: value }))
                  }
                />
              </section>
            )
          })}

          <Button
            type="submit"
            unstyled
            disabled={saving || totalItems === 0}
            className={btnPrimaryClass}
          >
            {saving ? 'Đang lưu…' : 'Lưu cấu hình'}
          </Button>
        </form>
      )}
    </ImportExportPageShell>
  )
}
