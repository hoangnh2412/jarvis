import type { Plugin } from 'vite'

/** react-draggable đọc `process.env.DRAGGABLE_DEBUG`. Chỉ gắn khi host dùng react-rnd. */
export function draggableDebugPlugin(): Plugin {
  return {
    name: 'platform-draggable-debug',
    config() {
      return {
        define: {
          'process.env.DRAGGABLE_DEBUG': 'undefined',
        },
      }
    },
  }
}
