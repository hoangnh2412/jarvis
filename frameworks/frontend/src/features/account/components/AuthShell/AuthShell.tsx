import type { FormEventHandler, ReactNode } from 'react'
import { Button } from 'primereact/button'
import Logo from '../../../../common/Logo'
import { AUTH_PRESETS } from '../../localization'
import type { AccountAuthVariant } from '../../types'

export type { AccountAuthVariant }

export type AuthShellProps = {
  variant?: AccountAuthVariant
  title?: string
  description?: string
  submitLabel?: string
  isSubmitting?: boolean
  onSubmit?: FormEventHandler<HTMLFormElement>
  children: ReactNode
  beforeSubmit?: ReactNode
  footer?: ReactNode
  logo?: ReactNode
  formHidden?: boolean
  hiddenMessage?: string
  className?: string
}

export default function AuthShell({
  variant = 'login',
  title,
  description,
  submitLabel,
  isSubmitting = false,
  onSubmit,
  children,
  beforeSubmit,
  footer,
  logo,
  formHidden = false,
  hiddenMessage = 'Kiểm tra hộp thư của bạn để tiếp tục.',
  className = '',
}: AuthShellProps) {
  const preset = AUTH_PRESETS[variant]
  const resolvedTitle = title ?? preset.title
  const resolvedDescription = description ?? preset.description
  const resolvedSubmit = submitLabel ?? preset.submitLabel
  const showForm = !formHidden
  const brand = logo ?? (
    <div className="flex items-center gap-3">
      <Logo size="md" showText={false} />
      <div className="min-w-0">
        <p className="m-0 text-sm font-semibold tracking-tight text-inherit">
          FE Component
        </p>
        <p className="m-0 text-xs text-zinc-400">Account</p>
      </div>
    </div>
  )

  return (
    <div
      className={[
        'box-border grid min-h-dvh w-full grid-cols-1 bg-paper font-sans text-ink',
        'min-[960px]:grid-cols-[minmax(280px,0.95fr)_minmax(0,1.05fr)]',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <aside className="relative hidden min-h-dvh flex-col justify-between overflow-hidden bg-panel p-10 text-zinc-50 min-[960px]:flex">
        <div
          aria-hidden
          className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_70%_55%_at_20%_15%,rgba(15,118,110,0.35),transparent_55%),radial-gradient(ellipse_50%_40%_at_90%_85%,rgba(39,39,42,0.9),transparent_50%)]"
        />
        <div className="relative z-[1] text-zinc-50">{brand}</div>
        <div className="relative z-[1] max-w-[360px]">
          <p className="m-0 text-[11px] font-semibold uppercase tracking-[0.16em] text-teal-300">
            {preset.panelEyebrow}
          </p>
          <p className="mt-3.5 m-0 text-[1.75rem] font-semibold leading-tight tracking-tight text-zinc-50">
            {preset.panelLine}
          </p>
        </div>
        <p className="relative z-[1] m-0 text-xs text-zinc-400">
          Bảo mật · Xác thực · Hồ sơ
        </p>
      </aside>

      <div className="box-border flex min-h-dvh animate-account-auth-in flex-col justify-center px-5 py-7 motion-reduce:animate-none">
        <div className="mb-7 flex justify-start min-[960px]:hidden">{brand}</div>
        <div className="mx-auto w-full max-w-[400px]">
          <h1 className="m-0 text-[1.625rem] font-semibold leading-tight tracking-tight text-ink">
            {resolvedTitle}
          </h1>
          {resolvedDescription && (
            <p className="mt-2 m-0 max-w-[36ch] text-[0.9375rem] leading-relaxed text-mute">
              {resolvedDescription}
            </p>
          )}
          {showForm ? (
            <form onSubmit={onSubmit} noValidate className="mt-7">
              <div className="flex flex-col gap-[0.95rem]">{children}</div>
              {beforeSubmit && (
                <div className="mt-3 flex justify-end text-sm">{beforeSubmit}</div>
              )}
              <div className="mt-[1.35rem]">
                <Button
                  type="submit"
                  unstyled
                  disabled={isSubmitting}
                  className="pr-btn-primary inline-flex h-11 w-full items-center justify-center gap-2 rounded-full border-0 px-4 text-sm font-semibold shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {isSubmitting ? 'Đang xử lý…' : resolvedSubmit}
                </Button>
              </div>
              {footer && (
                <div className="mt-[1.35rem] border-t border-line pt-[1.15rem] text-left text-sm text-mute">
                  {footer}
                </div>
              )}
            </form>
          ) : (
            <div className="mt-7">
              <div className="rounded-xl border border-line bg-white px-[1.15rem] py-5">
                <p className="m-0 text-[0.9375rem] leading-relaxed text-ink">
                  {hiddenMessage}
                </p>
              </div>
              {footer && <div className="mt-5 text-sm text-mute">{footer}</div>}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

export { AuthShell as AccountAuthPage }
export type { AuthShellProps as AccountAuthPageProps }

type AccountAuthLinkProps = {
  children: ReactNode
  onClick?: () => void
  href?: string
  className?: string
}

export function AccountAuthLink({
  children,
  onClick,
  href,
  className = '',
}: AccountAuthLinkProps) {
  const classes = [
    // Reset button chrome (app có thể không load Tailwind preflight)
    'inline cursor-pointer border-0 bg-transparent p-0 font-sans text-inherit appearance-none',
    'font-medium text-teal-700 underline-offset-2 transition-colors hover:text-teal-800 hover:underline',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  if (href) {
    return (
      <a
        href={href}
        className={classes}
        onClick={(e) => {
          if (onClick) {
            e.preventDefault()
            onClick()
          }
        }}
      >
        {children}
      </a>
    )
  }

  return (
    <button type="button" className={classes} onClick={onClick}>
      {children}
    </button>
  )
}
