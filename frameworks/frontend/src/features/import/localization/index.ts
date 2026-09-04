export type ImportPageMessages = {
  title: string
  description: string
  backLabel: string
  fileLabel: string
  confirmLabel: string
  cancelLabel: string
  validatingLabel: string
  committingLabel: string
  resultTitle: string
  processedLabel: string
  validLabel: string
  invalidLabel: string
  validSectionTitle: string
  invalidSectionTitle: string
  noInvalidRows: string
  sheetPrefix: string
  validShort: string
  errorShort: string
  colIndex: string
  colAge: string
  colName: string
  colErrors: string
  validateError: string
  commitSuccess: string
  commitError: string
  dropzoneTitle: string
  dropzoneHint: string
}

export type ImportMessages = {
  page: ImportPageMessages
}

export type ImportLocale = 'vi' | 'en'

export const importMessagesVi: ImportMessages = {
  page: {
    title: 'Import dữ liệu',
    description: 'Chọn file để xem và kiểm tra kết quả.',
    backLabel: 'Quay lại',
    fileLabel: 'Upload File',
    confirmLabel: 'Xác nhận',
    cancelLabel: 'Hủy',
    validatingLabel: 'Đang kiểm tra file…',
    committingLabel: 'Đang import…',
    resultTitle: 'Kết quả kiểm tra',
    processedLabel: 'Đã xử lý',
    validLabel: 'Hợp lệ',
    invalidLabel: 'Không hợp lệ',
    validSectionTitle: 'DỮ LIỆU HỢP LỆ',
    invalidSectionTitle: 'DỮ LIỆU KHÔNG HỢP LỆ',
    noInvalidRows: 'Không có dòng lỗi trên sheet này.',
    sheetPrefix: 'Sheet',
    validShort: 'hợp lệ',
    errorShort: 'lỗi',
    colIndex: '#',
    colAge: 'AGE',
    colName: 'NAME',
    colErrors: 'LỖI',
    validateError: 'Kiểm tra file thất bại.',
    commitSuccess: 'Import dữ liệu thành công.',
    commitError: 'Import dữ liệu thất bại.',
    dropzoneTitle: 'Kéo thả file Excel/CSV vào đây',
    dropzoneHint: 'hoặc bấm để chọn từ máy',
  },
}

const importMessagesEn: ImportMessages = {
  page: {
    title: 'Import data',
    description: 'Select a file to preview validation, then confirm or cancel.',
    backLabel: 'Back',
    fileLabel: 'File',
    confirmLabel: 'Confirm',
    cancelLabel: 'Cancel',
    validatingLabel: 'Validating file…',
    committingLabel: 'Importing…',
    resultTitle: 'Validation result',
    processedLabel: 'Processed',
    validLabel: 'Valid',
    invalidLabel: 'Invalid',
    validSectionTitle: 'VALID ROWS',
    invalidSectionTitle: 'INVALID ROWS',
    noInvalidRows: 'No error rows on this sheet.',
    sheetPrefix: 'Sheet',
    validShort: 'valid',
    errorShort: 'errors',
    colIndex: '#',
    colAge: 'AGE',
    colName: 'NAME',
    colErrors: 'ERRORS',
    validateError: 'File validation failed.',
    commitSuccess: 'Data imported successfully.',
    commitError: 'Data import failed.',
    dropzoneTitle: 'Drag and drop Excel/CSV here',
    dropzoneHint: 'or click to browse',
  },
}

export function getImportMessages(locale: ImportLocale = 'vi'): ImportMessages {
  return locale === 'en' ? importMessagesEn : importMessagesVi
}

/** @deprecated Dùng `getImportMessages(locale).page` */
export function getImportPageMessages(locale: ImportLocale = 'vi'): ImportPageMessages {
  return getImportMessages(locale).page
}

export const importMessages = importMessagesVi
