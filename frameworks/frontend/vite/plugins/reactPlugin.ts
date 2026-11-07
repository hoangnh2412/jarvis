import react from '@vitejs/plugin-react'
import type { Plugin } from 'vite'

/** JSX và Fast Refresh. Host gọi plugin này, không import `@vitejs/plugin-react`. */
export function reactPlugin(): Plugin[] {
  return react()
}
