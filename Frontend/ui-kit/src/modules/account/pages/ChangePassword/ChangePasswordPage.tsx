import { type FormEventHandler, type ReactNode } from 'react'
import { Controller } from 'react-hook-form'
import type {
  Control,
  FieldErrors,
  UseFormHandleSubmit,
} from 'react-hook-form'
import { Button } from 'primereact/button'
import { Label } from 'primereact/label'
import PasswordInput from '../../../../common/PasswordInput'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { useChangePasswordForm } from '../../hooks'
import { callUpdatePassword } from '../../services'
import {
  resolveAccountContent,
  type AccountSlotContent,
} from '../../utils'
import {
  changePasswordFormDefaultValues,
  type ChangePasswordFormData,
} from '../../validation'

function ChangePasswordFields({
  control,
  errors,
}: {
  control: Control<ChangePasswordFormData>
  errors: FieldErrors<ChangePasswordFormData>
}) {
  return (
    <div className="mt-6 flex flex-col gap-4">
      <div>
        <Label
          htmlFor="current-password"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Mật khẩu hiện tại <span className="text-red-500">*</span>
        </Label>
        <Controller
          name="currentPassword"
          control={control}
          render={({ field }) => (
            <PasswordInput
              id="current-password"
              autoComplete="current-password"
              placeholder="••••••••"
              invalid={Boolean(errors.currentPassword)}
              value={field.value ?? ''}
              onValueChange={(e: { value?: string | null }) =>
                field.onChange(e.value ?? '')
              }
              onBlur={field.onBlur}
            />
          )}
        />
        {errors.currentPassword?.message && (
          <p className="mt-1.5 text-sm text-red-600">
            {errors.currentPassword.message}
          </p>
        )}
      </div>

      <div>
        <Label
          htmlFor="new-password"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Mật khẩu mới <span className="text-red-500">*</span>
        </Label>
        <Controller
          name="newPassword"
          control={control}
          render={({ field }) => (
            <PasswordInput
              id="new-password"
              autoComplete="new-password"
              placeholder="••••••••"
              invalid={Boolean(errors.newPassword)}
              value={field.value ?? ''}
              onValueChange={(e: { value?: string | null }) =>
                field.onChange(e.value ?? '')
              }
              onBlur={field.onBlur}
            />
          )}
        />
        {errors.newPassword?.message && (
          <p className="mt-1.5 text-sm text-red-600">{errors.newPassword.message}</p>
        )}
      </div>

      <div>
        <Label
          htmlFor="confirm-password"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Xác nhận mật khẩu <span className="text-red-500">*</span>
        </Label>
        <Controller
          name="confirmPassword"
          control={control}
          render={({ field }) => (
            <PasswordInput
              id="confirm-password"
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
    </div>
  )
}

export type ChangePasswordPageContentContext = {
  control: Control<ChangePasswordFormData>
  errors: FieldErrors<ChangePasswordFormData>
  isSubmitting: boolean
  handleSubmit: UseFormHandleSubmit<ChangePasswordFormData>
  submit: FormEventHandler<HTMLFormElement>
  DefaultContent: ReactNode
}

export type ChangePasswordPageProps = {
  /** Action callback kiểu jQuery ajax. `callback.onSubmit` thay `callUpdatePassword`. */
  callback?: ActionProps<
    { data: ChangePasswordFormData },
    ChangePasswordFormData
  >
  title?: string
  description?: string
  submitLabel?: string
  className?: string
  defaultValues?: Partial<ChangePasswordFormData>
  content?: AccountSlotContent<ChangePasswordPageContentContext>
  /** @default true */
  withShell?: boolean
}

export function ChangePasswordPage({
  callback,
  title = 'Đổi mật khẩu',
  description = 'Nhập mật khẩu hiện tại và mật khẩu mới.',
  submitLabel = 'Cập nhật mật khẩu',
  className = '',
  defaultValues,
  content,
  withShell = true,
}: ChangePasswordPageProps) {
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useChangePasswordForm({ defaultValues })

  const submit = handleSubmit(async (data) => {
    try {
      const outcome = await handleAction({
        ctx: { data },
        callback,
        defaultSubmit: callUpdatePassword,
        getPayload: ({ data: payload }) => payload,
        onSuccess: async () => {
          reset(changePasswordFormDefaultValues)
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success('Mật khẩu đã được cập nhật')
    } catch (error) {
      notify.error(
        getErrorMessage(error, 'Không đổi được mật khẩu. Vui lòng thử lại.'),
      )
    }
  })

  const defaultFields = <ChangePasswordFields control={control} errors={errors} />

  const contentCtx: ChangePasswordPageContentContext = {
    control,
    errors,
    isSubmitting,
    handleSubmit,
    submit,
    DefaultContent: defaultFields,
  }

  const resolvedContent = resolveAccountContent(content, contentCtx, defaultFields)

  if (!withShell) {
    return <>{resolvedContent}</>
  }

  return (
    <form
      onSubmit={submit}
      noValidate
      className={[
        'mx-auto w-full max-w-[480px] rounded-xl border border-line bg-white p-6 font-sans text-ink',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <h2 className="m-0 text-[1.35rem] font-semibold leading-tight tracking-tight text-ink">
        {title}
      </h2>
      {description && (
        <p className="mt-1.5 m-0 text-sm leading-relaxed text-mute">{description}</p>
      )}

      {resolvedContent}

      <div className="mt-6">
        <Button
          type="submit"
          unstyled
          disabled={isSubmitting}
          className="pr-btn-primary inline-flex h-11 w-full items-center justify-center gap-2 rounded-xl border-0 px-4 text-sm font-semibold shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isSubmitting ? 'Đang lưu…' : submitLabel}
        </Button>
      </div>
    </form>
  )
}
