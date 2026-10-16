import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { PrimeReactProvider } from '@primereact/core'
import { kitPrimeReactConfig, Toaster } from '@platform/core'
import {
  configureImportExportHttp,
  resolveImportExportTenantId,
} from '@platform/importexport'
import './index.css'
import App from './App'

configureImportExportHttp({
  baseURL: import.meta.env.VITE_IMPORT_EXPORT_API_URL || '/ie-api',
  apiKey: () => import.meta.env.VITE_IMPORT_EXPORT_API_KEY || undefined,
  getTenantId: () =>
    resolveImportExportTenantId(import.meta.env.VITE_TENANT_ID),
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <PrimeReactProvider {...kitPrimeReactConfig}>
        <App />
        <Toaster />
      </PrimeReactProvider>
    </BrowserRouter>
  </StrictMode>,
)
