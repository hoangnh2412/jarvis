import { useState, type ComponentType, type ReactNode } from 'react'
import { MoreHorizontal } from 'lucide-react'
import { Popover } from 'primereact/popover'

export type RowActionItem = {
  id: string
  label: string
  icon?: ComponentType<{ className?: string }>
  onClick: () => void
  danger?: boolean
  separatorBefore?: boolean
}

export type RowActionsProps = {
  actions: RowActionItem[]
  ariaLabel?: string
  disabled?: boolean
  triggerClassName?: string
  popupClassName?: string
  triggerIcon?: ReactNode
}

const itemClass =
  'flex w-full items-center gap-2.5 border-0 bg-transparent px-3 py-2 text-left text-sm text-slate-700 hover:bg-slate-50'

const itemDangerClass =
  'flex w-full items-center gap-2.5 border-0 bg-transparent px-3 py-2 text-left text-sm text-red-600 hover:bg-red-50'

export function RowActions({
  actions,
  ariaLabel = 'Thao tác',
  disabled = false,
  triggerClassName = 'inline-flex size-8 items-center justify-center rounded-lg border border-transparent text-slate-500 transition-colors hover:border-slate-200 hover:bg-white hover:text-slate-800',
  popupClassName = 'min-w-[11rem] overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-lg',
  triggerIcon,
}: RowActionsProps) {
  const [open, setOpen] = useState(false)

  if (actions.length === 0) return null

  return (
    <Popover.Root
      open={open}
      onOpenChange={(e: { value?: boolean }) => setOpen(Boolean(e.value))}
    >
      <Popover.Trigger
        type="button"
        disabled={disabled}
        aria-label={ariaLabel}
        className={triggerClassName}
      >
        {triggerIcon ?? <MoreHorizontal className="size-4" />}
      </Popover.Trigger>
      <Popover.Portal>
        <Popover.Positioner
          className="z-[200]"
          side="bottom"
          align="end"
          sideOffset={4}
          collisionPadding={8}
        >
          <Popover.Popup className={popupClassName}>
            {actions.map((action) => {
              const Icon = action.icon
              return (
                <span key={action.id} className="contents">
                  {action.separatorBefore ? (
                    <div className="my-1 border-t border-slate-100" role="separator" />
                  ) : null}
                  <button
                    type="button"
                    className={action.danger ? itemDangerClass : itemClass}
                    onClick={() => {
                      action.onClick()
                      setOpen(false)
                    }}
                  >
                    {Icon ? <Icon className="size-4 shrink-0" aria-hidden /> : null}
                    {action.label}
                  </button>
                </span>
              )
            })}
          </Popover.Popup>
        </Popover.Positioner>
      </Popover.Portal>
    </Popover.Root>
  )
}
