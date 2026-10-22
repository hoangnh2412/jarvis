import type { ReactNode } from 'react'
import type { FieldErrors, UseFormRegister } from 'react-hook-form'
import { InputText } from '@platform/core'
import type { OrgFormData } from '../../validation'
import { fieldInputClass, fieldInputInvalidClass } from '../fieldStyles'

export type OrgFormProps = {
  register: UseFormRegister<OrgFormData>
  errors: FieldErrors<OrgFormData>
  codeLabel: string
  nameLabel: string
  codePlaceholder?: string
  namePlaceholder?: string
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

export function OrgForm({
  register,
  errors,
  codeLabel,
  nameLabel,
  codePlaceholder,
  namePlaceholder,
}: OrgFormProps) {
  return (
    <div className="flex flex-col gap-4">
      <Field label={codeLabel} error={errors.code?.message}>
        <InputText
          {...register('code')}
          unstyled
          autoComplete="off"
          className={errors.code ? fieldInputInvalidClass : fieldInputClass}
          placeholder={codePlaceholder}
        />
      </Field>
      <Field label={nameLabel} error={errors.name?.message}>
        <InputText
          {...register('name')}
          unstyled
          className={errors.name ? fieldInputInvalidClass : fieldInputClass}
          placeholder={namePlaceholder}
        />
      </Field>
    </div>
  )
}
