import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const root = path.dirname(fileURLToPath(import.meta.url))
const nm = (...segs: string[]) => path.resolve(root, 'node_modules', ...segs)
const jarvisCoreSrc = path.resolve(root, '../../../frameworks/frontend/src')
const demoApiKey = 'dev-notifications-demo-key'
const demoTenantId = '00000000-0000-0000-0000-000000000001'
const demoProxyHeaders = {
  'X-API-KEY': demoApiKey,
  'X-Tenant-Id': demoTenantId,
}
const notificationsSrc = path.resolve(
  root,
  '../../../modules/notifications/frontend/src',
)
const settingsSrc = path.resolve(root, '../../../modules/settings/frontend/src')

/**
 * Linked @jarvis/core có node_modules riêng — dedupe React + Prime core.
 * Dev: proxy `/api` → Sample BE để tránh CORS (FE :5173, BE :5167).
 * Build/publish: output thẳng vào Sample/wwwroot (static only)
 */
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, root, '')
  const apiTarget =
    env.VITE_API_PROXY_TARGET ||
    env.VITE_DEV_API_PROXY ||
    process.env.SAMPLE_API_URL ||
    'http://127.0.0.1:5167'

  return {
    plugins: [react(), tailwindcss()],
    define: {
      'process.env.DRAGGABLE_DEBUG': 'undefined',
      'process.env.NODE_ENV': JSON.stringify(
        process.env.NODE_ENV ?? 'development',
      ),
      'import.meta.env.VITE_NOTIFICATION_API_KEY': JSON.stringify(demoApiKey),
      'import.meta.env.VITE_NOTIFICATION_TENANT_ID': JSON.stringify(demoTenantId),
    },
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
        '@microsoft/signalr',
        '@jarvis/core',
      ],
      alias: [
        {
          find: /^@jarvis\/core$/,
          replacement: path.resolve(jarvisCoreSrc, 'index.ts'),
        },
        {
          find: /^@jarvis\/core\/theme\.css$/,
          replacement: path.resolve(jarvisCoreSrc, 'styles/theme.css'),
        },
        {
          find: /^@jarvis\/core\/styles\.css$/,
          replacement: path.resolve(jarvisCoreSrc, 'styles/kit.css'),
        },
        {
          find: /^@jarvis\/core\/dashboard\.css$/,
          replacement: path.resolve(
            jarvisCoreSrc,
            'features/dashboard/styles/dashboard.css',
          ),
        },
        {
          find: /^@jarvis\/notifications$/,
          replacement: path.resolve(
            notificationsSrc,
            'features/notifications/index.ts',
          ),
        },
        {
          find: /^@jarvis\/notifications\/styles\.css$/,
          replacement: path.resolve(
            notificationsSrc,
            'features/notifications/styles/notifications.css',
          ),
        },
        {
          find: /^@jarvis\/setting$/,
          replacement: path.resolve(settingsSrc, 'index.ts'),
        },
        { find: '@', replacement: path.resolve(root, 'src') },
        { find: 'react', replacement: nm('react') },
        { find: 'react-dom', replacement: nm('react-dom') },
        { find: 'react-router-dom', replacement: nm('react-router-dom') },
        { find: 'react-hook-form', replacement: nm('react-hook-form') },
        { find: 'lucide-react', replacement: nm('lucide-react') },
        { find: 'primereact', replacement: nm('primereact') },
        { find: '@primereact/core', replacement: nm('@primereact/core') },
        {
          find: '@primereact/headless',
          replacement: nm('@primereact/headless'),
        },
        { find: '@microsoft/signalr', replacement: nm('@microsoft/signalr') },
      ],
    },
    optimizeDeps: {
      include: [
        '@primereact/core',
        '@primeuix/themes',
        '@primeuix/themes/aura',
        '@primeuix/styled',
        'flatpickr',
        '@microsoft/signalr',
      ],
    },
    server: {
      host: '127.0.0.1',
      port: 5173,
      strictPort: true,
      fs: {
        allow: [root, jarvisCoreSrc, notificationsSrc, settingsSrc],
      },
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
        },
        '/api/notifications': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
          headers: demoProxyHeaders,
        },
        '/api/signalr-demo': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
          headers: demoProxyHeaders,
        },
        '/hubs/notifications': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
          ws: true,
          headers: demoProxyHeaders,
        },
      },
    },
    build: {
      outDir: path.resolve(root, '../../wwwroot'),
      emptyOutDir: true,
    },
  }
})
