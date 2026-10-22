export const ORG_TENANT_HEADER = 'X-Tenant-Id'

export const ORG_ID_HEADER = 'X-Org-Id'

/**
 * Tenant demo trùng Sample (`vite` proxy notifications và ImportExport).
 * Org làm việc không có mặc định — thiếu `X-Org-Id` thì API D16 trả 403.
 */
export const ORG_DEFAULT_TENANT_ID = '00000000-0000-0000-0000-000000000001'

/** Tương đối sau `VITE_API_URL` (`/api/`). */
export const ORG_API_PATH = 'v1/orgs'
