declare global {
  interface ImportMetaEnv {
    /** Shared API base, e.g. `/api/` (same-origin + Vite proxy) */
    readonly VITE_API_URL?: string
    /** Dev proxy target for Vite, e.g. `http://localhost:5167` */
    readonly VITE_API_PROXY_TARGET?: string
    readonly VITE_API_KEY?: string
    readonly VITE_API_KEY_NAME?: string
    /**
     * Bật Vite middleware mock planner + timesheet (Sample).
     * Axios gọi `/api/v1/planner/*`, `/api/v1/timesheet/*` → middleware trả JSON từ
     * `@platform/core` → `features/*/mocks/*.json`.
     * Mặc định: bật. Set `false` khi nối BE thật.
     */
    readonly VITE_USE_MOCK?: string
    /** @deprecated Use VITE_API_URL */
    readonly VITE_API_URL_TENANT?: string
    /** @deprecated Use VITE_API_URL */
    readonly VITE_API_URL_ACCOUNT?: string
    /** @deprecated Use VITE_API_URL */
    readonly VITE_API_URL_CRAFT_PDF?: string
    /** @deprecated Use VITE_API_URL */
    readonly VITE_API_URL_DASHBOARD?: string
    /** @deprecated Use VITE_API_URL */
    readonly VITE_API_URL_QUERY_BUILDER?: string
    /** @deprecated Use VITE_API_KEY */
    readonly VITE_API_KEY_QUERY_BUILDER?: string
    /** @deprecated Use VITE_API_KEY */
    readonly VITE_API_KEY_TENANT?: string
    /** @deprecated Use VITE_API_KEY_NAME */
    readonly VITE_API_KEY_NAME_QUERY_BUILDER?: string
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

declare module 'gridstack/dist/gridstack.css' {
  const content: string
  export default content
}

declare module 'quill/dist/quill.snow.css' {
  const content: string
  export default content
}

export {}
