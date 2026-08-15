import type { Field } from 'react-querybuilder'
import { getFakeQueryBuilderFields } from './fakeFields'
import type { QueryBuilderFieldsResult } from '../types'
import {
  BOOLEAN_OPERATORS,
  DATE_OPERATORS,
  NUMBER_OPERATORS,
  STRING_OPERATORS,
} from '../utils/operators'

/**
 * Filterable fields for API 1 (`GET .../employees`).
 * HireDate / CreatedAt appear in UI filter JSON; stripped from API wire AST until BE supports them.
 */
export const EMPLOYEE_ALLOWED_FIELDS: Field[] = [
  { name: 'Id', label: 'Id', operators: STRING_OPERATORS },
  { name: 'FullName', label: 'Full Name', operators: STRING_OPERATORS },
  {
    name: 'IsActive',
    label: 'Is Active',
    valueEditorType: 'checkbox',
    datatype: 'boolean',
    defaultValue: true,
    operators: BOOLEAN_OPERATORS,
  },
  {
    name: 'BaseSalary',
    label: 'Base Salary',
    inputType: 'number',
    datatype: 'number',
    operators: NUMBER_OPERATORS,
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
    operators: STRING_OPERATORS,
  },
  {
    name: 'Position',
    label: 'Position',
    operators: STRING_OPERATORS,
  },
]

/**
 * Filterable fields for API 3 (`GET .../employees/list-custom`).
 */
export const EMPLOYEE_CUSTOM_ALLOWED_FIELDS: Field[] = [
  { name: 'Id', label: 'Id', operators: STRING_OPERATORS },
  { name: 'FullName', label: 'Full Name', operators: STRING_OPERATORS },
  {
    name: 'IsActive',
    label: 'Is Active',
    valueEditorType: 'checkbox',
    datatype: 'boolean',
    defaultValue: true,
    operators: BOOLEAN_OPERATORS,
  },
  {
    name: 'BaseSalary',
    label: 'Base Salary',
    inputType: 'number',
    datatype: 'number',
    operators: NUMBER_OPERATORS,
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
    operators: STRING_OPERATORS,
  },
  {
    name: 'Position',
    label: 'Position',
    operators: STRING_OPERATORS,
  },
  {
    name: 'Position.Level',
    label: 'Position Level',
    inputType: 'number',
    datatype: 'number',
    operators: NUMBER_OPERATORS,
  },
]
export type QueryBuilderFieldsMode = 'employees' | 'employees-custom' | 'demo'

export function getQueryBuilderFields(
  mode: QueryBuilderFieldsMode = 'employees',
): QueryBuilderFieldsResult {
  if (mode === 'employees-custom') {
    return { fields: EMPLOYEE_CUSTOM_ALLOWED_FIELDS.map((f) => ({ ...f })) }
  }
  if (mode === 'demo') {
    return { fields: getFakeQueryBuilderFields() }
  }
  return { fields: EMPLOYEE_ALLOWED_FIELDS.map((f) => ({ ...f })) }
}

/** @deprecated Prefer getQueryBuilderFields — kept for existing imports */
export async function callGetQueryBuilderFields(
  mode: QueryBuilderFieldsMode = 'employees',
): Promise<QueryBuilderFieldsResult> {
  return getQueryBuilderFields(mode)
}

export function resolveCompanyApiMode(
  companyApi: boolean | 'employees' | 'employees-custom' | undefined,
): 'employees' | 'employees-custom' | null {
  if (companyApi === false || companyApi == null) return null
  if (companyApi === 'employees-custom') return 'employees-custom'
  return 'employees'
}

export function normalizeQueryBuilderFields(
  data: unknown,
): QueryBuilderFieldsResult {
  if (data && typeof data === 'object' && 'fields' in data) {
    const fields = (data as QueryBuilderFieldsResult).fields
    if (Array.isArray(fields)) return { fields }
  }

  if (Array.isArray(data)) {
    if (data.length === 0) return { fields: [] }

    if (data.every((x) => typeof x === 'string')) {
      return {
        fields: (data as string[]).map((name) => ({
          name,
          label: name,
          operators: [...STRING_OPERATORS],
        })),
      }
    }

    if (
      data.every(
        (x) =>
          x != null &&
          typeof x === 'object' &&
          'name' in x &&
          typeof (x as Field).name === 'string',
      )
    ) {
      return { fields: data as Field[] }
    }
  }

  return { fields: getFakeQueryBuilderFields() }
}
