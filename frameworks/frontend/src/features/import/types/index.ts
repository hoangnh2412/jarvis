export type ImportValidRow = {
  row: number
  age: number
  name: string
}

export type ImportInvalidRow = {
  row: number
  age?: string | number
  name?: string
  errors: string[]
}

export type ImportValidationResult = {
  success: boolean
  message: string
  processed: number
  validCount: number
  invalidCount: number
  sheetName: string
  validRows: ImportValidRow[]
  invalidRows: ImportInvalidRow[]
}

export type ImportCommitResult = {
  success: boolean
  message: string
  importedCount: number
}

export type ImportValidatePayload = {
  file: File
}

export type ImportCommitPayload = {
  file: File
}
