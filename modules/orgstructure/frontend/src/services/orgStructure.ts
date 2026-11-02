import { orgHttp } from '../http'
import { beginOrgStructureFilterLoad } from './orgStructureFilterGate'

const API = '/api/v1/org-structure'

export const orgStructureQueryKeys = {
  abacFilter: ['org-structure', 'abac-filter'] as const,
  units: ['org-structure', 'units'] as const,
  legalEntities: ['org-structure', 'legal-entities'] as const,
  unitTypes: ['org-structure', 'unit-types'] as const,
  jobPositions: ['org-structure', 'job-positions'] as const,
  jobGrades: ['org-structure', 'job-grades'] as const,
  jobTitles: ['org-structure', 'job-titles'] as const,
  unitMembers: (unitId: string) => ['org-structure', 'unit-members', unitId] as const,
  unitMemberSummary: ['org-structure', 'unit-member-summary'] as const,
  preview: (templateCode: string, rootName: string) =>
    ['org-structure', 'preview', templateCode, rootName] as const,
}

/** Mã mẫu khớp Platform.OrgStructure.OrgStructureLimits. */
export const ORG_STRUCTURE_TEMPLATES = [
  { code: 'simple-company', label: 'Công ty — các phòng ban' },
  { code: 'corporation', label: 'Tổng công ty — khối & phòng ban' },
  { code: 'holding-group', label: 'Tập đoàn — công ty & phòng ban' },
] as const

export type OrgStructureTemplateCode = (typeof ORG_STRUCTURE_TEMPLATES)[number]['code']

export type OrgUnit = {
  id: string
  orgId: string
  parentId: string | null
  code: string
  name: string
  unitType: string
  sortOrder: number
  isActive: boolean
  legalEntityId: string | null
  path: string
}

export type LegalEntityProfile = {
  address: string
  province: string
  bankAccount: string
  bankName: string
  paymentCode: string
  delegate: string
  position: string
  legalRepPhone: string
  legalRepDob: string
  legalRepPassport: string
  identityCard: string
  useIdentityCard: boolean
  legalRepGender: number
  email: string
  tel: string
  webSite: string
  fax: string
  cityName: string
  taxAuthorityName: string
  agentInfo: string
}

export type LegalEntity = {
  id: string
  code: string
  name: string
  taxId: string | null
  isActive: boolean
  profile?: LegalEntityProfile
}

export type LegalEntityWriteInput = {
  code: string
  name: string
  taxId?: string
  profile?: LegalEntityProfile
}

export type OrgUnitTypeCatalogItem = {
  id: string
  code: string
  name: string
  icon: string
  sortOrder: number
}

export type JobPositionCatalogItem = {
  id: string
  code: string
  name: string
  sortOrder: number
}

export type JobGradeCatalogItem = {
  id: string
  code: string
  name: string
  sortOrder: number
}

export type JobTitleCatalogItem = {
  id: string
  code: string
  name: string
}

export type OrgUnitMember = {
  id: string
  userId: string
  userName: string
  displayName: string | null
  jobPositionId: string
  jobPositionCode: string
  jobPositionName: string
  jobGradeId: string
  jobGradeCode: string
  jobGradeName: string
  jobTitleId: string | null
  jobTitleCode: string | null
  jobTitleName: string | null
  isActive: boolean
}

export type TemplateNode = {
  code: string
  name: string
  unitType: string
  parentCode: string | null
  path: string
}

function text(value: unknown): string {
  return typeof value === 'string' ? value : ''
}

/** Chuẩn hoá id GUID từ API (string hoặc giá trị truthy) để khớp key Map. */
function idText(value: unknown): string {
  if (value === null || value === undefined) return ''
  const raw = typeof value === 'string' ? value : String(value)
  const trimmed = raw.trim()
  return trimmed ? trimmed.toLowerCase() : ''
}

function mapUnit(raw: Record<string, unknown>): OrgUnit {
  const parent = raw.parentId ?? raw.ParentId
  const legal = raw.legalEntityId ?? raw.LegalEntityId
  return {
    id: idText(raw.id ?? raw.Id),
    orgId: idText(raw.orgId ?? raw.OrgId),
    parentId: parent ? idText(parent) : null,
    code: text(raw.code ?? raw.Code),
    name: text(raw.name ?? raw.Name),
    unitType: text(raw.unitType ?? raw.UnitType),
    sortOrder: Number(raw.sortOrder ?? raw.SortOrder ?? 0),
    isActive: Boolean(raw.isActive ?? raw.IsActive ?? true),
    legalEntityId: legal ? String(legal) : null,
    path: text(raw.path ?? raw.Path),
  }
}

export async function getOrgStructureAbacFilter(): Promise<boolean> {
  const payload = await orgHttp.get<{ filterAbacByOrgStructure?: boolean }>(`${API}/abac-filter`)
  return Boolean(payload?.filterAbacByOrgStructure)
}

export function ensureOrgStructureFilterLoaded(): Promise<boolean> {
  return beginOrgStructureFilterLoad(getOrgStructureAbacFilter)
}

export async function listOrgUnits(): Promise<OrgUnit[]> {
  const payload = await orgHttp.get<unknown[]>(`${API}/units`)
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapUnit)
    .filter((unit) => unit.id)
}

export async function createOrgUnit(input: {
  code: string
  name: string
  unitType: string
  parentId?: string | null
  legalEntityId?: string | null
  sortOrder?: number
}): Promise<OrgUnit> {
  const payload = await orgHttp.post<Record<string, unknown>>(`${API}/units`, {
    code: input.code,
    name: input.name,
    unitType: input.unitType,
    parentId: input.parentId || null,
    legalEntityId: input.legalEntityId || null,
    sortOrder: input.sortOrder ?? 0,
  })
  return mapUnit(payload)
}

export async function updateOrgUnit(
  id: string,
  input: {
    code: string
    name: string
    unitType: string
    legalEntityId?: string | null
    sortOrder?: number
  },
): Promise<OrgUnit> {
  const payload = await orgHttp.put<Record<string, unknown>>(`${API}/units/${id}`, {
    code: input.code.trim(),
    name: input.name.trim(),
    unitType: input.unitType,
    sortOrder: input.sortOrder ?? 0,
    legalEntityId: input.legalEntityId || null,
  })
  return mapUnit(payload)
}

export async function moveOrgUnit(id: string, parentId: string | null): Promise<OrgUnit> {
  const payload = await orgHttp.put<Record<string, unknown>>(`${API}/units/${id}/parent`, { parentId })
  return mapUnit(payload)
}

export async function setOrgUnitActive(id: string, isActive: boolean): Promise<OrgUnit> {
  const payload = await orgHttp.patch<Record<string, unknown>>(`${API}/units/${id}/active`, { isActive })
  return mapUnit(payload)
}

function mapUnitTypeCatalogItem(raw: Record<string, unknown>): OrgUnitTypeCatalogItem {
  return {
    id: text(raw.id ?? raw.Id),
    code: text(raw.code ?? raw.Code),
    name: text(raw.name ?? raw.Name),
    icon: text(raw.icon ?? raw.Icon) || 'circle',
    sortOrder: Number(raw.sortOrder ?? raw.SortOrder ?? 0),
  }
}

export async function listUnitTypes(): Promise<OrgUnitTypeCatalogItem[]> {
  const payload = await orgHttp.get<unknown[]>(`${API}/unit-types`)
  if (!Array.isArray(payload)) return []
  return payload
    .map((row) => {
      if (typeof row === 'string') {
        return { id: '', code: row, name: row, icon: 'circle', sortOrder: 0 }
      }
      if (row && typeof row === 'object') {
        return mapUnitTypeCatalogItem(row as Record<string, unknown>)
      }
      return null
    })
    .filter((item): item is OrgUnitTypeCatalogItem => Boolean(item?.code))
}

export async function createOrgUnitType(input: {
  code: string
  name: string
  icon: string
}): Promise<OrgUnitTypeCatalogItem> {
  const payload = await orgHttp.post<Record<string, unknown>>(`${API}/unit-types`, input)
  return mapUnitTypeCatalogItem(payload)
}

export async function updateOrgUnitType(
  id: string,
  input: { code: string; name: string; icon: string },
): Promise<OrgUnitTypeCatalogItem> {
  const payload = await orgHttp.put<Record<string, unknown>>(`${API}/unit-types/${id}`, input)
  return mapUnitTypeCatalogItem(payload)
}

export async function deleteOrgUnitType(id: string): Promise<void> {
  await orgHttp.delete(`${API}/unit-types/${id}`)
}

export async function listLegalEntities(): Promise<LegalEntity[]> {
  const payload = await orgHttp.get<unknown[]>(`${API}/legal-entities`)
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapLegalEntity)
}

function mapLegalEntityProfile(raw: Record<string, unknown> | undefined): LegalEntityProfile | undefined {
  if (!raw) return undefined
  return {
    address: text(raw.address ?? raw.Address),
    province: text(raw.province ?? raw.Province),
    bankAccount: text(raw.bankAccount ?? raw.BankAccount),
    bankName: text(raw.bankName ?? raw.BankName),
    paymentCode: text(raw.paymentCode ?? raw.PaymentCode),
    delegate: text(raw.delegate ?? raw.Delegate),
    position: text(raw.position ?? raw.Position),
    legalRepPhone: text(raw.legalRepPhone ?? raw.LegalRepPhone),
    legalRepDob: text(raw.legalRepDob ?? raw.LegalRepDob),
    legalRepPassport: text(raw.legalRepPassport ?? raw.LegalRepPassport),
    identityCard: text(raw.identityCard ?? raw.IdentityCard),
    useIdentityCard: Boolean(raw.useIdentityCard ?? raw.UseIdentityCard ?? false),
    legalRepGender: Number(raw.legalRepGender ?? raw.LegalRepGender ?? 1),
    email: text(raw.email ?? raw.Email),
    tel: text(raw.tel ?? raw.Tel),
    webSite: text(raw.webSite ?? raw.WebSite),
    fax: text(raw.fax ?? raw.Fax),
    cityName: text(raw.cityName ?? raw.CityName),
    taxAuthorityName: text(raw.taxAuthorityName ?? raw.TaxAuthorityName),
    agentInfo: text(raw.agentInfo ?? raw.AgentInfo),
  }
}

function mapLegalEntity(raw: Record<string, unknown>): LegalEntity {
  const profileRaw = raw.profile ?? raw.Profile
  return {
    id: text(raw.id ?? raw.Id),
    code: text(raw.code ?? raw.Code),
    name: text(raw.name ?? raw.Name),
    taxId: raw.taxId || raw.TaxId ? String(raw.taxId ?? raw.TaxId) : null,
    isActive: Boolean(raw.isActive ?? raw.IsActive ?? true),
    profile: profileRaw && typeof profileRaw === 'object'
      ? mapLegalEntityProfile(profileRaw as Record<string, unknown>)
      : undefined,
  }
}

export async function createLegalEntity(input: LegalEntityWriteInput): Promise<LegalEntity> {
  const payload = await orgHttp.post<Record<string, unknown>>(`${API}/legal-entities`, input)
  return mapLegalEntity(payload)
}

export async function updateLegalEntity(id: string, input: LegalEntityWriteInput): Promise<LegalEntity> {
  const payload = await orgHttp.put<Record<string, unknown>>(`${API}/legal-entities/${id}`, input)
  return mapLegalEntity(payload)
}

export async function setLegalEntityActive(id: string, isActive: boolean): Promise<LegalEntity> {
  const payload = await orgHttp.patch<Record<string, unknown>>(`${API}/legal-entities/${id}/active`, { isActive })
  return mapLegalEntity(payload)
}

export async function previewOrgTemplate(
  templateCode: OrgStructureTemplateCode,
  rootName: string,
): Promise<TemplateNode[]> {
  const payload = await orgHttp.get<Record<string, unknown>>(
    `${API}/templates/${encodeURIComponent(templateCode)}/preview`,
    { params: { rootName } },
  )
  const rows = payload.nodes ?? payload.Nodes
  if (!Array.isArray(rows)) return []
  return rows
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map((raw) => ({
      code: text(raw.code ?? raw.Code),
      name: text(raw.name ?? raw.Name),
      unitType: text(raw.unitType ?? raw.UnitType),
      parentCode: raw.parentCode || raw.ParentCode ? String(raw.parentCode ?? raw.ParentCode) : null,
      path: text(raw.path ?? raw.Path),
    }))
}

export async function applyOrgTemplate(input: {
  templateCode: OrgStructureTemplateCode
  rootName: string
  nodes: TemplateNode[]
}): Promise<OrgUnit[]> {
  const payload = await orgHttp.post<unknown[]>(`${API}/templates/apply`, {
    templateCode: input.templateCode,
    rootName: input.rootName,
    confirmed: true,
    nodes: input.nodes,
  })
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapUnit)
}

export async function setUserOrgScope(userId: string, organizationalUnitId: string | null): Promise<void> {
  await orgHttp.put(`${API}/users/${userId}/scope`, {
    organizationalUnitId,
  })
}

function mapJobPosition(raw: Record<string, unknown>): JobPositionCatalogItem {
  return {
    id: text(raw.id ?? raw.Id),
    code: text(raw.code ?? raw.Code),
    name: text(raw.name ?? raw.Name),
    sortOrder: Number(raw.sortOrder ?? raw.SortOrder ?? 0),
  }
}

function mapJobGrade(raw: Record<string, unknown>): JobGradeCatalogItem {
  return {
    id: text(raw.id ?? raw.Id),
    code: text(raw.code ?? raw.Code),
    name: text(raw.name ?? raw.Name),
    sortOrder: Number(raw.sortOrder ?? raw.SortOrder ?? 0),
  }
}

function mapJobTitle(raw: Record<string, unknown>): JobTitleCatalogItem {
  return {
    id: text(raw.id ?? raw.Id),
    code: text(raw.code ?? raw.Code),
    name: text(raw.name ?? raw.Name),
  }
}

function mapOrgUnitMember(raw: Record<string, unknown>): OrgUnitMember {
  return {
    id: text(raw.id ?? raw.Id),
    userId: text(raw.userId ?? raw.UserId),
    userName: text(raw.userName ?? raw.UserName),
    displayName: raw.displayName || raw.DisplayName ? String(raw.displayName ?? raw.DisplayName) : null,
    jobPositionId: text(raw.jobPositionId ?? raw.JobPositionId),
    jobPositionCode: text(raw.jobPositionCode ?? raw.JobPositionCode),
    jobPositionName: text(raw.jobPositionName ?? raw.JobPositionName),
    jobGradeId: text(raw.jobGradeId ?? raw.JobGradeId),
    jobGradeCode: text(raw.jobGradeCode ?? raw.JobGradeCode),
    jobGradeName: text(raw.jobGradeName ?? raw.JobGradeName),
    jobTitleId: raw.jobTitleId || raw.JobTitleId ? String(raw.jobTitleId ?? raw.JobTitleId) : null,
    jobTitleCode: raw.jobTitleCode || raw.JobTitleCode ? String(raw.jobTitleCode ?? raw.JobTitleCode) : null,
    jobTitleName: raw.jobTitleName || raw.JobTitleName ? String(raw.jobTitleName ?? raw.JobTitleName) : null,
    isActive: Boolean(raw.isActive ?? raw.IsActive ?? true),
  }
}

export async function listJobPositions(): Promise<JobPositionCatalogItem[]> {
  const payload = await orgHttp.get<unknown[]>(`${API}/job-positions`)
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapJobPosition)
    .filter((item) => item.id)
}

export async function listJobGrades(): Promise<JobGradeCatalogItem[]> {
  const payload = await orgHttp.get<unknown[]>(`${API}/job-grades`)
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapJobGrade)
    .filter((item) => item.id)
}

export async function listJobTitles(): Promise<JobTitleCatalogItem[]> {
  const payload = await orgHttp.get<unknown[]>(`${API}/job-titles`)
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapJobTitle)
    .filter((item) => item.id)
}

export type UnitMemberCount = {
  organizationalUnitId: string
  activeCount: number
  totalCount: number
}

export async function listUnitMemberSummary(): Promise<UnitMemberCount[]> {
  const payload = await orgHttp.get<unknown[]>(`${API}/units/member-summary`)
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map((raw) => ({
      organizationalUnitId: idText(raw.organizationalUnitId ?? raw.OrganizationalUnitId),
      activeCount: Number(raw.activeCount ?? raw.ActiveCount ?? 0),
      totalCount: Number(raw.totalCount ?? raw.TotalCount ?? 0),
    }))
    .filter((row) => row.organizationalUnitId)
}

export async function listOrgUnitMembers(unitId: string): Promise<OrgUnitMember[]> {
  const payload = await orgHttp.get<unknown[]>(`${API}/units/${unitId}/members`)
  if (!Array.isArray(payload)) return []
  return payload
    .filter((row): row is Record<string, unknown> => Boolean(row) && typeof row === 'object')
    .map(mapOrgUnitMember)
    .filter((item) => item.id)
}

export async function createOrgUnitMember(
  unitId: string,
  input: {
    userId: string
    jobPositionId: string
    jobGradeId: string
    jobTitleId?: string | null
  },
): Promise<OrgUnitMember> {
  const payload = await orgHttp.post<Record<string, unknown>>(`${API}/units/${unitId}/members`, {
    ...input,
    jobTitleId: input.jobTitleId || null,
  })
  return mapOrgUnitMember(payload)
}

export async function updateOrgUnitMember(
  id: string,
  input: { jobPositionId: string; jobGradeId: string; jobTitleId?: string | null },
): Promise<OrgUnitMember> {
  const payload = await orgHttp.put<Record<string, unknown>>(`${API}/unit-members/${id}`, {
    ...input,
    jobTitleId: input.jobTitleId || null,
  })
  return mapOrgUnitMember(payload)
}

export async function setOrgUnitMemberActive(id: string, isActive: boolean): Promise<OrgUnitMember> {
  const payload = await orgHttp.patch<Record<string, unknown>>(`${API}/unit-members/${id}/active`, { isActive })
  return mapOrgUnitMember(payload)
}
