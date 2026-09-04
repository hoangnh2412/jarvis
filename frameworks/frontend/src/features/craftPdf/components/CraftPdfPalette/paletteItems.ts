import {
  BarChart3,
  Circle,
  FileText,
  Image as ImageIcon,
  Minus,
  QrCode,
  ScanBarcode,
  Square,
  Table2,
  Type,
  type LucideIcon,
} from 'lucide-react'
import {
  PdfElementType,
  type CraftPdfDataField,
  type PdfElement,
  type PdfElementTypeValue,
} from '../../types'

export type CraftPdfPaletteCategory = 'standard' | 'form' | 'signature'

export type CraftPdfPaletteItem = {
  id: string
  label: string
  category: CraftPdfPaletteCategory
  type: PdfElementTypeValue
  icon: LucideIcon
  defaults?: Partial<PdfElement>
  /** true = hiện trong palette nhưng chưa tạo element (UI only) */
  soon?: boolean
}

export const CRAFT_PDF_PALETTE_ITEMS: CraftPdfPaletteItem[] = [
  {
    id: 'text',
    label: 'Text',
    category: 'standard',
    type: PdfElementType.Text,
    icon: Type,
  },
  {
    id: 'image',
    label: 'Image',
    category: 'standard',
    type: PdfElementType.Image,
    icon: ImageIcon,
  },
  {
    id: 'line',
    label: 'Line',
    category: 'standard',
    type: PdfElementType.Shape,
    icon: Minus,
    defaults: {
      shape: 'line',
      width: 220,
      height: 12,
      fill: 'transparent',
      stroke: '#0f172a',
      strokeWidth: 2,
      name: 'Line',
    } as Partial<PdfElement>,
  },
  {
    id: 'rect',
    label: 'Rect',
    category: 'standard',
    type: PdfElementType.Shape,
    icon: Square,
    defaults: {
      shape: 'rect',
      name: 'Rect',
    } as Partial<PdfElement>,
  },
  {
    id: 'circle',
    label: 'Circle',
    category: 'standard',
    type: PdfElementType.Shape,
    icon: Circle,
    defaults: {
      shape: 'ellipse',
      width: 100,
      height: 100,
      name: 'Circle',
    } as Partial<PdfElement>,
  },
  {
    id: 'qrcode',
    label: 'QRCode',
    category: 'standard',
    type: PdfElementType.QrCode,
    icon: QrCode,
  },
  {
    id: 'barcode',
    label: 'Barcode',
    category: 'standard',
    type: PdfElementType.Barcode,
    icon: ScanBarcode,
  },
  {
    id: 'chart',
    label: 'Chart',
    category: 'standard',
    type: PdfElementType.Chart,
    icon: BarChart3,
  },
  {
    id: 'richtext',
    label: 'HTML / Rich Text',
    category: 'standard',
    type: PdfElementType.RichText,
    icon: FileText,
  },
  {
    id: 'table',
    label: 'Table',
    category: 'standard',
    type: PdfElementType.Table,
    icon: Table2,
  },
]

export const CRAFT_PDF_DROP_MIME = 'application/x-craft-pdf-payload'

export function groupDataFields(fields: CraftPdfDataField[]) {
  const map = new Map<string, CraftPdfDataField[]>()
  for (const field of fields) {
    const group = field.group ?? 'Fields'
    const list = map.get(group) ?? []
    list.push(field)
    map.set(group, list)
  }
  return Array.from(map.entries())
}
