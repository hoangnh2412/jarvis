import type { Field } from 'react-querybuilder'
import fieldsMock from '../mocks/get-fields.json'
import {
  BOOLEAN_OPERATORS,
  DATE_OPERATORS,
  NUMBER_OPERATORS,
  STRING_OPERATORS,
} from '../utils/operators'

type FieldSeed = Omit<Field, 'operators'> & {
  operators?: Array<{ name: string; label: string }>
}

const FIELD_SEEDS = fieldsMock.fields as FieldSeed[]

function operatorsFor(field: FieldSeed): Field['operators'] {
  if (field.datatype === 'number') return NUMBER_OPERATORS
  if (field.datatype === 'boolean') return BOOLEAN_OPERATORS
  if (field.datatype === 'date' || field.datatype === 'datetime') return DATE_OPERATORS
  if (field.valueEditorType === 'select') {
    return [
      { name: '=', label: '=' },
      { name: '!=', label: '!=' },
      { name: 'in', label: 'in' },
      { name: 'null', label: 'is null' },
      { name: 'notNull', label: 'is not null' },
    ]
  }
  return STRING_OPERATORS
}

/**
 * Demo field catalog — nguồn: `mocks/get-fields.json`.
 * Operators gắn tại runtime (RQB names → FilterParser khi serialize).
 */
export const FAKE_QUERY_BUILDER_FIELDS: Field[] = FIELD_SEEDS.map((f) => ({
  ...(f as Field),
  operators: operatorsFor(f),
}))

export function getFakeQueryBuilderFields(): Field[] {
  return FAKE_QUERY_BUILDER_FIELDS.map((f) => ({
    ...f,
    operators: f.operators
      ? [...(f.operators as Array<{ name: string; label: string }>)].map(
          (o) => ({ ...o }),
        )
      : undefined,
    values: f.values
      ? [...(f.values as Array<{ name: string; label: string }>)].map((v) => ({
          ...v,
        }))
      : undefined,
  }))
}
