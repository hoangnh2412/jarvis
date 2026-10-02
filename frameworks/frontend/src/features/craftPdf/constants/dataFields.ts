import dataFieldsMock from '../mocks/get-data-fields.json'
import type { CraftPdfDataField } from '../types'

/**
 * Fake data fields — nguồn: `mocks/get-data-fields.json`.
 * Binding expression dạng `{{data.company_name}}`.
 */
export const FAKE_CRAFT_PDF_DATA_FIELDS: CraftPdfDataField[] =
  dataFieldsMock.items as CraftPdfDataField[]

export function getFakeCraftPdfDataFields(): CraftPdfDataField[] {
  return FAKE_CRAFT_PDF_DATA_FIELDS.map((f) => ({ ...f }))
}

export function toBindingExpression(field: CraftPdfDataField): string {
  return `{{${field.path ?? `data.${field.key}`}}}`
}
