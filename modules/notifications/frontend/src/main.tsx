import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { PrimeReactProvider, Toaster, kitPrimeReactConfig } from '@jarvis/core'
import './index.css'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <PrimeReactProvider {...kitPrimeReactConfig}>
      <App />
      <Toaster />
    </PrimeReactProvider>
  </StrictMode>,
)
