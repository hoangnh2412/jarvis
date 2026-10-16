import { useEffect, useState } from 'react'
import {
  Button,
  FieldSelect,
  InputText,
  PasswordInput,
  Textarea,
  notify,
} from '@platform/core'
import { ChevronDown, RefreshCw, Save } from 'lucide-react'
import {
  btnOutlinedClass,
  btnPrimaryClass,
  fieldInputClass,
} from '../components/fieldStyles'
import {
  getSettingForm,
  getSettingGroups,
  saveSettingGroup,
} from '../services/settings'
import {
  parseSettingOptions,
  type SettingFormItem,
  type SettingGroup,
} from '../types/settings'

type GroupFormState = {
  items: SettingFormItem[]
  values: Record<string, string>
  loading: boolean
  saving: boolean
}

type Status = {
  kind: 'success' | 'error'
  message: string
} | null

const emptyFormState: GroupFormState = {
  items: [],
  values: {},
  loading: false,
  saving: false,
}

function createFormState(items: SettingFormItem[]): GroupFormState {
  return {
    items,
    values: Object.fromEntries(
      items.map((item) => [item.key, item.value ?? '']),
    ),
    loading: false,
    saving: false,
  }
}

export function SettingPage() {
  const [groups, setGroups] = useState<SettingGroup[]>([])
  const [forms, setForms] = useState<Record<string, GroupFormState>>({})
  const [loadingGroups, setLoadingGroups] = useState(false)
  const [status, setStatus] = useState<Status>(null)

  const patchForm = (
    groupName: string,
    update: Partial<GroupFormState> | ((state: GroupFormState) => GroupFormState),
  ) => {
    setForms((current) => {
      const state = current[groupName] ?? emptyFormState
      const next =
        typeof update === 'function' ? update(state) : { ...state, ...update }
      return { ...current, [groupName]: next }
    })
  }

  const loadGroup = async (groupName: string) => {
    patchForm(groupName, { loading: true })
    try {
      const items = await getSettingForm(groupName)
      patchForm(groupName, createFormState(items))
    } catch (error) {
      patchForm(groupName, { loading: false, items: [], values: {} })
      setStatus({
        kind: 'error',
        message:
          error instanceof Error ? error.message : 'Không tải được Setting.',
      })
    }
  }

  const loadGroups = async () => {
    setLoadingGroups(true)
    setStatus(null)
    try {
      const nextGroups = await getSettingGroups()
      setGroups(nextGroups)
      setForms(
        Object.fromEntries(
          nextGroups.map((group) => [
            group.name,
            { ...emptyFormState, loading: true },
          ]),
        ),
      )
      await Promise.all(nextGroups.map((group) => loadGroup(group.name)))
    } catch (error) {
      setGroups([])
      setForms({})
      setStatus({
        kind: 'error',
        message:
          error instanceof Error
            ? error.message
            : 'Không tải được danh sách nhóm.',
      })
    } finally {
      setLoadingGroups(false)
    }
  }

  useEffect(() => {
    void loadGroups()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const handleSave = async (group: SettingGroup) => {
    const form = forms[group.name]
    if (!form) return

    const values = Object.fromEntries(
      form.items
        .filter((item) => !item.isReadOnly)
        .map((item) => [item.key, form.values[item.key] ?? '']),
    )
    if (!Object.keys(values).length) return

    patchForm(group.name, { saving: true })
    setStatus(null)
    try {
      await saveSettingGroup(group.name, values)
      await loadGroup(group.name)
      const message = `Đã lưu nhóm ${group.displayName || group.name}.`
      setStatus({ kind: 'success', message })
      notify.success(message)
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Không lưu được Setting.'
      setStatus({ kind: 'error', message })
      notify.error(message)
    } finally {
      patchForm(group.name, { saving: false })
    }
  }

  return (
    <div className="flex h-full min-h-0 w-full flex-col overflow-hidden font-sans text-slate-900">
      <div className="mb-4 flex shrink-0 flex-wrap items-start justify-between gap-3 border-b border-slate-200 pb-4">
        <div>
          <h2 className="m-0 text-[1.35rem] font-semibold tracking-tight text-slate-900">
            Cấu hình hệ thống
          </h2>
          <p className="mt-1 mb-0 text-sm text-slate-500">
            Quản lý cấu hình theo nhóm (Email, Import/Export, …).
          </p>
        </div>
        <Button
          type="button"
          unstyled
          className={btnOutlinedClass}
          disabled={loadingGroups}
          onClick={() => void loadGroups()}
        >
          <RefreshCw
            className={`size-4 ${loadingGroups ? 'animate-spin' : ''}`}
            aria-hidden
          />
          Tải lại
        </Button>
      </div>

      {status && (
        <div
          role="status"
          className={[
            'mb-4 shrink-0 rounded-xl border px-4 py-3 text-sm',
            status.kind === 'success'
              ? 'border-emerald-200 bg-emerald-50 text-emerald-800'
              : 'border-red-200 bg-red-50 text-red-700',
          ].join(' ')}
        >
          {status.message}
        </div>
      )}

      <div className="min-h-0 flex-1 overflow-y-auto">
        {loadingGroups && !groups.length ? (
          <div className="flex h-full items-center justify-center text-sm text-slate-500">
            Đang tải danh sách cấu hình…
          </div>
        ) : groups.length ? (
          <div className="flex flex-col gap-3 pb-2">
            {groups.map((group) => {
              const form = forms[group.name] ?? emptyFormState
              return (
                <details
                  key={group.name}
                  className="group overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm"
                  open={
                    group.name === 'ImportGroup' ||
                    group.name === 'ExportGroup'
                  }
                >
                  <summary className="flex cursor-pointer list-none items-center justify-between gap-4 bg-slate-50 px-5 py-4 marker:hidden">
                    <div className="min-w-0">
                      <h3 className="m-0 text-base font-semibold text-slate-900">
                        {group.displayName || group.name}
                      </h3>
                      <p className="mt-1 m-0 text-xs leading-relaxed text-slate-500">
                        {group.description || group.name}
                      </p>
                    </div>
                    <div className="flex shrink-0 items-center gap-3">
                      <span className="rounded-full bg-slate-200/70 px-2.5 py-1 text-xs font-medium text-slate-600">
                        {form.items.length} cấu hình
                      </span>
                      <ChevronDown
                        className="size-5 text-slate-500 transition-transform group-open:rotate-180"
                        aria-hidden
                      />
                    </div>
                  </summary>

                  <div className="border-t border-slate-200">
                    <div className="p-5">
                      {form.loading ? (
                        <div className="py-10 text-center text-sm text-slate-500">
                          Đang tải cấu hình…
                        </div>
                      ) : form.items.length ? (
                        <div className="grid gap-4 xl:grid-cols-2">
                          {form.items.map((item) => (
                            <SettingFieldCard
                              key={item.key}
                              item={item}
                              value={form.values[item.key] ?? ''}
                              onChange={(value) =>
                                patchForm(group.name, (current) => ({
                                  ...current,
                                  values: {
                                    ...current.values,
                                    [item.key]: value,
                                  },
                                }))
                              }
                            />
                          ))}
                        </div>
                      ) : (
                        <div className="py-10 text-center text-sm text-slate-500">
                          Không có cấu hình trong nhóm này.
                        </div>
                      )}
                    </div>

                    <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-200 bg-slate-50/60 px-5 py-4">
                      <p className="m-0 text-xs text-slate-500">
                        Các trường Password được che khi hiển thị.
                      </p>
                      <Button
                        type="button"
                        unstyled
                        className={btnPrimaryClass}
                        disabled={
                          !form.items.length || form.saving || form.loading
                        }
                        onClick={() => void handleSave(group)}
                      >
                        <Save className="size-4" aria-hidden />
                        {form.saving ? 'Đang lưu…' : 'Lưu nhóm cấu hình'}
                      </Button>
                    </div>
                  </div>
                </details>
              )
            })}
          </div>
        ) : (
          <div className="flex h-full items-center justify-center text-sm text-slate-500">
            Không có nhóm Setting. Kiểm tra API và header X-Tenant-Id.
          </div>
        )}
      </div>
    </div>
  )
}

function SettingFieldCard({
  item,
  value,
  onChange,
}: {
  item: SettingFormItem
  value: string
  onChange: (value: string) => void
}) {
  return (
    <article className="flex flex-col rounded-xl border border-slate-200 bg-white p-4">
      <div className="mb-3 flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h4 className="m-0 text-sm font-semibold text-slate-900">
            {item.name || item.key}
          </h4>
          <code className="mt-1 block break-all text-[11px] text-slate-400">
            {item.key}
          </code>
        </div>
        <span className="shrink-0 rounded-full bg-slate-100 px-2 py-1 text-[10px] font-medium text-slate-600">
          {item.isPersisted ? 'Đã lưu' : 'Mặc định'}
        </span>
      </div>

      {item.description ? (
        <p className="mb-3 mt-0 text-xs leading-relaxed text-slate-500">
          {item.description}
        </p>
      ) : null}

      <div className="mt-auto">
        <SettingControl item={item} value={value} onChange={onChange} />
      </div>
    </article>
  )
}

function SettingControl({
  item,
  value,
  onChange,
}: {
  item: SettingFormItem
  value: string
  onChange: (value: string) => void
}) {
  const type = item.type.toLowerCase()

  if (type === 'textarea') {
    return (
      <Textarea
        unstyled
        className={`${fieldInputClass} !h-auto min-h-24 resize-y py-3`}
        value={value}
        disabled={item.isReadOnly}
        onChange={(event: { target: { value: string } }) =>
          onChange(event.target.value)
        }
      />
    )
  }

  if (type === 'combobox') {
    return (
      <FieldSelect
        value={value}
        options={parseSettingOptions(item.options)}
        disabled={item.isReadOnly}
        onChange={onChange}
      />
    )
  }

  if (type === 'checkbox') {
    const checked = value.trim().toLowerCase() === 'true'
    return (
      <label className="inline-flex h-11 items-center gap-2.5 text-sm text-slate-800">
        <input
          type="checkbox"
          className="size-4 rounded border-slate-300 text-teal-700 accent-teal-700 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:opacity-70"
          checked={checked}
          disabled={item.isReadOnly}
          onChange={(event) =>
            onChange(event.target.checked ? 'true' : 'false')
          }
        />
        <span>{checked ? 'Bật' : 'Tắt'}</span>
      </label>
    )
  }

  if (type === 'password') {
    return (
      <PasswordInput
        value={value}
        autoComplete="new-password"
        disabled={item.isReadOnly}
        className={fieldInputClass}
        onValueChange={(event) => onChange(event.value ?? '')}
      />
    )
  }

  return (
    <InputText
      unstyled
      type={type === 'number' ? 'number' : 'text'}
      className={fieldInputClass}
      value={value}
      disabled={item.isReadOnly}
      onChange={(event: { target: { value: string } }) =>
        onChange(event.target.value)
      }
    />
  )
}
