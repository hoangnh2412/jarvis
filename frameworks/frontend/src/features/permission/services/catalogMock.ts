import {
  AUTHORIZATION_PERMISSIONS,
  POLICY_CATALOG,
  type AuthorizationPermission,
  type PolicyDefinition,
} from '../../authorization/catalog'

export type AuthorizationCatalog = {
  permissions: AuthorizationPermission[]
  policies: PolicyDefinition[]
}

const delay = (ms = 80) => new Promise((resolve) => setTimeout(resolve, ms))

export async function mockGetAuthorizationCatalog(): Promise<AuthorizationCatalog> {
  await delay()
  return {
    permissions: AUTHORIZATION_PERMISSIONS.map((item) => ({ ...item })),
    policies: POLICY_CATALOG.map((item) => ({ ...item })),
  }
}
