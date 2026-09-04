/**
 * react-draggable (peer của react-rnd) đọc `process.env.DRAGGABLE_DEBUG`.
 * Trên Vite/browser không có `process` → ReferenceError, Rnd không kéo được.
 */
type ProcessLike = { env: Record<string, string | undefined> }

export function ensureBrowserProcess(): void {
  if (typeof globalThis === 'undefined') return
  const g = globalThis as typeof globalThis & { process?: ProcessLike }
  if (!g.process) {
    g.process = { env: { NODE_ENV: 'production' } }
    return
  }
  if (!g.process.env) {
    g.process.env = { NODE_ENV: 'production' }
    return
  }
  if (g.process.env.NODE_ENV == null) {
    g.process.env.NODE_ENV = 'production'
  }
}

ensureBrowserProcess()
