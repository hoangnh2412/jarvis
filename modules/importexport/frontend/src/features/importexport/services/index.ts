import axios from 'axios'
import type { AxiosInstance } from 'axios'
import { normalizeApiBaseUrl } from '@platform/core'
import {
  IMPORT_EXPORT_API_KEY_HEADER,
  IMPORT_EXPORT_TENANT_HEADER,
} from '../constants'
import type {
  CreateTemplateInput,
  ExcelTemplate,
  FileDownloadPayload,
  ImportResult,
  ImportExportEntitySummary,
  ImportExportFieldSchema,
  PlatformBaseResponse,
  UpdateTemplateInput,
  UpdateTemplateMetadataInput,
} from '../types'
import {
  unwrap,
  unwrapArray,
} from './response'

export type ImportExportHttpConfig = {
  baseURL?: string
  apiKey?: string | (() => string | undefined)
  /** Optional — host demo only; resolved server-side into ICurrentTenant */
  getTenantId?: () => string | undefined
}

let http: AxiosInstance = axios.create({
  baseURL: '',
})

let configRef: ImportExportHttpConfig = {}

function resolveApiKey() {
  const key = configRef.apiKey
  return typeof key === 'function' ? key() : key
}

export function configureImportExportHttp(config: ImportExportHttpConfig) {
  configRef = config
  http = axios.create({
    baseURL: normalizeApiBaseUrl(config.baseURL ?? '/api/'),
  })

  http.interceptors.request.use((req) => {
    const apiKey = resolveApiKey()
    if (apiKey) {
      req.headers.set(IMPORT_EXPORT_API_KEY_HEADER, apiKey)
    }
    const tenantId = config.getTenantId?.()
    if (tenantId) {
      req.headers.set(IMPORT_EXPORT_TENANT_HEADER, tenantId)
    }
    // Avoid 304 with empty body (axios → unwrap treats "" as valid payload).
    req.headers.set('Cache-Control', 'no-cache')
    req.headers.set('Pragma', 'no-cache')
    return req
  })

  http.interceptors.response.use(
    (response) => response,
    (error) => {
      const payload = error.response?.data
      if (payload && typeof payload === 'object') {
        const platform = payload as PlatformBaseResponse<unknown>
        if (platform.error?.systemMessage || platform.error?.message) {
          return Promise.reject(
            new Error(
              platform.error.systemMessage ||
                platform.error.message ||
                'Request failed',
            ),
          )
        }
        const legacy = payload as { message?: string }
        if ('message' in legacy && typeof legacy.message === 'string') {
          return Promise.reject(payload)
        }
      }
      return Promise.reject(error)
    },
  )
}

function mappingsJson(
  mappings: CreateTemplateInput['mappings'],
  sheetName?: string | null,
) {
  if (sheetName) {
    return JSON.stringify({ sheetName, mappings })
  }
  return JSON.stringify(mappings)
}

export async function listTemplates(): Promise<ExcelTemplate[]> {
  const { data } = await http.get('v1/templates')
  return unwrapArray<ExcelTemplate>(
    data,
    'Không tải được danh sách template',
  )
}

export async function getTemplate(code: string): Promise<ExcelTemplate> {
  const { data } = await http.get(
    `v1/templates/${encodeURIComponent(code)}`,
  )
  return unwrap<ExcelTemplate>(data, 'Không tải được template')
}

export async function createTemplate(
  input: CreateTemplateInput,
): Promise<ExcelTemplate> {
  const form = new FormData()
  form.append('Code', input.code)
  form.append('Name', input.name)
  form.append('File', input.file)
  form.append('Mappings', mappingsJson(input.mappings, input.sheetName))
  form.append('HeaderRow', String(input.headerRow))
  form.append('StartRow', String(input.startRow))
  if (input.rowCount != null) {
    form.append('RowCount', String(input.rowCount))
  }

  const { data } = await http.post('v1/templates', form)
  return unwrap<ExcelTemplate>(data, 'Tạo template thất bại')
}

export async function updateTemplateMetadata(
  code: string,
  input: UpdateTemplateMetadataInput,
): Promise<ExcelTemplate> {
  const { data } = await http.patch(
    `v1/templates/${encodeURIComponent(code)}`,
    { name: input.name },
  )
  return unwrap<ExcelTemplate>(data, 'Cập nhật tên template thất bại')
}

function appendConfigurationForm(
  form: FormData,
  input: UpdateTemplateInput,
) {
  if (input.file) form.append('File', input.file)
  if (input.mappings) {
    form.append('Mappings', mappingsJson(input.mappings, input.sheetName))
  }
  if (input.headerRow != null) form.append('HeaderRow', String(input.headerRow))
  if (input.startRow != null) form.append('StartRow', String(input.startRow))
  if (input.rowCount != null) form.append('RowCount', String(input.rowCount))
}

export async function updateTemplateConfiguration(
  code: string,
  input: UpdateTemplateInput,
): Promise<ExcelTemplate> {
  const form = new FormData()
  appendConfigurationForm(form, input)

  const { data } = await http.put(
    `v1/templates/${encodeURIComponent(code)}/configuration`,
    form,
  )
  return unwrap<ExcelTemplate>(data, 'Cập nhật cấu hình template thất bại')
}

/** @deprecated Use updateTemplateConfiguration or updateTemplateMetadata */
export async function updateTemplate(
  code: string,
  input: UpdateTemplateInput & { name?: string },
): Promise<ExcelTemplate> {
  const form = new FormData()
  if (input.name != null) form.append('Name', input.name)
  appendConfigurationForm(form, input)

  const { data } = await http.put(
    `v1/templates/${encodeURIComponent(code)}/configuration`,
    form,
  )
  return unwrap<ExcelTemplate>(data, 'Cập nhật template thất bại')
}

export async function deleteTemplate(code: string): Promise<void> {
  await http.delete(`v1/templates/${encodeURIComponent(code)}`)
}

function fileNameFromDisposition(header: string | undefined, fallback: string) {
  if (!header) return fallback
  const utf = /filename\*=UTF-8''([^;]+)/i.exec(header)
  if (utf?.[1]) return decodeURIComponent(utf[1])
  const plain = /filename="?([^";]+)"?/i.exec(header)
  return plain?.[1] ?? fallback
}

function triggerDownload(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName
  a.click()
  URL.revokeObjectURL(url)
}

function downloadFromBase64(file: FileDownloadPayload) {
  const binary = atob(file.contentBase64)
  const bytes = new Uint8Array(binary.length)
  for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i)
  triggerDownload(
    new Blob([bytes], { type: file.contentType || 'application/octet-stream' }),
    file.fileName,
  )
}

function isFileDownloadPayload(value: unknown): value is FileDownloadPayload {
  return (
    !!value &&
    typeof value === 'object' &&
    'contentBase64' in value &&
    'fileName' in value
  )
}

async function resolveFileDownload(
  response: {
    data: unknown
    headers: Record<string, unknown>
  },
  fallbackName: string,
) {
  const raw = response.data

  if (raw instanceof Blob) {
    const contentType = String(response.headers['content-type'] ?? '')
    if (contentType.includes('application/json') || contentType.includes('json')) {
      const text = await raw.text()
      const parsed: unknown = JSON.parse(text)
      const file = unwrap<FileDownloadPayload>(parsed, 'Tải file thất bại')
      downloadFromBase64(file)
      return
    }
    const name = fileNameFromDisposition(
      response.headers['content-disposition'] as string | undefined,
      fallbackName,
    )
    triggerDownload(raw, name)
    return
  }

  const file = unwrap<FileDownloadPayload>(raw, 'Tải file thất bại')
  if (!isFileDownloadPayload(file)) {
    throw new Error('Phản hồi download không hợp lệ')
  }
  downloadFromBase64(file)
}

const blobRequestConfig = { responseType: 'blob' as const }

export async function downloadTemplateFile(code: string): Promise<void> {
  const response = await http.get(
    `v1/templates/${encodeURIComponent(code)}/downloadFile`,
    blobRequestConfig,
  )
  await resolveFileDownload(response, `${code}.xlsx`)
}

export async function listImportExportEntities(): Promise<
  ImportExportEntitySummary[]
> {
  const { data } = await http.get('v1/importexport/entities')
  return unwrapArray<ImportExportEntitySummary>(
    data,
    'Không tải được danh sách đối tượng',
  )
}

export async function getImportExportEntityFields(
  key: string,
): Promise<ImportExportFieldSchema[]> {
  const { data } = await http.get(
    `v1/importexport/entities/${encodeURIComponent(key)}/fields`,
  )
  return unwrapArray<ImportExportFieldSchema>(
    data,
    'Không tải được danh sách field',
  )
}

export async function validateExcelImport(
  templateCode: string,
  file: File,
): Promise<ImportResult> {
  const form = new FormData()
  form.append('File', file)
  form.append('TemplateCode', templateCode)

  const { data } = await http.post('v1/excel/import/validate', form, {
    validateStatus: (s) => (s >= 200 && s < 300) || s === 422,
  })
  return unwrap<ImportResult>(data, 'Validate import thất bại')
}

export async function importExcel(
  templateCode: string,
  file: File,
): Promise<ImportResult> {
  const form = new FormData()
  form.append('File', file)
  form.append('TemplateCode', templateCode)

  const { data } = await http.post('v1/excel/import', form, {
    validateStatus: (s) => (s >= 200 && s < 300) || s === 422,
  })
  return unwrap<ImportResult>(data, 'Import thất bại')
}

export function getImportExportHttp() {
  return http
}
