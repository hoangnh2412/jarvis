import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { PrimeReactProvider } from '@primereact/core'
import { kitPrimeReactConfig, Toaster } from '@jarvis/core'
import { configureNotificationAuth } from '@jarvis/notifications'
import { getAccessToken, setupSampleAccountAuth } from './auth'
import { configureSampleHttp } from './constants'
import './index.css'
import App from './App.tsx'

configureSampleHttp()
setupSampleAccountAuth()
configureNotificationAuth(getAccessToken)

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
