export const CRAFT_DOC_ROUTES = {
  builder: '/craft-doc',
} as const

export type CraftDocRouteKey = keyof typeof CRAFT_DOC_ROUTES

export function getCraftDocBuilderPath() {
  return CRAFT_DOC_ROUTES.builder
}
