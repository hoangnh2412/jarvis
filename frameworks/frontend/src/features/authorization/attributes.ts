/** ABAC trên UI: màn tính từ dữ liệu dòng. Engine thật vẫn ở server. */

export function allowSameTenant(
  actorTenantId: string | null | undefined,
  resourceTenantId: string | null | undefined,
): boolean {
  return Boolean(
    actorTenantId && resourceTenantId && actorTenantId === resourceTenantId,
  )
}

export function allowSameOrg(
  actorOrgId: string | null | undefined,
  resourceOrgId: string | null | undefined,
): boolean {
  return Boolean(actorOrgId && resourceOrgId && actorOrgId === resourceOrgId)
}

export function allowOwner(
  actorUserId: string | null | undefined,
  createdBy: string | null | undefined,
): boolean {
  return Boolean(actorUserId && createdBy && actorUserId === createdBy)
}

export function allowApproveLimit(
  approveLimit: number | null | undefined,
  amount: number,
): boolean {
  if (approveLimit == null) return false
  return amount <= approveLimit
}
