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
import { RegisterForm } from '../../components/RegisterForm'
import { useRegisterForm } from '../../hooks'
import { ACCOUNT_ROUTES } from '../../routes/paths'
import { callRegister } from '../../services'
import {
  resolveAccountContent,
  type AccountSlotContent,
} from '../../utils'
import type { RegisterFormData } from '../../validation'

export type RegisterPageContentContext = {
  register: UseFormRegister<RegisterFormData>
  control: Control<RegisterFormData>
  errors: FieldErrors<RegisterFormData>
  isSubmitting: boolean
  handleSubmit: UseFormHandleSubmit<RegisterFormData>
  submit: FormEventHandler<HTMLFormElement>
  DefaultContent: ReactNode
}

export type RegisterPageProps = {
  /** Action callback kiểu jQuery ajax. `callback.onSubmit` thay `callRegister`. */
  callback?: ActionProps<{ data: RegisterFormData }, RegisterFormData>
  onLoginClick?: () => void
  /** @default ACCOUNT_ROUTES.login */
  loginHref?: string
  /** @default true */
  showLoginLink?: boolean
  title?: string
  description?: string
  submitLabel?: string
  logo?: ReactNode
  className?: string
  defaultValues?: Partial<RegisterFormData>
  content?: AccountSlotContent<RegisterPageContentContext>
  /** @default true */
  withShell?: boolean
}

export function RegisterPage({
  callback,
  onLoginClick,
  loginHref = ACCOUNT_ROUTES.login,
  showLoginLink = true,
  title,
  description,
  submitLabel,
  logo,
  className,
  defaultValues,
  content,
  withShell = true,
}: RegisterPageProps) {
  const navigate = useNavigate()
  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useRegisterForm({ defaultValues })

  const submit = handleSubmit(async (data) => {
    try {
      const outcome = await handleAction({
        ctx: { data },
        callback,
        defaultSubmit: callRegister,
        getPayload: ({ data: payload }) => payload,
        onSuccess: async () => {
          navigate(loginHref, { replace: true })
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success('Đăng ký thành công')
    } catch (error) {
      notify.error(getErrorMessage(error, 'Đăng ký thất bại. Vui lòng thử lại.'))
    }
  })

  const handleLogin = onLoginClick ?? (() => navigate(loginHref))

  const defaultFields = (
    <RegisterForm register={register} control={control} errors={errors} />
  )

  const contentCtx: RegisterPageContentContext = {
    register,
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
    <AuthShell
      variant="register"
      title={title}
      description={description}
      submitLabel={submitLabel}
      logo={logo}
      className={className}
      isSubmitting={isSubmitting}
      onSubmit={submit}
      footer={
        showLoginLink ? (
          <>
            Đã có tài khoản?{' '}
            <AccountAuthLink href={loginHref} onClick={handleLogin}>
              Đăng nhập
            </AccountAuthLink>
          </>
        ) : undefined
      }
    >
      {resolvedContent}
    </AuthShell>
  )
}
