import type {
  FieldProps,
  FormContextType,
  RJSFSchema,
  RegistryFieldsType,
  StrictRJSFSchema,
  UiSchema,
} from '@rjsf/utils'
import type { MouseEvent } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { Button } from 'primereact/button'
import { DataTable } from 'primereact/datatable'
import { InputText } from 'primereact/inputtext'
import { btnOutlinedClass, fieldInputClass } from '../components/fieldStyles'

type ColumnDef = {
  key: string
  title: string
  type: 'string' | 'number' | 'boolean'
  format?: string
  enumValues?: string[]
  enumNames?: string[]
  placeholder?: string
}

type RowRecord = Record<string, unknown> & { __rid: string }

function createRowId() {
  return `row_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 8)}`
}

function stripRowMeta(row: RowRecord): Record<string, unknown> {
  const { __rid: _rid, ...rest } = row
  return rest
}

function inferColumns(
  schema: RJSFSchema,
  uiSchema: UiSchema | undefined,
): ColumnDef[] {
  const items = schema.items
  if (!items || typeof items !== 'object' || Array.isArray(items)) return []
  const itemSchema = items as RJSFSchema
  const properties = (itemSchema.properties ?? {}) as Record<string, RJSFSchema>
  const itemsUi = (uiSchema?.items as UiSchema | undefined) ?? {}
  const order = Array.isArray(itemsUi['ui:order'])
    ? (itemsUi['ui:order'] as string[]).filter((k) => k !== '*')
    : Object.keys(properties)
  const keys = [
    ...order.filter((k) => k in properties),
    ...Object.keys(properties).filter((k) => !order.includes(k)),
  ]

  return keys.map((key) => {
    const prop = properties[key] ?? {}
    const colUi = (itemsUi[key] as UiSchema | undefined) ?? {}
    const enumValues = Array.isArray(prop.enum)
      ? prop.enum.map(String)
      : undefined
    const enumNames = Array.isArray(colUi['ui:enumNames'])
      ? (colUi['ui:enumNames'] as string[])
      : enumValues
    return {
      key,
      title: typeof prop.title === 'string' ? prop.title : key,
      type:
        prop.type === 'boolean'
          ? 'boolean'
          : prop.type === 'number' || prop.type === 'integer'
            ? 'number'
            : 'string',
      format: typeof prop.format === 'string' ? prop.format : undefined,
      enumValues,
      enumNames,
      placeholder:
        typeof colUi['ui:placeholder'] === 'string'
          ? colUi['ui:placeholder']
          : undefined,
    }
  })
}

function emptyRow(columns: ColumnDef[]): RowRecord {
  const row: RowRecord = { __rid: createRowId() }
  for (const col of columns) {
    if (col.type === 'boolean') row[col.key] = false
    else if (col.type === 'number') row[col.key] = undefined
    else row[col.key] = ''
  }
  return row
}

function toRows(value: unknown, columns: ColumnDef[]): RowRecord[] {
  if (!Array.isArray(value)) return []
  return value.map((item, index) => {
    const base =
      item && typeof item === 'object' && !Array.isArray(item)
        ? (item as Record<string, unknown>)
        : {}
    const row: RowRecord = {
      ...emptyRow(columns),
      ...base,
      __rid:
        typeof base.__rid === 'string' ? base.__rid : `legacy_${index}_${createRowId()}`,
    }
    return row
  })
}

function CellEditor({
  column,
  value,
  disabled,
  onChange,
}: {
  column: ColumnDef
  value: unknown
  disabled: boolean
  onChange: (next: unknown) => void
}) {
  if (column.type === 'boolean') {
    return (
      <input
        type="checkbox"
        className="h-4 w-4 rounded border-slate-300 text-teal-700 focus:ring-teal-600/30"
        checked={Boolean(value)}
        disabled={disabled}
        onClick={(e) => e.stopPropagation()}
        onChange={(e) => {
          e.stopPropagation()
          onChange(e.target.checked)
        }}
      />
    )
  }

  if (column.enumValues?.length) {
    return (
      <select
        className={fieldInputClass}
        value={value == null ? '' : String(value)}
        disabled={disabled}
        onClick={(e) => e.stopPropagation()}
        onChange={(e) => {
          const next = e.target.value
          onChange(next === '' ? undefined : next)
        }}
      >
        <option value="">—</option>
        {column.enumValues.map((opt, index) => (
          <option key={opt} value={opt}>
            {column.enumNames?.[index] ?? opt}
          </option>
        ))}
      </select>
    )
  }

  const inputType =
    column.type === 'number'
      ? 'number'
      : column.format === 'date'
        ? 'date'
        : column.format === 'email'
          ? 'email'
          : 'text'

  return (
    <InputText
      unstyled
      type={inputType}
      className={fieldInputClass}
      value={value == null ? '' : String(value)}
      placeholder={column.placeholder}
      disabled={disabled}
      onClick={(e: MouseEvent) => e.stopPropagation()}
      onChange={(e: { target: { value: string } }) => {
        const raw = e.target.value
        if (column.type === 'number') {
          onChange(raw === '' ? undefined : Number(raw))
          return
        }
        onChange(raw)
      }}
    />
  )
}

function DynamicTableField<
  T = unknown,
  S extends StrictRJSFSchema = RJSFSchema,
  F extends FormContextType = any,
>(props: FieldProps<T, S, F>) {
  const {
    schema,
    uiSchema,
    formData,
    onChange,
    disabled,
    readonly,
    fieldPathId,
  } = props

  const options = (uiSchema?.['ui:options'] ?? {}) as {
    addable?: boolean
    removable?: boolean
  }
  // Default off: table rows are loaded from backend formData.
  const canAdd = options.addable === true && !disabled && !readonly
  const canRemove = options.removable === true && !disabled && !readonly

  const columns = inferColumns(
    schema as RJSFSchema,
    uiSchema as UiSchema | undefined,
  )
  const rows = toRows(formData, columns)
  const changePath = fieldPathId?.path ?? []

  const commit = (nextRows: RowRecord[]) => {
    onChange(nextRows.map(stripRowMeta) as T, changePath)
  }

  const updateCell = (rowId: string, key: string, value: unknown) => {
    commit(
      rows.map((row) =>
        row.__rid === rowId ? { ...row, [key]: value } : row,
      ),
    )
  }

  const addRow = () => {
    commit([...rows, emptyRow(columns)])
  }

  const removeRow = (rowId: string) => {
    commit(rows.filter((row) => row.__rid !== rowId))
  }

  if (columns.length === 0) {
    return (
      <div className="kit-df-rjsf-table kit-df-rjsf-table--empty">
        <p className="kit-df-rjsf-table__hint">
          No columns yet — configure columns in field settings.
        </p>
      </div>
    )
  }

  return (
    <div
      className="kit-df-rjsf-table"
      onClick={(e) => e.stopPropagation()}
    >
      <DataTable.Root
        data={rows}
        dataKey="__rid"
        className="kit-df-rjsf-table__root"
      >
        <DataTable.TableContainer className="overflow-x-auto">
          <DataTable.Table className="w-full min-w-[420px] border-collapse text-left text-sm">
            <DataTable.THead>
              <DataTable.THeadRow className="border-b border-line bg-slate-50/80">
                {columns.map((col) => (
                  <DataTable.THeadCell
                    key={col.key}
                    className="px-3 py-2.5 text-left align-middle"
                  >
                    <span className="block text-xs font-semibold uppercase tracking-wide text-mute">
                      {col.title}
                    </span>
                  </DataTable.THeadCell>
                ))}
                {canRemove ? (
                  <DataTable.THeadCell className="w-14 px-2 py-2.5 text-right align-middle">
                    <span className="sr-only">Actions</span>
                  </DataTable.THeadCell>
                ) : null}
              </DataTable.THeadRow>
            </DataTable.THead>

            <DataTable.TBody>
              {({ item }) => {
                const row = item as RowRecord
                return (
                  <DataTable.Row className="border-b border-line/80 transition-colors last:border-b-0 hover:bg-teal-50/30">
                    {columns.map((col) => (
                      <DataTable.Cell
                        key={col.key}
                        className="px-3 py-2 align-middle"
                      >
                        <CellEditor
                          column={col}
                          value={row[col.key]}
                          disabled={Boolean(disabled || readonly)}
                          onChange={(next) =>
                            updateCell(row.__rid, col.key, next)
                          }
                        />
                      </DataTable.Cell>
                    ))}
                    {canRemove ? (
                      <DataTable.Cell className="px-2 py-2 text-right align-middle">
                        <Button
                          type="button"
                          unstyled
                          className="inline-flex size-8 items-center justify-center rounded-lg text-slate-400 transition hover:bg-red-50 hover:text-red-600"
                          aria-label="Remove row"
                          disabled={disabled || readonly}
                          onClick={() => removeRow(row.__rid)}
                        >
                          <Trash2 className="size-3.5" aria-hidden />
                        </Button>
                      </DataTable.Cell>
                    ) : null}
                  </DataTable.Row>
                )
              }}
            </DataTable.TBody>

            <DataTable.EmptyTBody>
              <tr>
                <td
                  colSpan={columns.length + (canRemove ? 1 : 0)}
                  className="px-3 py-8 text-center"
                >
                  <p className="m-0 text-sm text-mute">No data</p>
                </td>
              </tr>
            </DataTable.EmptyTBody>
          </DataTable.Table>
        </DataTable.TableContainer>
      </DataTable.Root>

      {canAdd ? (
        <div className="kit-df-rjsf-table__toolbar">
          <Button
            type="button"
            unstyled
            className={`${btnOutlinedClass} gap-1.5`}
            onClick={addRow}
          >
            <Plus className="size-3.5" aria-hidden />
            Add row
          </Button>
        </div>
      ) : null}
    </div>
  )
}

export const dynamicFormFields = {
  dynamicTable: DynamicTableField,
} satisfies RegistryFieldsType
