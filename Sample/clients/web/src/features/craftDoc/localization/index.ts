export type CraftDocMessages = {
  page: {
    title: string
    description: string
    empty: string
    error: string
    saved: string
    exported: string
    uploaded: string
    loading: string
    previewError: string
  }
  form: {
    title: string
    changeTemplate: string
    upload: string
    create: string
    editTemplate: string
    save: string
    export: string
    empty: string
    saved: string
    invalidDocx: string
  }
  preview: {
    title: string
    changeTemplate: string
    empty: string
    loading: string
    error: string
    tabPreview: string
    tabRequestJson: string
    requestJson: {
      request: string
      fields: string
      values: string
      copy: string
      copied: string
      copyFailed: string
    }
  }
  design: {
    title: string
    fieldAdded: string
    fieldDeleted: string
  }
  layout: {
    resizePanel: string
    sidebarWidth: string
  }
}

export type CraftDocLocale = 'en' | 'vi'

export const craftDocMessagesEn: CraftDocMessages = {
  page: {
    title: 'DOCX Template Builder',
    description: 'Upload templates, fill placeholders, preview and export filled documents.',
    empty: 'No template selected.',
    error: 'Something went wrong.',
    saved: 'Document saved.',
    exported: 'Document exported.',
    uploaded: 'Template uploaded.',
    loading: 'Loading document…',
    previewError: 'Unable to preview this document.',
  },
  form: {
    title: 'DOCX document',
    changeTemplate: 'Change template',
    upload: 'Upload',
    create: 'New',
    editTemplate: 'Edit template',
    save: 'Save',
    export: 'Export',
    empty: 'Select or upload a DOCX template.',
    saved: 'Document saved.',
    invalidDocx: 'Please upload a valid .docx file.',
  },
  preview: {
    title: 'Preview',
    changeTemplate: 'Change template',
    empty: 'No template yet.',
    loading: 'Loading document…',
    error: 'Unable to preview document.',
    tabPreview: 'Preview',
    tabRequestJson: 'Request JSON',
    requestJson: {
      request: 'Request',
      fields: 'Fields',
      values: 'Values',
      copy: 'Copy',
      copied: 'Copied',
      copyFailed: 'Copy failed',
    },
  },
  design: {
    title: 'Edit template',
    fieldAdded: 'Field added.',
    fieldDeleted: 'Field deleted.',
  },
  layout: {
    resizePanel: 'Drag to resize form and preview panels',
    sidebarWidth: 'Form panel width: {width} pixels',
  },
}

export const craftDocMessagesVi: CraftDocMessages = {
  page: {
    title: 'Trình tạo mẫu DOCX',
    description: 'Tải mẫu, điền placeholder, xem trước và xuất tài liệu đã điền.',
    empty: 'Chưa chọn mẫu.',
    error: 'Đã xảy ra lỗi.',
    saved: 'Đã lưu tài liệu.',
    exported: 'Đã xuất tài liệu.',
    uploaded: 'Đã tải mẫu lên.',
    loading: 'Đang tải tài liệu…',
    previewError: 'Không thể xem trước tài liệu.',
  },
  form: {
    title: 'Tài liệu DOCX',
    changeTemplate: 'Đổi mẫu',
    upload: 'Tải lên',
    create: 'Tạo mới',
    editTemplate: 'Sửa mẫu',
    save: 'Lưu',
    export: 'Xuất',
    empty: 'Chọn hoặc tải mẫu DOCX.',
    saved: 'Đã lưu tài liệu.',
    invalidDocx: 'Vui lòng tải lên file .docx hợp lệ.',
  },
  preview: {
    title: 'Xem trước',
    changeTemplate: 'Đổi mẫu',
    empty: 'Chưa có mẫu tài liệu.',
    loading: 'Đang tải tài liệu…',
    error: 'Không thể xem trước tài liệu.',
    tabPreview: 'Xem trước',
    tabRequestJson: 'Request JSON',
    requestJson: {
      request: 'Request',
      fields: 'Fields',
      values: 'Values',
      copy: 'Sao chép',
      copied: 'Đã sao chép',
      copyFailed: 'Sao chép thất bại',
    },
  },
  design: {
    title: 'Chỉnh sửa mẫu',
    fieldAdded: 'Đã thêm field.',
    fieldDeleted: 'Đã xóa field.',
  },
  layout: {
    resizePanel: 'Kéo để đổi độ rộng form và xem trước',
    sidebarWidth: 'Độ rộng panel form: {width}px',
  },
}

const messages: Record<CraftDocLocale, CraftDocMessages> = {
  en: craftDocMessagesEn,
  vi: craftDocMessagesVi,
}

export function getCraftDocMessages(locale: CraftDocLocale = 'vi'): CraftDocMessages {
  return messages[locale] ?? craftDocMessagesVi
}

/** @deprecated Dùng `getCraftDocMessages(locale)` */
export { craftDocMessagesVi as craftDocMessages }
