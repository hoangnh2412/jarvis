import type { Field } from 'react-querybuilder'
import {
  BOOLEAN_OPERATORS,
  DATE_OPERATORS,
  NUMBER_OPERATORS,
  STRING_OPERATORS,
} from '../utils/operators'

/**
 * Demo field catalog — operators use RQB names; serialize maps to FilterParser.
 * Used when `VITE_API_URL_QUERY_BUILDER` is unset.
 */
export const FAKE_QUERY_BUILDER_FIELDS: Field[] = [
  {
    name: 'BaseSalary',
    label: 'Base Salary',
    inputType: 'number',
    datatype: 'number',
    operators: NUMBER_OPERATORS,
  },
  {
    name: 'IsActive',
    label: 'Is Active',
    valueEditorType: 'checkbox',
    datatype: 'boolean',
    defaultValue: true,
    operators: BOOLEAN_OPERATORS,
  },
  {
    name: 'FullName',
    label: 'Full Name',
    operators: STRING_OPERATORS,
  },
  {
    name: 'Email',
    label: 'Email',
    operators: STRING_OPERATORS,
  },
  {
    name: 'HireDate',
    label: 'Hire Date',
    inputType: 'date',
    datatype: 'date',
    operators: DATE_OPERATORS,
  },
  {
    name: 'CreatedAt',
    label: 'Created At',
    inputType: 'datetime',
    datatype: 'datetime',
    operators: DATE_OPERATORS,
  },
  {
    name: 'Department',
    label: 'Department',
    valueEditorType: 'select',
    values: [
      { name: 'Engineering', label: 'Engineering' },
      { name: 'Sales', label: 'Sales' },
      { name: 'HR', label: 'HR' },
      { name: 'Finance', label: 'Finance' },
    ],
    operators: [
      { name: '=', label: '=' },
      { name: '!=', label: '!=' },
      { name: 'in', label: 'in' },
      { name: 'null', label: 'is null' },
      { name: 'notNull', label: 'is not null' },
    ],
  },
  {
    name: 'Age',
    label: 'Age',
    inputType: 'number',
    datatype: 'number',
    operators: NUMBER_OPERATORS,
  },
]

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
