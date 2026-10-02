import type { DocumentField, DocumentTemplate } from '../types'
import {
  bufferToBlob,
  downloadBlob,
  generateDocument,
  readDocxInput,
} from './docxTemplateService'
import { getCraftDocTemplatesMock } from '../mocks'
import instance from './req'
import {
  fetchKnownCraftDocTemplateMetadata,
  getKnownCraftDocTemplateMetadata,
  mergeCraftDocTemplateFields,
  resolveKnownCraftDocTemplateId,
} from './knownTemplate'
import { applyValuesMapToFields, toPlaceholderMap } from '../utils'

export type CraftDocPageSizeMeta = {
  format: string
  widthTwips: number
  heightTwips: number
  widthMm?: number
  heightMm?: number
  widthPx96Dpi?: number
  heightPx96Dpi?: number
}

export type CraftDocMarginsTwips = {
  top: number
  right: number
  bottom: number
  left: number
  header?: number
  footer?: number
}

export type CraftDocTemplateMeta = Pick<
  DocumentTemplate,
  'id' | 'name' | 'fileName' | 'updatedAt'
> & {
  pageSize?: CraftDocPageSizeMeta
  marginsTwips?: CraftDocMarginsTwips
  contentWidthTwips?: number
  contentWidthPx96Dpi?: number
}

export type CraftDocTemplateListResult = {
  templates: Array<Pick<DocumentTemplate, 'id' | 'name' | 'fileName'>>
}

export type CraftDocTemplateDetailResult = {
  template: CraftDocTemplateMeta
  fields: DocumentField[]
  sampleData: Record<string, string>
}

export type CraftDocTemplateDetailApiResponse = CraftDocTemplateDetailResult

export type CraftDocSampleDataResult = {
  sampleData: Record<string, string>
}

export type CraftDocSavePayload = {
  template: DocumentTemplate
  /** Fields kèm value — nguồn dữ liệu form chính. */
  fields: DocumentField[]
  /** @deprecated Dùng fields[].value — giữ cho API legacy. */
  sampleData?: Record<string, string>
}

export type CraftDocExportPayload = CraftDocSavePayload

export type CraftDocSaveResult = {
  saved: boolean
  templateId: string
}

export type CraftDocExportResult = {
  fileName: string
}

export type CraftDocUploadPayload = {
  fileName: string
  name: string
  fileBase64: string
}

export type CraftDocUploadApiResult = {
  id: string
  name: string
  fileName: string
  updatedAt: string
  fileBase64: string
  fields?: DocumentField[]
  sampleData?: Record<string, string>
}

function arrayBufferToBase64(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer)
  let binary = ''
  for (let i = 0; i < bytes.length; i += 1) {
    binary += String.fromCharCode(bytes[i]!)
  }
  return btoa(binary)
}

function base64ToArrayBuffer(base64: string): ArrayBuffer {
  const binary = atob(base64)
  const bytes = new Uint8Array(binary.length)
  for (let i = 0; i < binary.length; i += 1) {
    bytes[i] = binary.charCodeAt(i)
  }
  return bytes.buffer
}

/** GET v1/craft-doc/templates — mock Sample trả danh sách mẫu. */
export async function callGetCraftDocTemplates(): Promise<CraftDocTemplateListResult> {
  try {
    const response = await instance.get<CraftDocTemplateListResult>('v1/craft-doc/templates')
    return response.data
  } catch {
    return { templates: getCraftDocTemplatesMock() }
  }
}

/** GET v1/craft-doc/templates/:id — metadata + fields + sampleData. */
export async function callGetCraftDocTemplateDetail(
  templateId: string,
): Promise<CraftDocTemplateDetailResult> {
  const response = await instance.get<CraftDocTemplateDetailApiResponse>(
    `v1/craft-doc/templates/${encodeURIComponent(templateId)}`,
  )
  return response.data
}

/** GET v1/craft-doc/templates/:id/sample-data */
export async function callGetCraftDocTemplateSampleData(
  templateId: string,
): Promise<CraftDocSampleDataResult> {
  const response = await instance.get<CraftDocSampleDataResult>(
    `v1/craft-doc/templates/${encodeURIComponent(templateId)}/sample-data`,
  )
  return response.data
}

/** GET v1/craft-doc/templates/:id/file — tải binary DOCX, trả DocumentTemplate đầy đủ. */
export async function callLoadCraftDocTemplateFromApi(
  templateId: string,
): Promise<DocumentTemplate> {
  const [detail, fileResponse] = await Promise.all([
    callGetCraftDocTemplateDetail(templateId),
    instance.get<ArrayBuffer>(`v1/craft-doc/templates/${encodeURIComponent(templateId)}/file`, {
      responseType: 'arraybuffer',
    }),
  ])

  const fileBuffer = fileResponse.data
  const merged = mergeCraftDocTemplateFields(fileBuffer, detail.fields, detail.template.id)
  const fields = applyValuesMapToFields(merged, detail.sampleData)

  return {
    id: detail.template.id,
    name: detail.template.name,
    fileName: detail.template.fileName,
    file: bufferToBlob(fileBuffer),
    fields,
    updatedAt: detail.template.updatedAt,
  }
}

/** Stub save — mặc định no-op, host persist qua `callback.save`. */
export async function callSaveCraftDocDocument(
  payload: CraftDocSavePayload,
): Promise<CraftDocSaveResult> {
  return {
    saved: true,
    templateId: payload.template.id,
  }
}

/** Stub export — tải file DOCX đã điền (client-side). */
export async function callExportCraftDocDocument(
  payload: CraftDocExportPayload,
): Promise<CraftDocExportResult> {
  const buffer = await readDocxInput(payload.template.file)
  const data =
    payload.sampleData ??
    toPlaceholderMap(payload.fields.length ? payload.fields : payload.template.fields)
  const blob = generateDocument(buffer, data)
  downloadBlob(blob, payload.template.fileName)
  return { fileName: payload.template.fileName }
}

/** POST v1/craft-doc/templates/upload — mock Sample ~700ms, trả template + file base64. */
export async function callUploadCraftDocTemplate(file: File): Promise<DocumentTemplate> {
  const buffer = await readDocxInput(file)
  const payload: CraftDocUploadPayload = {
    fileName: file.name,
    name: file.name.replace(/\.docx$/i, ''),
    fileBase64: arrayBufferToBase64(buffer),
  }

  const response = await instance.post<CraftDocUploadApiResult>(
    'v1/craft-doc/templates/upload',
    payload,
  )

  const uploaded = response.data
  const fileBuffer = base64ToArrayBuffer(uploaded.fileBase64)
  const knownId = resolveKnownCraftDocTemplateId(file.name)

  let metadataFields = uploaded.fields ?? []
  let templateId = uploaded.id
  let templateName = uploaded.name
  let legacyValues = uploaded.sampleData

  if (metadataFields.length === 0 && knownId) {
    const meta =
      (await fetchKnownCraftDocTemplateMetadata(knownId, callGetCraftDocTemplateDetail)) ??
      getKnownCraftDocTemplateMetadata(knownId)
    if (meta) {
      metadataFields = meta.fields
      templateId = meta.id
      templateName = meta.name
      legacyValues = { ...meta.sampleData, ...legacyValues }
    }
  }

  const fields = applyValuesMapToFields(
    mergeCraftDocTemplateFields(fileBuffer, metadataFields, templateId),
    legacyValues,
  )

  return {
    id: templateId,
    name: templateName,
    fileName: uploaded.fileName,
    file: bufferToBlob(fileBuffer),
    fields,
    updatedAt: uploaded.updatedAt,
  }
}

/** Phân tích file upload cục bộ — gắn metadata cho mẫu đã biết (vd. LLA-IDAS.docx). */
export async function resolveCraftDocTemplateFromFile(file: File): Promise<DocumentTemplate> {
  const buffer = await readDocxInput(file)
  const knownId = resolveKnownCraftDocTemplateId(file.name)

  if (knownId) {
    const meta =
      (await fetchKnownCraftDocTemplateMetadata(knownId, callGetCraftDocTemplateDetail)) ??
      getKnownCraftDocTemplateMetadata(knownId)
    if (meta) {
      const fields = applyValuesMapToFields(
        mergeCraftDocTemplateFields(buffer, meta.fields, meta.id),
        meta.sampleData,
      )
      return {
        id: meta.id,
        name: meta.name,
        fileName: file.name,
        file: bufferToBlob(buffer),
        fields,
        updatedAt: meta.updatedAt ?? new Date().toISOString(),
      }
    }
  }

  const templateId = `upload-${Date.now()}`

  return {
    id: templateId,
    name: file.name.replace(/\.docx$/i, ''),
    fileName: file.name,
    file: bufferToBlob(buffer),
    fields: mergeCraftDocTemplateFields(buffer, [], templateId),
    updatedAt: new Date().toISOString(),
  }
}
