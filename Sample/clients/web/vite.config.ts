import path from 'node:path'
import { fileURLToPath } from 'node:url'
import tailwindcss from '@tailwindcss/vite'
import { defineConfig, loadEnv } from 'vite'
import { reactPlugin } from '@platform/core/vite/react'
import { draggableDebugPlugin } from '@platform/core/vite/draggableDebug'
import { nodeEnvPlugin } from '@platform/core/vite/nodeEnv'
import { requireEnv } from '@platform/core/vite/hostEnv'
import { platformProxy } from '@platform/core/vite/hostProxy'
import {
  HOST_OPTIMIZE_DEPS,
  buildNpmResolve,
  buildPlatformResolve,
} from '@platform/core/vite/hostResolve'
import { resolvePlatformModuleRoots } from '@platform/core/vite/platformModules'
import { sampleMockPlugins } from './mocks/container.js'

const hostRoot = path.dirname(fileURLToPath(import.meta.url))
const platformRoot = path.resolve(hostRoot, '../../..')
const appSrcDir = 'src'
const outDir = '../../wwwroot'
/** true: alias sang source trong monorepo. false: package npm, resolve theo exports. */
const resolveFromSource = true

function sampleProxy(proxyUrl: string) {
  const proxy = () => ({ target: proxyUrl, changeOrigin: true, secure: false })
  return {
    '/api': proxy(),
    '/preview': proxy(),
    '/sse/notifications': proxy(),
  }
}

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, hostRoot, '')
  const proxyUrl = requireEnv(env, 'PROXY_URL', 'http://127.0.0.1:5167')
  const host = requireEnv(env, 'DEV_SERVER_HOST', '127.0.0.1')
  const port = Number(requireEnv(env, 'DEV_SERVER_PORT', '5173'))
  const strictPort = requireEnv(env, 'DEV_SERVER_STRICT_PORT', 'true') === 'true'

  const platformResolve = resolveFromSource
    ? buildPlatformResolve(hostRoot, platformRoot, appSrcDir)
    : buildNpmResolve(hostRoot, appSrcDir)
  const reactQueryEntry = path.resolve(
    platformRoot,
    'modules/identity/frontend/node_modules/@tanstack/react-query/build/legacy/index.js',
  )

  return {
    plugins: [
      reactPlugin(),
      tailwindcss(),
      draggableDebugPlugin(),
      nodeEnvPlugin(),
      ...sampleMockPlugins(hostRoot),
    ],
    resolve: {
      ...platformResolve,
      dedupe: [...platformResolve.dedupe, '@tanstack/react-query'],
      alias: [
        {
          find: '@platform/identity',
          replacement: path.resolve(platformRoot, 'modules/identity/frontend/src/index.ts'),
        },
        {
          find: '@platform/orgstructure',
          replacement: path.resolve(platformRoot, 'modules/orgstructure/frontend/src/index.ts'),
        },
        ...platformResolve.alias,
        { find: '@tanstack/react-query', replacement: reactQueryEntry },
      ],
    },
    optimizeDeps: {
      include: [...HOST_OPTIMIZE_DEPS],
    },
    server: {
      host,
      port,
      strictPort,
      fs: {
        allow: resolveFromSource
          ? [
              hostRoot,
              ...resolvePlatformModuleRoots(platformRoot),
              path.resolve(platformRoot, 'modules/identity/frontend'),
              path.resolve(platformRoot, 'modules/orgstructure/frontend'),
            ]
          : [hostRoot],
      },
      proxy: {
        ...platformProxy(proxyUrl),
        ...sampleProxy(proxyUrl),
      },
    },
    build: {
      outDir: path.resolve(hostRoot, outDir),
      emptyOutDir: true,
      rolldownOptions: { external: [] },
    },
  }
})
