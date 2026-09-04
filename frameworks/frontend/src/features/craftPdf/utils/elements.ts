import { PDF_PAGE_A4 } from '../constants'
import {
  PdfElementType,
  PdfTemplateStatus,
  type PdfElement,
  type PdfElementTypeValue,
  type PdfTemplate,
} from '../types'

let seq = 0

export function createElementId(prefix = 'el') {
  seq += 1
  return `${prefix}-${Date.now().toString(36)}-${seq}`
}

export function createDefaultElement(
  type: PdfElementTypeValue,
  overrides?: Partial<PdfElement>,
): PdfElement {
  const id = createElementId(type)
  const base = {
    id,
    x: 40,
    y: 40,
    zIndex: 1,
    locked: false,
    visible: true,
    rotation: 0,
  }

  switch (type) {
    case PdfElementType.Text:
      return {
        ...base,
        type: PdfElementType.Text,
        name: 'Text',
        width: 220,
        height: 36,
        content: 'Sample text',
        fontFamily: 'Manrope, system-ui, sans-serif',
        fontSize: 14,
        fontWeight: 'normal',
        fontStyle: 'normal',
        textDecoration: 'none',
        color: '#0f172a',
        backgroundColor: 'transparent',
        align: 'left',
        bindingKey: '',
        ...overrides,
      } as PdfElement
    case PdfElementType.RichText:
      return {
        ...base,
        type: PdfElementType.RichText,
        name: 'Rich Text',
        width: 280,
        height: 72,
        html: '<p>Rich text content</p>',
        fontSize: 14,
        color: '#0f172a',
        bindingKey: '',
        ...overrides,
      } as PdfElement
    case PdfElementType.Image:
      return {
        ...base,
        type: PdfElementType.Image,
        name: 'Image',
        width: 160,
        height: 120,
        src: '',
        alt: 'Image',
        objectFit: 'contain',
        bindingKey: '',
        ...overrides,
      } as PdfElement
    case PdfElementType.Table:
      return {
        ...base,
        type: PdfElementType.Table,
        name: 'Table',
        width: 400,
        height: 140,
        columns: 3,
        rows: 3,
        headerLabel: 'Table',
        bindingKey: '',
        ...overrides,
      } as PdfElement
    case PdfElementType.QrCode:
      return {
        ...base,
        type: PdfElementType.QrCode,
        name: 'QR Code',
        width: 96,
        height: 96,
        value: 'https://example.com',
        bindingKey: '',
        ...overrides,
      } as PdfElement
    case PdfElementType.Barcode:
      return {
        ...base,
        type: PdfElementType.Barcode,
        name: 'Barcode',
        width: 180,
        height: 56,
        value: '1234567890',
        bindingKey: '',
        ...overrides,
      } as PdfElement
    case PdfElementType.Shape:
      return {
        ...base,
        type: PdfElementType.Shape,
        name: 'Shape',
        width: 120,
        height: 80,
        shape: 'rect',
        fill: '#ccfbf1',
        stroke: '#0f766e',
        strokeWidth: 1,
        ...overrides,
      } as PdfElement
    case PdfElementType.Chart:
      return {
        ...base,
        type: PdfElementType.Chart,
        name: 'Chart',
        width: 280,
        height: 160,
        chartType: 'bar',
        title: 'Chart',
        bindingKey: '',
        ...overrides,
      } as PdfElement
    default:
      return {
        ...base,
        type: PdfElementType.Text,
        name: 'Text',
        width: 220,
        height: 36,
        content: 'Sample text',
        fontFamily: 'Manrope, system-ui, sans-serif',
        fontSize: 14,
        fontWeight: 'normal',
        fontStyle: 'normal',
        textDecoration: 'none',
        color: '#0f172a',
        align: 'left',
      }
  }
}

const now = () => new Date().toISOString()

export function createMockTemplates(): PdfTemplate[] {
  const t = now()
  return [
    {
      id: 'tpl-invoice',
      name: 'Complex invoice',
      description: 'Invoice với bảng và QR — sample CraftMyPDF-style',
      status: PdfTemplateStatus.Published,
      pageSize: PDF_PAGE_A4,
      updatedAt: t,
      createdAt: t,
      elements: [
        createDefaultElement(PdfElementType.Text, {
          id: 'inv-title',
          x: 48,
          y: 40,
          width: 280,
          height: 40,
          content: 'INVOICE',
          fontSize: 28,
          fontWeight: 'bold',
          bindingKey: '',
        } as Partial<PdfElement>),
        createDefaultElement(PdfElementType.Text, {
          id: 'inv-number',
          x: 48,
          y: 90,
          width: 240,
          height: 28,
          content: '{{data.invoice_number}}',
          fontSize: 14,
          bindingKey: 'invoice_number',
        } as Partial<PdfElement>),
        createDefaultElement(PdfElementType.Table, {
          id: 'inv-table',
          x: 48,
          y: 160,
          width: 700,
          height: 200,
          bindingKey: 'line_items',
        } as Partial<PdfElement>),
        createDefaultElement(PdfElementType.QrCode, {
          id: 'inv-qr',
          x: 650,
          y: 40,
          width: 96,
          height: 96,
          bindingKey: 'qr_payload',
        } as Partial<PdfElement>),
      ],
    },
    {
      id: 'tpl-certificate',
      name: 'Certificate Sample',
      description: 'Chứng nhận hiện đại — layout trống để customize',
      status: PdfTemplateStatus.Draft,
      pageSize: PDF_PAGE_A4,
      updatedAt: t,
      createdAt: t,
      elements: [
        createDefaultElement(PdfElementType.Text, {
          id: 'cert-title',
          x: 120,
          y: 200,
          width: 550,
          height: 48,
          content: 'Certificate of Completion',
          fontSize: 26,
          fontWeight: 'bold',
          align: 'center',
        } as Partial<PdfElement>),
        createDefaultElement(PdfElementType.Text, {
          id: 'cert-name',
          x: 120,
          y: 280,
          width: 550,
          height: 36,
          content: '{{data.customer_name}}',
          fontSize: 18,
          align: 'center',
          bindingKey: 'customer_name',
        } as Partial<PdfElement>),
      ],
    },
    {
      id: 'tpl-packing',
      name: 'Packing List',
      description: 'Packing list với barcode',
      status: PdfTemplateStatus.Published,
      pageSize: PDF_PAGE_A4,
      updatedAt: t,
      createdAt: t,
      elements: [
        createDefaultElement(PdfElementType.Text, {
          id: 'pack-title',
          content: 'Packing List',
          fontSize: 22,
          fontWeight: 'bold',
        } as Partial<PdfElement>),
        createDefaultElement(PdfElementType.Barcode, {
          id: 'pack-barcode',
          x: 48,
          y: 100,
          bindingKey: 'tracking_code',
        } as Partial<PdfElement>),
      ],
    },
  ]
}

/** In-memory store cho stub services (dev / UI-only) */
let memoryTemplates: PdfTemplate[] | null = null

export function getMemoryTemplates() {
  if (!memoryTemplates) memoryTemplates = createMockTemplates()
  return memoryTemplates
}

export function setMemoryTemplates(next: PdfTemplate[]) {
  memoryTemplates = next
}

export function resetMemoryTemplates() {
  memoryTemplates = createMockTemplates()
}
