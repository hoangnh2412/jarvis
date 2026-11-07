export const FieldType = {
  Text: 'text',
  Number: 'number',
  Date: 'date',
  Currency: 'currency',
  Textarea: 'textarea',
  Select: 'select',
  Checkbox: 'checkbox',
} as const

export type FieldTypeValue = (typeof FieldType)[keyof typeof FieldType]

export const FIELD_TYPE_LABEL: Record<FieldTypeValue, string> = {
  text: 'Text',
  number: 'Number',
  date: 'Date',
  currency: 'Currency',
  textarea: 'Textarea',
  select: 'Select',
  checkbox: 'Checkbox',
}

export const FIELD_TYPE_OPTIONS: { label: string; value: FieldTypeValue }[] = (
  Object.entries(FIELD_TYPE_LABEL) as [FieldTypeValue, string][]
).map(([value, label]) => ({ value, label }))

export type DocumentField = {
  id: string
  name: string
  label: string
  type: FieldTypeValue
  placeholder: string
  required: boolean
  /** Giá trị người dùng điền — nguồn chính thay cho sampleData phẳng. */
  value?: string
  defaultValue?: string
  description?: string
  group?: string
  placeholderText?: string
  options?: { label: string; value: string }[]
}

export type DocumentTemplate = {
  id: string
  name: string
  fileName: string
  file: Blob
  fields: DocumentField[]
  updatedAt?: string
}

export type PreviewMode = 'template' | 'filled'

export type DocxPreviewStatus = 'idle' | 'loading' | 'parsing' | 'ready' | 'error'

export type CraftDocBuilderState = {
  templates: DocumentTemplate[]
  activeTemplateId: string | null
  activeTemplate: DocumentTemplate | null
  selectedFieldId: string | null
  selectedField: DocumentField | null
  previewMode: PreviewMode
  previewBlob: Blob | null
  /** @deprecated Dùng activeTemplate.fields[].value */
  sampleData: Record<string, string>
  zoom: number
  fitMode: 'none' | 'width' | 'page'
  currentPage: number
  totalPages: number
  fullscreen: boolean
  status: DocxPreviewStatus
  errorMessage: string | null
  dirty: boolean
  fieldSearch: string
  fieldTypeFilter: FieldTypeValue | 'all'
  leftCollapsed: boolean
  rightCollapsed: boolean
}

export type CreateDocumentFieldPayload = {
  name: string
  label: string
  type: FieldTypeValue
  required?: boolean
  defaultValue?: string
  description?: string
  group?: string
  placeholderText?: string
  options?: { label: string; value: string }[]
}

export type UpdateDocumentFieldPayload = Partial<
  Omit<DocumentField, 'id' | 'placeholder'>
> & {
  placeholder?: string
}

export type {
  CraftDocBuilderController,
  CraftDocPreviewControls,
  CraftDocTemplateDesignControls,
  CraftDocFormPanelProps,
  CraftDocPreviewPanelProps,
  CraftDocTemplateDesignDrawerProps,
} from './view'
