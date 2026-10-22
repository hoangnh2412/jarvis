import type { CreateUserPayload, GetUserListParams, UpdateUserPayload } from '../types'
import instance from './req'

export const callGetUserList = async (params?: GetUserListParams) => {
  return await instance.get('v1/identity-admin/users', { params })
}

export const callGetUser = async (id: string) => {
  return await instance.get(`v1/identity-admin/users/${id}`)
}

export const callCreateUser = async (data: CreateUserPayload) => {
  return await instance.post('v1/identity-admin/users', data)
}

export const callUpdateUser = async (id: string, data: UpdateUserPayload) => {
  return await instance.put(`v1/identity-admin/users/${id}`, data)
}

export const callDeleteUser = async (id: string) => {
  return await instance.delete(`v1/identity-admin/users/${id}`)
}
