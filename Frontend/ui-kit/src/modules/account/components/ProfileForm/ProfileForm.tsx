import { type FieldErrors, type UseFormRegister } from 'react-hook-form'
import { InputText } from 'primereact/inputtext'
import { Label } from 'primereact/label'
import type { ProfileFormData } from '../../validation'

export type ProfileFormProps = {
  register: UseFormRegister<ProfileFormData>
  errors: FieldErrors<ProfileFormData>
}

export function ProfileForm({ register, errors }: ProfileFormProps) {
  return (
    <>
      <div>
        <Label
          htmlFor="profile-name"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Họ và tên <span className="text-red-500">*</span>
        </Label>
        <InputText
          id="profile-name"
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
          htmlFor="profile-email"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Email <span className="text-red-500">*</span>
        </Label>
        <InputText
          id="profile-email"
          type="email"
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
          htmlFor="profile-phone"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Số điện thoại
        </Label>
        <InputText
          id="profile-phone"
          type="tel"
          placeholder="0901234567"
          unstyled
          className={
            errors.phone
              ? 'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70 !border-red-500 focus:!border-red-500 focus:!ring-red-500/20'
              : 'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70'
          }
          {...register('phone')}
        />
        <p className="mt-1.5 text-sm text-slate-500">
          Tùy chọn — dùng để liên hệ khi cần.
        </p>
        {errors.phone?.message && (
          <p className="mt-1.5 text-sm text-red-600">{errors.phone.message}</p>
        )}
      </div>
    </>
  )
}
