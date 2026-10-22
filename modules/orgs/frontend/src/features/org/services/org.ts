import { platformHttp } from '@platform/core'
import {
  ORG_API_PATH,
  ORG_DEFAULT_TENANT_ID,
  ORG_ID_HEADER,
  ORG_TENANT_HEADER,
} from '../constants'
import type {
  CreateOrgPayload,
  ListOrgsParams,
  SetOrgActivePayload,
  UpdateOrgPayload,
} from '../types'

export const orgHttp = platformHttp

type OrgRequestScope = {
  tenantId?: string
  orgId?: string
}

let scopeOverride: OrgRequestScope = {}

/** Host tiêm tenant / org làm việc trước khi gọi API. Tham chiếu không cần ổn định. */
export function configureOrgRequestScope(scope: OrgRequestScope) {
  scopeOverride = {
    tenantId: scope.tenantId?.trim() || undefined,
    orgId: scope.orgId?.trim() || undefined,
  }
}

function scopeHeaders() {
  const headers: Record<string, string> = {}
  const tenantId =
    scopeOverride.tenantId ||
    import.meta.env.VITE_TENANT_ID?.trim() ||
    ORG_DEFAULT_TENANT_ID
  const orgId = scopeOverride.orgId || import.meta.env.VITE_ORG_ID?.trim()
  if (tenantId) headers[ORG_TENANT_HEADER] = tenantId
  if (orgId) headers[ORG_ID_HEADER] = orgId
  return headers
}

function listQuery(params?: ListOrgsParams) {
  const query: Record<string, string | boolean> = {}
  const search = params?.search?.trim()
  if (search) query.search = search
  if (params?.isActive != null) query.isActive = params.isActive
  return query
}

export function callListOrgs(params?: ListOrgsParams) {
  return orgHttp.get(ORG_API_PATH, {
    params: listQuery(params),
    headers: scopeHeaders(),
  })
}

export function callGetOrg(id: string) {
  return orgHttp.get(`${ORG_API_PATH}/${id}`, { headers: scopeHeaders() })
}

export function callCreateOrg(data: CreateOrgPayload) {
  return orgHttp.post(ORG_API_PATH, data, { headers: scopeHeaders() })
}

export function callUpdateOrg(id: string, data: UpdateOrgPayload) {
  return orgHttp.put(`${ORG_API_PATH}/${id}`, data, { headers: scopeHeaders() })
}

export function callSetOrgActive(id: string, data: SetOrgActivePayload) {
  return orgHttp.patch(`${ORG_API_PATH}/${id}/active`, data, {
    headers: scopeHeaders(),
  })
}
