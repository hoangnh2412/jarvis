import type {
  CreateDocumentFieldPayload,
  DocumentField,
  DocumentTemplate,
  DocxPreviewStatus,
  FieldTypeValue,
  PreviewMode,
  UpdateDocumentFieldPayload,
} from './index'

export type CraftDocPreviewControls = {
  zoom: number
  fitMode: 'none' | 'width' | 'page'
  currentPage: number
  totalPages: number
  fullscreen: boolean
  onZoomIn: () => void
  onZoomOut: () => void
  onResetZoom: () => void
  onFitWidth: () => void
  onFitPage: () => void
  onFullscreenToggle: () => void
  onPreviousPage: () => void
  onNextPage: () => void
  onCurrentPageChange: (page: number) => void
  onTotalPagesChange: (pages: number) => void
}

export type CraftDocTemplateDesignControls = {
  filteredFields: DocumentField[]
  selectedFieldId: string | null
  selectedField: DocumentField | null
  fieldSearch: string
  fieldTypeFilter: FieldTypeValue | 'all'
  onSearchChange: (value: string) => void
  onTypeFilterChange: (value: FieldTypeValue | 'all') => void
  onSelectField: (fieldId: string | null) => void
  onUpdateField: (
    fieldId: string,
    patch: UpdateDocumentFieldPayload,
  ) => void | Promise<void>
  onAddField: (payload: CreateDocumentFieldPayload) => void | Promise<DocumentField | null>
  onDeleteField: (fieldId: string) => void | Promise<void>
}

/** View-model truyền từ page xuống builder / slot tuỳ biến. */
export type CraftDocBuilderController = {
  templates: DocumentTemplate[]
  activeTemplateId: string | null
  activeTemplate: DocumentTemplate | null
  /** @deprecated Dùng activeTemplate.fields[].value */
  sampleData: Record<string, string>
  fields: DocumentField[]
  previewBlob: Blob | null
  previewMode: PreviewMode
  status: DocxPreviewStatus
  errorMessage: string | null
  dirty: boolean
  saving: boolean
  designDrawerOpen: boolean
  templatePickerOpen: boolean
  preview: CraftDocPreviewControls
  design: CraftDocTemplateDesignControls
  setFieldValue: (fieldName: string, value: string) => void
  setFieldValues: (updates: Record<string, string>) => void
  selectTemplate: (templateId: string) => void
  setDesignDrawerOpen: (open: boolean) => void
  setTemplatePickerOpen: (open: boolean) => void
  onSave: () => void | Promise<void>
  onExport: () => void | Promise<void>
  onUploadFile: (file: File) => Promise<void>
  onCreateTemplate: (name?: string) => Promise<void>
}

export type CraftDocFormPanelProps = {
  locale?: import('../localization').CraftDocLocale
  template: DocumentTemplate | null
  saving?: boolean
  onFieldChange: (fieldName: string, value: string) => void
  /** Cập nhật nhiều field cùng lúc (vd. day/month/year từ date picker gộp). */
  onFieldValuesChange?: (updates: Record<string, string>) => void
  onUpload: () => void
  onCreateTemplate: () => void
  onOpenDesign: () => void
  onOpenTemplatePicker: () => void
  onSave: () => void
  onExport: () => void
}

export type CraftDocPreviewPanelProps = {
  locale?: import('../localization').CraftDocLocale
  template: DocumentTemplate | null
  fields: DocumentField[]
  previewBlob: Blob | null
  status: DocxPreviewStatus
  preview: CraftDocPreviewControls
  onChangeTemplate: () => void
}

export type CraftDocTemplateDesignDrawerProps = {
  locale?: import('../localization').CraftDocLocale
  open: boolean
  onClose: () => void
  template: DocumentTemplate | null
  design: CraftDocTemplateDesignControls
}
