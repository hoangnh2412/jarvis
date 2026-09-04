/** Chế độ trang auth (login / register / quên mật khẩu) */
export type AccountAuthVariant = 'login' | 'register' | 'forgot'

/** Handler submit dùng chung cho các form account */
export type AccountSubmitHandler<T> = (data: T) => void | Promise<void>

/** Kết quả / payload API điển hình phía consumer có thể map */
export type AuthTokenPayload = {
  accessToken: string
  refreshToken?: string
  expiresIn?: number
}

export type AuthUser = {
  id: string
  email: string
  fullName: string
  phone?: string | null
  avatarUrl?: string | null
}

export type LoginResult = {
  user: AuthUser
  tokens?: AuthTokenPayload
}

export type RegisterResult = {
  user: AuthUser
  tokens?: AuthTokenPayload
}

export type ForgotPasswordResult = {
  /** true nếu server xác nhận đã nhận yêu cầu */
  accepted: boolean
  message?: string
}

export type ProfileUpdateResult = {
  user: AuthUser
}

export type ChangePasswordResult = {
  success: boolean
  message?: string
}

export type AuthPreset = {
  title: string
  description: string
  submitLabel: string
  panelEyebrow: string
  panelLine: string
}
