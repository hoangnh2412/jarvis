import { useEffect, useRef, useState, type CSSProperties } from 'react'
import { dropTargetForElements } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { Plus } from 'lucide-react'
import type { PlannerColumn, PlannerItem } from '../../types'
import { getPlannerMessages, type PlannerLocale } from '../../localization'
import { KanbanCard } from '../KanbanCard'

export type KanbanColumnProps = {
  column: PlannerColumn
  items: PlannerItem[]
  locale?: PlannerLocale
  /** Cột cuối (thường là Done) không hiển thị nhãn quá hạn. */
  isTerminal?: boolean
  onOpenItem?: (item: PlannerItem) => void
  onAddItem?: (statusId: string) => void
}

export function KanbanColumn({
  column,
  items,
  locale = 'vi',
  isTerminal = false,
  onOpenItem,
  onAddItem,
}: KanbanColumnProps) {
  const ref = useRef<HTMLDivElement>(null)
  const [isOver, setIsOver] = useState(false)
  const messages = getPlannerMessages(locale)

  useEffect(() => {
    const element = ref.current
    if (!element) return

    return dropTargetForElements({
      element,
      getData: () => ({
        type: 'planner-column',
        statusId: column.id,
      }),
      canDrop: ({ source }) => source.data.type === 'planner-card',
      onDragEnter: () => setIsOver(true),
      onDragLeave: () => setIsOver(false),
      onDrop: () => setIsOver(false),
    })
  }, [column.id])

  return (
    <section
      ref={ref}
      style={
        {
          '--kit-column-accent': column.color ?? '#94a3b8',
        } as CSSProperties
      }
      className={['kit-planner-column', isOver ? 'is-over' : '']
        .filter(Boolean)
        .join(' ')}
    >
      <header className="kit-planner-column__header">
        <div className="kit-planner-column__heading">
          <span className="kit-planner-column__dot" aria-hidden />
          <h3 className="kit-planner-column__title">{column.name}</h3>
        </div>
        <span
          className="kit-planner-column__count"
          aria-label={messages.page.itemsCount(items.length)}
        >
          {items.length}
        </span>
      </header>

      <div className="kit-planner-column__body">
        {items.length === 0 ? (
          <div className="kit-planner-empty">
            <p className="kit-planner-empty__title">{messages.page.emptyColumn}</p>
            <p className="kit-planner-empty__hint">
              {messages.page.emptyColumnHint}
            </p>
          </div>
        ) : (
          items.map((item) => (
            <KanbanCard
              key={item.id}
              item={item}
              locale={locale}
              showOverdue={!isTerminal}
              onOpen={onOpenItem}
            />
          ))
        )}
      </div>

      {onAddItem ? (
        <div className="kit-planner-column__footer">
          <button
            type="button"
            className="kit-planner-column__add"
            onClick={() => onAddItem(column.id)}
          >
            <Plus className="h-3.5 w-3.5" />
            {messages.page.addItem}
          </button>
        </div>
      ) : null}
    </section>
  )
}
