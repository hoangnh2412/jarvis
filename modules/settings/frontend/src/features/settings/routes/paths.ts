export const SETTING_ROUTES = {
  home: '/settings',
} as const

export type SettingRouteKey = keyof typeof SETTING_ROUTES

export function getSettingHomePath() {
  return SETTING_ROUTES.home
}
