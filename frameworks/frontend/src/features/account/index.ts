// Pages
export { LoginPage } from './pages/Login'
export type { LoginPageProps, LoginPageContentContext } from './pages/Login'

export { RegisterPage } from './pages/Register'
export type {
  RegisterPageProps,
  RegisterPageContentContext,
} from './pages/Register'

export { ForgotPasswordPage } from './pages/ForgotPassword'
export type {
  ForgotPasswordPageProps,
  ForgotPasswordPageContentContext,
} from './pages/ForgotPassword'

export { AccountProfilePage } from './pages/Profile'
export type {
  AccountProfilePageProps,
  AccountProfilePageContentContext,
} from './pages/Profile'

export { ChangePasswordPage } from './pages/ChangePassword'
export type {
  ChangePasswordPageProps,
  ChangePasswordPageContentContext,
} from './pages/ChangePassword'

// Components
export {
  AuthShell,
  AccountAuthPage,
  AccountAuthLink,
} from './components/AuthShell'
export type {
  AuthShellProps,
  AccountAuthPageProps,
  AccountAuthVariant,
} from './components/AuthShell'

export { LoginForm } from './components/LoginForm'
export type { LoginFormProps } from './components/LoginForm'

export { RegisterForm } from './components/RegisterForm'
export type { RegisterFormProps } from './components/RegisterForm'

export { ForgotPasswordForm } from './components/ForgotPasswordForm'
export type { ForgotPasswordFormProps } from './components/ForgotPasswordForm'

export { ProfileForm } from './components/ProfileForm'
export type { ProfileFormProps } from './components/ProfileForm'

export { ProfileCard } from './components/ProfileCard'
export type { ProfileCardProps } from './components/ProfileCard'

export { AvatarUploader } from './components/AvatarUploader'

// Validation (schemas)
export {
  REGEX,
  loginSchema,
  loginFormDefaultValues,
  registerSchema,
  registerFormDefaultValues,
  forgotPasswordSchema,
  forgotPasswordFormDefaultValues,
  profileSchema,
  profileFormDefaultValues,
  changePasswordSchema,
  changePasswordFormDefaultValues,
} from './validation'
export type {
  LoginFormData,
  RegisterFormData,
  ForgotPasswordFormData,
  ProfileFormData,
  ChangePasswordFormData,
} from './validation'

// Types
export type {
  AccountSubmitHandler,
  AuthTokenPayload,
  AuthUser,
  LoginResult,
  RegisterResult,
  ForgotPasswordResult,
  ProfileUpdateResult,
  ChangePasswordResult,
  AuthPreset,
} from './types'

// Hooks
export {
  useLoginForm,
  useRegisterForm,
  useForgotPasswordForm,
  useProfileForm,
  useChangePasswordForm,
} from './hooks'
export type {
  UseLoginFormOptions,
  UseRegisterFormOptions,
  UseForgotPasswordFormOptions,
  UseProfileFormOptions,
  UseChangePasswordFormOptions,
} from './hooks'

// Routes
export { ACCOUNT_ROUTES, getAccountRouteList } from './routes'
export type { AccountRouteKey, AccountRouteItem } from './routes'

// Permission
export { ACCOUNT_PERMISSIONS, hasPermission } from './permission'
export type { AccountPermissionKey } from './permission'

// Menu
export { accountMenuItems } from './menu'
export type { AccountMenuItem } from './menu'

// Localization
export {
  AUTH_PRESETS,
  accountMessages,
  getAuthPreset,
  getAccountMessages,
} from './localization'
export type { AccountLocale } from './localization'

// Constants
export {
  ACCOUNT_ROUTES as ACCOUNT_ROUTE_PATHS,
  BASE_URL_ACCOUNT,
} from './constants'

// Services (axios API)
export {
  accountHttp,
  callRegister,
  callLogin,
  callGetCurrentUser,
  callUpdateProfile,
  callUpdatePassword,
  callForgotPassword,
  callLogout,
} from './services'

// Theme
export { defaultAccountTheme } from './theme'
export type { AccountTheme } from './theme'

// Content slot helpers
export { resolveAccountContent } from './utils'
export type { AccountSlotContent } from './utils'
