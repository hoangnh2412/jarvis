export const CRAFT_PDF_ROUTES = {
  list: '/craft-pdf',
  create: '/craft-pdf/create',
  editor: '/craft-pdf/:id/editor',
  preview: '/craft-pdf/:id/preview',
} as const

export type CraftPdfRouteKey = keyof typeof CRAFT_PDF_ROUTES

function withId(pattern: string, id: string) {
  return pattern.replace(':id', id)
}

export function getCraftPdfListPath() {
  return CRAFT_PDF_ROUTES.list
}

export function getCraftPdfCreatePath() {
  return CRAFT_PDF_ROUTES.create
}

export function getCraftPdfEditorPath(id: string) {
  return withId(CRAFT_PDF_ROUTES.editor, id)
}

export function getCraftPdfPreviewPath(id: string) {
  return withId(CRAFT_PDF_ROUTES.preview, id)
}
