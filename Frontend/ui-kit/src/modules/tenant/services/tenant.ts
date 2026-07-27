import type {
  CreateTenantPayload,
  GetTenantListParams,
  SetTenantStatusPayload,
  UpdateTenantPayload,
} from '../types'
import instance from './req'

export const callGetTenantList = async (params?: GetTenantListParams) => {
  return await instance.get('/', { params })
}

export const callGetTenant = async (id: string) => {
  return await instance.get(`/${id}`)
}

export const callCreateTenant = async (data: CreateTenantPayload) => {
  return await instance.post('/', data)
}

export const callUpdateTenant = async (
  id: string,
  data: UpdateTenantPayload,
) => {
  return await instance.put(`/${id}`, data)
}

export const callDeleteTenant = async (id: string) => {
  return await instance.delete(`/${id}`)
}

export const callSetTenantStatus = async (
  id: string,
  data: SetTenantStatusPayload,
) => {
  return await instance.patch(`/${id}/status`, data)
}
