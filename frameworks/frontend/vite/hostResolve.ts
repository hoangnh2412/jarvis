import path from 'node:path'
import {
  PLATFORM_FRONTEND_MODULES,
} from './platformModules.ts'

type ViteAlias = { find: string | RegExp; replacement: string }

/** Dependency của @platform/core. Host không khai báo các package này. */
const PLATFORM_PACKAGES = [
  'react',
  'react-dom',
  'react-router-dom',
  'react-hook-form',
  'zod',
  '@hookform/resolvers',
  '@primereact/core',
  '@primereact/headless',
  '@primeuix/themes',
  '@primeuix/styled',
] as const

/**
 * Package hoist lên `node_modules` của host (kit hoặc module sở hữu).
 * Đưa vào `optimizeDeps.include` và `resolve.dedupe` để Vite pre-bundle một bản.
 * Không alias sang thư mục `node_modules`: alias thư mục khiến Vite bỏ qua
 * pre-bundle và chỉ gom ở request sau (reload cả trang).
 */
export const HOST_OPTIMIZE_DEPS = [
  'flatpickr',
  '@microsoft/fetch-event-source',
  'axios',
  'lucide-react',
] as const

/**
 * PrimeReact 11 không có file entry `.` (không có index.mjs).
 * `optimizeDeps.include` của bare `primereact` làm scan lỗi.
 * `dedupe` vẫn cần tên package để `primereact/*` từ module resolve về một bản ở host.
 */
const HOST_DEDUPE_DEPS = [...HOST_OPTIMIZE_DEPS, 'primereact'] as const

function escapeRegExp(value: string) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
}

function platformPackageAlias(platformRoot: string, name: string): ViteAlias {
  const dir = path
    .resolve(platformRoot, 'frameworks/frontend/node_modules', name)
    .replace(/\\/g, '/')
  return {
    find: new RegExp(`^${escapeRegExp(name)}(/.*)?$`),
    replacement: `${dir}$1`,
  }
}

export function buildPlatformResolve(hostRoot: string, platformRoot: string, appSrcDir: string) {
  const alias: ViteAlias[] = []

  for (const mod of PLATFORM_FRONTEND_MODULES) {
    const srcRoot = path.resolve(platformRoot, mod.rootDir)
    alias.push({
      find: new RegExp(`^${escapeRegExp(mod.packageName)}$`),
      replacement: path.resolve(srcRoot, mod.entryFile),
    })
    if (mod.cssExports) {
      for (const [exportKey, relPath] of Object.entries(mod.cssExports)) {
        const importFile =
          exportKey === 'styles' ? 'styles.css' : `${exportKey}.css`
        alias.push({
          find: new RegExp(
            `^${escapeRegExp(mod.packageName)}/${importFile.replace('.', '\\.')}$`,
          ),
          replacement: path.resolve(srcRoot, relPath),
        })
      }
    }
  }

  for (const name of PLATFORM_PACKAGES) {
    alias.push(platformPackageAlias(platformRoot, name))
  }

  alias.push(
    { find: '@', replacement: path.resolve(hostRoot, appSrcDir) },
  )

  // Export `@primeuix/themes/*` nằm dưới `dist/*/index.mjs`, không phải cạnh package root.
  const primeThemeDist = path
    .resolve(platformRoot, 'frameworks/frontend/node_modules/@primeuix/themes/dist')
    .replace(/\\/g, '/')
  alias.unshift({
    find: /^@primeuix\/themes\/(.+)$/,
    replacement: `${primeThemeDist}/$1/index.mjs`,
  })

  return {
    dedupe: [...HOST_DEDUPE_DEPS],
    alias,
  }
}

/** Package đã cài từ npm. `@platform/*` đi theo `exports` tới `dist`, không alias sang source. */
export function buildNpmResolve(hostRoot: string, appSrcDir: string) {
  return {
    dedupe: [...HOST_DEDUPE_DEPS],
    alias: [
      { find: '@', replacement: path.resolve(hostRoot, appSrcDir) },
    ],
  }
}
