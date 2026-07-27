import type { ReactNode } from 'react'
import { ProgressSpinner } from 'primereact/progressspinner'

type LoadingProps = {
  label?: string
  fullScreen?: boolean
  size?: 'sm' | 'md' | 'lg'
  className?: string
}

const sizeClass = {
  sm: 'h-5 w-5',
  md: 'h-8 w-8',
  lg: 'h-12 w-12',
} as const

export default function Loading({
  label = 'Đang tải...',
  fullScreen = false,
  size = 'md',
  className = '',
}: LoadingProps) {
  const content = (
    <div
      className={[
        'flex flex-col items-center justify-center gap-3 text-slate-500',
        className,
      ].join(' ')}
      role="status"
      aria-live="polite"
    >
      <ProgressSpinner.Root className={sizeClass[size]}>
        <ProgressSpinner.Track />
        <ProgressSpinner.Range />
      </ProgressSpinner.Root>
      {label && <p className="text-sm font-medium">{label}</p>}
      <span className="sr-only">{label}</span>
    </div>
  )

  if (fullScreen) {
    return (
      <div className="flex min-h-svh w-full items-center justify-center bg-slate-50">
        {content}
      </div>
    )
  }

  return content
}

export type { LoadingProps }
