// import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { PrimeReactProvider } from '@primereact/core'
import { kitPrimeReactConfig, Toaster } from '@platform/core'
import { DEFAULT_IDENTITY_ROUTES, configureIdentity } from '@platform/identity'
import { configureOrgStructure } from '@platform/orgstructure'
import {
  configureNotificationAuth,
  configureNotificationOrg,
  configureNotificationRealtime,
  configureNotificationTenant,
} from '@platform/notifications'
import {
  clearLoginScope,
  getAccessToken,
  getLoginOrgId,
  getLoginScopeHeaders,
  getLoginTenantId,
  setLoginScope,
  setupSampleAccountAuth,
} from './auth'
import {
  configureImportExportHttp,
  resolveImportExportTenantId,
} from '@platform/importexport'
import { configureSampleHttp } from './constants'
import './index.css'
import App from './App.tsx'

configureSampleHttp()
setupSampleAccountAuth()
configureIdentity({
  getScopeHeaders: getLoginScopeHeaders,
  loginPath: DEFAULT_IDENTITY_ROUTES.appLogin,
  rememberScope: (tenantId, orgId) => setLoginScope({ tenantId, orgId }),
  clearScope: clearLoginScope,
})
configureOrgStructure({ getScopeHeaders: getLoginScopeHeaders })

const queryClient = new QueryClient()
configureNotificationAuth(getAccessToken)
configureNotificationTenant(getLoginTenantId)
configureNotificationOrg(getLoginOrgId)
// Tạm tắt SSE /sse/notifications trên Sample. Bật lại bằng enabled: true.
configureNotificationRealtime({ enabled: false })
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
    getLoginTenantId() || resolveImportExportTenantId(import.meta.env.VITE_TENANT_ID),
  getOrgId: () => getLoginOrgId() ?? undefined,
})

createRoot(document.getElementById('root')!).render(
  // <StrictMode>
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>
        <PrimeReactProvider {...kitPrimeReactConfig}>
          <App />
          <Toaster />
        </PrimeReactProvider>
      </QueryClientProvider>
    </BrowserRouter>
  // </StrictMode>,
)
