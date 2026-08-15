import type {
  ImportCommitPayload,
  ImportCommitResult,
  ImportValidatePayload,
  ImportValidationResult,
} from '../types'

const DEMO_VALID_ROWS = [
  { row: 1, age: 15, name: 'Phạm Thế Anh' },
  { row: 2, age: 22, name: 'Phạm Quốc Khánh' },
  { row: 3, age: 21, name: 'Phạm Tuấn Ngọc' },
] as const

const DEMO_INVALID_ROWS = [
  {
    row: 4,
    age: '',
    name: 'Nguyễn Văn A',
    errors: ['AGE không được để trống'],
  },
  {
    row: 5,
    age: -1,
    name: 'Trần Thị B',
    errors: ['AGE phải lớn hơn 0'],
  },
] as const

function delay(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

function pickSheetName(file: File) {
  const base = file.name.replace(/\.[^.]+$/, '')
  return base ? base.slice(0, 24) : 'Sheet1'
}

export async function mockValidateImport({
  file,
}: ImportValidatePayload): Promise<ImportValidationResult> {
  await delay(700)

  const hasErrors = /invalid|error|loi|lỗi/i.test(file.name)
  const validRows = hasErrors
    ? DEMO_VALID_ROWS.slice(0, 2).map((row) => ({ ...row }))
    : DEMO_VALID_ROWS.map((row) => ({ ...row }))
  const invalidRows = hasErrors
    ? DEMO_INVALID_ROWS.map((row) => ({
        row: row.row,
        age: row.age,
        name: row.name,
        errors: [...row.errors],
      }))
    : []

  const validCount = validRows.length
  const invalidCount = invalidRows.length
  const processed = validCount + invalidCount

  return {
    success: invalidCount === 0,
    message:
      invalidCount === 0
        ? 'Kiểm tra file hoàn tất, không có lỗi.'
        : 'Kiểm tra file hoàn tất, phát hiện lỗi.',
    processed,
    validCount,
    invalidCount,
    sheetName: pickSheetName(file) || 'Sheet1',
    validRows,
    invalidRows,
  }
}

export async function mockCommitImport({
  file,
}: ImportCommitPayload): Promise<ImportCommitResult> {
  const validation = await mockValidateImport({ file })
  await delay(500)

  if (validation.invalidCount > 0) {
    return {
      success: false,
      message: 'Không thể import khi còn dòng lỗi. Vui lòng chọn file khác.',
      importedCount: 0,
    }
  }

  return {
    success: true,
    message: `Import thành công ${validation.validCount} dòng dữ liệu.`,
    importedCount: validation.validCount,
  }
}

/** @deprecated Dùng `mockValidateImport` */
export const mockValidateStudentImport = mockValidateImport

/** @deprecated Dùng `mockCommitImport` */
export const mockImportStudents = mockCommitImport
