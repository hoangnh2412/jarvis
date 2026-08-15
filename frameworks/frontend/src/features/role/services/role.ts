import type {
  CreateRolePayload,
  GetRoleListParams,
  UpdateRolePayload,
  UpdateRolePermissionsPayload,
} from '../types'
import instance from './req'

/** API CRUD vai trò — host gắn qua `callback.onSubmit` hoặc gọi trực tiếp. */

export const callGetRoleList = async (params?: GetRoleListParams) => {
  return await instance.get('v1/roles', { params })
}

export const callGetRole = async (id: string) => {
  return await instance.get(`v1/roles/${id}`)
}

export const callCreateRole = async (data: CreateRolePayload) => {
  return await instance.post('v1/roles', data)
}

export const callUpdateRole = async (id: string, data: UpdateRolePayload) => {
  return await instance.put(`v1/roles/${id}`, data)
}

export const callDeleteRole = async (id: string) => {
  return await instance.delete(`v1/roles/${id}`)
}

export const callUpdateRolePermissions = async (
  id: string,
  data: UpdateRolePermissionsPayload,
) => {
  return await instance.put(`v1/roles/${id}/permissions`, data)
}
