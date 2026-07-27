import type { ReactNode } from 'react'

type FormActionsProps = {
  children: ReactNode
  className?: string
  sticky?: boolean
  align?: 'start' | 'end' | 'between'
}

const alignClasses = {
  start: 'justify-start',
  end: 'justify-end',
  between: 'justify-between',
}

/** Layout hàng nút form — dùng kèm PrimeReact Button */
export default function FormActions({
  children,
  className = '',
  sticky = false,
  align = 'end',
}: FormActionsProps) {
  return (
    <div
      className={[
        'flex flex-wrap items-center gap-2 pt-4',
        alignClasses[align],
        sticky
          ? 'sticky bottom-0 z-10 -mx-3 mt-2 bg-slate-50/95 px-3 py-2 backdrop-blur lg:-mx-4 lg:px-4'
          : 'mt-2 pt-2',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      {children}
    </div>
  )
}

export type { FormActionsProps }
