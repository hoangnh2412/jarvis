import { type FieldErrors, type UseFormRegister } from 'react-hook-form'
import { InputText } from 'primereact/inputtext'
import { Label } from 'primereact/label'
import type { ForgotPasswordFormData } from '../../validation'

export type ForgotPasswordFormProps = {
  register: UseFormRegister<ForgotPasswordFormData>
  errors: FieldErrors<ForgotPasswordFormData>
}

export function ForgotPasswordForm({
  register,
  errors,
}: ForgotPasswordFormProps) {
  return (
    <div>
      <Label
        htmlFor="forgot-email"
        className="mb-1.5 block text-sm font-medium text-slate-700"
      >
        Email <span className="text-red-500">*</span>
      </Label>
      <InputText
        id="forgot-email"
        type="email"
        autoComplete="email"
        placeholder="you@example.com"
        unstyled
        className={
          errors.email
            ? 'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70 !border-red-500 focus:!border-red-500 focus:!ring-red-500/20'
            : 'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70'
        }
        {...register('email')}
      />
      {errors.email?.message && (
        <p className="mt-1.5 text-sm text-red-600">{errors.email.message}</p>
      )}
    </div>
  )
}
