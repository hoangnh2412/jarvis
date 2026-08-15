/** Element kinds — palette CraftMyPDF-style */
export const PdfElementType = {
  Text: 'text',
  RichText: 'richtext',
  Image: 'image',
  Table: 'table',
  QrCode: 'qrcode',
  Barcode: 'barcode',
  Shape: 'shape',
  Chart: 'chart',
} as const

export type PdfElementTypeValue =
  (typeof PdfElementType)[keyof typeof PdfElementType]

export const PDF_ELEMENT_TYPE_LABEL: Record<PdfElementTypeValue, string> = {
  [PdfElementType.Text]: 'Text',
  [PdfElementType.RichText]: 'Rich Text',
  [PdfElementType.Image]: 'Image',
  [PdfElementType.Table]: 'Table',
  [PdfElementType.QrCode]: 'QR Code',
  [PdfElementType.Barcode]: 'Barcode',
  [PdfElementType.Shape]: 'Shape',
  [PdfElementType.Chart]: 'Chart',
}

export const PDF_ELEMENT_TYPE_OPTIONS = (
  Object.values(PdfElementType) as PdfElementTypeValue[]
).map((value) => ({
  value,
  label: PDF_ELEMENT_TYPE_LABEL[value],
}))

export const PdfTemplateStatus = {
  Draft: 'draft',
  Published: 'published',
} as const

export type PdfTemplateStatusValue =
  (typeof PdfTemplateStatus)[keyof typeof PdfTemplateStatus]

export const PDF_TEMPLATE_STATUS_LABEL: Record<PdfTemplateStatusValue, string> =
  {
    [PdfTemplateStatus.Draft]: 'Nháp',
    [PdfTemplateStatus.Published]: 'Đã xuất bản',
  }

export type PdfElementBase = {
  id: string
  type: PdfElementTypeValue
  name?: string
  x: number
  y: number
  width: number
  height: number
  /** Binding field — backend map JSON sau, UI chỉ giữ placeholder */
  bindingKey?: string
  zIndex?: number
  locked?: boolean
  visible?: boolean
  rotation?: number
}

export type PdfTextElement = PdfElementBase & {
  type: typeof PdfElementType.Text
  content: string
  fontFamily: string
  fontSize: number
  fontWeight: 'normal' | 'bold'
  fontStyle: 'normal' | 'italic'
  textDecoration: 'none' | 'underline'
  color: string
  backgroundColor?: string
  align: 'left' | 'center' | 'right'
}

export type PdfRichTextElement = PdfElementBase & {
  type: typeof PdfElementType.RichText
  /** HTML từ Quill */
  html: string
  fontSize: number
  color: string
}

export type PdfImageElement = PdfElementBase & {
  type: typeof PdfElementType.Image
  src?: string
  alt?: string
  objectFit: 'contain' | 'cover' | 'fill'
}

export type PdfTableElement = PdfElementBase & {
  type: typeof PdfElementType.Table
  columns: number
  rows: number
  headerLabel: string
}

export type PdfQrCodeElement = PdfElementBase & {
  type: typeof PdfElementType.QrCode
  value: string
}

export type PdfBarcodeElement = PdfElementBase & {
  type: typeof PdfElementType.Barcode
  value: string
}

export type PdfShapeKind = 'rect' | 'ellipse' | 'line'

export type PdfShapeElement = PdfElementBase & {
  type: typeof PdfElementType.Shape
  shape: PdfShapeKind
  fill: string
  stroke: string
  strokeWidth: number
}

export type PdfChartElement = PdfElementBase & {
  type: typeof PdfElementType.Chart
  chartType: 'bar' | 'line' | 'pie'
  title: string
}

export type PdfElement =
  | PdfTextElement
  | PdfRichTextElement
  | PdfImageElement
  | PdfTableElement
  | PdfQrCodeElement
  | PdfBarcodeElement
  | PdfShapeElement
  | PdfChartElement

export type PdfPageSize = {
  width: number
  height: number
  label: string
}

export type PdfTemplate = {
  id: string
  name: string
  description?: string
  status: PdfTemplateStatusValue
  pageSize: PdfPageSize
  elements: PdfElement[]
  thumbnailUrl?: string
  updatedAt: string
  createdAt: string
}

export type CreatePdfTemplatePayload = {
  name: string
  description?: string
  pageSize?: PdfPageSize
  elements?: PdfElement[]
}

export type UpdatePdfTemplatePayload = {
  name?: string
  description?: string
  status?: PdfTemplateStatusValue
  pageSize?: PdfPageSize
  elements?: PdfElement[]
}

export type GetPdfTemplateListParams = {
  search?: string
  status?: PdfTemplateStatusValue | 'all'
  page?: number
  size?: number
}

export type PdfTemplateListResult = {
  items: PdfTemplate[]
  total: number
  page: number
  size: number
}

/** Request generate — data JSON do backend cung cấp sau */
export type GeneratePdfRequest = {
  templateId: string
  data?: Record<string, unknown>
  outputFile?: string
}

export type GeneratePdfResponse = {
  fileUrl?: string
  /** Binary preview khi app tự fetch blob */
  blobUrl?: string
}

export type CraftPdfSubmitHandler<T> = (data: T) => void | Promise<void>

/** Data field schema — fake hiện tại, sau lấy từ API */
export type CraftPdfDataField = {
  key: string
  label: string
  type: 'string' | 'number' | 'boolean' | 'date' | 'array' | 'object'
  sample?: string
  path?: string
  group?: string
}

/** Payload khi kéo component / data field vào canvas */
export type CraftPdfDropPayload =
  | {
      kind: 'component'
      type: PdfElementTypeValue
      defaults?: Partial<PdfElement>
    }
  | {
      kind: 'dataField'
      field: CraftPdfDataField
    }
