import { useState, type ReactNode } from 'react'
import type { Control, FieldErrors, UseFormRegister } from 'react-hook-form'
import { Controller } from 'react-hook-form'
import { InputText } from 'primereact/inputtext'
import { Select } from 'primereact/select'
import { ToggleSwitch } from 'primereact/toggleswitch'
import PasswordInput from '../../../../common/PasswordInput'
import type { UserMessages } from '../../localization'
import type { UserRoleOption } from '../../types'
import type { UserFormData } from '../../validation'
import { fieldInputClass, fieldInputInvalidClass } from '../fieldStyles'

export type UserFormProps = {
  mode: 'create' | 'edit'
  messages: UserMessages
  register: UseFormRegister<UserFormData>
  control: Control<UserFormData>
  errors: FieldErrors<UserFormData>
  roles: UserRoleOption[]
}

function Field({
  label,
  error,
  children,
}: {
  label: string
  error?: string
  children: ReactNode
}) {
  return (
    <label className="flex flex-col gap-1.5">
      <span className="text-sm font-medium text-slate-700">{label}</span>
      {children}
      {error ? <span className="text-xs text-red-600">{error}</span> : null}
    </label>
  )
}

export function UserForm({
  mode,
  messages,
  register,
  control,
  errors,
  roles,
}: UserFormProps) {
  const [roleOpen, setRoleOpen] = useState(false)
  const roleOptions = roles.map((role) => ({
    label: `${role.displayName} (${role.name})`,
    value: role.id,
  }))

  return (
    <div className="grid gap-4 md:grid-cols-2">
      <Field label={messages.form.username} error={errors.username?.message}>
        <InputText
          {...register('username')}
          readOnly={mode === 'edit'}
          unstyled
          className={
            errors.username ? fieldInputInvalidClass : fieldInputClass
          }
        />
      </Field>
      <Field label={messages.form.email} error={errors.email?.message}>
        <InputText
          {...register('email')}
          readOnly={mode === 'edit'}
          unstyled
          className={errors.email ? fieldInputInvalidClass : fieldInputClass}
        />
      </Field>
      {mode === 'create' ? (
        <Field label={messages.form.password} error={errors.password?.message}>
          <Controller
            name="password"
            control={control}
            render={({ field }) => (
              <PasswordInput
                name={field.name}
                value={field.value}
                invalid={Boolean(errors.password)}
                autoComplete="new-password"
                onBlur={field.onBlur}
                onValueChange={(event) => field.onChange(event.value ?? '')}
              />
            )}
          />
        </Field>
      ) : null}
      <Field label={messages.form.role} error={errors.roleId?.message}>
        <Controller
          name="roleId"
          control={control}
          render={({ field }) => (
            <Select.Root
              value={field.value}
              open={roleOpen}
              options={roleOptions}
              optionLabel="label"
              optionValue="value"
              onOpenChange={(event: { value: boolean }) =>
                setRoleOpen(event.value)
              }
              onValueChange={(event: { value?: unknown }) =>
                field.onChange(event.value == null ? '' : String(event.value))
              }
            >
              <Select.Trigger
                type="button"
                className={
                  errors.roleId ? fieldInputInvalidClass : fieldInputClass
                }
              >
                <Select.Value placeholder={messages.form.role} />
                <Select.Indicator className="text-slate-400">▾</Select.Indicator>
              </Select.Trigger>
              <Select.Portal>
                <Select.Positioner className="z-[200]">
                  <Select.Popup className="max-h-64 min-w-[var(--px-positioner-anchor-width)] overflow-auto rounded-xl border border-slate-200 bg-white py-1 shadow-lg">
                    <Select.List className="m-0 list-none p-0 outline-none">
                      {roleOptions.map((option, index) => (
                        <Select.Option
                          key={option.value}
                          index={index}
                          className="cursor-pointer px-3 py-2 text-sm text-slate-800 outline-none data-[focused]:bg-slate-50 data-[selected]:bg-teal-50 data-[selected]:font-medium data-[selected]:text-teal-800"
                        >
                          {option.label}
                        </Select.Option>
                      ))}
                    </Select.List>
                  </Select.Popup>
                </Select.Positioner>
              </Select.Portal>
            </Select.Root>
          )}
        />
      </Field>
      <Field label={messages.form.tenantId} error={errors.tenantId?.message}>
        <InputText
          {...register('tenantId')}
          readOnly={mode === 'edit'}
          unstyled
          className={fieldInputClass}
          placeholder="tenant-a"
        />
      </Field>
      <Field label={messages.form.orgId} error={errors.orgId?.message}>
        <InputText
          {...register('orgId')}
          readOnly={mode === 'edit'}
          unstyled
          className={fieldInputClass}
          placeholder="org-a"
        />
      </Field>
      {mode === 'edit' ? (
        <Controller
          name="isLocked"
          control={control}
          render={({ field }) => (
            <label className="flex items-center justify-between gap-3 rounded-xl border border-slate-200 bg-slate-50/60 p-4 md:col-span-2">
              <span className="text-sm text-slate-700">{messages.form.locked}</span>
              <ToggleSwitch.Root
                checked={Boolean(field.value)}
                onCheckedChange={(event: { checked: boolean }) =>
                  field.onChange(event.checked)
                }
                className="relative inline-flex h-6 w-11 shrink-0"
                inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0"
                ariaLabel={messages.form.locked}
              >
                <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-6 w-11 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
                  <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-5" />
                </ToggleSwitch.Control>
              </ToggleSwitch.Root>
            </label>
          )}
        />
      ) : null}
    </div>
  )
}
