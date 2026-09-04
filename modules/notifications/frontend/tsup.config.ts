import { defineConfig } from 'tsup'

export default defineConfig({
  entry: ['src/features/notifications/index.ts'],
  format: ['esm', 'cjs'],
  dts: true,
  splitting: false,
  sourcemap: true,
  clean: true,
  treeshake: true,
  external: [
    'react',
    'react-dom',
    'react/jsx-runtime',
    'lucide-react',
    '@microsoft/signalr',
    '@jarvis/core',
    'primereact',
    /^primereact\//,
    '@primereact/core',
    /^@primereact\//,
    '@primeuix/themes',
    /^@primeuix\//,
  ],
  tsconfig: './tsconfig.build.json',
  esbuildOptions(options) {
    options.loader = {
      ...options.loader,
      '.css': 'empty',
    }
  },
})
