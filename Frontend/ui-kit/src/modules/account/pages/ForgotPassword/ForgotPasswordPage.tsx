import { useState, type FormEventHandler, type ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import type {
  FieldErrors,
  UseFormHandleSubmit,
  UseFormRegister,
} from 'react-hook-form'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { AuthShell, AccountAuthLink } from '../../components/AuthShell'
import { ForgotPasswordForm } from '../../components/ForgotPasswordForm'
import { useForgotPasswordForm } from '../../hooks'
import { ACCOUNT_ROUTES } from '../../routes/paths'
import { callForgotPassword } from '../../services'
import {
  resolveAccountContent,
  type AccountSlotContent,
} from '../../utils'
import type { ForgotPasswordFormData } from '../../validation'

export type ForgotPasswordPageContentContext = {
  register: UseFormRegister<ForgotPasswordFormData>
  errors: FieldErrors<ForgotPasswordFormData>
  isSubmitting: boolean
  sent: boolean
  handleSubmit: UseFormHandleSubmit<ForgotPasswordFormData>
  submit: FormEventHandler<HTMLFormElement>
  DefaultContent: ReactNode
}

export type ForgotPasswordPageProps = {
  /** Action callback kiểu jQuery ajax. `callback.onSubmit` thay `callForgotPassword`. */
  callback?: ActionProps<
    { data: ForgotPasswordFormData },
    ForgotPasswordFormData
  >
  onLoginClick?: () => void
  /** @default ACCOUNT_ROUTES.login */
  loginHref?: string
  /** @default true */
  showLoginLink?: boolean
  title?: string
  description?: string
  submitLabel?: string
  successMessage?: string
  logo?: ReactNode
  className?: string
  defaultValues?: Partial<ForgotPasswordFormData>
  content?: AccountSlotContent<ForgotPasswordPageContentContext>
  /** @default true */
  withShell?: boolean
}

export function ForgotPasswordPage({
  callback,
  onLoginClick,
  loginHref = ACCOUNT_ROUTES.login,
  showLoginLink = true,
  title,
  description,
  submitLabel,
  successMessage = 'Nếu email tồn tại, chúng tôi đã gửi liên kết đặt lại mật khẩu.',
  logo,
  className,
  defaultValues,
  content,
  withShell = true,
}: ForgotPasswordPageProps) {
  const navigate = useNavigate()
  const [sent, setSent] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForgotPasswordForm({ defaultValues })

  const submit = handleSubmit(async (data) => {
    try {
      const outcome = await handleAction({
        ctx: { data },
        callback,
        defaultSubmit: callForgotPassword,
        getPayload: ({ data: payload }) => payload,
        onSuccess: async () => {
          setSent(true)
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(successMessage)
    } catch (error) {
      notify.error(
        getErrorMessage(error, 'Không gửi được yêu cầu. Vui lòng thử lại.'),
      )
    }
  })

  const handleLogin = onLoginClick ?? (() => navigate(loginHref))

  const defaultFields = <ForgotPasswordForm register={register} errors={errors} />

  const contentCtx: ForgotPasswordPageContentContext = {
    register,
    errors,
    isSubmitting,
    sent,
    handleSubmit,
    submit,
    DefaultContent: defaultFields,
  }

  const resolvedContent = resolveAccountContent(content, contentCtx, defaultFields)

  if (!withShell) {
    return <>{resolvedContent}</>
  }

  return (
    <AuthShell
      variant="forgot"
      title={title}
      description={description}
      submitLabel={submitLabel}
      logo={logo}
      className={className}
      isSubmitting={isSubmitting}
      onSubmit={submit}
      formHidden={sent}
      hiddenMessage={successMessage}
      footer={
        showLoginLink ? (
          <AccountAuthLink href={loginHref} onClick={handleLogin}>
            Quay lại đăng nhập
          </AccountAuthLink>
        ) : undefined
      }
    >
      {resolvedContent}
    </AuthShell>
  )
}
