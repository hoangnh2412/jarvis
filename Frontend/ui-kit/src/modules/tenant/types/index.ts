/** Trạng thái tenant — khớp ADR TenantStatus */
export const TenantStatus = {
  Inactive: 0,
  Active: 1,
} as const

export type TenantStatusValue = (typeof TenantStatus)[keyof typeof TenantStatus]

export const TENANT_STATUS_LABEL: Record<TenantStatusValue, string> = {
  [TenantStatus.Inactive]: 'Ngừng hoạt động',
  [TenantStatus.Active]: 'Đang hoạt động',
}

export const TENANT_STATUS_OPTIONS = [
  {
    value: TenantStatus.Active,
    label: TENANT_STATUS_LABEL[TenantStatus.Active],
  },
  {
    value: TenantStatus.Inactive,
    label: TENANT_STATUS_LABEL[TenantStatus.Inactive],
  },
] as const

/** Nhà cung cấp DB — khớp ADR DbProviderType */
export const DbProviderType = {
  Postgres: 0,
  SqlServer: 1,
  MySql: 2,
} as const

export type DbProviderTypeValue =
  (typeof DbProviderType)[keyof typeof DbProviderType]

export const DB_PROVIDER_LABEL: Record<DbProviderTypeValue, string> = {
  [DbProviderType.Postgres]: 'PostgreSQL',
  [DbProviderType.SqlServer]: 'SQL Server',
  [DbProviderType.MySql]: 'MySQL',
}

export const DB_PROVIDER_OPTIONS = [
  {
    value: DbProviderType.Postgres,
    label: DB_PROVIDER_LABEL[DbProviderType.Postgres],
  },
  {
    value: DbProviderType.SqlServer,
    label: DB_PROVIDER_LABEL[DbProviderType.SqlServer],
  },
  {
    value: DbProviderType.MySql,
    label: DB_PROVIDER_LABEL[DbProviderType.MySql],
  },
] as const

/** Audit + soft-delete dùng chung (IFullAudited) */
export type AuditedFields = {
  createdAt: string
  createdBy?: string | null
  updatedAt?: string | null
  updatedBy?: string | null
  deletedId: string
  deletedAt?: string | null
  deletedBy?: string | null
}

export type Tenant = AuditedFields & {
  id: string
  code: string
  name: string
  status: TenantStatusValue
  parentId?: string | null
}

export type TenantConnection = AuditedFields & {
  id: string
  tenantId: string
  providerType: DbProviderTypeValue
  connectionString: string
  partitionFrom?: string | null
  partitionTo?: string | null
  isDefault: boolean
}

export type TenantDomain = AuditedFields & {
  id: string
  tenantId: string
  domain: string
  isPrimary: boolean
}

/** Payload tạo / cập nhật Tenant */
export type CreateTenantPayload = {
  code: string
  name: string
  status?: TenantStatusValue
  parentId?: string | null
}

export type UpdateTenantPayload = {
  code: string
  name: string
  parentId?: string | null
}

export type SetTenantStatusPayload = {
  status: TenantStatusValue
}

/** Payload TenantConnection */
export type CreateTenantConnectionPayload = {
  providerType: DbProviderTypeValue
  connectionString: string
  partitionFrom?: string | null
  partitionTo?: string | null
  isDefault?: boolean
}

export type UpdateTenantConnectionPayload = CreateTenantConnectionPayload

/** Payload TenantDomain */
export type CreateTenantDomainPayload = {
  domain: string
  isPrimary?: boolean
}

export type UpdateTenantDomainPayload = CreateTenantDomainPayload

/** Query danh sách tenant — filter/pagination xử lý ở backend */
export type GetTenantListParams = {
  search?: string
  status?: TenantStatusValue
  parentId?: string | null
  page?: number
  /** Page size — query `?size=` */
  size?: number
}

export type TenantListResult = {
  items: Tenant[]
  total: number
  page?: number
  size?: number
}

export type TenantSubmitHandler<T> = (data: T) => void | Promise<void>

export type { ActionProps } from './mutation'
