const TENANT_KEY = 'platform.tenantId'
const ORG_KEY = 'platform.orgId'

export function getLoginTenantId(): string | null {
  if (typeof localStorage === 'undefined') return null
  const value = localStorage.getItem(TENANT_KEY)?.trim()
  return value || null
}

export function getLoginOrgId(): string | null {
  if (typeof localStorage === 'undefined') return null
  const value = localStorage.getItem(ORG_KEY)?.trim()
  return value || null
}

export function setLoginScope(scope: { tenantId: string; orgId: string }) {
  localStorage.setItem(TENANT_KEY, scope.tenantId)
  localStorage.setItem(ORG_KEY, scope.orgId)
}

export function clearLoginScope() {
  localStorage.removeItem(TENANT_KEY)
  localStorage.removeItem(ORG_KEY)
}

/** Header phạm vi cho mọi request sau khi người chọn tenant và org. */
export function getLoginScopeHeaders(): Record<string, string> {
  const headers: Record<string, string> = {}
  const tenantId = getLoginTenantId()
  const orgId = getLoginOrgId()
  if (tenantId) headers['X-Tenant-Id'] = tenantId
  if (orgId) headers['X-Org-Id'] = orgId
  return headers
}
