import { useEffect, useMemo } from 'react'
import { monitorForElements } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import type { PlannerColumn, PlannerItem } from '../../types'
import type { PlannerLocale } from '../../localization'
import { KanbanColumn } from '../KanbanColumn'

export type KanbanBoardProps = {
  columns: PlannerColumn[]
  items: PlannerItem[]
  locale?: PlannerLocale
  onMoveItem?: (itemId: string, statusId: string) => void
  onOpenItem?: (item: PlannerItem) => void
  onAddItem?: (statusId: string) => void
  className?: string
}

export function KanbanBoard({
  columns,
  items,
  locale = 'vi',
  onMoveItem,
  onOpenItem,
  onAddItem,
  className = '',
}: KanbanBoardProps) {
  const sortedColumns = useMemo(
    () => columns.slice().sort((a, b) => a.order - b.order),
    [columns],
  )

  const itemsByColumn = useMemo(() => {
    const map = new Map<string, PlannerItem[]>()
    for (const column of sortedColumns) {
      map.set(column.id, [])
    }
    for (const item of items) {
      const list = map.get(item.statusId) ?? []
      list.push(item)
      map.set(item.statusId, list)
    }
    return map
  }, [items, sortedColumns])

  useEffect(() => {
    return monitorForElements({
      onDrop: ({ source, location }) => {
        if (source.data.type !== 'planner-card') return
        const target = location.current.dropTargets[0]
        if (!target || target.data.type !== 'planner-column') return

        const itemId = String(source.data.itemId)
        const fromStatusId = String(source.data.statusId)
        const toStatusId = String(target.data.statusId)
        if (!itemId || !toStatusId || fromStatusId === toStatusId) return
        onMoveItem?.(itemId, toStatusId)
      },
    })
  }, [onMoveItem])

  return (
    <div className={['kit-planner-board', className].filter(Boolean).join(' ')}>
      {sortedColumns.map((column, index) => (
        <KanbanColumn
          key={column.id}
          column={column}
          items={itemsByColumn.get(column.id) ?? []}
          locale={locale}
          isTerminal={index === sortedColumns.length - 1}
          onOpenItem={onOpenItem}
          onAddItem={onAddItem}
        />
      ))}
    </div>
  )
}
