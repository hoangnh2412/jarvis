import { useMemo, useState } from 'react'
import { Eye, MoreHorizontal, Pencil, Trash2 } from 'lucide-react'
import { Popover } from 'primereact/popover'
import type { UserMessages } from '../../localization'
import type { AdminUser } from '../../types'
import { btnTextClass } from '../fieldStyles'

export type UserRowActionsProps = {
  user: AdminUser
  messages: UserMessages
  onView?: (user: AdminUser) => void
  onEdit?: (user: AdminUser) => void
  onDelete?: (user: AdminUser) => void
}

type RowAction = {
  id: string
  label: string
  icon: typeof Eye
  onClick: () => void
  danger?: boolean
  separatorBefore?: boolean
}

const itemClass =
  'flex w-full items-center gap-2.5 border-0 bg-transparent px-3 py-2 text-left text-sm text-slate-700 hover:bg-slate-50'

const itemDangerClass =
  'flex w-full items-center gap-2.5 border-0 bg-transparent px-3 py-2 text-left text-sm text-red-600 hover:bg-red-50'

export function UserRowActions({
  user,
  messages,
  onView,
  onEdit,
  onDelete,
}: UserRowActionsProps) {
  const [open, setOpen] = useState(false)

  const actions = useMemo(() => {
    const list: RowAction[] = []
    if (onView) {
      list.push({
        id: 'view',
        label: messages.actions.view,
        icon: Eye,
        onClick: () => onView(user),
      })
    }
    if (onEdit) {
      list.push({
        id: 'edit',
        label: messages.actions.edit,
        icon: Pencil,
        onClick: () => onEdit(user),
      })
    }
    if (onDelete) {
      list.push({
        id: 'delete',
        label: messages.actions.delete,
        icon: Trash2,
        onClick: () => onDelete(user),
        danger: true,
        separatorBefore: list.length > 0,
      })
    }
    return list
  }, [messages, onDelete, onEdit, onView, user])

  if (actions.length === 0) return null

  return (
    <Popover.Root
      open={open}
      onOpenChange={(event: { value?: boolean }) => setOpen(Boolean(event.value))}
    >
      <Popover.Trigger
        type="button"
        aria-label={messages.actions.menu}
        className={`${btnTextClass} inline-flex !h-9 !w-9 !px-0`}
      >
        <MoreHorizontal className="size-4" />
      </Popover.Trigger>
      <Popover.Portal>
        <Popover.Positioner
          className="z-[200]"
          side="bottom"
          align="end"
          sideOffset={4}
          collisionPadding={8}
        >
          <Popover.Popup className="min-w-[11rem] overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-lg">
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
                    <Icon className="size-4 shrink-0" aria-hidden />
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
