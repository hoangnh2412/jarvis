import { definePreset } from '@primeuix/themes'
import Aura from '@primeuix/themes/aura'

/**
 * Aura + primary teal (đủ token color / contrast / hover / active).
 */
export const KitAura = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{teal.50}',
      100: '{teal.100}',
      200: '{teal.200}',
      300: '{teal.300}',
      400: '{teal.400}',
      500: '{teal.500}',
      600: '{teal.600}',
      700: '{teal.700}',
      800: '{teal.800}',
      900: '{teal.900}',
      950: '{teal.950}',
      color: 'light-dark({primary.500}, {primary.400})',
      contrastColor: 'light-dark(#ffffff, {surface.900})',
      hoverColor: 'light-dark({primary.600}, {primary.300})',
      activeColor: 'light-dark({primary.700}, {primary.200})',
    },
  },
})

/**
 * Theme Provider — cssLayer để utilities Tailwind thắng style theme khi cần.
 * Form controls / buttons trong kit dùng `unstyled` + Tailwind className.
 */
export const kitPrimeReactTheme = {
  preset: KitAura,
  options: {
    prefix: 'p',
    darkModeSelector: 'none',
    cssLayer: {
      name: 'primereact',
      order: 'theme, base, primereact, utilities',
    },
  },
}

/** `<PrimeReactProvider {...kitPrimeReactConfig}>` */
export const kitPrimeReactConfig = {
  theme: kitPrimeReactTheme,
}
