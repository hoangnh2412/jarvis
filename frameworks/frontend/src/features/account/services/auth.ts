import type {
  ChangePasswordFormData,
  ForgotPasswordFormData,
  LoginFormData,
  ProfileFormData,
  RegisterFormData,
} from '../validation'
import instance from './req'

export const callRegister = async (data: RegisterFormData) => {
  return await instance.post('v1/account/register', data)
}

export const callLogin = async (data: LoginFormData) => {
  return await instance.post('v1/account/login', data)
}

export const callGetCurrentUser = async () => {
  return await instance.get('v1/account/current')
}

export const callUpdateProfile = async (data: ProfileFormData) => {
  return await instance.put('v1/account/profile', data)
}

export const callUpdatePassword = async (data: ChangePasswordFormData) => {
  return await instance.put('v1/account/password', {
    oldPassword: data.currentPassword,
    newPassword: data.newPassword,
  })
}

export const callForgotPassword = async (data: ForgotPasswordFormData) => {
  return await instance.post('v1/account/forgot-password', data)
}

export const callLogout = async () => {
  return await instance.post('v1/account/logout')
}
