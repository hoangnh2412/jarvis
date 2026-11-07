export type DynamicFormLocale = 'vi' | 'en'

export type DynamicFormMessages = {
  list: {
    title: string
    description: string
    create: string
    empty: string
    fields: string
    edit: string
    fill: string
    delete: string
    deleted: string
    error: string
    newFormName: string
  }
  builder: {
    title: string
    description: string
    palette: string
    canvas: string
    properties: string
    save: string
    saved: string
    back: string
    emptyCanvas: string
    formName: string
    formDescription: string
    fieldLabel: string
    fieldKey: string
    fieldRequired: string
    fieldPlaceholder: string
    fieldHelp: string
    fieldDefault: string
    fieldDefaultNone: string
    fieldOptions: string
    optionSetup: string
    optionValue: string
    addOption: string
    removeOption: string
    quickEdit: string
    quickEditHint: string
    sortAlpha: string
    sortAlphaHint: string
    remindEmpty: string
    remindEmptyHint: string
    removeField: string
    collapsePalette: string
    expandPalette: string
    closeProperties: string
    preview: string
    previewOk: string
    error: string
    schemaLive: string
    schemaLiveHint: string
  }
  fill: {
    title: string
    description: string
    submit: string
    submitted: string
    back: string
    empty: string
    error: string
  }
}

export const dynamicFormMessagesVi: DynamicFormMessages = {
  list: {
    title: 'Form động (RJSF)',
    description: 'Kéo-thả thiết kế form — runtime bằng react-jsonschema-form.',
    create: 'Tạo form',
    empty: 'Chưa có form nào',
    fields: 'field',
    edit: 'Thiết kế',
    fill: 'Điền form',
    delete: 'Xóa',
    deleted: 'Đã xóa form',
    error: 'Không tải được danh sách form',
    newFormName: 'Form mới',
  },
  builder: {
    title: 'Thiết kế form',
    description:
      'Playground kiểu RJSF — Designer kéo-thả + JSON Schema live + form PrimeReact.',
    palette: 'Palette',
    canvas: 'Form',
    properties: 'Thuộc tính trường',
    save: 'Lưu form',
    saved: 'Đã lưu form',
    back: 'Quay lại',
    emptyCanvas: 'Chưa có field — thêm từ palette bên trái',
    formName: 'Tên form',
    formDescription: 'Mô tả',
    fieldLabel: 'Tên trường',
    fieldKey: 'Key (name)',
    fieldRequired: 'Bắt buộc',
    fieldPlaceholder: 'Placeholder',
    fieldHelp: 'Gợi ý / mô tả',
    fieldDefault: 'Giá trị mặc định',
    fieldDefaultNone: '— Không chọn —',
    fieldOptions: 'Giá trị',
    optionSetup: 'Thiết lập',
    optionValue: 'Value (code)',
    addOption: 'Thêm giá trị',
    removeOption: 'Xóa giá trị',
    quickEdit: 'Thêm/Sửa nhanh giá trị',
    quickEditHint: 'Cho phép bổ sung option khi điền form.',
    sortAlpha: 'Sắp xếp A–Z',
    sortAlphaHint: 'Tự sắp xếp options khi render.',
    remindEmpty: 'Nhắc khi để trống',
    remindEmptyHint: 'Coi như bắt buộc khi validate (required trong schema).',
    removeField: 'Xóa field',
    collapsePalette: 'Thu gọn palette',
    expandPalette: 'Mở rộng palette',
    closeProperties: 'Đóng thuộc tính',
    preview: 'Xem trước',
    previewOk: 'Preview OK',
    error: 'Không tải / lưu được form',
    schemaLive: 'JSON Schema (live)',
    schemaLiveHint: 'Tự sinh khi bạn thêm / sửa / sắp xếp field trên canvas.',
  },
  fill: {
    title: 'Điền form',
    description: 'Render bằng react-jsonschema-form',
    submit: 'Gửi',
    submitted: 'Đã gửi form',
    back: 'Quay lại',
    empty: 'Form chưa có field',
    error: 'Không gửi được form',
  },
}

export const dynamicFormMessagesEn: DynamicFormMessages = {
  list: {
    title: 'Dynamic Form (RJSF)',
    description: 'Drag-and-drop designer — runtime powered by react-jsonschema-form.',
    create: 'Create form',
    empty: 'No forms yet',
    fields: 'fields',
    edit: 'Design',
    fill: 'Fill form',
    delete: 'Delete',
    deleted: 'Form deleted',
    error: 'Failed to load forms',
    newFormName: 'New form',
  },
  builder: {
    title: 'Form designer',
    description:
      'RJSF-style playground — drag-and-drop Designer, live schema, PrimeReact form.',
    palette: 'Palette',
    canvas: 'Form',
    properties: 'Field properties',
    save: 'Save form',
    saved: 'Form saved',
    back: 'Back',
    emptyCanvas: 'No fields yet — add from the palette',
    formName: 'Form name',
    formDescription: 'Description',
    fieldLabel: 'Label',
    fieldKey: 'Key (name)',
    fieldRequired: 'Required',
    fieldPlaceholder: 'Placeholder',
    fieldHelp: 'Help text',
    fieldDefault: 'Default value',
    fieldDefaultNone: '— None —',
    fieldOptions: 'Options',
    optionSetup: 'Setup',
    optionValue: 'Value (code)',
    addOption: 'Add option',
    removeOption: 'Remove option',
    quickEdit: 'Quick edit options',
    quickEditHint: 'Allow adding options while filling the form.',
    sortAlpha: 'Sort A–Z',
    sortAlphaHint: 'Sort options alphabetically when rendering.',
    remindEmpty: 'Remind when empty',
    remindEmptyHint: 'Treat as required in schema validation.',
    removeField: 'Remove field',
    collapsePalette: 'Collapse palette',
    expandPalette: 'Expand palette',
    closeProperties: 'Close properties',
    preview: 'Preview',
    previewOk: 'Preview OK',
    error: 'Failed to load / save form',
    schemaLive: 'JSON Schema (live)',
    schemaLiveHint: 'Auto-generated when you add / edit / reorder canvas fields.',
  },
  fill: {
    title: 'Fill form',
    description: 'Rendered by react-jsonschema-form',
    submit: 'Submit',
    submitted: 'Form submitted',
    back: 'Back',
    empty: 'Form has no fields',
    error: 'Failed to submit form',
  },
}

const messages: Record<DynamicFormLocale, DynamicFormMessages> = {
  vi: dynamicFormMessagesVi,
  en: dynamicFormMessagesEn,
}

export function getDynamicFormMessages(locale: DynamicFormLocale = 'vi') {
  return messages[locale] ?? dynamicFormMessagesVi
}

export { dynamicFormMessagesVi as dynamicFormMessages }
