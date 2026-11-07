import type { DocumentField } from '../types'

/** Map placeholder name → giá trị text (engine DOCX). Chỉ gồm field đã có value — field trống giữ {{placeholder}}. */
export function toPlaceholderMap(fields: DocumentField[]): Record<string, string> {
  const map: Record<string, string> = {}
  for (const field of fields) {
    const value = (field.value ?? field.defaultValue ?? '').trim()
    if (value) {
      map[field.name] = field.value ?? field.defaultValue ?? ''
    }
  }
  return map
}

/** Gán value cho từng field; ưu tiên value có sẵn trên field, rồi defaults map. */
export function ensureFieldValues(
  fields: DocumentField[],
  defaults?: Record<string, string>,
): DocumentField[] {
  return fields.map((field) => ({
    ...field,
    value: field.value ?? defaults?.[field.name] ?? field.defaultValue ?? '',
  }))
}

/** Merge values từ field cũ sau khi parse lại template. */
export function mergeParsedFieldsWithValues(
  parsed: DocumentField[],
  previous: DocumentField[],
): DocumentField[] {
  const previousByName = new Map(previous.map((field) => [field.name, field]))
  return parsed.map((field) => {
    const prev = previousByName.get(field.name)
    return {
      ...field,
      value: prev?.value ?? field.value ?? field.defaultValue ?? '',
    }
  })
}

/** Áp map phẳng (API legacy sampleData) lên danh sách field. */
export function applyValuesMapToFields(
  fields: DocumentField[],
  values?: Record<string, string>,
): DocumentField[] {
  if (!values) return ensureFieldValues(fields)
  return fields.map((field) => ({
    ...field,
    value: values[field.name] ?? field.value ?? field.defaultValue ?? '',
  }))
}
