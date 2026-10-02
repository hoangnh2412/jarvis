import { Plus } from 'lucide-react'
import { Button } from 'primereact/button'
import { FIELD_TYPE_OPTIONS } from '../../types'
import type { DocumentField, FieldTypeValue } from '../../types'
import { fieldTypeIcon } from '../../utils'
import { CraftDocSearchInput } from '../CraftDocSearchInput'
import { CraftDocSelect } from '../CraftDocSelect'
import { btnCompactOutlinedClass } from '../fieldStyles'

export type FieldListProps = {
  fields: DocumentField[]
  selectedFieldId: string | null
  search: string
  typeFilter: FieldTypeValue | 'all'
  onSearchChange: (value: string) => void
  onTypeFilterChange: (value: FieldTypeValue | 'all') => void
  onSelect: (fieldId: string) => void
  onAdd: () => void
}

const TYPE_FILTER_OPTIONS: { label: string; value: FieldTypeValue | 'all' }[] = [
  { label: 'All types', value: 'all' },
  ...FIELD_TYPE_OPTIONS,
]

export function FieldList({
  fields,
  selectedFieldId,
  search,
  typeFilter,
  onSearchChange,
  onTypeFilterChange,
  onSelect,
  onAdd,
}: FieldListProps) {
  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between gap-2">
        <h3 className="text-xs font-semibold uppercase tracking-[0.14em] text-slate-500">
          Fields
        </h3>
        <Button type="button" unstyled className={btnCompactOutlinedClass} onClick={onAdd}>
          <Plus className="h-3.5 w-3.5" />
          Add Field
        </Button>
      </div>

      <div className="space-y-2">
        <CraftDocSearchInput
          value={search}
          onChange={onSearchChange}
          placeholder="Search field..."
        />

        <CraftDocSelect
          value={typeFilter}
          options={TYPE_FILTER_OPTIONS}
          placeholder="All types"
          onChange={onTypeFilterChange}
        />
      </div>

      <div className="space-y-1.5">
        {fields.length === 0 ? (
          <div className="rounded-xl border border-dashed border-slate-200 bg-white px-3 py-4 text-center text-sm text-slate-500">
            No fields detected yet.
          </div>
        ) : null}

        {fields.map((field) => {
          const selected = field.id === selectedFieldId
          return (
            <button
              key={field.id}
              type="button"
              onClick={() => onSelect(field.id)}
              className={`craft-doc-field-chip flex w-full items-start gap-2 rounded-xl border px-3 py-2.5 text-left transition ${
                selected
                  ? 'is-selected border-teal-300 bg-teal-50'
                  : 'border-transparent bg-white hover:border-slate-200 hover:bg-slate-50'
              }`}
            >
              <span className="mt-0.5 text-base leading-none">{fieldTypeIcon(field.type)}</span>
              <span className="min-w-0">
                <span className="block truncate text-sm font-medium text-slate-800">
                  {field.label}
                </span>
                <span className="block truncate font-mono text-xs text-slate-500">
                  {field.placeholder}
                </span>
              </span>
            </button>
          )
        })}
      </div>
    </div>
  )
}
