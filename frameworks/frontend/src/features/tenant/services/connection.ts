import type {
  CreateTenantConnectionPayload,
  UpdateTenantConnectionPayload,
} from '../types'
import instance from './req'

export const callGetTenantConnectionList = async (tenantId: string) => {
  return await instance.get(`v1/tenants/${tenantId}/connections`)
}

export const callAddTenantConnection = async (
  tenantId: string,
  data: CreateTenantConnectionPayload,
) => {
  return await instance.post(`v1/tenants/${tenantId}/connections`, data)
}

export const callUpdateTenantConnection = async (
  tenantId: string,
  connectionId: string,
  data: UpdateTenantConnectionPayload,
) => {
  return await instance.put(
    `v1/tenants/${tenantId}/connections/${connectionId}`,
    data,
  )
}

export const callDeleteTenantConnection = async (
  tenantId: string,
  connectionId: string,
) => {
  return await instance.delete(
    `v1/tenants/${tenantId}/connections/${connectionId}`,
  )
}
