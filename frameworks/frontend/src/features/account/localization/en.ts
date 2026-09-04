import type { AccountAuthVariant, AuthPreset } from '../types'

export const AUTH_PRESETS: Record<AccountAuthVariant, AuthPreset> = {
  login: {
    title: 'Sign in',
    description: 'Enter your email and password to continue.',
    submitLabel: 'Sign in',
    panelEyebrow: 'Secure access',
    panelLine: 'One gateway into the system — fast, clear, trustworthy.',
  },
  register: {
    title: 'Create account',
    description: 'Fill in your details to get started.',
    submitLabel: 'Create account',
    panelEyebrow: 'Get started',
    panelLine: 'A new account with the same security and experience standards.',
  },
  forgot: {
    title: 'Forgot password',
    description: 'Enter your email to receive a reset link.',
    submitLabel: 'Send link',
    panelEyebrow: 'Account recovery',
    panelLine: 'We send a reset link — without revealing whether the email exists.',
  },
}

export const enMessages = {
  auth: {
    processing: 'Processing…',
    checkInbox: 'Check your inbox to continue.',
    securityFooter: 'Security · Authentication · Profile',
    brandName: 'FE Component',
    brandSubtitle: 'Account',
  },
  routes: {
    login: 'Sign in',
    register: 'Register',
    forgotPassword: 'Forgot password',
    profile: 'Profile',
    changePassword: 'Change password',
  },
  menu: {
    profile: 'Profile',
    changePassword: 'Change password',
  },
}
