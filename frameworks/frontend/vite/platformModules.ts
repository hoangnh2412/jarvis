import path from 'node:path'

/** Monorepo layout: `platformRoot/modules|frameworks/...` */
export type PlatformModuleSpec = {
  packageName: string
  rootDir: string
  entryFile: string
  cssExports?: Record<string, string>
}

export const PLATFORM_FRONTEND_MODULES: PlatformModuleSpec[] = [
  {
    packageName: '@platform/core',
    rootDir: 'frameworks/frontend/src',
    entryFile: 'index.ts',
    cssExports: {
      theme: 'styles/theme.css',
      styles: 'styles/kit.css',
      sources: 'styles/sources.css',
    },
  },
  {
    packageName: '@platform/notifications',
    rootDir: 'modules/notifications/frontend/src',
    entryFile: 'features/notifications/index.ts',
    cssExports: { styles: 'features/notifications/styles/notifications.css' },
  },
  {
    packageName: '@platform/setting',
    rootDir: 'modules/settings/frontend/src',
    entryFile: 'index.ts',
  },
  {
    packageName: '@platform/orgs',
    rootDir: 'modules/orgs/frontend/src',
    entryFile: 'index.ts',
  },
  {
    packageName: '@platform/identity',
    rootDir: 'modules/identity/frontend/src',
    entryFile: 'index.ts',
  },
  {
    packageName: '@platform/orgstructure',
    rootDir: 'modules/orgstructure/frontend/src',
    entryFile: 'index.ts',
  },
  {
    packageName: '@platform/importexport',
    rootDir: 'modules/importexport/frontend/src',
    entryFile: 'index.ts',
  },
  {
    packageName: '@platform/files',
    rootDir: 'modules/files/frontend/src',
    entryFile: 'index.ts',
  },
]

export function resolvePlatformModuleRoots(platformRoot: string): string[] {
  return PLATFORM_FRONTEND_MODULES.map((m) =>
    path.resolve(platformRoot, m.rootDir),
  )
}
