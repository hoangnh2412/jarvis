export type LoginScopeOption = {
  id: string
  code: string
  name: string
}

function mapItem(raw: Record<string, unknown>): LoginScopeOption {
  return {
    id: String(raw.id ?? raw.Id ?? ''),
    code: String(raw.code ?? raw.Code ?? ''),
    name: String(raw.name ?? raw.Name ?? ''),
  }
}

function unwrapList(body: unknown): LoginScopeOption[] {
  const payload =
    body && typeof body === 'object' && 'data' in body
      ? (body as { data?: unknown }).data
      : body
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapItem)
    .filter((row) => row.id)
}

async function readList(url: string, failure: string): Promise<LoginScopeOption[]> {
  const response = await fetch(url)
  const body: unknown = await response.json().catch(() => null)
  if (!response.ok) throw new Error(failure)
  return unwrapList(body)
}

export function listLoginTenants(): Promise<LoginScopeOption[]> {
  return readList(
    '/api/authentication/catalog/tenants',
    'Không tải được danh sách tenant.',
  )
}

export function listLoginOrgs(tenantId: string): Promise<LoginScopeOption[]> {
  const id = tenantId.trim()
  if (!id) return Promise.resolve([])
  return readList(
    `/api/authentication/catalog/tenants/${encodeURIComponent(id)}/orgs`,
    'Không tải được danh sách tổ chức.',
  )
}

export function formatLoginScopeLabel(item: LoginScopeOption): string {
  const name = item.name.trim()
  const code = item.code.trim()
  if (name && code && name !== code) return `${code} — ${name}`
  return name || code || item.id
}

/** Nhãn ô header: `Tên (Mã)`. */
export function formatWorkingScopeLabel(item: LoginScopeOption): string {
  const name = item.name.trim()
  const code = item.code.trim()
  if (name && code && name !== code) return `${name} (${code})`
  return name || code || item.id
}
