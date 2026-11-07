import type { Plugin } from 'vite'

/** Thay `process.env.NODE_ENV` bằng mode của Vite (`development` hoặc `production`). */
export function nodeEnvPlugin(): Plugin {
  return {
    name: 'platform-node-env',
    config(_config, { mode }) {
      return {
        define: {
          'process.env.NODE_ENV': JSON.stringify(mode),
        },
      }
    },
  }
}
