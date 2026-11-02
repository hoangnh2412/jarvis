import { useEffect, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { PageHeader, notify } from '@platform/core'
import {
  companyProfileQueryKeys,
  emptyCompanyProfileCore,
  getCompanyProfile,
  saveCompanyProfile,
  type CompanyProfileCore,
} from '../services/companyProfile'
import { notifyApiError } from '../notify'
import { btnPrimaryClass, fieldInputClass, labelClass } from '../components/fieldStyles'

export type CompanyProfileExtensionField = {
  name: string
  label: string
  type?: 'text' | 'email' | 'tel' | 'date' | 'select' | 'checkbox' | 'textarea'
  options?: { value: string; label: string }[]
  placeholder?: string
}

/** Khối mở rộng do sản phẩm đăng ký: render theo field, lưu vào extensions[key] của hồ sơ. */
export type CompanyProfileExtensionSlot = {
  key: string
  title: string
  description?: string
  fields: CompanyProfileExtensionField[]
}

export type CompanyProfilePageProps = {
  extensions?: CompanyProfileExtensionSlot[]
  title?: string
  description?: string
  /** Tóm tắt tenant (tên, mã số thuế) do host nạp từ catalog — chỉ hiển thị. */
  tenantSummary?: { name: string; taxCode: string } | null
  headerExtra?: ReactNode
}

type ExtensionValues = Record<string, Record<string, unknown>>

function extensionInitial(
  slots: CompanyProfileExtensionSlot[],
  loaded: Record<string, Record<string, unknown>>,
): ExtensionValues {
  const values: ExtensionValues = {}
  for (const slot of slots) {
    const current = loaded[slot.key] ?? {}
    const record: Record<string, unknown> = {}
    for (const field of slot.fields) {
      record[field.name] = current[field.name] ?? (field.type === 'checkbox' ? false : '')
    }
    values[slot.key] = record
  }
  return values
}

/** Trang Thông tin doanh nghiệp: khối lõi platform + các khối mở rộng sản phẩm đăng ký. */
export function CompanyProfilePage({
  extensions = [],
  title = 'Thông tin doanh nghiệp',
  description = 'Hồ sơ doanh nghiệp dùng trên hóa đơn và tra cứu.',
  tenantSummary,
  headerExtra,
}: CompanyProfilePageProps) {
  const queryClient = useQueryClient()
  const profileQuery = useQuery({
    queryKey: companyProfileQueryKeys.current,
    queryFn: getCompanyProfile,
  })

  const [core, setCore] = useState<CompanyProfileCore>({ ...emptyCompanyProfileCore })
  const [extValues, setExtValues] = useState<ExtensionValues>({})

  useEffect(() => {
    if (!profileQuery.data) return
    setCore(profileQuery.data.core)
    setExtValues(extensionInitial(extensions, profileQuery.data.extensions))
    // extensions là cấu hình tĩnh của host trong suốt vòng đời trang.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [profileQuery.data])

  useEffect(() => {
    if (profileQuery.isError) notifyApiError(profileQuery.error)
  }, [profileQuery.error, profileQuery.isError])

  const saveMutation = useMutation({
    mutationFn: () => saveCompanyProfile({ core, extensions: extValues }),
    onError: notifyApiError,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: companyProfileQueryKeys.current })
      notify.success('Đã lưu thông tin doanh nghiệp.')
    },
  })

  const setCoreField = (name: keyof CompanyProfileCore, value: string | boolean | number) => {
    setCore((current) => ({ ...current, [name]: value }))
  }

  const setExtField = (slotKey: string, name: string, value: unknown) => {
    setExtValues((current) => ({
      ...current,
      [slotKey]: { ...(current[slotKey] ?? {}), [name]: value },
    }))
  }

  const textField = (
    label: string,
    value: string,
    onChange: (value: string) => void,
    props?: { type?: string; placeholder?: string },
  ) => (
    <label className="block">
      <span className={labelClass}>{label}</span>
      <input
        className={fieldInputClass}
        type={props?.type ?? 'text'}
        value={value}
        placeholder={props?.placeholder}
        onChange={(e) => onChange(e.target.value)}
      />
    </label>
  )

  return (
    <section className="flex h-full min-h-0 flex-col gap-4 overflow-y-auto p-4 text-sm text-slate-800 sm:p-6">
      <PageHeader
        title={title}
        description={description}
        actions={(
          <>
            {headerExtra}
            <button
              type="button"
              className={btnPrimaryClass}
              disabled={saveMutation.isPending || profileQuery.isLoading}
              onClick={() => saveMutation.mutate()}
            >
              Lưu thông tin
            </button>
          </>
        )}
      />
      {tenantSummary ? (
        <div className="rounded-xl border border-slate-200 bg-white px-4 py-3 shadow-sm">
          <span className="font-semibold text-slate-900">{tenantSummary.name}</span>
          {tenantSummary.taxCode ? (
            <span className="ml-2 text-slate-500">MST: {tenantSummary.taxCode}</span>
          ) : null}
        </div>
      ) : null}

      {profileQuery.isLoading ? (
        <p role="status" className="text-sm text-slate-500">Đang tải thông tin doanh nghiệp…</p>
      ) : (
        <form
          className="flex flex-col gap-4"
          onSubmit={(event) => {
            event.preventDefault()
            saveMutation.mutate()
          }}
        >
          <fieldset className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
            <legend className="px-1 text-sm font-semibold text-slate-900">Thông tin chung</legend>
            <div className="grid gap-4 sm:grid-cols-2">
              {textField('Địa chỉ', core.address, (v) => setCoreField('address', v))}
              {textField('Tỉnh/Thành phố', core.province, (v) => setCoreField('province', v))}
              {textField('Điện thoại', core.phone, (v) => setCoreField('phone', v), { type: 'tel' })}
              {textField('Fax', core.fax, (v) => setCoreField('fax', v))}
              {textField('Email', core.email, (v) => setCoreField('email', v), { type: 'email' })}
              {textField('Website', core.website, (v) => setCoreField('website', v))}
              {textField('Logo (URL)', core.logoUrl, (v) => setCoreField('logoUrl', v))}
              {textField('Mã thanh toán', core.paymentCode, (v) => setCoreField('paymentCode', v))}
            </div>
          </fieldset>

          <fieldset className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
            <legend className="px-1 text-sm font-semibold text-slate-900">Người đại diện pháp luật</legend>
            <div className="grid gap-4 sm:grid-cols-2">
              {textField('Người đại diện', core.representative, (v) => setCoreField('representative', v))}
              {textField('Chức danh', core.representativeTitle, (v) => setCoreField('representativeTitle', v))}
              {textField('Điện thoại đại diện', core.legalRepPhone, (v) => setCoreField('legalRepPhone', v), { type: 'tel' })}
              {textField('Ngày sinh', core.legalRepDob, (v) => setCoreField('legalRepDob', v), { type: 'date' })}
              <label className="block">
                <span className={labelClass}>Giới tính</span>
                <select
                  className={fieldInputClass}
                  value={String(core.legalRepGender)}
                  onChange={(e) => setCoreField('legalRepGender', Number(e.target.value))}
                >
                  <option value="1">Nam</option>
                  <option value="0">Nữ</option>
                </select>
              </label>
              {textField('Số hộ chiếu', core.legalRepPassport, (v) => setCoreField('legalRepPassport', v))}
              {textField('Số CCCD/CMND', core.identityCard, (v) => setCoreField('identityCard', v))}
              <label className="flex items-center gap-2 self-end pb-2 text-sm">
                <input
                  type="checkbox"
                  className="size-4 accent-teal-700"
                  checked={core.useIdentityCard}
                  onChange={(e) => setCoreField('useIdentityCard', e.target.checked)}
                />
                Dùng CCCD/CMND thay hộ chiếu
              </label>
            </div>
          </fieldset>

          <fieldset className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
            <legend className="px-1 text-sm font-semibold text-slate-900">Ngân hàng</legend>
            <div className="grid gap-4 sm:grid-cols-2">
              {textField('Số tài khoản', core.bankAccount, (v) => setCoreField('bankAccount', v))}
              {textField('Ngân hàng', core.bankName, (v) => setCoreField('bankName', v))}
            </div>
          </fieldset>

          {extensions.map((slot) => (
            <fieldset key={slot.key} className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
              <legend className="px-1 text-sm font-semibold text-slate-900">{slot.title}</legend>
              {slot.description ? <p className="mb-3 text-sm text-slate-500">{slot.description}</p> : null}
              <div className="grid gap-4 sm:grid-cols-2">
                {slot.fields.map((field) => {
                  const value = extValues[slot.key]?.[field.name]
                  if (field.type === 'checkbox') {
                    return (
                      <label key={field.name} className="flex items-center gap-2 self-end pb-2 text-sm">
                        <input
                          type="checkbox"
                          className="size-4 accent-teal-700"
                          checked={Boolean(value)}
                          onChange={(e) => setExtField(slot.key, field.name, e.target.checked)}
                        />
                        {field.label}
                      </label>
                    )
                  }
                  if (field.type === 'select') {
                    return (
                      <label key={field.name} className="block">
                        <span className={labelClass}>{field.label}</span>
                        <select
                          className={fieldInputClass}
                          value={typeof value === 'string' ? value : ''}
                          onChange={(e) => setExtField(slot.key, field.name, e.target.value)}
                        >
                          <option value="">— Chọn —</option>
                          {(field.options ?? []).map((option) => (
                            <option key={option.value} value={option.value}>{option.label}</option>
                          ))}
                        </select>
                      </label>
                    )
                  }
                  if (field.type === 'textarea') {
                    return (
                      <label key={field.name} className="block sm:col-span-2">
                        <span className={labelClass}>{field.label}</span>
                        <textarea
                          className={`${fieldInputClass} h-auto resize-y py-2`}
                          rows={3}
                          value={typeof value === 'string' ? value : ''}
                          placeholder={field.placeholder}
                          onChange={(e) => setExtField(slot.key, field.name, e.target.value)}
                        />
                      </label>
                    )
                  }
                  return (
                    <label key={field.name} className="block">
                      <span className={labelClass}>{field.label}</span>
                      <input
                        className={fieldInputClass}
                        type={field.type ?? 'text'}
                        value={typeof value === 'string' ? value : ''}
                        placeholder={field.placeholder}
                        onChange={(e) => setExtField(slot.key, field.name, e.target.value)}
                      />
                    </label>
                  )
                })}
              </div>
            </fieldset>
          ))}

          <div className="flex justify-end">
            <button type="submit" className={btnPrimaryClass} disabled={saveMutation.isPending}>
              Lưu thông tin
            </button>
          </div>
        </form>
      )}
    </section>
  )
}
