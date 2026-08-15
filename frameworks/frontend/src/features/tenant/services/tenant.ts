import type {
  CreateTenantPayload,
  GetTenantListParams,
  SetTenantStatusPayload,
  UpdateTenantPayload,
} from '../types'
import instance from './req'

export const callGetTenantList = async (params?: GetTenantListParams) => {
  return await instance.get('v1/tenants', { params })
}

export const callGetTenant = async (id: string) => {
  return await instance.get(`v1/tenants/${id}`)
}

export const callCreateTenant = async (data: CreateTenantPayload) => {
  return await instance.post('v1/tenants', data)
}

export const callUpdateTenant = async (
  id: string,
  data: UpdateTenantPayload,
) => {
  return await instance.put(`v1/tenants/${id}`, data)
}

export const callDeleteTenant = async (id: string) => {
  return await instance.delete(`v1/tenants/${id}`)
}

export const callSetTenantStatus = async (
  id: string,
  data: SetTenantStatusPayload,
) => {
  return await instance.patch(`v1/tenants/${id}/status`, data)
}
