import { type FormEventHandler, type ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import type {
  Control,
  FieldErrors,
  UseFormHandleSubmit,
  UseFormRegister,
} from 'react-hook-form'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { AuthShell, AccountAuthLink } from '../../components/AuthShell'
import { LoginForm } from '../../components/LoginForm'
import { useLoginForm } from '../../hooks'
import { ACCOUNT_ROUTES } from '../../routes/paths'
import { callLogin } from '../../services'
import {
  resolveAccountContent,
  type AccountSlotContent,
} from '../../utils'
import type { LoginFormData } from '../../validation'

export type LoginPageContentContext = {
  register: UseFormRegister<LoginFormData>
  control: Control<LoginFormData>
  errors: FieldErrors<LoginFormData>
  isSubmitting: boolean
  handleSubmit: UseFormHandleSubmit<LoginFormData>
  /** Handler gắn vào <form onSubmit> — đã validate + toast */
  submit: FormEventHandler<HTMLFormElement>
  /** Form fields mặc định của kit */
  DefaultContent: ReactNode
}

export type LoginPageProps = {
  /**
   * Action callback kiểu jQuery ajax.
   * `callback.onSubmit` thay API mặc định `callLogin`.
   */
  callback?: ActionProps<{ data: LoginFormData }, LoginFormData>
  onForgotClick?: () => void
  onRegisterClick?: () => void
  /** @default ACCOUNT_ROUTES.forgotPassword */
  forgotHref?: string
  /** @default ACCOUNT_ROUTES.register */
  registerHref?: string
  /** @default true */
  showForgotLink?: boolean
  /** @default true */
  showRegisterLink?: boolean
  title?: string
  description?: string
  submitLabel?: string
  logo?: ReactNode
  className?: string
  defaultValues?: Partial<LoginFormData>
  content?: AccountSlotContent<LoginPageContentContext>
  /** @default true */
  withShell?: boolean
}

export function LoginPage({
  callback,
  onForgotClick,
  onRegisterClick,
  forgotHref = ACCOUNT_ROUTES.forgotPassword,
  registerHref = ACCOUNT_ROUTES.register,
  showForgotLink = true,
  showRegisterLink = true,
  title,
  description,
  submitLabel,
  logo,
  className,
  defaultValues,
  content,
  withShell = true,
}: LoginPageProps) {
  const navigate = useNavigate()
  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useLoginForm({
    defaultValues: {
      email: '',
      password: '',
      ...defaultValues,
    },
  })

  const submit = handleSubmit(async (data) => {
    try {
      const outcome = await handleAction({
        ctx: { data },
        callback,
        defaultSubmit: callLogin,
        getPayload: ({ data: payload }) => payload,
        onSuccess: async () => {
          navigate('/', { replace: true })
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success('Đăng nhập thành công')
    } catch (error) {
      notify.error(
        getErrorMessage(error, 'Đăng nhập thất bại. Vui lòng thử lại.'),
      )
    }
  })

  const handleForgot = onForgotClick ?? (() => navigate(forgotHref))
  const handleRegister = onRegisterClick ?? (() => navigate(registerHref))

  const defaultFields = (
    <>
      <LoginForm register={register} control={control} errors={errors} />
    </>
  )

  const contentCtx: LoginPageContentContext = {
    register,
    control,
    errors,
    isSubmitting,
    handleSubmit,
    submit,
    DefaultContent: defaultFields,
  }

  const resolvedContent = resolveAccountContent(
    content,
    contentCtx,
    defaultFields,
  )

  if (!withShell) {
    return <>{resolvedContent}</>
  }

  return (
    <AuthShell
      variant="login"
      title={title}
      description={description}
      submitLabel={submitLabel}
      logo={logo}
      className={className}
      isSubmitting={isSubmitting}
      onSubmit={submit}
      beforeSubmit={
        showForgotLink ? (
          <AccountAuthLink href={forgotHref} onClick={handleForgot}>
            Quên mật khẩu?
          </AccountAuthLink>
        ) : undefined
      }
      footer={
        showRegisterLink ? (
          <>
            Chưa có tài khoản?{' '}
            <AccountAuthLink href={registerHref} onClick={handleRegister}>
              Đăng ký
            </AccountAuthLink>
          </>
        ) : undefined
      }
    >
      {resolvedContent}
    </AuthShell>
  )
}
