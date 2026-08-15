import type {
  Control,
  FieldErrors,
  UseFormRegister,
} from 'react-hook-form'
import { Controller } from 'react-hook-form'
import { InputText } from 'primereact/inputtext'
import { Textarea } from 'primereact/textarea'
import { ToggleSwitch } from 'primereact/toggleswitch'
import type { RoleFormData } from '../../validation'
import { fieldInputClass, fieldInputInvalidClass } from '../fieldStyles'

export type RoleFormProps = {
  register: UseFormRegister<RoleFormData>
  control: Control<RoleFormData>
  errors: FieldErrors<RoleFormData>
  readOnly?: boolean
}

function Field({
  label,
  error,
  children,
}: {
  label: string
  error?: string
  children: React.ReactNode
}) {
  return (
    <label className="flex flex-col gap-1.5">
      <span className="text-sm font-medium text-slate-700">{label}</span>
      {children}
      {error ? <span className="text-xs text-red-600">{error}</span> : null}
    </label>
  )
}

export function RoleForm({
  register,
  control,
  errors,
  readOnly = false,
}: RoleFormProps) {
  return (
    <div className="flex flex-col gap-4">
      <Field label="Tên vai trò *" error={errors.name?.message}>
        <InputText
          {...register('name')}
          readOnly={readOnly}
          unstyled
          className={errors.name ? fieldInputInvalidClass : fieldInputClass}
          placeholder="supporter"
        />
      </Field>

      <Field label="Tên hiển thị *" error={errors.displayName?.message}>
        <InputText
          {...register('displayName')}
          readOnly={readOnly}
          unstyled
          className={errors.displayName ? fieldInputInvalidClass : fieldInputClass}
          placeholder="Supporter"
        />
      </Field>

      <Field label="Mô tả" error={errors.description?.message}>
        <Textarea
          {...register('description')}
          readOnly={readOnly}
          rows={3}
          className="box-border w-full rounded-xl border border-slate-200 px-3.5 py-2.5 text-sm shadow-sm outline-none focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20"
          placeholder="Mô tả ngắn về vai trò…"
        />
      </Field>

      <div className="flex flex-col gap-3 rounded-xl border border-slate-200 bg-slate-50/60 p-4">
        <Controller
          name="isDefault"
          control={control}
          render={({ field }) => (
            <label className="flex items-center justify-between gap-3">
              <span className="text-sm text-slate-700">Vai trò mặc định</span>
              <ToggleSwitch.Root
                checked={Boolean(field.value)}
                disabled={readOnly}
                onCheckedChange={(e: { checked: boolean }) =>
                  field.onChange(e.checked)
                }
                className="relative inline-flex h-6 w-11 shrink-0"
                inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0 disabled:cursor-not-allowed"
                ariaLabel="Vai trò mặc định"
              >
                <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-6 w-11 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
                  <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-5" />
                </ToggleSwitch.Control>
              </ToggleSwitch.Root>
            </label>
          )}
        />
        <Controller
          name="isPublic"
          control={control}
          render={({ field }) => (
            <label className="flex items-center justify-between gap-3">
              <span className="text-sm text-slate-700">Công khai (public)</span>
              <ToggleSwitch.Root
                checked={Boolean(field.value)}
                disabled={readOnly}
                onCheckedChange={(e: { checked: boolean }) =>
                  field.onChange(e.checked)
                }
                className="relative inline-flex h-6 w-11 shrink-0"
                inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0 disabled:cursor-not-allowed"
                ariaLabel="Công khai"
              >
                <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-6 w-11 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
                  <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-5" />
                </ToggleSwitch.Control>
              </ToggleSwitch.Root>
            </label>
          )}
        />
      </div>
    </div>
  )
}
