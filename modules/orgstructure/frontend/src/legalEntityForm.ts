import type { LegalEntity, LegalEntityProfile, LegalEntityWriteInput } from './services/orgStructure'

/** Dữ liệu form pháp nhân: mã/MST/tên ở cấp pháp nhân, còn lại nằm trong hồ sơ (profile). */
export type LegalEntityFormData = LegalEntityProfile & {
  taxCode: string
  identityCode: string
  name: string
}

export function companyFormToLegalEntityInput(data: LegalEntityFormData): LegalEntityWriteInput {
  const {
    taxCode,
    identityCode,
    name,
    ...profileRest
  } = data
  return {
    code: identityCode.trim(),
    name: name.trim(),
    taxId: taxCode.trim() || undefined,
    profile: profileRest as LegalEntityProfile,
  }
}

export function legalEntityToCompanyForm(entity: LegalEntity | null): Partial<LegalEntityFormData> {
  if (!entity) return {}
  const profile = entity.profile
  return {
    taxCode: entity.taxId ?? '',
    identityCode: entity.code,
    name: entity.name,
    address: profile?.address ?? '',
    province: profile?.province ?? '',
    bankAccount: profile?.bankAccount ?? '',
    bankName: profile?.bankName ?? '',
    paymentCode: profile?.paymentCode ?? '',
    delegate: profile?.delegate ?? '',
    position: profile?.position ?? '',
    legalRepPhone: profile?.legalRepPhone ?? '',
    legalRepDob: profile?.legalRepDob ?? '',
    legalRepPassport: profile?.legalRepPassport ?? '',
    identityCard: profile?.identityCard ?? '',
    useIdentityCard: profile?.useIdentityCard ?? false,
    legalRepGender: profile?.legalRepGender ?? 1,
    email: profile?.email ?? '',
    tel: profile?.tel ?? '',
    webSite: profile?.webSite ?? '',
    fax: profile?.fax ?? '',
    cityName: profile?.cityName ?? '',
    taxAuthorityName: profile?.taxAuthorityName ?? '',
    agentInfo: profile?.agentInfo ?? '',
  }
}
