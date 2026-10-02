import type { DocumentField } from '../types'
import { FieldType } from '../types'
import { buildDynamicFieldGroups, DYNAMIC_GROUP_LABELS, inferFieldGroup } from './fieldGrouping'
import {
  getTemplateFieldGroups,
  hasPredefinedFieldGroups,
  type FieldGroupDef,
} from '../constants/fieldMeta'

export type DatePartCompositeDef = {
  id: string
  label: string
  /** Tên placeholder day, month, year trong DOCX. */
  parts: [day: string, month: string, year: string]
}

/** Cấu hình gộp ngày/tháng/năm → một date picker trên form. */
export const DATE_PART_COMPOSITE_DEFS: DatePartCompositeDef[] = [
  {
    id: 'document-date',
    label: 'Ngày văn bản',
    parts: ['day', 'month', 'year'],
  },
]

export type DatePartCompositeView = {
  id: string
  label: string
  partNames: { day: string; month: string; year: string }
  partFields: { day: DocumentField; month: DocumentField; year: DocumentField }
  value: string
  group?: string
  required: boolean
}

export type FormFieldItem =
  | { kind: 'field'; field: DocumentField }
  | { kind: 'date-composite'; composite: DatePartCompositeView }

export type GroupedFormFieldItems = FieldGroupDef & {
  items: FormFieldItem[]
}

function fieldByName(fields: DocumentField[], name: string): DocumentField | undefined {
  return fields.find((field) => field.name === name)
}

export function detectDatePartComposites(fields: DocumentField[]): DatePartCompositeDef[] {
  const names = new Set(fields.map((field) => field.name))
  return DATE_PART_COMPOSITE_DEFS.filter((def) =>
    def.parts.every((part) => names.has(part)),
  )
}

/** Ghép day/month/year → dd/mm/yyyy cho input date. */
export function datePartsToFormValue(
  day: string,
  month: string,
  year: string,
): string {
  const d = day.trim()
  const m = month.trim()
  const y = year.trim()
  if (!d || !m || !y) return ''
  const dayNum = Number(d)
  const monthNum = Number(m)
  const yearNum = Number(y)
  if (!Number.isFinite(dayNum) || !Number.isFinite(monthNum) || !Number.isFinite(yearNum)) {
    return ''
  }
  return `${String(dayNum).padStart(2, '0')}/${String(monthNum).padStart(2, '0')}/${y}`
}

/** Tách dd/mm/yyyy hoặc yyyy-mm-dd → day, month, year (số tự nhiên cho DOCX). */
export function formValueToDateParts(
  value: string,
): { day: string; month: string; year: string } | null {
  const trimmed = value.trim()
  if (!trimmed) return null

  let day: string
  let month: string
  let year: string

  const slashMatch = trimmed.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/)
  if (slashMatch) {
    ;[, day, month, year] = slashMatch
  } else {
    const isoMatch = trimmed.match(/^(\d{4})-(\d{2})-(\d{2})$/)
    if (!isoMatch) return null
    ;[, year, month, day] = isoMatch
  }

  const dayNum = Number(day)
  const monthNum = Number(month)
  const yearNum = Number(year)
  if (
    !Number.isFinite(dayNum) ||
    !Number.isFinite(monthNum) ||
    !Number.isFinite(yearNum) ||
    dayNum < 1 ||
    dayNum > 31 ||
    monthNum < 1 ||
    monthNum > 12
  ) {
    return null
  }

  return {
    day: String(dayNum),
    month: String(monthNum),
    year: String(yearNum),
  }
}

export function buildDatePartCompositeView(
  def: DatePartCompositeDef,
  fields: DocumentField[],
): DatePartCompositeView | null {
  const [dayName, monthName, yearName] = def.parts
  const dayField = fieldByName(fields, dayName)
  const monthField = fieldByName(fields, monthName)
  const yearField = fieldByName(fields, yearName)
  if (!dayField || !monthField || !yearField) return null

  const value = datePartsToFormValue(
    dayField.value ?? '',
    monthField.value ?? '',
    yearField.value ?? '',
  )

  return {
    id: def.id,
    label: def.label,
    partNames: { day: dayName, month: monthName, year: yearName },
    partFields: { day: dayField, month: monthField, year: yearField },
    value,
    group: dayField.group ?? monthField.group ?? yearField.group ?? inferFieldGroup(dayName),
    required: dayField.required || monthField.required || yearField.required,
  }
}

export function buildFormFieldItems(fields: DocumentField[]): FormFieldItem[] {
  const composites = detectDatePartComposites(fields)
  if (!composites.length) {
    return fields.map((field) => ({ kind: 'field', field }))
  }

  const hiddenPartNames = new Set(
    composites.flatMap((def) => def.parts),
  )
  const compositeByFirstPart = new Map(
    composites.map((def) => [def.parts[0], def]),
  )

  const items: FormFieldItem[] = []

  for (const field of fields) {
    const def = compositeByFirstPart.get(field.name)
    if (def) {
      const view = buildDatePartCompositeView(def, fields)
      if (view) {
        items.push({ kind: 'date-composite', composite: view })
      }
      continue
    }
    if (hiddenPartNames.has(field.name)) continue
    items.push({ kind: 'field', field })
  }

  return items
}

function bucketFormItems(
  items: FormFieldItem[],
  templateId: string,
): Map<string, FormFieldItem[]> {
  const usePredefined = hasPredefinedFieldGroups(templateId)
  const groupDefs = getTemplateFieldGroups(templateId)
  const buckets = new Map<string, FormFieldItem[]>(
    usePredefined ? groupDefs.map((group) => [group.id, []]) : [],
  )

  for (const item of items) {
    const groupId =
      item.kind === 'date-composite'
        ? item.composite.group ?? 'general'
        : item.field.group ?? inferFieldGroup(item.field.name)

    if (!buckets.has(groupId)) {
      buckets.set(groupId, [])
    }
    buckets.get(groupId)?.push(item)
  }

  return buckets
}

/** Nhóm field form — ẩn day/month/year riêng lẻ, hiện date picker gộp. */
export function groupFormFieldItems(
  fields: DocumentField[],
  templateId: string,
): GroupedFormFieldItems[] {
  const items = buildFormFieldItems(fields)

  if (!hasPredefinedFieldGroups(templateId)) {
    const dynamicGroups = buildDynamicFieldGroups(fields)
    const buckets = bucketFormItems(items, templateId)
    return dynamicGroups
      .map((group) => ({
        id: group.id,
        label: group.label,
        items: buckets.get(group.id) ?? [],
      }))
      .filter((group) => group.items.length > 0)
  }

  const groupDefs = getTemplateFieldGroups(templateId)
  const buckets = bucketFormItems(items, templateId)
  const knownIds = new Set(groupDefs.map((group) => group.id))

  const predefinedGroups = groupDefs
    .map((group) => ({
      ...group,
      items: buckets.get(group.id) ?? [],
    }))
    .filter((group) => group.items.length > 0)

  const extraGroups = [...buckets.keys()]
    .filter((id) => !knownIds.has(id) && (buckets.get(id)?.length ?? 0) > 0)
    .map((id) => ({
      id,
      label: DYNAMIC_GROUP_LABELS[id] ?? id,
      items: buckets.get(id) ?? [],
    }))

  return [...predefinedGroups, ...extraGroups]
}

export function compositeDatePartsToUpdates(
  composite: DatePartCompositeView,
  formDateValue: string,
): Record<string, string> {
  if (!formDateValue.trim()) {
    return {
      [composite.partNames.day]: '',
      [composite.partNames.month]: '',
      [composite.partNames.year]: '',
    }
  }

  const parts = formValueToDateParts(formDateValue)
  if (!parts) {
    return {
      [composite.partNames.day]: '',
      [composite.partNames.month]: '',
      [composite.partNames.year]: '',
    }
  }

  return {
    [composite.partNames.day]: parts.day,
    [composite.partNames.month]: parts.month,
    [composite.partNames.year]: parts.year,
  }
}

export function validateFormFieldItems(items: FormFieldItem[]): string[] {
  const errors: string[] = []

  for (const item of items) {
    if (item.kind === 'date-composite') {
      if (!item.composite.required) continue
      if (!item.composite.value.trim()) {
        errors.push(`Vui lòng chọn ${item.composite.label.toLowerCase()}`)
      }
      continue
    }

    const field = item.field
    if (!field.required) continue
    if (!field.value?.trim()) {
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
