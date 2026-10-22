/** Permission seed (ADR D19) — gán vào Role, không tạo mã tùy ý. */
export type AuthorizationPermission = {
  code: string
  label: string
}

export const AUTHORIZATION_PERMISSIONS: AuthorizationPermission[] = [
  { code: 'User.Manage', label: 'Quản lý người dùng' },
  { code: 'Role.Manage', label: 'Quản lý vai trò' },
  { code: 'Role.AssignPermission', label: 'Gán permission' },
  { code: 'Role.AssignPolicy', label: 'Gán policy' },
  { code: 'Expense.View', label: 'Xem phiếu chi' },
  { code: 'Expense.Approve', label: 'Duyệt phiếu chi' },
  { code: 'HealthCheck.View', label: 'Xem health check' },
  { code: 'System.All', label: 'Mọi permission (wildcard)' },
]

/** Policy seed (ADR D9). Rule do hệ thống định nghĩa. */
export type PolicyDefinition = {
  code: string
  name: string
  summary: string
}

export const POLICY_CATALOG: PolicyDefinition[] = [
  {
    code: 'P_SAME_TENANT',
    name: 'Same Tenant',
    summary: 'Resource.TenantId == User.TenantId',
  },
  {
    code: 'P_SAME_ORG',
    name: 'Same Organization',
    summary: 'Resource.OrgId == User.OrgId',
  },
  {
    code: 'P_SAME_DEPARTMENT',
    name: 'Same Department',
    summary: 'Resource.DepartmentId == User.DepartmentId',
  },
  {
    code: 'P_OWNER_ONLY',
    name: 'Owner Only',
    summary: 'Resource.CreatedBy == User.Id',
  },
  {
    code: 'P_APPROVE_LIMIT',
    name: 'Approve Limit',
    summary: 'Resource.Amount <= User.ApproveLimit',
  },
  {
    code: 'P_MANAGED_PROJECT',
    name: 'Managed Project',
    summary: 'Resource.ProjectId thuộc dự án user phụ trách',
  },
  {
    code: 'P_MANAGED_DEPARTMENT',
    name: 'Managed Department',
    summary: 'Resource.DepartmentId thuộc phòng user quản lý',
  },
]
