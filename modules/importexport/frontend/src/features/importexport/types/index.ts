export type ColumnMapping = {
  columnName: string
  fieldName: string
  required?: boolean
  unique?: boolean
  regex?: string | null
  minLength?: number | null
  maxLength?: number | null
  min?: number | null
  max?: number | null
}

export type SheetTemplate = {
  sheetName?: string | null
  mappings: ColumnMapping[]
  startRow?: number
  headerRow?: number | null
  rowCount?: number | null
}

export type ExcelTemplate = {
  code: string
  name: string
  fileName: string
  location?: string | null
  headerRow: number
  startRow: number
  rowCount?: number | null
  sheetName?: string | null
  mappings: ColumnMapping[]
  sheets?: SheetTemplate[]
  createdAt: string
  updatedAt: string
}

/** Legacy ImportExport.Api envelope */
export type ApiEnvelope<T> = {
  data: T | null
  status: number
  message: string
}

/** Platform Sample / ApiResponseWrapperMiddleware envelope */
export type PlatformBaseResponse<T> = {
  code?: string
  data?: T | null
  error?: {
    message?: string | null
    systemMessage?: string | null
  } | null
  timestamp?: string
}

export type FileDownloadPayload = {
  fileName: string
  contentType: string
  contentBase64: string
}

export type ImportErrorSummary = {
  message: string
  rowNumbers: number[]
  columnName?: string | null
  fieldName?: string | null
  errorCode?: string | null
  count?: number
}

export type SheetImportResult = {
  sheetName: string
  rows?: Record<string, unknown>[]
  errorSummary: ImportErrorSummary[]
  processedRowCount: number
  failedRowCount: number
  validRowCount: number
  rowsOmitted: boolean
}

export type ImportResult = {
  sheets: SheetImportResult[]
  processedRowCount: number
  failedRowCount: number
  savedCount: number
}

export type CreateTemplateInput = {
  code: string
  name: string
  file: File
  mappings: ColumnMapping[]
  sheetName?: string | null
  headerRow: number
  startRow: number
  rowCount?: number | null
}

export type UpdateTemplateInput = {
  file?: File | null
  mappings?: ColumnMapping[]
  sheetName?: string | null
  headerRow?: number
  startRow?: number
  rowCount?: number | null
}

export type UpdateTemplateMetadataInput = {
  name: string
}

export type ImportExportEntitySummary = {
  key: string
  displayName: string
}

export type ImportExportFieldSchema = {
  name: string
  displayName: string
  dataType: string
  clrType: string
  required: boolean
  minLength?: number | null
  maxLength?: number | null
  min?: number | null
  max?: number | null
  regex?: string | null
  isNumeric: boolean
}
