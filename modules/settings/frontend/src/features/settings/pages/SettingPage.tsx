import { useEffect, useMemo, useRef, useState, type ToggleEvent } from 'react'
import {
  Button,
  FormActions,
  InputText,
  PageHeader,
  PasswordInput,
  Textarea,
  ToggleSwitch,
  notify,
} from '@jarvis/core'
import { ChevronDown, RefreshCw, Save } from 'lucide-react'
import {
  DEFAULT_SETTING_CONNECTION,
  getSettingForm,
  getSettingGroups,
  saveSettingGroup,
} from '../services'
import {
  DEFAULT_DATE_FORMAT,
  DEFAULT_TIME_FORMAT,
  buildDateTimeDisplayFormat,
  joinMultiSelectValue,
  splitMultiSelectValue,
} from '../utils'
import { SettingDatePicker } from '../components/SettingDatePicker'
import { SettingImageField } from '../components/SettingImageField'
import { SettingNumberField } from '../components/SettingNumberField'
import { SettingSelect } from '../components/SettingSelect'
import {
  parseSettingOptions,
  type SettingFormItem,
  type SettingGroup,
} from '../types'
import { clampTextLines, countTextLines, getMaxTextareaRows, getTextareaRows } from '../utils/typeOptions'
import { validateSettingForm } from '../validation'

const COMBOBOX_FILTER_THRESHOLD = 8
const LOCALIZATION_GROUP = 'Localization'
const DATE_FORMAT_KEY = 'Localization.DateFormat'
const TIME_FORMAT_KEY = 'Localization.TimeFormat'

const fieldClass =
  'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70'
const fieldInvalidClass =
  '!border-red-500 focus:!border-red-500 focus:!ring-red-500/20'
const primaryButtonClass =
  'pr-btn-primary inline-flex h-11 items-center justify-center gap-2 rounded-xl border-0 px-4 text-sm font-semibold shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30 disabled:cursor-not-allowed disabled:opacity-60'
const outlinedButtonClass =
  'pr-btn-outlined inline-flex h-10 items-center justify-center gap-2 rounded-xl border px-4 text-sm font-medium shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60'

type GroupFormState = {
  items: SettingFormItem[]
  values: Record<string, string>
  errors: Record<string, string>
  loading: boolean
  saving: boolean
  /** Đã gọi GetForm thành công ít nhất một lần (kể cả khi group rỗng). */
  loaded: boolean
}

type Status = {
  kind: 'success' | 'error'
  message: string
} | null

const emptyFormState: GroupFormState = {
  items: [],
  values: {},
  errors: {},
  loading: false,
  saving: false,
  loaded: false,
}

function createFormState(items: SettingFormItem[]): GroupFormState {
  return {
    items,
    values: Object.fromEntries(
      items.map((item) => [item.key, item.value ?? '']),
    ),
    errors: {},
    loading: false,
    saving: false,
    loaded: true,
  }
}

export function SettingPage() {
  const [groups, setGroups] = useState<SettingGroup[]>([])
  const [forms, setForms] = useState<Record<string, GroupFormState>>({})
  const [loadingGroups, setLoadingGroups] = useState(false)
  const [status, setStatus] = useState<Status>(null)
  const loadedGroupsRef = useRef(new Set<string>())
  const inflightGroupsRef = useRef(new Set<string>())
  const openGroupsRef = useRef(new Set<string>())

  const localizationValues = forms[LOCALIZATION_GROUP]?.values
  const dateFormat =
    localizationValues?.[DATE_FORMAT_KEY]?.trim() || DEFAULT_DATE_FORMAT
  const timeFormat =
    localizationValues?.[TIME_FORMAT_KEY]?.trim() || DEFAULT_TIME_FORMAT
  const dateTimeFormat = buildDateTimeDisplayFormat(dateFormat, timeFormat)

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

  const loadGroup = async (groupName: string, force = false) => {
    if (
      !force &&
      (loadedGroupsRef.current.has(groupName) ||
        inflightGroupsRef.current.has(groupName))
    ) {
      return
    }

    inflightGroupsRef.current.add(groupName)
    patchForm(groupName, { loading: true })
    try {
      const items = await getSettingForm(
        DEFAULT_SETTING_CONNECTION,
        groupName,
      )
      loadedGroupsRef.current.add(groupName)
      patchForm(groupName, createFormState(items))
    } catch (error) {
      loadedGroupsRef.current.delete(groupName)
      patchForm(groupName, {
        loading: false,
        loaded: false,
        items: [],
        values: {},
        errors: {},
      })
      setStatus({
        kind: 'error',
        message:
          error instanceof Error ? error.message : 'Không tải được Setting.',
      })
    } finally {
      inflightGroupsRef.current.delete(groupName)
    }
  }

  const loadGroups = async () => {
    setLoadingGroups(true)
    setStatus(null)
    try {
      const nextGroups = await getSettingGroups(DEFAULT_SETTING_CONNECTION)
      setGroups(nextGroups)
      loadedGroupsRef.current.clear()
      inflightGroupsRef.current.clear()
      setForms({})

      // Chỉ tải lại form của các accordion đang mở (không prefetch tất cả).
      const openNames = [...openGroupsRef.current].filter((name) =>
        nextGroups.some((group) => group.name === name),
      )
      await Promise.all(openNames.map((name) => loadGroup(name, true)))
    } catch (error) {
      setGroups([])
      setForms({})
      loadedGroupsRef.current.clear()
      inflightGroupsRef.current.clear()
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

  const handleAccordionToggle = (
    groupName: string,
    event: ToggleEvent<HTMLDetailsElement>,
  ) => {
    if (event.currentTarget.open) {
      openGroupsRef.current.add(groupName)
      void loadGroup(groupName)
      return
    }
    openGroupsRef.current.delete(groupName)
  }

  useEffect(() => {
    void loadGroups()
    // Chỉ tải danh sách group một lần khi mở trang.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const handleSave = async (group: SettingGroup) => {
    const form = forms[group.name]
    if (!form) return

    const validationErrors = validateSettingForm(form.items, form.values)
    if (validationErrors.length) {
      const errors = Object.fromEntries(
        validationErrors.map((error) => [error.key, error.message]),
      )
      patchForm(group.name, { errors })
      const message = `Vui lòng kiểm tra ${validationErrors.length} trường không hợp lệ trước khi lưu.`
      setStatus({ kind: 'error', message })
      notify.error(message)
      return
    }

    // Secret rỗng không được gửi lên API (backend từ chối persist/encrypt chuỗi rỗng).
    // Bỏ key khỏi payload = không đụng secret đã lưu; muốn đổi thì phải gửi giá trị non-empty.
    const values = Object.fromEntries(
      form.items
        .filter((item) => !item.isReadOnly)
        .flatMap((item) => {
          const value = form.values[item.key] ?? ''
          const isSecret =
            item.isEncrypted === true || item.type.toLowerCase() === 'password'
          if (isSecret && value.trim() === '') return []
          return [[item.key, value] as const]
        }),
    )
    if (!Object.keys(values).length) return

    patchForm(group.name, { saving: true, errors: {} })
    setStatus(null)
    try {
      await saveSettingGroup(
        DEFAULT_SETTING_CONNECTION,
        group.name,
        values,
      )
      await loadGroup(group.name, true)
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
    <div className="flex h-full min-h-0 w-full flex-col overflow-hidden font-sans text-ink">
      <PageHeader
        title="Cấu hình hệ thống"
        className="mb-4 shrink-0 border-b border-line pb-4"
        actions={
          <Button
            type="button"
            unstyled
            className={outlinedButtonClass}
            disabled={loadingGroups}
            onClick={() => void loadGroups()}
          >
            <RefreshCw
              className={`size-4 ${loadingGroups ? 'animate-spin' : ''}`}
              aria-hidden
            />
            Tải lại
          </Button>
        }
      />

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
          <div className="flex h-full items-center justify-center text-sm text-mute">
            Đang tải danh sách cấu hình…
          </div>
        ) : groups.length ? (
          <div className="flex flex-col gap-3 pb-2">
            {groups.map((group) => {
              const form = forms[group.name] ?? emptyFormState
              return (
                <details
                  key={group.name}
                  className="group overflow-hidden rounded-xl border border-line bg-white shadow-sm"
                  onToggle={(event) =>
                    handleAccordionToggle(group.name, event)
                  }
                >
                  <summary className="flex cursor-pointer list-none items-center justify-between gap-4 bg-paper px-5 py-4 marker:hidden">
                    <div className="min-w-0">
                      <h3 className="m-0 text-base font-semibold text-ink">
                        {group.displayName || group.name}
                      </h3>
                      <p className="mt-1 m-0 text-xs leading-relaxed text-mute">
                        {group.description || group.name}
                      </p>
                    </div>
                    <div className="flex shrink-0 items-center gap-3">
                      {form.loaded ? (
                        <span className="rounded-full bg-slate-200/70 px-2.5 py-1 text-xs font-medium text-slate-600">
                          {form.items.length} cấu hình
                        </span>
                      ) : null}
                      <ChevronDown
                        className="size-5 text-slate-500 transition-transform group-open:rotate-180"
                        aria-hidden
                      />
                    </div>
                  </summary>

                  <div className="border-t border-line">
                    <div className="p-5">
                      {form.loading ? (
                        <div className="py-10 text-center text-sm text-mute">
                          Đang tải cấu hình…
                        </div>
                      ) : form.loaded && form.items.length ? (
                        <div className="grid gap-4 xl:grid-cols-2">
                          {form.items.map((item) => (
                            <SettingFieldCard
                              key={item.key}
                              item={item}
                              value={form.values[item.key] ?? ''}
                              error={form.errors[item.key]}
                              dateFormat={dateFormat}
                              dateTimeFormat={dateTimeFormat}
                              onChange={(value) =>
                                patchForm(group.name, (current) => {
                                  const nextErrors = { ...current.errors }
                                  delete nextErrors[item.key]
                                  return {
                                    ...current,
                                    values: {
                                      ...current.values,
                                      [item.key]: value,
                                    },
                                    errors: nextErrors,
                                  }
                                })
                              }
                              onFieldError={(message) =>
                                patchForm(group.name, (current) => ({
                                  ...current,
                                  errors: {
                                    ...current.errors,
                                    [item.key]: message,
                                  },
                                }))
                              }
                            />
                          ))}
                        </div>
                      ) : form.loaded ? (
                        <div className="py-10 text-center text-sm text-mute">
                          Không có cấu hình trong nhóm này.
                        </div>
                      ) : (
                        <div className="py-10 text-center text-sm text-mute">
                          Mở nhóm để tải cấu hình…
                        </div>
                      )}
                    </div>

                    <div className="border-t border-line bg-paper/60 px-5">
                      <FormActions>
                        <Button
                          type="button"
                          unstyled
                          className={primaryButtonClass}
                          disabled={
                            !form.items.length || form.saving || form.loading
                          }
                          onClick={() => void handleSave(group)}
                        >
                          <Save className="size-4" aria-hidden />
                          {form.saving ? 'Đang lưu…' : 'Lưu nhóm cấu hình'}
                        </Button>
                      </FormActions>
                    </div>
                  </div>
                </details>
              )
            })}
          </div>
        ) : (
          <div className="flex h-full items-center justify-center text-sm text-mute">
            Không có nhóm Setting. Kiểm tra kết nối tới Sample.
          </div>
        )}
      </div>
    </div>
  )
}

function SettingFieldCard({
  item,
  value,
  error,
  dateFormat,
  dateTimeFormat,
  onChange,
  onFieldError,
}: {
  item: SettingFormItem
  value: string
  error?: string
  dateFormat: string
  dateTimeFormat: string
  onChange: (value: string) => void
  onFieldError: (message: string) => void
}) {
  return (
    <article className="flex flex-col rounded-xl border border-line bg-white p-4">
      <div className="mb-3 flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h4 className="m-0 text-sm font-semibold text-ink">
            {item.name || item.key}
          </h4>
          <code className="mt-1 block break-all text-[11px] text-mute">
            {item.key}
          </code>
        </div>
        {item.isReadOnly && (
          <span className="shrink-0 rounded-full bg-amber-50 px-2 py-1 text-[10px] font-medium text-amber-700">
            Chỉ đọc
          </span>
        )}
      </div>

      {item.description && (
        <p className="mb-3 mt-0 text-xs leading-relaxed text-mute">
          {item.description}
        </p>
      )}

      <div className="mt-auto">
        <SettingControl
          item={item}
          value={value}
          invalid={Boolean(error)}
          dateFormat={dateFormat}
          dateTimeFormat={dateTimeFormat}
          onChange={onChange}
          onFieldError={onFieldError}
        />
        {error && (
          <p className="mb-0 mt-2 text-xs text-red-600" role="alert">
            {error}
          </p>
        )}
      </div>
    </article>
  )
}

function SettingControl({
  item,
  value,
  invalid,
  dateFormat,
  dateTimeFormat,
  onChange,
  onFieldError,
}: {
  item: SettingFormItem
  value: string
  invalid?: boolean
  dateFormat: string
  dateTimeFormat: string
  onChange: (value: string) => void
  onFieldError: (message: string) => void
}) {
  const type = item.type.toLowerCase()
  const controlClass = [fieldClass, invalid ? fieldInvalidClass : '']
    .filter(Boolean)
    .join(' ')

  if (type === 'textarea') {
    const rows = getTextareaRows(item.options)
    const maxRows = getMaxTextareaRows(item.options)
    return (
      <Textarea
        unstyled
        rows={rows}
        className={`${controlClass} !h-auto resize-y py-3`}
        style={{ minHeight: `${Math.max(rows, 3) * 1.5}rem` }}
        value={value}
        disabled={item.isReadOnly}
        onKeyDown={(event: { key: string; preventDefault: () => void }) => {
          if (item.isReadOnly || maxRows === null || event.key !== 'Enter') return
          if (countTextLines(value) >= maxRows) {
            event.preventDefault()
            onFieldError(`Tối đa ${maxRows} dòng.`)
          }
        }}
        onChange={(event: { target: { value: string } }) => {
          const next = event.target.value
          if (maxRows !== null && countTextLines(next) > maxRows) {
            onChange(clampTextLines(next, maxRows))
            onFieldError(`Tối đa ${maxRows} dòng.`)
            return
          }
          onChange(next)
        }}
      />
    )
  }

  if (type === 'combobox') {
    const options = parseSettingOptions(item.options)
    return (
      <SettingSelect
        value={value}
        options={options}
        disabled={item.isReadOnly}
        invalid={invalid}
        filter={options.length >= COMBOBOX_FILTER_THRESHOLD}
        filterPlaceholder="Tìm kiếm…"
        onChange={onChange}
      />
    )
  }

  if (type === 'radio') {
    const options = parseSettingOptions(item.options)
    return (
      <div className="flex flex-col gap-2 py-1" role="radiogroup">
        {options.map((option) => {
          const inputId = `${item.key}-${option.value}`
          return (
            <label
              key={option.value}
              htmlFor={inputId}
              className="inline-flex items-center gap-2.5 text-sm text-slate-800"
            >
              <input
                id={inputId}
                type="radio"
                name={item.key}
                className="size-4 border-slate-300 text-teal-700 accent-teal-700 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:opacity-70"
                value={option.value}
                checked={value === option.value}
                disabled={item.isReadOnly}
                onChange={() => onChange(option.value)}
              />
              <span>{option.label}</span>
            </label>
          )
        })}
      </div>
    )
  }

  if (type === 'multiselect') {
    return (
      <MultiSelectCheckboxes
        itemKey={item.key}
        optionsRaw={item.options}
        value={value}
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

  if (type === 'switch') {
    const checked = value.trim().toLowerCase() === 'true'
    return (
      <div className="flex h-11 items-center gap-3">
        <ToggleSwitch.Root
          checked={checked}
          disabled={item.isReadOnly}
          onCheckedChange={(event: { checked: boolean }) =>
            onChange(event.checked ? 'true' : 'false')
          }
          className="relative inline-flex h-6 w-11 shrink-0 data-[disabled]:cursor-not-allowed data-[disabled]:opacity-70"
          inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0 disabled:cursor-not-allowed"
          ariaLabel={item.name || item.key}
        >
          <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-6 w-11 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
            <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-5" />
          </ToggleSwitch.Control>
        </ToggleSwitch.Root>
        <span className="text-sm text-slate-800">{checked ? 'Bật' : 'Tắt'}</span>
      </div>
    )
  }

  if (type === 'password') {
    return (
      <PasswordInput
        value={value}
        autoComplete="new-password"
        disabled={item.isReadOnly}
        invalid={invalid}
        className={controlClass}
        onValueChange={(event) => onChange(event.value ?? '')}
      />
    )
  }

  if (type === 'datetime') {
    return (
      <SettingDatePicker
        value={value}
        showTime
        displayFormat={dateTimeFormat}
        disabled={item.isReadOnly}
        invalid={invalid}
        onChange={onChange}
      />
    )
  }

  if (type === 'date') {
    return (
      <SettingDatePicker
        value={value}
        displayFormat={dateFormat}
        disabled={item.isReadOnly}
        invalid={invalid}
        onChange={onChange}
      />
    )
  }

  if (type === 'image') {
    return (
      <SettingImageField
        value={value}
        options={item.options}
        disabled={item.isReadOnly}
        invalid={invalid}
        onChange={onChange}
        onError={onFieldError}
      />
    )
  }

  if (type === 'number') {
    return (
      <SettingNumberField
        value={value}
        options={item.options}
        disabled={item.isReadOnly}
        invalid={invalid}
        className={controlClass}
        onChange={onChange}
      />
    )
  }

  return (
    <InputText
      unstyled
      type={type === 'email' ? 'email' : 'text'}
      className={controlClass}
      value={value}
      disabled={item.isReadOnly}
      onChange={(event: { target: { value: string } }) =>
        onChange(event.target.value)
      }
    />
  )
}

function MultiSelectCheckboxes({
  itemKey,
  optionsRaw,
  value,
  disabled,
  onChange,
}: {
  itemKey: string
  optionsRaw?: string | null
  value: string
  disabled?: boolean
  onChange: (value: string) => void
}) {
  const options = useMemo(() => parseSettingOptions(optionsRaw), [optionsRaw])
  const selected = useMemo(() => new Set(splitMultiSelectValue(value)), [value])

  return (
    <div className="flex flex-col gap-2 py-1">
      {options.map((option) => {
        const inputId = `${itemKey}-${option.value}`
        const checked = selected.has(option.value)
        return (
          <label
            key={option.value}
            htmlFor={inputId}
            className="inline-flex items-center gap-2.5 text-sm text-slate-800"
          >
            <input
              id={inputId}
              type="checkbox"
              className="size-4 rounded border-slate-300 text-teal-700 accent-teal-700 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:opacity-70"
              checked={checked}
              disabled={disabled}
              onChange={(event) => {
                const next = new Set(selected)
                if (event.target.checked) next.add(option.value)
                else next.delete(option.value)
                onChange(
                  joinMultiSelectValue(
                    options
                      .map((entry) => entry.value)
                      .filter((entry) => next.has(entry)),
                  ),
                )
              }}
            />
            <span>{option.label}</span>
          </label>
        )
      })}
    </div>
  )
}
