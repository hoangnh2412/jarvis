import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  return {
    plugins: [react(), tailwindcss()],
    resolve: {
      dedupe: ['react', 'react-dom', 'primereact', '@primereact/core'],
    },
    server: {
      proxy: {
        '/api/notifications': {
          target: env.NOTIFICATION_API_URL || 'http://localhost:5167',
          changeOrigin: true,
          secure: false,
        },
        '/hubs/notifications': {
          target: env.NOTIFICATION_API_URL || 'http://localhost:5167',
          changeOrigin: true,
          secure: false,
          ws: true,
        },
      },
    },
  }
})
