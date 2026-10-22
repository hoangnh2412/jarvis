/** Snapshot cột chuẩn — khớp `OrgDescriptor` (camelCase JSON). */
export type Org = {
  id: string
  tenantId: string
  code: string
  name: string
  isActive: boolean
}

export type ListOrgsParams = {
  search?: string
  isActive?: boolean
}

export type CreateOrgPayload = {
  code: string
  name: string
}

export type UpdateOrgPayload = CreateOrgPayload

export type SetOrgActivePayload = {
  isActive: boolean
}
