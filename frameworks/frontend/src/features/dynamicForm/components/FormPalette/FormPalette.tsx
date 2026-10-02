import { useEffect, useRef } from 'react'
import { draggable } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import {
  AlignLeft,
  Calendar,
  CheckSquare,
  CircleDot,
  Hash,
  KeyRound,
  LayoutPanelTop,
  List,
  Mail,
  PanelLeftClose,
  PanelLeftOpen,
  Table2,
  Type,
  type LucideIcon,
} from 'lucide-react'
import type { DynamicFormFieldType } from '../../types'
import { FIELD_LIBRARY_TYPES, FIELD_TYPE_META } from '../../utils'

const FIELD_ICONS: Record<DynamicFormFieldType, LucideIcon> = {
  text: Type,
  number: Hash,
  email: Mail,
  password: KeyRound,
  select: List,
  radio: CircleDot,
  checkbox: CheckSquare,
  date: Calendar,
  textarea: AlignLeft,
  group: LayoutPanelTop,
  table: Table2,
}

export type FormPaletteProps = {
  title?: string
  collapsed?: boolean
  onCollapsedChange?: (collapsed: boolean) => void
  collapseLabel?: string
  expandLabel?: string
  onAdd: (type: DynamicFormFieldType) => void
}

function PaletteItem({
  type,
  collapsed,
  onAdd,
}: {
  type: DynamicFormFieldType
  collapsed: boolean
  onAdd: (type: DynamicFormFieldType) => void
}) {
  const ref = useRef<HTMLButtonElement>(null)
  const Icon = FIELD_ICONS[type]

  useEffect(() => {
    const el = ref.current
    if (!el) return
    return draggable({
      element: el,
      getInitialData: () => ({
        source: 'palette',
        fieldType: type,
      }),
    })
  }, [type])

  return (
    <button
      ref={ref}
      type="button"
      title={FIELD_TYPE_META[type].label}
      className="kit-df-palette-item flex w-full cursor-grab items-center gap-2.5 rounded-[0.625rem] border border-slate-200 bg-white px-2.5 py-2 text-left text-slate-700 shadow-sm transition hover:border-teal-300 hover:bg-teal-50/60 hover:text-teal-700 active:cursor-grabbing"
      onClick={() => onAdd(type)}
    >
      <span
        className="kit-df-palette-item-icon inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-slate-500"
        aria-hidden
      >
        <Icon className="h-3.5 w-3.5" />
      </span>
      {!collapsed ? (
        <>
          <span className="kit-df-palette-item-label min-w-0 flex-1 truncate text-[13px] font-medium">
            {FIELD_TYPE_META[type].label}
          </span>
          <span className="kit-df-palette-item-hint text-[11px] text-slate-400">
            +
          </span>
        </>
      ) : null}
    </button>
  )
}

export function FormPalette({
  title = 'Palette',
  collapsed = false,
  onCollapsedChange,
  collapseLabel = 'Collapse',
  expandLabel = 'Expand',
  onAdd,
}: FormPaletteProps) {
  return (
    <aside
      className={['kit-df-palette', collapsed ? 'is-collapsed' : '']
        .filter(Boolean)
        .join(' ')}
    >
      <div className="kit-df-palette-header">
        <h3 className="kit-df-palette-title">{title}</h3>
        {onCollapsedChange ? (
          <button
            type="button"
            className="kit-df-palette-toggle"
            aria-label={collapsed ? expandLabel : collapseLabel}
            onClick={() => onCollapsedChange(!collapsed)}
          >
            {collapsed ? (
              <PanelLeftOpen className="h-4 w-4" />
            ) : (
              <PanelLeftClose className="h-4 w-4" />
            )}
          </button>
        ) : null}
      </div>
      <div className="kit-df-palette-list flex flex-col gap-1.5">
        {FIELD_LIBRARY_TYPES.map((type) => (
          <PaletteItem
            key={type}
            type={type}
            collapsed={collapsed}
            onAdd={onAdd}
          />
        ))}
      </div>
    </aside>
  )
}
