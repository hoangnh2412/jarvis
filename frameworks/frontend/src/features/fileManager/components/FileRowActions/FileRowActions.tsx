import { useMemo, useState } from 'react'
import {
  FolderOutput,
  FolderOpen,
  MoreHorizontal,
  Pencil,
  Trash2,
} from 'lucide-react'
import { Popover } from 'primereact/popover'
import type { FileEntry } from '../../types'
import { btnTextClass } from '../fieldStyles'

export type FileRowActionsProps = {
  entry: FileEntry
  labels: {
    actions: string
    open: string
    rename: string
    move: string
    delete: string
  }
  disabled?: boolean
  onOpen?: (entry: FileEntry) => void
  onRename?: (entry: FileEntry) => void
  onMove?: (entry: FileEntry) => void
  onDelete?: (entry: FileEntry) => void
}

type RowAction = {
  id: string
  label: string
  icon: typeof FolderOpen
  onClick: () => void
  danger?: boolean
  separatorBefore?: boolean
}

const itemClass =
  'flex w-full items-center gap-2.5 border-0 bg-transparent px-3 py-2.5 text-left text-sm text-slate-700 hover:bg-slate-50'

const itemDangerClass =
  'flex w-full items-center gap-2.5 border-0 bg-transparent px-3 py-2.5 text-left text-sm text-red-600 hover:bg-red-50'

export function FileRowActions({
  entry,
  labels,
  disabled = false,
  onOpen,
  onRename,
  onMove,
  onDelete,
}: FileRowActionsProps) {
  const [open, setOpen] = useState(false)

  const actions = useMemo(() => {
    const list: RowAction[] = []

    if (onOpen) {
      list.push({
        id: 'open',
        label: labels.open,
        icon: FolderOpen,
        onClick: () => onOpen(entry),
      })
    }
    if (onRename) {
      list.push({
        id: 'rename',
        label: labels.rename,
        icon: Pencil,
        onClick: () => onRename(entry),
      })
    }
    if (onMove) {
      list.push({
        id: 'move',
        label: labels.move,
        icon: FolderOutput,
        onClick: () => onMove(entry),
      })
    }
    if (onDelete) {
      list.push({
        id: 'delete',
        label: labels.delete,
        icon: Trash2,
        onClick: () => onDelete(entry),
        danger: true,
        separatorBefore: list.length > 0,
      })
    }

    return list
  }, [entry, labels, onOpen, onRename, onMove, onDelete])

  if (actions.length === 0) return null

  return (
    <Popover.Root
      open={open}
      onOpenChange={(e: { value?: boolean }) => setOpen(Boolean(e.value))}
    >
      <Popover.Trigger
        type="button"
        disabled={disabled}
        aria-label={labels.actions}
        className={`${btnTextClass} inline-flex !h-9 !w-9 !px-0 data-[focused]:outline-none data-[focused]:ring-0`}
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
