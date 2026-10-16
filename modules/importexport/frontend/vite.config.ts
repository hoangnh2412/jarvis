import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const root = path.dirname(fileURLToPath(import.meta.url))
const nm = (...segs: string[]) => path.resolve(root, 'node_modules', ...segs)

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    dedupe: [
      'react',
      'react-dom',
      'react-router-dom',
      'react-hook-form',
      '@primereact/core',
      '@primereact/headless',
      '@primeuix/themes',
      '@primeuix/styled',
    ],
    alias: {
      '@platform/importexport': path.resolve(root, 'src'),
      react: nm('react'),
      'react-dom': nm('react-dom'),
      'react-router-dom': nm('react-router-dom'),
      'react-hook-form': nm('react-hook-form'),
      '@primereact/core': nm('@primereact/core'),
      '@primereact/headless': nm('@primereact/headless'),
    },
  },
  optimizeDeps: {
    include: [
      '@primereact/core',
      '@primeuix/themes',
      '@primeuix/themes/aura',
      '@primeuix/styled',
    ],
  },
  server: {
    port: 5174,
    strictPort: true,
    proxy: {
      '/ie-api': {
        target: 'http://localhost:5251',
        changeOrigin: true,
        rewrite: (p) => p.replace(/^\/ie-api/, ''),
      },
    },
  },
})
