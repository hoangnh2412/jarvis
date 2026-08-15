import type { PdfPageSize } from '../types'

export { CRAFT_PDF_ROUTES } from '../routes/paths'

/** A4 @ 96 DPI — bề mặt thiết kế DOM */
export const PDF_PAGE_A4: PdfPageSize = {
  width: 794,
  height: 1123,
  label: 'A4',
}

export const PDF_PAGE_SIZES: PdfPageSize[] = [
  PDF_PAGE_A4,
  { width: 612, height: 792, label: 'Letter' },
  { width: 420, height: 595, label: 'A5' },
]

export const PDF_ZOOM_MIN = 0.5
export const PDF_ZOOM_MAX = 1.5
export const PDF_ZOOM_STEP = 0.1
export const PDF_ZOOM_DEFAULT = 1

export {
  FAKE_CRAFT_PDF_DATA_FIELDS,
  getFakeCraftPdfDataFields,
  toBindingExpression,
} from './dataFields'

/** @deprecated Use shared `BASE_URL` from `@jarvis/core` / `lib/http` */
export { BASE_URL as BASE_URL_CRAFT_PDF } from '../../../lib/http/constants'
