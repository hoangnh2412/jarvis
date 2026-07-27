declare global {
  interface ImportMetaEnv {
    readonly VITE_API_URL_TENANT: string
    readonly VITE_API_URL_ACCOUNT: string
  }

  interface ImportMeta {
    readonly env: ImportMetaEnv
  }
}

export {}
