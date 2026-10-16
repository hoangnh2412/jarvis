import { Plus, Trash2 } from 'lucide-react'
import type { ChangeEvent } from 'react'
import type { FieldError } from 'react-hook-form'
import { Button, Card, InputText, ToggleSwitch } from '@platform/core'
import type { ColumnMapping, ImportExportFieldSchema } from '../types'
import {
  btnPrimaryCompactClass,
  btnTextClass,
  fieldInputCompactClass,
  fieldInputCompactInvalidClass,
} from './fieldStyles'

export type MappingRowErrors = Array<
  | {
      columnName?: FieldError
      fieldName?: FieldError
    }
  | undefined
>

export type MappingTableProps = {
  value: ColumnMapping[]
  onChange: (next: ColumnMapping[]) => void
  disabled?: boolean
  fieldSchemas?: Record<string, ImportExportFieldSchema>
  /** Per-row errors from react-hook-form (`errors.mappings`) after submit */
  rowErrors?: MappingRowErrors
}

/** RHF + zod có thể trả array hoặc object keyed by index */
export function normalizeMappingRowErrors(
  errors: MappingRowErrors | Record<number, MappingRowErrors[number]> | undefined,
): MappingRowErrors | undefined {
  if (!errors) return undefined
  if (Array.isArray(errors)) return errors

  const rows: MappingRowErrors = []
  for (const [key, row] of Object.entries(errors)) {
    if (!/^\d+$/.test(key) || !row || typeof row !== 'object') continue
    rows[Number(key)] = row as MappingRowErrors[number]
  }
  return rows.length > 0 ? rows : undefined
}

function columnNameError(
  rowErrors: MappingRowErrors | undefined,
  index: number,
): string | undefined {
  if (!Array.isArray(rowErrors)) return undefined
  const message = rowErrors[index]?.columnName?.message
  return typeof message === 'string' && message.trim() !== '' ? message : undefined
}

const emptyRow = (): ColumnMapping => ({
  columnName: '',
  fieldName: '',
  required: false,
  unique: false,
  regex: null,
  minLength: null,
  maxLength: null,
  min: null,
  max: null,
})

function parseOptionalNumber(raw: string): number | null {
  const trimmed = raw.trim()
  if (trimmed === '') return null
  const n = Number(trimmed)
  return Number.isFinite(n) ? n : null
}

/** Chưa nạp schema → không suy đoán được kiểu, mở cả rule text và numeric. */
function isTextField(schema: ImportExportFieldSchema | undefined): boolean {
  if (!schema) return true
  return schema.dataType === 'string' || schema.dataType === 'enum'
}

function isNumericField(schema: ImportExportFieldSchema | undefined): boolean {
  if (!schema) return true
  return schema.isNumeric
}

function MappingToggle({
  checked,
  disabled,
  ariaLabel,
  onCheckedChange,
}: {
  checked: boolean
  disabled?: boolean
  ariaLabel: string
  onCheckedChange: (checked: boolean) => void
}) {
  return (
    <ToggleSwitch.Root
      checked={checked}
      disabled={disabled}
      onCheckedChange={(e: { checked: boolean }) => onCheckedChange(e.checked)}
      className="relative inline-flex h-5 w-9 shrink-0"
      inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0 disabled:cursor-not-allowed"
      ariaLabel={ariaLabel}
    >
      <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-5 w-9 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
        <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-4 w-4 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-4" />
      </ToggleSwitch.Control>
    </ToggleSwitch.Root>
  )
}

export function MappingTable({
  value,
  onChange,
  disabled = false,
  fieldSchemas,
  rowErrors,
}: MappingTableProps) {
  const updateRow = (index: number, patch: Partial<ColumnMapping>) => {
    onChange(value.map((row, i) => (i === index ? { ...row, ...patch } : row)))
  }

  return (
    <Card.Root className="overflow-hidden rounded-xl border border-line bg-white shadow-sm">
      <Card.Header className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-4 py-3">
        <div className="min-w-0">
          <Card.Title className="m-0 text-sm font-semibold text-ink">
            Column mappings
          </Card.Title>
          <Card.Subtitle className="mt-0.5 m-0 text-xs text-mute">
            Map header Excel → field name output
          </Card.Subtitle>
        </div>
        <Button
          type="button"
          unstyled
          disabled={disabled}
          className={btnPrimaryCompactClass}
          onClick={() => onChange([...value, emptyRow()])}
        >
          <Plus className="size-3.5" />
          Thêm cột
        </Button>
      </Card.Header>

      <Card.Body className="overflow-x-auto p-0">
        <table className="w-full min-w-[1200px] border-collapse text-left text-sm">
          <thead>
            <tr className="border-b border-line bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-mute">
              <th className="px-3 py-2">Column</th>
              <th className="px-3 py-2">Field</th>
              <th className="px-3 py-2 text-center">Req</th>
              <th className="px-3 py-2 text-center">Unique</th>
              <th className="px-3 py-2">Regex</th>
              <th className="px-3 py-2">MinLen</th>
              <th className="px-3 py-2">MaxLen</th>
              <th className="px-3 py-2">Min</th>
              <th className="px-3 py-2">Max</th>
              <th className="w-12 px-3 py-2" />
            </tr>
          </thead>
          <tbody>
            {value.map((row, index) => {
              const columnError = columnNameError(rowErrors, index)
              const schema = fieldSchemas?.[row.fieldName]
              const showTextLength = isTextField(schema)
              const showNumeric = isNumericField(schema)

              return (
                <tr
                  key={index}
                  className="border-b border-line/80 last:border-b-0"
                >
                  <td className="px-3 py-2 align-top">
                    <InputText
                      unstyled
                      disabled={disabled}
                      value={row.columnName}
                      required
                      aria-invalid={columnError ? true : undefined}
                      aria-describedby={
                        columnError ? `mapping-column-error-${index}` : undefined
                      }
                      onChange={(e: ChangeEvent<HTMLInputElement>) =>
                        updateRow(index, { columnName: e.target.value })
                      }
                      className={[
                        columnError
                          ? fieldInputCompactInvalidClass
                          : fieldInputCompactClass,
                        'min-w-[100px]',
                      ].join(' ')}
                    />
                    {columnError ? (
                      <p
                        id={`mapping-column-error-${index}`}
                        className="mt-1 mb-0 text-xs text-red-600"
                      >
                        {columnError}
                      </p>
                    ) : null}
                  </td>
                  <td className="px-3 py-2">
                    <InputText
                      unstyled
                      disabled={disabled}
                      value={row.fieldName}
                      onChange={(e: ChangeEvent<HTMLInputElement>) =>
                        updateRow(index, { fieldName: e.target.value })
                      }
                      className={`${fieldInputCompactClass} min-w-[100px]`}
                    />
                  </td>
                  <td className="px-3 py-2 text-center">
                    <div className="flex justify-center">
                      <MappingToggle
                        checked={Boolean(row.required)}
                        disabled={disabled}
                        ariaLabel="Required"
                        onCheckedChange={(checked) =>
                          updateRow(index, { required: checked })
                        }
                      />
                    </div>
                  </td>
                  <td className="px-3 py-2 text-center">
                    <div className="flex justify-center">
                      <MappingToggle
                        checked={Boolean(row.unique)}
                        disabled={disabled}
                        ariaLabel="Unique"
                        onCheckedChange={(checked) =>
                          updateRow(index, { unique: checked })
                        }
                      />
                    </div>
                  </td>
                  <td className="px-3 py-2">
                    <InputText
                      unstyled
                      disabled={disabled}
                      value={row.regex ?? ''}
                      onChange={(e: ChangeEvent<HTMLInputElement>) => {
                        const raw = e.target.value
                        updateRow(index, {
                          regex: raw.trim() === '' ? null : raw,
                        })
                      }}
                      className={`${fieldInputCompactClass} min-w-[140px] font-mono text-xs`}
                      placeholder="^[A-Z0-9]+$"
                    />
                  </td>
                  <td className="px-3 py-2">
                    <InputText
                      unstyled
                      disabled={disabled || !showTextLength}
                      value={row.minLength == null ? '' : String(row.minLength)}
                      onChange={(e: ChangeEvent<HTMLInputElement>) =>
                        updateRow(index, {
                          minLength: parseOptionalNumber(e.target.value),
                        })
                      }
                      className={`${fieldInputCompactClass} w-20`}
                      placeholder="—"
                    />
                  </td>
                  <td className="px-3 py-2">
                    <InputText
                      unstyled
                      disabled={disabled || !showTextLength}
                      value={row.maxLength == null ? '' : String(row.maxLength)}
                      onChange={(e: ChangeEvent<HTMLInputElement>) =>
                        updateRow(index, {
                          maxLength: parseOptionalNumber(e.target.value),
                        })
                      }
                      className={`${fieldInputCompactClass} w-20`}
                      placeholder="—"
                    />
                  </td>
                  <td className="px-3 py-2">
                    <InputText
                      unstyled
                      disabled={disabled || !showNumeric}
                      value={row.min == null ? '' : String(row.min)}
                      onChange={(e: ChangeEvent<HTMLInputElement>) =>
                        updateRow(index, {
                          min: parseOptionalNumber(e.target.value),
                        })
                      }
                      className={`${fieldInputCompactClass} w-20`}
                      placeholder="—"
                    />
                  </td>
                  <td className="px-3 py-2">
                    <InputText
                      unstyled
                      disabled={disabled || !showNumeric}
                      value={row.max == null ? '' : String(row.max)}
                      onChange={(e: ChangeEvent<HTMLInputElement>) =>
                        updateRow(index, {
                          max: parseOptionalNumber(e.target.value),
                        })
                      }
                      className={`${fieldInputCompactClass} w-20`}
                      placeholder="—"
                    />
                  </td>
                  <td className="px-3 py-2 text-right">
                    <Button
                      type="button"
                      unstyled
                      disabled={disabled || value.length <= 1}
                      className={`${btnTextClass} !text-slate-400 hover:!bg-red-50 hover:!text-red-600`}
                      aria-label="Xóa mapping"
                      onClick={() =>
                        onChange(value.filter((_, i) => i !== index))
                      }
                    >
                      <Trash2 className="size-4" />
                    </Button>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </Card.Body>
    </Card.Root>
  )
}
