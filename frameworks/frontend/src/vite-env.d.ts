declare global {
  interface ImportMetaEnv {
    /** Shared API base, e.g. `/api/` (same-origin + Vite proxy) */
    readonly VITE_API_URL?: string
  }

  interface ImportMeta {
    readonly env: ImportMetaEnv
  }
}

declare module '*.css' {
  const content: string
  export default content
}

declare module '*.json' {
  const value: unknown
  export default value
}

declare module 'quill/dist/quill.snow.css' {
  const content: string
  export default content
}

export {}
