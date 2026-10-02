import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { plannerApiMockPlugin } from './vite.plugins/plannerApiMock.js'
import { timesheetApiMockPlugin } from './vite.plugins/timesheetApiMock.js'
import { craftDocApiMockPlugin } from './vite.plugins/craftDocApiMock.js'

const root = path.dirname(fileURLToPath(import.meta.url))
const nm = (...segs: string[]) => path.resolve(root, 'node_modules', ...segs)
const platformCoreSrc = path.resolve(root, '../../../frameworks/frontend/src')
const plannerBoardMockJson = path.resolve(
  platformCoreSrc,
  'features/planner/mocks/get-board.json',
)
const timesheetBoardMockJson = path.resolve(
  platformCoreSrc,
  'features/timesheet/mocks/get-board.json',
)
const timesheetOptionsMockJson = path.resolve(
  platformCoreSrc,
  'features/timesheet/mocks/get-options.json',
)
const craftDocTemplatesListMockJson = path.resolve(
  platformCoreSrc,
  'features/craftDoc/mocks/get-templates-list.json',
)
const craftDocLlaIdasTemplateMockJson = path.resolve(
  platformCoreSrc,
  'features/craftDoc/mocks/get-lla-idas-template.json',
)
const craftDocLlaIdasDocxPath = path.resolve(
  platformCoreSrc,
  'features/craftDoc/mocks/assets/LLA-IDAS.docx',
)
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
 * Linked @platform/core có node_modules riêng — dedupe React + Prime core.
 * Dev: proxy `/api` → Sample BE (FE :5173, BE :5167).
 * Planner mock: Vite middleware `/api/v1/planner/*` khi `VITE_USE_MOCK` ≠ false
 * Timesheet mock: Vite middleware `/api/v1/timesheet/*` (cùng điều kiện)
 * CraftDoc mock: Vite middleware `/api/v1/craft-doc/*` (cùng điều kiện)
 * (không dùng MSW SW — tránh treo khi mở DevTools Network).
 */
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, root, '')
  const apiTarget =
    env.VITE_API_PROXY_TARGET ||
    env.VITE_DEV_API_PROXY ||
    process.env.SAMPLE_API_URL ||
    'http://127.0.0.1:5167'
  const useMock = String(env.VITE_USE_MOCK ?? 'true').toLowerCase() !== 'false'
  const workflowPreviewTarget =
    env.VITE_WORKFLOW_PREVIEW_PROXY_TARGET ||
    env.VITE_WORKFLOW_SERVER_URL ||
    apiTarget

  return {
    plugins: [
      react(),
      tailwindcss(),
      plannerApiMockPlugin({
        boardJsonPath: plannerBoardMockJson,
        enabled: useMock,
      }),
      timesheetApiMockPlugin({
        boardJsonPath: timesheetBoardMockJson,
        optionsJsonPath: timesheetOptionsMockJson,
        enabled: useMock,
      }),
      craftDocApiMockPlugin({
        enabled: useMock,
        templatesListJsonPath: craftDocTemplatesListMockJson,
        llaIdasTemplateJsonPath: craftDocLlaIdasTemplateMockJson,
        llaIdasDocxPath: craftDocLlaIdasDocxPath,
      }),
    ],
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
        '@platform/core',
        'ag-grid-community',
        'ag-grid-react',
      ],
      alias: [
        {
          find: /^@platform\/core$/,
          replacement: path.resolve(platformCoreSrc, 'index.ts'),
        },
        {
          find: /^@platform\/core\/theme\.css$/,
          replacement: path.resolve(platformCoreSrc, 'styles/theme.css'),
        },
        {
          find: /^@platform\/core\/styles\.css$/,
          replacement: path.resolve(platformCoreSrc, 'styles/kit.css'),
        },
        {
          find: /^@platform\/core\/dashboard\.css$/,
          replacement: path.resolve(
            platformCoreSrc,
            'features/dashboard/styles/dashboard.css',
          ),
        },
        {
          find: /^@platform\/core\/planner\.css$/,
          replacement: path.resolve(
            platformCoreSrc,
            'features/planner/styles/planner.css',
          ),
        },
        {
          find: /^@platform\/core\/timesheet\.css$/,
          replacement: path.resolve(
            platformCoreSrc,
            'features/timesheet/styles/timesheet.css',
          ),
        },
        {
          find: /^@platform\/core\/dynamicForm\.css$/,
          replacement: path.resolve(
            platformCoreSrc,
            'features/dynamicForm/styles/dynamicForm.css',
          ),
        },
        {
          find: /^@platform\/core\/craftDoc\.css$/,
          replacement: path.resolve(
            platformCoreSrc,
            'features/craftDoc/styles/craftDoc.css',
          ),
        },
        {
          find: /^@platform\/notifications$/,
          replacement: path.resolve(
            notificationsSrc,
            'features/notifications/index.ts',
          ),
        },
        {
          find: /^@platform\/notifications\/styles\.css$/,
          replacement: path.resolve(
            notificationsSrc,
            'features/notifications/styles/notifications.css',
          ),
        },
        {
          find: /^@platform\/setting$/,
          replacement: path.resolve(settingsSrc, 'index.ts'),
        },
        { find: '@', replacement: path.resolve(root, 'src') },
        { find: 'react', replacement: nm('react') },
        { find: 'react-dom', replacement: nm('react-dom') },
        { find: 'react-router-dom', replacement: nm('react-router-dom') },
        { find: 'react-hook-form', replacement: nm('react-hook-form') },
        { find: 'lucide-react', replacement: nm('lucide-react') },
        { find: 'ag-grid-community', replacement: nm('ag-grid-community') },
        { find: 'ag-grid-react', replacement: nm('ag-grid-react') },
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
        'ag-grid-community',
        'ag-grid-react',
      ],
    },
    server: {
      host: '127.0.0.1',
      port: 5173,
      strictPort: true,
      fs: {
        allow: [root, platformCoreSrc, notificationsSrc, settingsSrc],
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
        '/preview': {
          target: workflowPreviewTarget,
          changeOrigin: true,
          secure: false,
        },
        '/_blazor': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
          ws: true,
        },
        '/_framework': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
        },
        '/_content': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
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
