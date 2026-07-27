import type {
  ChangePasswordFormData,
  ForgotPasswordFormData,
  LoginFormData,
  ProfileFormData,
  RegisterFormData,
} from '../validation'
import instance from './req'

export const callRegister = async (data: RegisterFormData) => {
  return await instance.post('/register', data)
}

export const callLogin = async (data: LoginFormData) => {
  return await instance.post('/login', data)
}

export const callGetCurrentUser = async () => {
  return await instance.get('/current')
}

export const callUpdateProfile = async (data: ProfileFormData) => {
  return await instance.put('/profile', data)
}

export const callUpdatePassword = async (data: ChangePasswordFormData) => {
  return await instance.put('/password', {
    oldPassword: data.currentPassword,
    newPassword: data.newPassword,
  })
}
export const callForgotPassword = async (data: ForgotPasswordFormData) => {
  return await instance.post('forgot-password', data)
}

export const callLogout = async () => {
  return await instance.post('/logout')
}
