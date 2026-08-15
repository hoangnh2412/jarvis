import { useEffect, useState } from 'react'
import { ChevronDown } from 'lucide-react'
import { Button } from 'primereact/button'
import { Dialog } from 'primereact/dialog'
import { InputText } from 'primereact/inputtext'
import { Select } from 'primereact/select'
import { ToggleSwitch } from 'primereact/toggleswitch'
import type {
  CanvasChartInstance,
  ChartSettings,
  SettingField,
} from '../../types'
import { SettingFieldType } from '../../types'
import {
  DASHBOARD_CHART_ANIMATION_FIELD_NAME,
  DASHBOARD_CHART_ANIMATION_NONE,
  DASHBOARD_CHART_ANIMATION_OPTIONS,
} from '../../constants/chartAnimations'
import {
  btnOutlinedClass,
  btnPrimaryClass,
  fieldInputClass,
} from '../fieldStyles'

export type ChartSettingsDialogProps = {
  visible: boolean
  onHide: () => void
  instance: CanvasChartInstance | null
  title?: string
  applyLabel?: string
  cancelLabel?: string
  onApply: (settings: ChartSettings) => void
}

const selectTriggerClass =
  'inline-flex h-11 w-full items-center justify-between gap-2 rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition hover:border-slate-300 focus-visible:border-teal-600 focus-visible:ring-2 focus-visible:ring-teal-600/20'

const CSS_ANIMATION_FIELD: SettingField = {
  name: DASHBOARD_CHART_ANIMATION_FIELD_NAME,
  label: 'CSS Animation',
  type: SettingFieldType.select,
  options: DASHBOARD_CHART_ANIMATION_OPTIONS,
}

function withCssAnimationField(fields: SettingField[]): SettingField[] {
  if (fields.some((f) => f.name === DASHBOARD_CHART_ANIMATION_FIELD_NAME)) {
    return fields
  }
  const chartTypeIdx = fields.findIndex((f) => f.name === 'chartType')
  if (chartTypeIdx < 0) return [...fields, CSS_ANIMATION_FIELD]
  return [
    ...fields.slice(0, chartTypeIdx + 1),
    CSS_ANIMATION_FIELD,
    ...fields.slice(chartTypeIdx + 1),
  ]
}

function SettingSelect({
  value,
  options,
  onChange,
  maxHeightPx = 280,
}: {
  value: string
  options: Array<{ label: string; value: string }>
  onChange: (v: string) => void
  /** Cap dropdown height so long lists (e.g. animations) can scroll */
  maxHeightPx?: number
}) {
  const [open, setOpen] = useState(false)
  return (
    <Select.Root
      value={value || undefined}
      open={open}
      options={options}
      optionLabel="label"
      optionValue="value"
      onOpenChange={(e: { value: boolean }) => setOpen(e.value)}
      onValueChange={(e: { value?: unknown }) => {
        if (e.value != null) onChange(String(e.value))
      }}
    >
      <Select.Trigger type="button" className={selectTriggerClass}>
        <Select.Value placeholder="Chọn…" />
        <Select.Indicator
          className={[
            'inline-flex shrink-0 text-slate-500 transition-transform',
            open ? 'rotate-180' : '',
          ].join(' ')}
        >
          <ChevronDown className="size-4" aria-hidden />
        </Select.Indicator>
      </Select.Trigger>
      <Select.Portal>
        <Select.Positioner className="z-[130]">
          <Select.Popup
            className="min-w-[var(--px-positioner-anchor-width)] overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-lg"
            style={{ maxHeight: maxHeightPx }}
          >
            <Select.List
              className="m-0 list-none p-0 outline-none"
              style={{
                maxHeight: maxHeightPx - 8,
                overflowY: 'auto',
                overscrollBehavior: 'contain',
                WebkitOverflowScrolling: 'touch',
              }}
            >
              {options.map((option, index) => (
                <Select.Option
                  key={option.value}
                  index={index}
                  className="cursor-pointer px-3.5 py-2.5 text-sm text-slate-800 outline-none transition-colors data-[focused]:bg-slate-50 data-[selected]:bg-teal-50 data-[selected]:font-medium data-[selected]:text-teal-800"
                >
                  {option.label}
                </Select.Option>
              ))}
            </Select.List>
          </Select.Popup>
        </Select.Positioner>
      </Select.Portal>
    </Select.Root>
  )
}

function FieldControl({
  field,
  value,
  onChange,
}: {
  field: SettingField
  value: unknown
  onChange: (v: unknown) => void
}) {
  if (field.type === 'switch') {
    return (
      <div className="flex items-center justify-between gap-3 rounded-xl border border-slate-100 bg-slate-50/70 px-3.5 py-3">
        <span className="text-sm font-medium text-slate-700">{field.label}</span>
        <ToggleSwitch.Root
          checked={Boolean(value)}
          onCheckedChange={(e: { checked: boolean }) => onChange(e.checked)}
          className="relative inline-flex h-6 w-11 shrink-0"
          inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0"
          ariaLabel={field.label}
        >
          <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-6 w-11 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
            <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-5" />
          </ToggleSwitch.Control>
        </ToggleSwitch.Root>
      </div>
    )
  }

  if (field.type === 'select') {
    return (
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-slate-600">
          {field.label}
        </label>
        <SettingSelect
          value={typeof value === 'string' ? value : ''}
          options={field.options ?? []}
          onChange={onChange}
          maxHeightPx={
            field.name === DASHBOARD_CHART_ANIMATION_FIELD_NAME ? 280 : 240
          }
        />
      </div>
    )
  }

  if (field.type === 'number') {
    return (
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-slate-600">
          {field.label}
        </label>
        <InputText
          unstyled
          type="number"
          value={value == null ? '' : String(value)}
          min={field.min}
          max={field.max}
          step={field.step ?? 1}
          className={fieldInputClass}
          onChange={(e: { target: { value: string } }) =>
            onChange(Number(e.target.value) || 0)
          }
        />
      </div>
    )
  }

  if (field.type === 'color') {
    return (
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-slate-600">
          {field.label}
        </label>
        <div className="flex items-center gap-2">
          <input
            type="color"
            value={typeof value === 'string' ? value : '#0d9488'}
            className="h-11 w-12 cursor-pointer rounded-xl border border-slate-200 bg-white p-1"
            onChange={(e) => onChange(e.target.value)}
          />
          <InputText
            unstyled
            value={typeof value === 'string' ? value : ''}
            className={fieldInputClass}
            onChange={(e: { target: { value: string } }) =>
              onChange(e.target.value)
            }
          />
        </div>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-1.5">
      <label className="text-xs font-medium text-slate-600">{field.label}</label>
      <InputText
        unstyled
        value={typeof value === 'string' ? value : ''}
        placeholder={field.placeholder}
        className={fieldInputClass}
        onChange={(e: { target: { value: string } }) =>
          onChange(e.target.value)
        }
      />
    </div>
  )
}

export function ChartSettingsDialog({
  visible,
  onHide,
  instance,
  title = 'Cài đặt biểu đồ',
  applyLabel = 'Áp dụng',
  cancelLabel = 'Hủy',
  onApply,
}: ChartSettingsDialogProps) {
  const [draft, setDraft] = useState<ChartSettings | null>(null)

  useEffect(() => {
    if (visible && instance) {
      setDraft({
        cssAnimation: DASHBOARD_CHART_ANIMATION_NONE,
        ...instance.settings,
      })
    }
    if (!visible) setDraft(null)
  }, [visible, instance])

  const fields = instance
    ? withCssAnimationField(instance.settingsForm)
    : []

  return (
    <Dialog.Root
      open={visible && !!instance && !!draft}
      onOpenChange={(e: { value?: boolean }) => {
        if (!e.value) onHide()
      }}
    >
      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-[100] bg-slate-900/40 backdrop-blur-[2px]" />
        <Dialog.Positioner className="fixed inset-0 z-[110] flex items-center justify-center p-4">
          <Dialog.Popup className="flex w-full max-w-[440px] flex-col overflow-hidden rounded-xl border border-slate-200 bg-white p-0 shadow-xl">
            <Dialog.Header className="flex items-start justify-between gap-3 border-b border-slate-100 px-5 py-4">
              <Dialog.Title className="text-lg font-semibold text-slate-900">
                {title}
              </Dialog.Title>
              <Dialog.Close
                className="rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                aria-label="Đóng"
              />
            </Dialog.Header>

            <Dialog.Content className="flex flex-col gap-4 px-5 py-4">
              {draft &&
                fields.map((field) => (
                  <FieldControl
                    key={field.name}
                    field={field}
                    value={draft[field.name]}
                    onChange={(v) =>
                      setDraft((prev) =>
                        prev ? { ...prev, [field.name]: v } : prev,
                      )
                    }
                  />
                ))}
            </Dialog.Content>

            <Dialog.Footer className="flex justify-end gap-2 border-t border-slate-100 px-5 py-3">
              <Button
                type="button"
                unstyled
                className={btnOutlinedClass}
                onClick={onHide}
              >
                {cancelLabel}
              </Button>
              <Button
                type="button"
                unstyled
                className={btnPrimaryClass}
                onClick={() => {
                  if (!draft) return
                  onApply(draft)
                  onHide()
                }}
              >
                {applyLabel}
              </Button>
            </Dialog.Footer>
          </Dialog.Popup>
        </Dialog.Positioner>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
