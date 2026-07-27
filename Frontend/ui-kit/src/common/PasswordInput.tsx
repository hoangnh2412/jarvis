import { useState, type ChangeEvent, type FocusEventHandler } from 'react'
import { Eye, EyeOff } from 'lucide-react'
import { InputText } from 'primereact/inputtext'

export type PasswordInputProps = {
  id?: string
  name?: string
  value?: string
  placeholder?: string
  autoComplete?: string
  disabled?: boolean
  invalid?: boolean
  className?: string
  onBlur?: FocusEventHandler<HTMLInputElement>
  onValueChange?: (event: { value?: string | null }) => void
}

/**
 * Ô mật khẩu + Eye / EyeOff — icon trong ô, bên phải.
 */
export default function PasswordInput({
  invalid = false,
  className = '',
  value,
  onValueChange,
  onBlur,
  ...props
}: PasswordInputProps) {
  const [masked, setMasked] = useState(true)

  return (
    <div className="relative block w-full">
      <InputText
        {...props}
        type={masked ? 'password' : 'text'}
        unstyled
        value={value ?? ''}
        onBlur={onBlur}
        onChange={(e: ChangeEvent<HTMLInputElement>) => {
          onValueChange?.({ value: e.target.value })
        }}
        className={[
          invalid
            ? 'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70 !border-red-500 focus:!border-red-500 focus:!ring-red-500/20'
            : 'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70',
          'pr-11 [&::-ms-reveal]:hidden [&::-ms-clear]:hidden',
          className,
        ]
          .filter(Boolean)
          .join(' ')}
      />
      <button
        type="button"
        tabIndex={-1}
        aria-label={masked ? 'Hiện mật khẩu' : 'Ẩn mật khẩu'}
        className="absolute inset-y-0 right-0 z-[1] flex w-11 items-center justify-center border-0 bg-transparent text-slate-400 hover:text-slate-600 focus-visible:rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30 focus-visible:ring-inset"
        onClick={() => setMasked((v) => !v)}
      >
        {masked ? (
          <Eye className="h-4 w-4" strokeWidth={1.75} aria-hidden />
        ) : (
          <EyeOff className="h-4 w-4" strokeWidth={1.75} aria-hidden />
        )}
      </button>
    </div>
  )
}
