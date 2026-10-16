import { getImportExportHttp } from '@platform/importexport'
import type { ImportResult } from '@platform/importexport'

export type Student = {
  id: string
  name: string
  age: number
}

export type StudentListResult = {
  items: Student[]
  totalCount: number
  page: number
  size: number
}

export type CreateStudentInput = {
  name: string
  age: number
}

export type UpdateStudentInput = CreateStudentInput

type PlatformResponse<T> = {
  code?: string
  data?: T
  error?: { message?: string; systemMessage?: string }
}

type ApiEnvelope<T> = {
  status: number
  message: string
  data?: T
}

type FileDownloadPayload = {
  fileName: string
  contentType: string
  contentBase64: string
}

function unwrap<T>(body: unknown, fallback: string): T {
  if (body == null || body === '') throw new Error(fallback)
  if (typeof body === 'object' && body !== null && 'code' in body) {
    const platform = body as PlatformResponse<T>
    if (platform.data == null) {
      throw new Error(
        platform.error?.systemMessage || platform.error?.message || fallback,
      )
    }
    return platform.data
  }
  return body as T
}

function isLegacyEnvelope(body: unknown): body is ApiEnvelope<unknown> {
  return (
    !!body &&
    typeof body === 'object' &&
    'status' in body &&
    'message' in body
  )
}

function mapImportResult(raw: Record<string, unknown>): ImportResult {
  const sheetsRaw = (raw.sheets ?? raw.Sheets ?? []) as Record<string, unknown>[]
  return {
    sheets: sheetsRaw.map((sheet) => ({
      sheetName: String(sheet.sheetName ?? sheet.SheetName ?? ''),
      rows: sheet.rows as Record<string, unknown>[] | undefined,
      errorSummary: (sheet.errorSummary ?? sheet.ErrorSummary ?? []) as ImportResult['sheets'][0]['errorSummary'],
      processedRowCount: Number(sheet.processedRowCount ?? sheet.ProcessedRowCount ?? 0),
      failedRowCount: Number(sheet.failedRowCount ?? sheet.FailedRowCount ?? 0),
      validRowCount: Number(sheet.validRowCount ?? sheet.ValidRowCount ?? 0),
      rowsOmitted: Boolean(sheet.rowsOmitted ?? sheet.RowsOmitted),
    })),
    processedRowCount: Number(raw.processedRowCount ?? raw.ProcessedRowCount ?? 0),
    failedRowCount: Number(raw.failedRowCount ?? raw.FailedRowCount ?? 0),
    savedCount: Number(raw.savedCount ?? raw.SavedCount ?? 0),
  }
}

function mapStudent(raw: Record<string, unknown>): Student {
  return {
    id: String(raw.id ?? raw.Id ?? ''),
    name: String(raw.name ?? raw.Name ?? ''),
    age: Number(raw.age ?? raw.Age ?? 0),
  }
}

function mapListResult(raw: Record<string, unknown>): StudentListResult {
  const itemsRaw = (raw.items ?? raw.Items ?? []) as Record<string, unknown>[]
  return {
    items: itemsRaw.map(mapStudent),
    totalCount: Number(raw.totalCount ?? raw.TotalCount ?? 0),
    page: Number(raw.page ?? raw.Page ?? 1),
    size: Number(raw.size ?? raw.Size ?? 10),
  }
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

const http = () => getImportExportHttp()

export async function listStudents(params?: {
  page?: number
  size?: number
  filter?: string
  sort?: string
}): Promise<StudentListResult> {
  const { data } = await http().get('v1/students', { params })
  return mapListResult(unwrap<Record<string, unknown>>(data, 'Không tải được danh sách học sinh'))
}

export async function getStudent(id: string): Promise<Student> {
  const { data } = await http().get(`v1/students/${encodeURIComponent(id)}`)
  return mapStudent(unwrap<Record<string, unknown>>(data, 'Không tải được học sinh'))
}

export async function createStudent(input: CreateStudentInput): Promise<Student> {
  const { data } = await http().post('v1/students', input)
  return mapStudent(unwrap<Record<string, unknown>>(data, 'Tạo học sinh thất bại'))
}

export async function updateStudent(
  id: string,
  input: UpdateStudentInput,
): Promise<Student> {
  const { data } = await http().put(
    `v1/students/${encodeURIComponent(id)}`,
    input,
  )
  return mapStudent(unwrap<Record<string, unknown>>(data, 'Cập nhật học sinh thất bại'))
}

export async function deleteStudent(id: string): Promise<void> {
  await http().delete(`v1/students/${encodeURIComponent(id)}`)
}

async function postStudentImport(
  file: File,
  path: 'import/validate' | 'import',
): Promise<{
  result: ImportResult
  message: string
  status: number
}> {
  const form = new FormData()
  form.append('File', file)

  const response = await http().post(`v1/students/${path}`, form, {
    validateStatus: (s) => (s >= 200 && s < 300) || s === 422,
  })

  const body = response.data
  if (body == null || body === '') throw new Error('Import thất bại')

  if (typeof body === 'object' && body !== null && 'code' in body) {
    const result = mapImportResult(
      unwrap<Record<string, unknown>>(body, 'Import thất bại'),
    )
    const message =
      result.failedRowCount > 0
        ? `Import failed: ${result.failedRowCount} row(s) have validation errors.`
        : path === 'import/validate'
          ? 'Validate completed successfully.'
          : 'Import completed successfully.'
    return { result, message, status: response.status }
  }

  if (isLegacyEnvelope(body)) {
    if (body.data == null) throw new Error(body.message || 'Import thất bại')
    const result = mapImportResult(body.data as Record<string, unknown>)
    return { result, message: body.message, status: response.status }
  }

  throw new Error('Import thất bại')
}

export async function validateStudentImport(file: File) {
  return postStudentImport(file, 'import/validate')
}

export async function importStudents(file: File): Promise<{
  result: ImportResult
  message: string
  status: number
}> {
  return postStudentImport(file, 'import')
}

export async function exportStudents(fileName = 'students.xlsx'): Promise<void> {
  const response = await http().post(
    'v1/students/export',
    null,
    {
      params: { fileName },
      responseType: 'blob',
    },
  )

  const raw = response.data
  if (raw instanceof Blob) {
    const contentType = String(response.headers['content-type'] ?? '')
    if (contentType.includes('json')) {
      const text = await raw.text()
      const parsed: unknown = JSON.parse(text)
      const file = unwrap<FileDownloadPayload>(parsed, 'Export thất bại')
      downloadFromBase64(file)
      return
    }
    triggerDownload(raw, fileName)
    return
  }

  const file = unwrap<FileDownloadPayload>(raw, 'Export thất bại')
  downloadFromBase64(file)
}
