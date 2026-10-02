import roleListMock from '../mocks/get-role-list.json'
import type { Role } from '../types'

/** Seed fake roles — nguồn: `mocks/get-role-list.json`. */
export const FAKE_ROLES: Role[] = roleListMock.items as Role[]
