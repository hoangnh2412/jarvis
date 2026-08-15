import { forwardRef } from 'react'
import { GripVertical } from 'lucide-react'
import type { DragHandleProps } from 'react-querybuilder'

/** Drag handle for reordering rules/groups (wired by `@react-querybuilder/dnd`). */
export const PrimeDragHandle = forwardRef<HTMLSpanElement, DragHandleProps>(
  function PrimeDragHandle(
    {
      className,
      title,
      disabled,
      testID,
      label,
      dragHandleAttributes,
    },
    ref,
  ) {
    return (
      <span
        ref={ref}
        data-testid={testID}
        title={title ?? 'Kéo để sắp xếp'}
        aria-label={typeof label === 'string' ? label : 'Kéo để sắp xếp'}
        aria-disabled={disabled || undefined}
        className={[
          'fe-rqb-drag-handle inline-flex h-9 w-7 shrink-0 cursor-grab items-center justify-center rounded-lg text-slate-400 transition hover:bg-slate-100 hover:text-slate-600 active:cursor-grabbing',
          disabled ? 'pointer-events-none opacity-40' : '',
          className,
        ]
          .filter(Boolean)
          .join(' ')}
        {...dragHandleAttributes}
      >
        <GripVertical className="size-4" aria-hidden />
      </span>
    )
  },
)
