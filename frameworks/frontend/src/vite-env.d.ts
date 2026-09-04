declare global {
  interface ImportMetaEnv {
    /** Shared API base, e.g. `/api/` (same-origin + Vite proxy) */
    readonly VITE_API_URL?: string
    /** Dev proxy target for Vite, e.g. `http://localhost:5167` */
    readonly VITE_API_PROXY_TARGET?: string
    readonly VITE_API_KEY?: string
    readonly VITE_API_KEY_NAME?: string
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

declare module 'gridstack/dist/gridstack.css' {
  const content: string
  export default content
}

declare module 'quill/dist/quill.snow.css' {
  const content: string
  export default content
}

export {}
