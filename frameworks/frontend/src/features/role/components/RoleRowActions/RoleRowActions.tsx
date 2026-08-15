import { useMemo, useState } from 'react'
import { Eye, KeyRound, MoreHorizontal, Pencil, Trash2 } from 'lucide-react'
import { Popover } from 'primereact/popover'
import type { Role } from '../../types'
import { btnTextClass } from '../fieldStyles'

export type RoleRowActionsProps = {
  role: Role
  onView?: (role: Role) => void
  onEdit?: (role: Role) => void
  onPermissions?: (role: Role) => void
  onDelete?: (role: Role) => void
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

export function RoleRowActions({
  role,
  onView,
  onEdit,
  onPermissions,
  onDelete,
}: RoleRowActionsProps) {
  const [open, setOpen] = useState(false)

  const actions = useMemo(() => {
    const list: RowAction[] = []

    if (onView) {
      list.push({
        id: 'view',
        label: 'Xem chi tiết',
        icon: Eye,
        onClick: () => onView(role),
      })
    }
    if (onEdit) {
      list.push({
        id: 'edit',
        label: 'Chỉnh sửa',
        icon: Pencil,
        onClick: () => onEdit(role),
      })
    }
    if (onPermissions) {
      list.push({
        id: 'permissions',
        label: 'Phân quyền',
        icon: KeyRound,
        onClick: () => onPermissions(role),
      })
    }
    if (onDelete) {
      list.push({
        id: 'delete',
        label: 'Xoá',
        icon: Trash2,
        onClick: () => onDelete(role),
        danger: true,
        separatorBefore: list.length > 0,
      })
    }

    return list
  }, [role, onView, onEdit, onPermissions, onDelete])

  if (actions.length === 0) return null

  return (
    <Popover.Root
      open={open}
      onOpenChange={(e: { value?: boolean }) => setOpen(Boolean(e.value))}
    >
      <Popover.Trigger
        type="button"
        aria-label="Thao tác"
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
