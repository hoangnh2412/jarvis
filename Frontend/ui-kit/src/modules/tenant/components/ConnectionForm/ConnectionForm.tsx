import {
  Controller,
  type Control,
  type FieldErrors,
  type UseFormRegister,
} from 'react-hook-form'
import { InputText } from 'primereact/inputtext'
import { Label } from 'primereact/label'
import { Message } from 'primereact/message'
import { ToggleSwitch } from 'primereact/toggleswitch'
import { FieldSelect } from '../FieldSelect'
import { fieldInputClass, fieldInputInvalidClass } from '../fieldStyles'
import { DB_PROVIDER_OPTIONS } from '../../types'
import type { TenantConnectionFormData } from '../../validation'

export type ConnectionFormProps = {
  register: UseFormRegister<TenantConnectionFormData>
  control: Control<TenantConnectionFormData>
  errors: FieldErrors<TenantConnectionFormData>
}

function FieldError({ message }: { message?: string }) {
  if (!message) return null
  return (
    <Message.Root severity="error" className="mt-1.5 border-0 bg-transparent p-0">
      <Message.Content>
        <Message.Text className="text-sm text-red-600">{message}</Message.Text>
      </Message.Content>
    </Message.Root>
  )
}

export function ConnectionForm({
  register,
  control,
  errors,
}: ConnectionFormProps) {
  return (
    <div className="flex flex-col gap-4">
      <div>
        <Label
          htmlFor="conn-provider"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Provider <span className="text-red-500">*</span>
        </Label>
        <Controller
          name="providerType"
          control={control}
          render={({ field }) => (
            <FieldSelect
              id="conn-provider"
              value={field.value}
              options={DB_PROVIDER_OPTIONS}
              onChange={field.onChange}
              placeholder="Chọn provider"
              invalid={Boolean(errors.providerType)}
            />
          )}
        />
        <FieldError message={errors.providerType?.message} />
      </div>

      <div>
        <Label
          htmlFor="conn-string"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Connection string <span className="text-red-500">*</span>
        </Label>
        <InputText
          id="conn-string"
          placeholder="Host=…;Database=…"
          unstyled
          className={
            errors.connectionString
              ? fieldInputInvalidClass
              : fieldInputClass
          }
          {...register('connectionString')}
        />
        <FieldError message={errors.connectionString?.message} />
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <Label
            htmlFor="conn-from"
            className="mb-1.5 block text-sm font-medium text-slate-700"
          >
            Partition from
          </Label>
          <InputText
            id="conn-from"
            type="date"
            unstyled
            className={fieldInputClass}
            {...register('partitionFrom', {
              setValueAs: (v: string) => (v?.trim() ? v.trim() : null),
            })}
          />
        </div>
        <div>
          <Label
            htmlFor="conn-to"
            className="mb-1.5 block text-sm font-medium text-slate-700"
          >
            Partition to
          </Label>
          <InputText
            id="conn-to"
            type="date"
            unstyled
            className={
              errors.partitionTo ? fieldInputInvalidClass : fieldInputClass
            }
            {...register('partitionTo', {
              setValueAs: (v: string) => (v?.trim() ? v.trim() : null),
            })}
          />
          <FieldError message={errors.partitionTo?.message} />
        </div>
      </div>

      <div className="flex items-center justify-between gap-3 rounded-xl border border-line bg-slate-50/70 px-3.5 py-3">
        <div>
          <p className="m-0 text-sm font-medium text-ink">Nguồn mặc định</p>
          <p className="m-0 mt-0.5 text-xs text-mute">
            Dùng khi partitionKey không khớp khoảng nào.
          </p>
        </div>
        <Controller
          name="isDefault"
          control={control}
          render={({ field }) => (
            <ToggleSwitch.Root
              checked={Boolean(field.value)}
              onCheckedChange={(e: { checked: boolean }) =>
                field.onChange(e.checked)
              }
              className="relative inline-flex h-6 w-11 shrink-0"
              inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0"
              ariaLabel="Nguồn mặc định"
            >
              <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-6 w-11 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
                <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-5" />
              </ToggleSwitch.Control>
            </ToggleSwitch.Root>
          )}
        />
      </div>
    </div>
  )
}
