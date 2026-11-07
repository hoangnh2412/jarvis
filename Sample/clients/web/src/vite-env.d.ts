/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL?: string
  readonly VITE_API_KEY?: string
  readonly VITE_API_KEY_NAME?: string
  readonly VITE_WORKFLOW_SERVER_URL?: string
  readonly VITE_IMPORT_EXPORT_API_URL?: string
  readonly VITE_IMPORT_EXPORT_API_KEY?: string
  readonly VITE_TENANT_ID?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

declare module 'gridstack/dist/gridstack.css' {
  const content: string
  export default content
}
