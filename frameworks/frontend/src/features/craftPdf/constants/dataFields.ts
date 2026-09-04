import type { CraftPdfDataField } from '../types'

/**
 * Fake data fields — sau này thay bằng API (schema / sample JSON).
 * Binding expression dạng `{{data.company_name}}`.
 */
export const FAKE_CRAFT_PDF_DATA_FIELDS: CraftPdfDataField[] = [
  {
    key: 'company_name',
    label: 'Company name',
    type: 'string',
    sample: 'Northwind Logistics',
    path: 'data.company_name',
    group: 'Company',
  },
  {
    key: 'company_address',
    label: 'Company address',
    type: 'string',
    sample: '128 Harbor Ave, Seattle',
    path: 'data.company_address',
    group: 'Company',
  },
  {
    key: 'email',
    label: 'Email',
    type: 'string',
    sample: 'ops@northwind.example',
    path: 'data.email',
    group: 'Company',
  },
  {
    key: 'phone',
    label: 'Phone',
    type: 'string',
    sample: '+1 (206) 555-0148',
    path: 'data.phone',
    group: 'Company',
  },
  {
    key: 'customer_name',
    label: 'Customer name',
    type: 'string',
    sample: 'Acme Retail Co.',
    path: 'data.customer_name',
    group: 'Customer',
  },
  {
    key: 'customer_address',
    label: 'Customer address',
    type: 'string',
    sample: '42 Market Street, Portland',
    path: 'data.customer_address',
    group: 'Customer',
  },
  {
    key: 'invoice_number',
    label: 'Invoice number',
    type: 'string',
    sample: 'INV-2026-0842',
    path: 'data.invoice_number',
    group: 'Document',
  },
  {
    key: 'order_date',
    label: 'Order date',
    type: 'date',
    sample: '2026-07-12',
    path: 'data.order_date',
    group: 'Document',
  },
  {
    key: 'tracking_code',
    label: 'Tracking code',
    type: 'string',
    sample: '1Z999AA10123456784',
    path: 'data.tracking_code',
    group: 'Shipping',
  },
  {
    key: 'qty_ordered',
    label: 'Qty ordered',
    type: 'number',
    sample: '12',
    path: 'row.qty_ordered',
    group: 'Line item',
  },
  {
    key: 'qty_shipped',
    label: 'Qty shipped',
    type: 'number',
    sample: '10',
    path: 'row.qty_shipped',
    group: 'Line item',
  },
  {
    key: 'description',
    label: 'Item description',
    type: 'string',
    sample: 'Corrugated carton 40x30',
    path: 'row.description',
    group: 'Line item',
  },
  {
    key: 'line_items',
    label: 'Line items',
    type: 'array',
    sample: '[{...}]',
    path: 'data.line_items',
    group: 'Collections',
  },
]

export function getFakeCraftPdfDataFields(): CraftPdfDataField[] {
  return FAKE_CRAFT_PDF_DATA_FIELDS
}

export function toBindingExpression(field: CraftPdfDataField): string {
  return `{{${field.path ?? `data.${field.key}`}}}`
}
