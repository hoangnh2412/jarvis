import { tenantsHttp } from '../http'

const COMPANY_PROFILE_API = '/api/v1/app/company-profile'

export const companyProfileQueryKeys = {
  current: ['company-profile', 'current'] as const,
}

/** Khối lõi hồ sơ doanh nghiệp phía platform (khớp CompanyProfileContent backend). */
export type CompanyProfileCore = {
  address: string
  phone: string
  fax: string
  email: string
  website: string
  logoUrl: string
  representative: string
  representativeTitle: string
  bankAccount: string
  bankName: string
  province: string
  paymentCode: string
  legalRepPhone: string
  legalRepDob: string
  legalRepPassport: string
  identityCard: string
  useIdentityCard: boolean
  /** 1 = Nam, 0 = Nữ */
  legalRepGender: number
}

export type CompanyProfileDto = {
  owner?: { type?: number; id?: string }
  core: CompanyProfileCore
  extensions: Record<string, Record<string, unknown>>
}

export const emptyCompanyProfileCore: CompanyProfileCore = {
  address: '',
  phone: '',
  fax: '',
  email: '',
  website: '',
  logoUrl: '',
  representative: '',
  representativeTitle: '',
  bankAccount: '',
  bankName: '',
  province: '',
  paymentCode: '',
  legalRepPhone: '',
  legalRepDob: '',
  legalRepPassport: '',
  identityCard: '',
  useIdentityCard: false,
  legalRepGender: 1,
}

function text(value: unknown): string {
  return typeof value === 'string' ? value : value == null ? '' : String(value)
}

function mapCore(raw: Record<string, unknown> | undefined): CompanyProfileCore {
  if (!raw) return { ...emptyCompanyProfileCore }
  return {
    address: text(raw.address ?? raw.Address),
    phone: text(raw.phone ?? raw.Phone),
    fax: text(raw.fax ?? raw.Fax),
    email: text(raw.email ?? raw.Email),
    website: text(raw.website ?? raw.Website),
    logoUrl: text(raw.logoUrl ?? raw.LogoUrl),
    representative: text(raw.representative ?? raw.Representative),
    representativeTitle: text(raw.representativeTitle ?? raw.RepresentativeTitle),
    bankAccount: text(raw.bankAccount ?? raw.BankAccount),
    bankName: text(raw.bankName ?? raw.BankName),
    province: text(raw.province ?? raw.Province),
    paymentCode: text(raw.paymentCode ?? raw.PaymentCode),
    legalRepPhone: text(raw.legalRepPhone ?? raw.LegalRepPhone),
    legalRepDob: text(raw.legalRepDob ?? raw.LegalRepDob),
    legalRepPassport: text(raw.legalRepPassport ?? raw.LegalRepPassport),
    identityCard: text(raw.identityCard ?? raw.IdentityCard),
    useIdentityCard: Boolean(raw.useIdentityCard ?? raw.UseIdentityCard ?? false),
    legalRepGender: Number(raw.legalRepGender ?? raw.LegalRepGender ?? 1),
  }
}

export async function getCompanyProfile(): Promise<CompanyProfileDto> {
  const payload = await tenantsHttp.get<Record<string, unknown>>(COMPANY_PROFILE_API)
  const coreRaw = (payload?.core ?? payload?.Core) as Record<string, unknown> | undefined
  const extRaw = (payload?.extensions ?? payload?.Extensions) as Record<string, unknown> | undefined
  const extensions: Record<string, Record<string, unknown>> = {}
  if (extRaw && typeof extRaw === 'object') {
    for (const [key, value] of Object.entries(extRaw)) {
      if (value && typeof value === 'object') {
        extensions[key] = value as Record<string, unknown>
      }
    }
  }
  return { core: mapCore(coreRaw), extensions }
}

export async function saveCompanyProfile(input: {
  core: CompanyProfileCore
  extensions: Record<string, Record<string, unknown>>
}): Promise<CompanyProfileDto> {
  const payload = await tenantsHttp.put<Record<string, unknown>>(COMPANY_PROFILE_API, {
    core: input.core,
    extensions: input.extensions,
  })
  const coreRaw = (payload?.core ?? payload?.Core) as Record<string, unknown> | undefined
  return { core: mapCore(coreRaw ?? (input.core as unknown as Record<string, unknown>)), extensions: input.extensions }
}
