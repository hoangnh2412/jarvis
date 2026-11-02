import { identityHttp } from '../http'
import { fetchPermissionCatalog } from './roles'

export type PolicyLimit = {
  code: string
  name: string
  isSystemPolicy?: boolean
}

export type AuthorizationPolicyRule = {
  id: string
  order: number
  leftOperand: string
  operator: string
  rightOperand: string
}

export type AuthorizationPolicyDetail = {
  id: string
  code: string
  name: string
  resourceType?: string | null
  action?: string | null
  isSystemPolicy: boolean
  /** Đang gán cho ít nhất một vai trò — không sửa/xóa catalog. */
  isInUse: boolean
  rules: AuthorizationPolicyRule[]
}

export type AuthorizationPolicyWrite = {
  code: string
  name: string
  resourceType?: string
  action?: string
  rules: { order: number; leftOperand: string; operator: string; rightOperand: string }[]
}

export type AuthorizationOperand = {
  path: string
  source: string
  kind: string
  valueType: string
}

export async function listPolicyLimits(): Promise<PolicyLimit[]> {
  const catalog = await fetchPermissionCatalog()
  return (catalog.policies ?? []).map((policy) => ({
    code: policy.code,
    name: policy.name,
    isSystemPolicy: policy.isSystemPolicy,
  }))
}

export async function listAuthorizationPolicies(): Promise<AuthorizationPolicyDetail[]> {
  return identityHttp.get<AuthorizationPolicyDetail[]>('/api/v1/app/authorization/policies')
}

export async function getAuthorizationPolicy(id: string): Promise<AuthorizationPolicyDetail> {
  return identityHttp.get<AuthorizationPolicyDetail>(`/api/v1/app/authorization/policies/${id}`)
}

export async function listAuthorizationOperands(): Promise<AuthorizationOperand[]> {
  return identityHttp.get<AuthorizationOperand[]>('/api/v1/app/authorization/policies/operands')
}

export async function listAuthorizationOperators(): Promise<string[]> {
  return identityHttp.get<string[]>('/api/v1/app/authorization/policies/operators')
}

export async function createAuthorizationPolicy(input: AuthorizationPolicyWrite): Promise<AuthorizationPolicyDetail> {
  return identityHttp.post<AuthorizationPolicyDetail>('/api/v1/app/authorization/policies', input)
}

export async function updateAuthorizationPolicy(id: string, input: AuthorizationPolicyWrite): Promise<AuthorizationPolicyDetail> {
  return identityHttp.put<AuthorizationPolicyDetail>(`/api/v1/app/authorization/policies/${id}`, input)
}

export async function deleteAuthorizationPolicy(id: string): Promise<void> {
  await identityHttp.delete<void>(`/api/v1/app/authorization/policies/${id}`)
}
