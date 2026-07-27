import type {
  CreateTenantDomainPayload,
  UpdateTenantDomainPayload,
} from '../types'
import instance from './req'

export const callGetTenantDomainList = async (tenantId: string) => {
  return await instance.get(`/tenants/${tenantId}/domains`)
}

export const callAddTenantDomain = async (
  tenantId: string,
  data: CreateTenantDomainPayload,
) => {
  return await instance.post(`/tenants/${tenantId}/domains`, data)
}

export const callUpdateTenantDomain = async (
  tenantId: string,
  domainId: string,
  data: UpdateTenantDomainPayload,
) => {
  return await instance.put(`/tenants/${tenantId}/domains/${domainId}`, data)
}

export const callDeleteTenantDomain = async (
  tenantId: string,
  domainId: string,
) => {
  return await instance.delete(`/tenants/${tenantId}/domains/${domainId}`)
}
