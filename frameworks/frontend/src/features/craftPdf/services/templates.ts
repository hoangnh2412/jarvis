import { PDF_PAGE_A4 } from '../constants'
import { PdfTemplateStatus, type CreatePdfTemplatePayload, type GeneratePdfRequest, type GeneratePdfResponse, type GetPdfTemplateListParams, type PdfTemplate, type PdfTemplateListResult, type UpdatePdfTemplatePayload } from '../types'
import {
  createElementId,
  getMemoryTemplates,
  setMemoryTemplates,
} from '../utils/elements'
import craftPdfHttp from './req'

/**
 * List templates — ưu tiên API nếu có BASE_URL; fallback memory mock.
 */
export async function callGetPdfTemplateList(
  params: GetPdfTemplateListParams = {},
): Promise<PdfTemplateListResult> {
  if (craftPdfHttp.defaults.baseURL) {
    const { data } = await craftPdfHttp.get<PdfTemplateListResult>('/templates', {
      params,
    })
    return data
  }

  const all = getMemoryTemplates()
  const search = (params.search ?? '').trim().toLowerCase()
  const status = params.status ?? 'all'
  let filtered = all
  if (search) {
    filtered = filtered.filter(
      (t) =>
        t.name.toLowerCase().includes(search) ||
        (t.description ?? '').toLowerCase().includes(search),
    )
  }
  if (status !== 'all') {
    filtered = filtered.filter((t) => t.status === status)
  }
  const page = params.page ?? 1
  const size = params.size ?? 20
  const start = (page - 1) * size
  return {
    items: filtered.slice(start, start + size),
    total: filtered.length,
    page,
    size,
  }
}

export async function callGetPdfTemplate(id: string): Promise<PdfTemplate> {
  if (craftPdfHttp.defaults.baseURL) {
    const { data } = await craftPdfHttp.get<PdfTemplate>(`/templates/${id}`)
    return data
  }
  const found = getMemoryTemplates().find((t) => t.id === id)
  if (!found) throw new Error(`Template không tồn tại: ${id}`)
  return structuredClone(found)
}

export async function callCreatePdfTemplate(
  payload: CreatePdfTemplatePayload,
): Promise<PdfTemplate> {
  if (craftPdfHttp.defaults.baseURL) {
    const { data } = await craftPdfHttp.post<PdfTemplate>('/templates', payload)
    return data
  }
  const t = new Date().toISOString()
  const created: PdfTemplate = {
    id: createElementId('tpl'),
    name: payload.name,
    description: payload.description,
    status: PdfTemplateStatus.Draft,
    pageSize: payload.pageSize ?? PDF_PAGE_A4,
    elements: payload.elements ?? [],
    createdAt: t,
    updatedAt: t,
  }
  setMemoryTemplates([created, ...getMemoryTemplates()])
  return created
}

export async function callUpdatePdfTemplate(
  id: string,
  payload: UpdatePdfTemplatePayload,
): Promise<PdfTemplate> {
  if (craftPdfHttp.defaults.baseURL) {
    const { data } = await craftPdfHttp.put<PdfTemplate>(
      `/templates/${id}`,
      payload,
    )
    return data
  }
  const list = getMemoryTemplates()
  const idx = list.findIndex((t) => t.id === id)
  if (idx < 0) throw new Error(`Template không tồn tại: ${id}`)
  const next: PdfTemplate = {
    ...list[idx],
    ...payload,
    updatedAt: new Date().toISOString(),
  }
  const copy = [...list]
  copy[idx] = next
  setMemoryTemplates(copy)
  return next
}

export async function callDeletePdfTemplate(id: string): Promise<void> {
  if (craftPdfHttp.defaults.baseURL) {
    await craftPdfHttp.delete(`/templates/${id}`)
    return
  }
  setMemoryTemplates(getMemoryTemplates().filter((t) => t.id !== id))
}

/**
 * Generate PDF — stub. App/backend wire sau qua callback hoặc BASE_URL.
 */
export async function callGeneratePdf(
  request: GeneratePdfRequest,
): Promise<GeneratePdfResponse> {
  if (craftPdfHttp.defaults.baseURL) {
    const { data } = await craftPdfHttp.post<GeneratePdfResponse>(
      '/generate',
      request,
    )
    return data
  }
  throw new Error(
    'Generate PDF chưa nối backend. Truyền callback.onGenerate hoặc set VITE_API_URL_CRAFT_PDF.',
  )
}
