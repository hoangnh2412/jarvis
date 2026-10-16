// import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { PrimeReactProvider } from '@primereact/core'
import { kitPrimeReactConfig, Toaster } from '@platform/core'
import { configureNotificationAuth } from '@platform/notifications'
import { getAccessToken, setupSampleAccountAuth } from './auth'
import {
  configureImportExportHttp,
  resolveImportExportTenantId,
} from '@platform/importexport'
import { configureSampleHttp } from './constants'
import './index.css'
import App from './App.tsx'

configureSampleHttp()
setupSampleAccountAuth()
configureNotificationAuth(getAccessToken)
configureImportExportHttp({
  // Same-origin Sample API — paths are `v1/...` relative to `/api/`
  baseURL:
    import.meta.env.VITE_IMPORT_EXPORT_API_URL ||
    import.meta.env.VITE_API_URL ||
    '/api/',
  apiKey: () =>
    import.meta.env.VITE_IMPORT_EXPORT_API_KEY ||
    import.meta.env.VITE_API_KEY ||
    undefined,
  getTenantId: () =>
    resolveImportExportTenantId(import.meta.env.VITE_TENANT_ID),
})

createRoot(document.getElementById('root')!).render(
  // <StrictMode>
    <BrowserRouter>
      <PrimeReactProvider {...kitPrimeReactConfig}>
        <App />
        <Toaster />
      </PrimeReactProvider>
    </BrowserRouter>
  // </StrictMode>,
)
