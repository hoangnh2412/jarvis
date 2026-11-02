export {}

declare global {
  interface ImportMetaEnv {
    readonly VITE_TENANT_ID?: string
    readonly VITE_ORG_ID?: string
  }
}
