
/** @type {import('tailwindcss').Config} */
module.exports = {
  theme: {
    extend: {
      colors: {
        ink: '#18181b',
        mute: '#71717a',
        line: '#e4e4e7',
        paper: '#fafafa',
        panel: '#09090b',
        accent: {
          DEFAULT: '#0f766e',
          soft: '#f0fdfa',
          'soft-bright': '#ccfbf1',
          hover: '#0d9488',
        },
      },
      keyframes: {
        'account-auth-in': {
          from: { opacity: '0', transform: 'translateY(10px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
        'fade-in': {
          from: { opacity: '0' },
          to: { opacity: '1' },
        },
        'slide-up': {
          from: { opacity: '0', transform: 'translateY(8px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
      },
      animation: {
        'account-auth-in':
          'account-auth-in 420ms cubic-bezier(0.22, 1, 0.36, 1) both',
        'fade-in': 'fade-in 200ms ease-out both',
        'slide-up': 'slide-up 280ms cubic-bezier(0.22, 1, 0.36, 1) both',
      },
      fontFamily: {
        sans: [
          '"Segoe UI"',
          '"Helvetica Neue"',
          'ui-sans-serif',
          'system-ui',
          '-apple-system',
          'sans-serif',
        ],
      },
    },
  },
}
