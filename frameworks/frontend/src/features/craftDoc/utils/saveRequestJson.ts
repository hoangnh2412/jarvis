import type { DocumentField, DocumentTemplate } from '../types'
import { toPlaceholderMap } from './fieldValues'

export type CraftDocSaveRequestJson = {
  template: {
    id: string
    name: string
    fileName: string
    updatedAt?: string
  }
  fields: DocumentField[]
  sampleData: Record<string, string>
}

export function buildCraftDocSaveRequestJson(
  template: DocumentTemplate,
  fields: DocumentField[],
): CraftDocSaveRequestJson {
  return {
    template: {
      id: template.id,
      name: template.name,
      fileName: template.fileName,
      updatedAt: template.updatedAt,
    },
    fields,
    sampleData: toPlaceholderMap(fields),
  }
}

export function prettyCraftDocJson(value: unknown): string {
  try {
    return JSON.stringify(value, null, 2)
  } catch {
    return '{}'
  }
}
