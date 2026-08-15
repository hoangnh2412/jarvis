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
import { fieldInputClass, fieldInputInvalidClass } from '../fieldStyles'
import type { TenantDomainFormData } from '../../validation'

export type DomainFormProps = {
  register: UseFormRegister<TenantDomainFormData>
  control: Control<TenantDomainFormData>
  errors: FieldErrors<TenantDomainFormData>
}

export function DomainForm({ register, control, errors }: DomainFormProps) {
  return (
    <div className="flex flex-col gap-4">
      <div>
        <Label
          htmlFor="domain-host"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Domain <span className="text-red-500">*</span>
        </Label>
        <InputText
          id="domain-host"
          placeholder="vd. sample.domain.com"
          unstyled
          className={errors.domain ? fieldInputInvalidClass : fieldInputClass}
          {...register('domain')}
        />
        {errors.domain?.message && (
          <Message.Root severity="error" className="mt-1.5 border-0 bg-transparent p-0">
            <Message.Content>
              <Message.Text className="text-sm text-red-600">
                {errors.domain.message}
              </Message.Text>
            </Message.Content>
          </Message.Root>
        )}
      </div>

      <div className="flex items-center justify-between gap-3 rounded-xl border border-line bg-slate-50/70 px-3.5 py-3">
        <div>
          <p className="m-0 text-sm font-medium text-ink">Domain chính</p>
          <p className="m-0 mt-0.5 text-xs text-mute">
            Đánh dấu domain dùng làm home tenant mặc định.
          </p>
        </div>
        <Controller
          name="isPrimary"
          control={control}
          render={({ field }) => (
            <ToggleSwitch.Root
              checked={Boolean(field.value)}
              onCheckedChange={(e: { checked: boolean }) =>
                field.onChange(e.checked)
              }
              className="relative inline-flex h-6 w-11 shrink-0"
              inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0"
              ariaLabel="Domain chính"
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
