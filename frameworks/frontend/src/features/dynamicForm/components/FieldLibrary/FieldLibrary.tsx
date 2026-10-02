import { useEffect, useMemo, useRef, useState } from 'react'
import { draggable } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import {
  AlignLeft,
  Calendar,
  CheckSquare,
  ChevronDown,
  CircleDot,
  Hash,
  KeyRound,
  LayoutPanelTop,
  List,
  Mail,
  Search,
  Table2,
  Type,
  type LucideIcon,
} from 'lucide-react'
import { InputText } from 'primereact/inputtext'
import type { DynamicFormFieldType } from '../../types'
import {
  FIELD_LIBRARY_GROUPS,
  FIELD_TYPE_META,
  type FieldLibraryGroupId,
} from '../../utils'
import { fieldInputClass } from '../fieldStyles'

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

export type FieldLibraryProps = {
  onAdd: (type: DynamicFormFieldType) => void
}

function ToolboxItem({
  type,
  onAdd,
}: {
  type: DynamicFormFieldType
  onAdd: (type: DynamicFormFieldType) => void
}) {
  const ref = useRef<HTMLButtonElement>(null)
  const Icon = FIELD_ICONS[type]
  const meta = FIELD_TYPE_META[type]

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
      title={`${meta.description} — drag to canvas or click to add`}
      className="kit-svc-toolbox__item"
      onClick={() => onAdd(type)}
    >
      <span className="kit-svc-toolbox__icon" aria-hidden>
        <Icon className="h-4 w-4" />
      </span>
      <span className="kit-svc-toolbox__label">{meta.label}</span>
    </button>
  )
}

export function FieldLibrary({ onAdd }: FieldLibraryProps) {
  const [query, setQuery] = useState('')
  const [openGroups, setOpenGroups] = useState<
    Record<FieldLibraryGroupId, boolean>
  >({
    basic: true,
    advanced: false,
    layout: true,
    data: true,
    premium: false,
  })

  const q = query.trim().toLowerCase()

  const groups = useMemo(() => {
    return FIELD_LIBRARY_GROUPS.map((group) => {
      const types = group.types.filter((type) => {
        if (!q) return true
        const meta = FIELD_TYPE_META[type]
        return (
          meta.label.toLowerCase().includes(q) ||
          meta.description.toLowerCase().includes(q) ||
          type.includes(q) ||
          group.label.toLowerCase().includes(q)
        )
      })
      return { ...group, types }
    }).filter((group) => (q ? group.types.length > 0 : true))
  }, [q])

  const toggle = (id: FieldLibraryGroupId) => {
    setOpenGroups((prev) => ({ ...prev, [id]: !prev[id] }))
  }

  return (
    <aside className="kit-svc-toolbox" aria-label="Toolbox">
      <div className="kit-svc-toolbox__search">
        <Search className="kit-svc-toolbox__search-icon" aria-hidden />
        <InputText
          unstyled
          className={`${fieldInputClass} kit-svc-toolbox__search-input`}
          value={query}
          placeholder="Type to search..."
          onChange={(e: { target: { value: string } }) =>
            setQuery(e.target.value)
          }
        />
      </div>

      <div className="kit-svc-toolbox__groups">
        {groups.map((group) => {
          const open = Boolean(q) || openGroups[group.id]
          return (
            <div
              key={group.id}
              className={[
                'kit-svc-toolbox__group',
                open ? 'is-open' : '',
              ]
                .filter(Boolean)
                .join(' ')}
            >
              <button
                type="button"
                className="kit-svc-toolbox__group-head"
                aria-expanded={open}
                onClick={() => toggle(group.id)}
              >
                <span>{group.label}</span>
                <ChevronDown
                  className={[
                    'kit-svc-toolbox__chevron',
                    open ? 'is-open' : '',
                  ]
                    .filter(Boolean)
                    .join(' ')}
                />
              </button>
              {open ? (
                <div className="kit-svc-toolbox__group-body">
                  {group.types.length > 0 ? (
                    <ul className="kit-svc-toolbox__list">
                      {group.types.map((type) => (
                        <li key={type}>
                          <ToolboxItem type={type} onAdd={onAdd} />
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <p className="kit-svc-toolbox__empty">No components yet</p>
                  )}
                </div>
              ) : null}
            </div>
          )
        })}
        {groups.length === 0 ? (
          <p className="kit-svc-toolbox__empty">No matching fields</p>
        ) : null}
      </div>
    </aside>
  )
}
