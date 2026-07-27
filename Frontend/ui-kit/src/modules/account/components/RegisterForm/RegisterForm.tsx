import {
  type Control,
  Controller,
  type FieldErrors,
  type UseFormRegister,
} from 'react-hook-form'
import { InputText } from 'primereact/inputtext'
import { Label } from 'primereact/label'
import PasswordInput from '../../../../common/PasswordInput'
import type { RegisterFormData } from '../../validation'

export type RegisterFormProps = {
  register: UseFormRegister<RegisterFormData>
  control: Control<RegisterFormData>
  errors: FieldErrors<RegisterFormData>
}

export function RegisterForm({ register, control, errors }: RegisterFormProps) {
  return (
    <>
      <div>
        <Label
          htmlFor="reg-name"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Họ và tên <span className="text-red-500">*</span>
        </Label>
        <InputText
          id="reg-name"
          autoComplete="name"
          placeholder="Nguyễn Văn A"
          unstyled
          className={
            errors.fullName
              ? 'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70 !border-red-500 focus:!border-red-500 focus:!ring-red-500/20'
              : 'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70'
          }
          {...register('fullName')}
        />
        {errors.fullName?.message && (
          <p className="mt-1.5 text-sm text-red-600">{errors.fullName.message}</p>
        )}
      </div>
      <div>
        <Label
          htmlFor="reg-email"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Email <span className="text-red-500">*</span>
        </Label>
        <InputText
          id="reg-email"
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
      <div>
        <Label
          htmlFor="reg-password"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Mật khẩu <span className="text-red-500">*</span>
        </Label>
        <Controller
          name="password"
          control={control}
          render={({ field }) => (
            <PasswordInput
              id="reg-password"
              autoComplete="new-password"
              placeholder="••••••••"
              invalid={Boolean(errors.password)}
              value={field.value ?? ''}
              onValueChange={(e: { value?: string | null }) =>
                field.onChange(e.value ?? '')
              }
              onBlur={field.onBlur}
            />
          )}
        />
        {errors.password?.message && (
          <p className="mt-1.5 text-sm text-red-600">{errors.password.message}</p>
        )}
      </div>
      <div>
        <Label
          htmlFor="reg-confirm"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Xác nhận mật khẩu <span className="text-red-500">*</span>
        </Label>
        <Controller
          name="confirmPassword"
          control={control}
          render={({ field }) => (
            <PasswordInput
              id="reg-confirm"
              autoComplete="new-password"
              placeholder="••••••••"
              invalid={Boolean(errors.confirmPassword)}
              value={field.value ?? ''}
              onValueChange={(e: { value?: string | null }) =>
                field.onChange(e.value ?? '')
              }
              onBlur={field.onBlur}
            />
          )}
        />
        {errors.confirmPassword?.message && (
          <p className="mt-1.5 text-sm text-red-600">
            {errors.confirmPassword.message}
          </p>
        )}
      </div>
    </>
  )
}
