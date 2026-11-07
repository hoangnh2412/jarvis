import type { ReactNode } from 'react'
import type { DynamicFormDefinition, DynamicFormField } from '../types'
import { getDynamicFormDefinitionMock } from '../mocks'
import {
  validateFieldDraft,
  type DynamicFormValidationIssue,
} from './fields'
import { syncDefinitionFromFields } from './schemaBridge'

export {
  createFieldId,
  createOptionId,
  FIELD_TYPE_META,
  FIELD_LIBRARY_TYPES,
  FIELD_LIBRARY_GROUPS,
  createSelectOption,
  normalizeSelectOptions,
  createField,
  createTableColumn,
  TABLE_COLUMN_TYPES,
  isTableColumnType,
  flattenFields,
  collectFieldKeys,
  findFieldById,
  findFieldByKey,
  getParentKeyOfField,
  updateFieldInTree,
  removeFieldFromTree,
  insertFieldInTree,
  moveFieldInTree,
  moveFieldById,
  validateFieldDraft,
  firstValidationMessage,
  canRemoveField,
} from './fields'
export type {
  DynamicFormValidationIssue,
  FieldLibraryGroup,
  FieldLibraryGroupId,
  FieldDropEdge,
} from './fields'

export {
  fieldsToRjsf,
  rjsfToFields,
  syncDefinitionFromFields,
} from './schemaBridge'

export {
  getFormFieldPropertiesLabels,
  getBuilderGridClass,
} from './builderUi'

export function createFormId() {
  return `frm_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 8)}`
}

export function createSubmissionId() {
  return `sub_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 8)}`
}

export function countSchemaProperties(
  schema: DynamicFormDefinition['schema'] | undefined,
): number {
  if (!schema?.properties || typeof schema.properties !== 'object') return 0
  return Object.keys(schema.properties).length
}

export function createEmptySchema(title = 'Form mới') {
  return {
    title,
    type: 'object' as const,
    properties: {},
  }
}

/** Seed demo — đọc từ `mocks/get-form-definition.json`. */
export function createDemoDefinitionSeed(
  _now = new Date().toISOString(),
): DynamicFormDefinition {
  const raw = structuredClone(
    getDynamicFormDefinitionMock,
  ) as DynamicFormDefinition
  const fields =
    Array.isArray(raw.fields) && raw.fields.length > 0 ? raw.fields : []
  return syncDefinitionFromFields({ ...raw, fields })
}

export function validateFormDefinition(
  form: DynamicFormDefinition,
): DynamicFormValidationIssue[] {
  const issues: DynamicFormValidationIssue[] = []
  if (!form.name?.trim()) {
    issues.push({ code: 'name.required', message: 'Tên form không được để trống' })
  }
  const walk = (list: DynamicFormField[]) => {
    for (const field of list) {
      issues.push(...validateFieldDraft(field, form.fields))
      if (field.type === 'group' && field.children) walk(field.children)
    }
  }
  walk(form.fields)
  return issues
}

export function prettyJson(value: unknown): string {
  return JSON.stringify(value ?? {}, null, 2)
}

export type DynamicFormSlotContent<TContext> =
  | ReactNode
  | ((ctx: TContext) => ReactNode)

export function resolveDynamicFormContent<TContext>(
  content: DynamicFormSlotContent<TContext> | undefined,
  ctx: TContext,
  fallback: ReactNode,
): ReactNode {
  if (content == null) return fallback
  if (typeof content === 'function') {
    return (content as (ctx: TContext) => ReactNode)(ctx)
  }
  return content
}
