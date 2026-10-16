/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_IMPORT_EXPORT_API_URL?: string
  readonly VITE_IMPORT_EXPORT_API_KEY?: string
  readonly VITE_TENANT_ID?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
