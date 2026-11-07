import type { SettingFieldOption } from '../types'

/**
 * CSS animation presets — mostly `tw-animate-css` class combos,
 * plus a few premium named classes from the kit bridge.
 */
export const DASHBOARD_CHART_ANIMATION_NONE = 'none'

export const DASHBOARD_CHART_ANIMATION_OPTIONS: SettingFieldOption[] = [
  { label: 'Không', value: DASHBOARD_CHART_ANIMATION_NONE },

  // —— Fade ——
  {
    label: 'Fade · Soft',
    value: 'animate-in fade-in duration-700 ease-out-quart',
  },
  {
    label: 'Fade · Quick',
    value: 'animate-in fade-in duration-300 ease-out-quart',
  },
  {
    label: 'Fade · Slow',
    value: 'animate-in fade-in duration-1000 ease-out-expo',
  },

  // —— Soft zoom / pop ——
  {
    label: 'Pop · Soft',
    value: 'animate-in fade-in zoom-in-95 duration-500 ease-out-expo',
  },
  {
    label: 'Pop · Medium',
    value: 'animate-in fade-in zoom-in-90 duration-500 ease-out-expo',
  },
  {
    label: 'Pop · Strong',
    value: 'animate-in fade-in zoom-in-50 duration-600 ease-out-expo',
  },
  {
    label: 'Zoom from zero',
    value: 'animate-in fade-in zoom-in duration-600 ease-out-expo',
  },

  // —— Rise / drop ——
  {
    label: 'Rise · Soft',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-bottom-4 duration-600 ease-out-expo',
  },
  {
    label: 'Rise · Strong',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-bottom-8 duration-700 ease-out-expo',
  },
  {
    label: 'Rise · Full',
    value:
      'animate-in fade-in slide-in-from-bottom duration-700 ease-out-expo',
  },
  {
    label: 'Drop · Soft',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-top-4 duration-600 ease-out-expo',
  },
  {
    label: 'Drop · Strong',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-top-8 duration-700 ease-out-expo',
  },
  {
    label: 'Drop · Full',
    value: 'animate-in fade-in slide-in-from-top duration-700 ease-out-expo',
  },

  // —— Horizontal ——
  {
    label: 'Slide left · Soft',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-right-4 duration-600 ease-out-expo',
  },
  {
    label: 'Slide left · Strong',
    value:
      'animate-in fade-in slide-in-from-right-8 duration-700 ease-out-expo',
  },
  {
    label: 'Slide left · Full',
    value: 'animate-in fade-in slide-in-from-right duration-700 ease-out-expo',
  },
  {
    label: 'Slide right · Soft',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-left-4 duration-600 ease-out-expo',
  },
  {
    label: 'Slide right · Strong',
    value:
      'animate-in fade-in slide-in-from-left-8 duration-700 ease-out-expo',
  },
  {
    label: 'Slide right · Full',
    value: 'animate-in fade-in slide-in-from-left duration-700 ease-out-expo',
  },

  // —— Diagonal ——
  {
    label: 'Diagonal · Top-left',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-top-8 slide-in-from-left-8 duration-700 ease-out-expo',
  },
  {
    label: 'Diagonal · Top-right',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-top-8 slide-in-from-right-8 duration-700 ease-out-expo',
  },
  {
    label: 'Diagonal · Bottom-left',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-bottom-8 slide-in-from-left-8 duration-700 ease-out-expo',
  },
  {
    label: 'Diagonal · Bottom-right',
    value:
      'animate-in fade-in zoom-in-95 slide-in-from-bottom-8 slide-in-from-right-8 duration-700 ease-out-expo',
  },

  // —— Blur reveal ——
  {
    label: 'Blur · Soft',
    value: 'animate-in fade-in blur-in-sm duration-700 ease-out-quart',
  },
  {
    label: 'Blur · Strong',
    value: 'animate-in fade-in blur-in duration-800 ease-out-quart',
  },
  {
    label: 'Blur + Rise',
    value:
      'animate-in fade-in blur-in-sm zoom-in-95 slide-in-from-bottom-4 duration-800 ease-out-expo',
  },
  {
    label: 'Blur + Pop',
    value:
      'animate-in fade-in blur-in-sm zoom-in-90 duration-700 ease-out-expo',
  },

  // —— Tilt / spin ——
  {
    label: 'Tilt in',
    value:
      'animate-in fade-in zoom-in-95 spin-in-12 duration-600 ease-out-expo',
  },
  {
    label: 'Spin soft',
    value:
      'animate-in fade-in zoom-in-90 spin-in duration-700 ease-out-expo',
  },
  {
    label: 'Spin + Rise',
    value:
      'animate-in fade-in zoom-in-95 spin-in-12 slide-in-from-bottom-4 duration-700 ease-out-expo',
  },

  // —— Premium named (kit bridge keyframes) ——
  {
    label: '★ Soft rise',
    value: 'dashboard-anim-soft-rise',
  },
  {
    label: '★ Soft drop',
    value: 'dashboard-anim-soft-drop',
  },
  {
    label: '★ Bounce in',
    value: 'dashboard-anim-bounce-in',
  },
  {
    label: '★ Elastic pop',
    value: 'dashboard-anim-elastic-pop',
  },
  {
    label: '★ Float in',
    value: 'dashboard-anim-float-in',
  },
  {
    label: '★ Reveal wipe',
    value: 'dashboard-anim-reveal-wipe',
  },
]

export const DASHBOARD_CHART_ANIMATION_FIELD_NAME = 'cssAnimation'

export function resolveChartAnimationClass(value: unknown): string {
  if (typeof value !== 'string') return ''
  const trimmed = value.trim()
  if (!trimmed || trimmed === DASHBOARD_CHART_ANIMATION_NONE) return ''
  return trimmed
}
