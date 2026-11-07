import { FAKE_CRAFT_PDF_DATA_FIELDS } from '../constants/dataFields'
import type { CraftPdfDataField, PdfElement } from '../types'
import { PdfElementType } from '../types'

const BINDING_RE = /\{\{\s*([^}]+?)\s*\}\}/g

/** Map path/key → sample string (từ fake fields hoặc schema app truyền vào). */
export function buildCraftPdfSampleMap(
  fields: CraftPdfDataField[] = FAKE_CRAFT_PDF_DATA_FIELDS,
): Record<string, string> {
  const map: Record<string, string> = {}
  for (const field of fields) {
    const sample = field.sample ?? ''
    map[field.key] = sample
    map[`data.${field.key}`] = sample
    if (field.path) map[field.path] = sample
  }
  return map
}

/**
 * Thay `{{data.invoice_number}}` bằng giá trị trong map.
 * Không khớp → giữ nguyên expression.
 */
export function resolveCraftPdfBindings(
  template: string,
  data: Record<string, string> = buildCraftPdfSampleMap(),
): string {
  if (!template.includes('{{')) return template
  return template.replace(BINDING_RE, (_, raw: string) => {
    const path = raw.trim()
    if (Object.prototype.hasOwnProperty.call(data, path)) return data[path]
    const short = path.replace(/^data\./, '')
    if (Object.prototype.hasOwnProperty.call(data, short)) return data[short]
    return `{{${path}}}`
  })
}

/** Clone element với content/html/value đã resolve — dùng cho preview. */
export function resolvePdfElementBindings(
  el: PdfElement,
  data: Record<string, string> = buildCraftPdfSampleMap(),
): PdfElement {
  switch (el.type) {
    case PdfElementType.Text:
      return { ...el, content: resolveCraftPdfBindings(el.content, data) }
    case PdfElementType.RichText:
      return { ...el, html: resolveCraftPdfBindings(el.html, data) }
    case PdfElementType.QrCode:
    case PdfElementType.Barcode:
      return { ...el, value: resolveCraftPdfBindings(el.value, data) }
    case PdfElementType.Image:
      return {
        ...el,
        src: el.src ? resolveCraftPdfBindings(el.src, data) : el.src,
        alt: el.alt ? resolveCraftPdfBindings(el.alt, data) : el.alt,
      }
    case PdfElementType.Chart:
      return { ...el, title: resolveCraftPdfBindings(el.title, data) }
    default:
      return el
  }
}
