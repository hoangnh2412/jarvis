import { useEffect, type FormEventHandler, type ReactNode } from 'react'
import type {
  FieldErrors,
  UseFormHandleSubmit,
  UseFormRegister,
  UseFormWatch,
} from 'react-hook-form'
import { Button } from 'primereact/button'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { ProfileCard } from '../../components/ProfileCard'
import { ProfileForm } from '../../components/ProfileForm'
import { useProfileForm } from '../../hooks'
import { callUpdateProfile } from '../../services'
import {
  resolveAccountContent,
  type AccountSlotContent,
} from '../../utils'
import {
  profileFormDefaultValues,
  type ProfileFormData,
} from '../../validation'

export type AccountProfilePageContentContext = {
  register: UseFormRegister<ProfileFormData>
  errors: FieldErrors<ProfileFormData>
  isSubmitting: boolean
  isDirty: boolean
  watch: UseFormWatch<ProfileFormData>
  handleSubmit: UseFormHandleSubmit<ProfileFormData>
  submit: FormEventHandler<HTMLFormElement>
  DefaultContent: ReactNode
  DefaultLeft: ReactNode
}

export type AccountProfilePageProps = {
  /** Action callback kiểu jQuery ajax. `callback.onSubmit` thay `callUpdateProfile`. */
  callback?: ActionProps<{ data: ProfileFormData }, ProfileFormData>
  onCancel?: () => void
  defaultValues?: Partial<ProfileFormData>
  title?: string
  description?: string
  submitLabel?: string
  cancelLabel?: string
  headerActions?: ReactNode
  className?: string
  /** Cột trái (card avatar). Mặc định ProfileCard */
  left?: ReactNode
  /**
   * Nội dung form (cột phải). Mặc định = ProfileForm.
   */
  content?: AccountSlotContent<AccountProfilePageContentContext>
  /**
   * `false` = chỉ render `content` (full custom layout).
   * @default true
   */
  withShell?: boolean
}

export default function AccountProfilePage({
  callback,
  onCancel,
  defaultValues,
  title = 'Hồ sơ tài khoản',
  description = 'Cập nhật thông tin cá nhân của bạn.',
  submitLabel = 'Lưu thay đổi',
  cancelLabel = 'Hủy',
  headerActions,
  className = '',
  left,
  content,
  withShell = true,
}: AccountProfilePageProps) {
  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors, isSubmitting, isDirty },
  } = useProfileForm({ defaultValues })

  useEffect(() => {
    reset({ ...profileFormDefaultValues, ...defaultValues })
  }, [defaultValues, reset])

  const fullName = watch('fullName')
  const email = watch('email')
  const phone = watch('phone')

  const submit = handleSubmit(async (data) => {
    try {
      const outcome = await handleAction({
        ctx: { data },
        callback,
        defaultSubmit: callUpdateProfile,
        getPayload: ({ data: payload }) => payload,
        onSuccess: async ({ data: payload }) => {
          reset(payload)
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success('Hồ sơ đã được cập nhật')
    } catch (error) {
      notify.error(
        getErrorMessage(error, 'Không lưu được hồ sơ. Vui lòng thử lại.'),
      )
    }
  })

  const defaultFields = <ProfileForm register={register} errors={errors} />
  const defaultLeft = (
    <ProfileCard fullName={fullName} email={email} phone={phone} />
  )

  const contentCtx: AccountProfilePageContentContext = {
    register,
    errors,
    isSubmitting,
    isDirty,
    watch,
    handleSubmit,
    submit,
    DefaultContent: defaultFields,
    DefaultLeft: defaultLeft,
  }

  const resolvedContent = resolveAccountContent(content, contentCtx, defaultFields)

  if (!withShell) {
    return <>{resolvedContent}</>
  }

  return (
    <form
      onSubmit={submit}
      className={[
        'flex h-full min-h-0 w-full animate-fade-in flex-col overflow-hidden font-sans text-ink motion-reduce:animate-none',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
      noValidate
    >
      <div className="mb-5 flex shrink-0 flex-wrap items-start justify-between gap-x-4 gap-y-3 border-b border-line pb-4">
        <div className="min-w-0">
          <h2 className="m-0 text-[1.35rem] font-semibold leading-tight tracking-tight text-ink">
            {title}
          </h2>
          {description && (
            <p className="mt-1.5 m-0 max-w-[48ch] text-sm leading-relaxed text-mute">
              {description}
            </p>
          )}
        </div>
        {headerActions && (
          <div className="flex shrink-0 flex-wrap items-center gap-2">
            {headerActions}
          </div>
        )}
      </div>

      <div className="grid min-h-0 flex-1 grid-cols-1 items-stretch gap-4 overflow-auto lg:grid-cols-[minmax(240px,280px)_minmax(0,1fr)]">
        {left ?? defaultLeft}

        <section className="flex min-h-0 flex-col overflow-hidden rounded-xl border border-line bg-white">
          <div className="shrink-0 border-b border-line bg-paper px-[1.35rem] py-[1.15rem]">
            <h3 className="m-0 text-[0.95rem] font-semibold tracking-tight text-ink">
              Thông tin cá nhân
            </h3>
            <p className="mt-1 m-0 text-[0.8125rem] leading-snug text-mute">
              Email dùng để đăng nhập. Chỉ lưu khi bạn bấm nút bên dưới.
            </p>
          </div>
          <div className="flex min-h-0 flex-1 flex-col gap-4 overflow-y-auto px-[1.35rem] py-5">
            {resolvedContent}
          </div>
        </section>
      </div>

      <div className="mt-[1.15rem] flex shrink-0 flex-wrap items-center justify-between gap-3 border-t border-line pt-4">
        <p
          className={[
            'm-0 text-[0.8125rem]',
            isDirty ? 'font-medium text-accent' : 'font-normal text-zinc-400',
          ].join(' ')}
        >
          {isDirty ? 'Có thay đổi chưa lưu' : 'Không có thay đổi'}
        </p>
        <div className="flex items-center gap-2.5">
          {onCancel && (
            <Button
              type="button"
              unstyled
              className="pr-btn-outlined inline-flex h-10 items-center justify-center gap-2 rounded-xl border px-4 text-sm font-medium shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60"
              onClick={onCancel}
            >
              {cancelLabel}
            </Button>
          )}
          <Button
            type="submit"
            unstyled
            className="pr-btn-primary inline-flex h-11 items-center justify-center gap-2 rounded-xl border-0 px-4 text-sm font-semibold shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={(!isDirty && !isSubmitting) || isSubmitting}
          >
            {isSubmitting ? 'Đang lưu…' : submitLabel}
          </Button>
        </div>
      </div>
    </form>
  )
}
