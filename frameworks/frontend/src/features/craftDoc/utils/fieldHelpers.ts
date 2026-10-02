import {
  FieldType,
  type CreateDocumentFieldPayload,
  type DocumentField,
  type FieldTypeValue,
} from '../types'
import {
  getTemplateFieldGroups,
  GLOBAL_FIELD_META,
  hasPredefinedFieldGroups,
  type FieldGroupDef,
} from '../constants/fieldMeta'
import { buildDynamicFieldGroups, inferFieldGroup } from './fieldGrouping'

let fieldCounter = 0

export function createFieldId(prefix = 'field'): string {
  fieldCounter += 1
  return `${prefix}-${Date.now()}-${fieldCounter}`
}

export function toPlaceholder(name: string): string {
  const trimmed = name.trim()
  if (!trimmed) return '{{field}}'
  if (trimmed.startsWith('{{') && trimmed.endsWith('}}')) return trimmed
  return `{{${trimmed}}}`
}

export function placeholderToName(placeholder: string): string {
  return placeholder.replace(/^\{\{|\}\}$/g, '').trim()
}

export function createFieldFromPlaceholder(name: string): DocumentField {
  const normalized = name.trim()
  return {
    id: createFieldId(normalized),
    name: normalized,
    label: humanizeFieldName(normalized),
    type: FieldType.Text,
    placeholder: toPlaceholder(normalized),
    required: false,
  }
}

export function humanizeFieldName(name: string): string {
  return name
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
    .replace(/^./, (char) => char.toUpperCase())
}

const DOCX_PLACEHOLDER_TOKEN = /^\{\{[^}]+\}\}$/

/** Token `{{field}}` trong DOCX — không dùng làm placeholder HTML input. */
export function isDocxPlaceholderToken(text: string): boolean {
  return DOCX_PLACEHOLDER_TOKEN.test(text.trim())
}

function inferFieldInputPlaceholderExample(field: DocumentField): string | undefined {
  switch (field.type) {
    case FieldType.Date:
    case FieldType.Checkbox:
    case FieldType.Select:
      return undefined
    case FieldType.Number:
      return 'VD: 0'
    case FieldType.Currency:
      return 'VD: 10.000.000'
    case FieldType.Textarea:
      return `Nhập ${field.label.charAt(0).toLowerCase()}${field.label.slice(1)}...`
    default:
      return `Nhập ${field.label.charAt(0).toLowerCase()}${field.label.slice(1)}`
  }
}

/** Gợi ý trong ô input form — không bao giờ trả về `{{placeholder}}`. */
export function getFieldInputPlaceholder(field: DocumentField): string | undefined {
  const explicit = field.placeholderText?.trim()
  if (explicit && !isDocxPlaceholderToken(explicit)) {
    return explicit
  }
  return inferFieldInputPlaceholderExample(field)
}

export function createFieldFromPayload(payload: CreateDocumentFieldPayload): DocumentField {
  const name = payload.name.trim()
  return {
    id: createFieldId(name),
    name,
    label: payload.label.trim() || humanizeFieldName(name),
    type: payload.type,
    placeholder: toPlaceholder(name),
    required: payload.required ?? false,
    defaultValue: payload.defaultValue,
    description: payload.description,
  }
}

export function mergeExtractedFields(
  extracted: DocumentField[],
  existing: DocumentField[],
): DocumentField[] {
  if (extracted.length === 0) return existing
  const existingByName = new Map(existing.map((field) => [field.name, field]))
  return extracted.map((field) => existingByName.get(field.name) ?? field)
}

export function buildFieldValuesFromFields(fields: DocumentField[]): DocumentField[] {
  return fields.map((field) => ({
    ...field,
    value: field.value ?? field.defaultValue ?? '',
  }))
}

/** @deprecated Dùng buildFieldValuesFromFields / toPlaceholderMap */
export function buildSampleDataFromFields(fields: DocumentField[]): Record<string, string> {
  const sample: Record<string, string> = {}
  for (const field of fields) {
    const value = field.value ?? field.defaultValue
    if (value) {
      sample[field.name] = value
    }
  }
  return sample
}

export function enrichDocumentField(
  field: DocumentField,
  templateId?: string,
): DocumentField {
  const meta = GLOBAL_FIELD_META[field.name]
  const usePredefinedGroups = templateId != null && hasPredefinedFieldGroups(templateId)

  let group = field.group
  if (!group) {
    if (usePredefinedGroups && meta?.group) {
      group = meta.group
    } else {
      group = inferFieldGroup(field.name)
    }
  }

  if (!meta) {
    return { ...field, group }
  }

  return {
    ...field,
    label: meta.label ?? field.label,
    type: meta.type ?? field.type,
    required: meta.required ?? field.required,
    group,
    description: meta.description ?? field.description,
    placeholderText: meta.placeholderText ?? field.placeholderText,
    options: meta.options ?? field.options,
  }
}

export function enrichDocumentFields(
  fields: DocumentField[],
  templateId?: string,
): DocumentField[] {
  return fields.map((field) => enrichDocumentField(field, templateId))
}

export type GroupedDocumentFields = FieldGroupDef & {
  fields: DocumentField[]
}

export function groupDocumentFields(
  fields: DocumentField[],
  templateId: string,
): GroupedDocumentFields[] {
  if (!hasPredefinedFieldGroups(templateId)) {
    return buildDynamicFieldGroups(fields)
  }

  const groupDefs = getTemplateFieldGroups(templateId)
  const buckets = new Map<string, DocumentField[]>(
    groupDefs.map((group) => [group.id, []]),
  )

  for (const field of fields) {
    const groupId = field.group ?? inferFieldGroup(field.name)
    if (!buckets.has(groupId)) {
      buckets.set(groupId, [])
    }
    buckets.get(groupId)?.push(field)
  }

  return groupDefs
    .map((group) => ({
      ...group,
      fields: buckets.get(group.id) ?? [],
    }))
    .filter((group) => group.fields.length > 0)
}

export function validateDocumentData(fields: DocumentField[]): string[] {
  const errors: string[] = []
  for (const field of fields) {
    if (!field.required) continue
    const value = field.value?.trim()
    if (!value) {
      if (field.name === 'legalEntity') {
        errors.push('Vui lòng chọn pháp nhân / công ty')
      } else if (field.type === FieldType.Select) {
        errors.push(`Vui lòng chọn ${field.label.toLowerCase()}`)
      } else {
        errors.push(`Vui lòng nhập ${field.label.toLowerCase()}`)
      }
    }
  }
  return errors
}

export function filterFields(
  fields: DocumentField[],
  search: string,
  typeFilter: FieldTypeValue | 'all',
): DocumentField[] {
  const query = search.trim().toLowerCase()
  return fields.filter((field) => {
    if (typeFilter !== 'all' && field.type !== typeFilter) return false
    if (!query) return true
    return (
      field.name.toLowerCase().includes(query) ||
      field.label.toLowerCase().includes(query) ||
      field.placeholder.toLowerCase().includes(query)
    )
  })
}

export function fieldTypeIcon(type: FieldTypeValue): string {
  switch (type) {
    case FieldType.Number:
      return '🔢'
    case FieldType.Date:
      return '📅'
    case FieldType.Currency:
      return '💰'
    case FieldType.Textarea:
      return '📝'
    case FieldType.Select:
      return '📋'
    case FieldType.Checkbox:
      return '☑'
    default:
      return '🔤'
  }
}
