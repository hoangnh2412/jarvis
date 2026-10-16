/** API key header — matches ImportExport.Api ApiKeyAuthenticationHandler.HeaderName */
export const IMPORT_EXPORT_API_KEY_HEADER = 'X-Excel-Engine-Key'

/** Host demo tenant header — host maps into ICurrentTenant; not an ImportExport DTO field */
export const IMPORT_EXPORT_TENANT_HEADER = 'X-Tenant-Id'

/**
 * Interim default tenant Guid while Multi-tenant module is not fully wired.
 * Prefer `VITE_TENANT_ID` from FE `.env`; this is the fallback only.
 */
export const IMPORT_EXPORT_DEFAULT_TENANT_ID =
  '00000000-0000-0000-0000-000000000001'

export const IMPORT_EXPORT_PERMISSIONS = {
  templateView: 'importexport.template.view',
  templateManage: 'importexport.template.manage',
} as const

export type ImportExportPermission =
  (typeof IMPORT_EXPORT_PERMISSIONS)[keyof typeof IMPORT_EXPORT_PERMISSIONS]

/** Resolve tenant id for X-Tenant-Id: env first, then interim default. */
export function resolveImportExportTenantId(
  envTenantId?: string | null,
): string {
  const fromEnv = envTenantId?.trim()
  return fromEnv || IMPORT_EXPORT_DEFAULT_TENANT_ID
}
