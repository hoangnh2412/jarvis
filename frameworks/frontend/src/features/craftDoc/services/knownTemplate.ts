import type { DocumentField } from '../types'
import { getLlaIdasTemplateDetailMock } from '../mocks'
import { extractFields } from './docxTemplateService'

/** Nhận diện mẫu có metadata API/mock (DOCX không chứa {{placeholder}}). */
export function resolveKnownCraftDocTemplateId(fileName: string): string | null {
  const base = fileName.trim().toLowerCase().replace(/\.docx$/i, '')
  if (base === 'lla-idas') return 'lla-idas'
  return null
}

export type KnownCraftDocTemplateMetadata = {
  id: string
  name: string
  fileName: string
  fields: DocumentField[]
  sampleData: Record<string, string>
  updatedAt?: string
}

export function getKnownCraftDocTemplateMetadata(
  templateId: string,
): KnownCraftDocTemplateMetadata | null {
  if (templateId !== 'lla-idas') return null
  const mock = getLlaIdasTemplateDetailMock()
  return {
    id: mock.template.id,
    name: mock.template.name,
    fileName: mock.template.fileName,
    fields: mock.fields,
    sampleData: mock.sampleData,
    updatedAt: mock.template.updatedAt,
  }
}

/** Gộp field từ DOCX (nếu có placeholder) với metadata mẫu đã biết. */
export function mergeCraftDocTemplateFields(
  buffer: ArrayBuffer,
  metadataFields: DocumentField[],
  templateId?: string,
): DocumentField[] {
  return extractFields(buffer, metadataFields, templateId)
}

export async function fetchKnownCraftDocTemplateMetadata(
  templateId: string,
  fetchDetail: (id: string) => Promise<{
    template: { id: string; name: string; fileName: string; updatedAt?: string }
    fields: DocumentField[]
    sampleData: Record<string, string>
  }>,
): Promise<KnownCraftDocTemplateMetadata | null> {
  try {
    const detail = await fetchDetail(templateId)
    return {
      id: detail.template.id,
      name: detail.template.name,
      fileName: detail.template.fileName,
      fields: detail.fields,
      sampleData: detail.sampleData,
      updatedAt: detail.template.updatedAt,
    }
  } catch {
    return getKnownCraftDocTemplateMetadata(templateId)
  }
}
