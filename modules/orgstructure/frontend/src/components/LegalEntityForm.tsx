import { zodResolver } from '@hookform/resolvers/zod'
import { Building2, ClipboardList, CreditCard, Landmark, Phone, UserRound } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import type { LegalEntityFormData } from '../legalEntityForm'
import { btnPrimaryClass, btnSecondaryClass, fieldInputClass, fieldInputInvalidClass, labelClass } from './fieldStyles'

export const legalEntityFormSchema = z.object({
  taxCode: z.string().min(1, 'Mã số thuế không được để trống'),
  identityCode: z.string(),
  name: z.string().min(1, 'Tên đơn vị không được để trống'),
  address: z.string().min(1, 'Địa chỉ không được để trống'),
  province: z.string().min(1, 'Tỉnh/TP không được để trống'),
  bankAccount: z.string(),
  bankName: z.string(),
  paymentCode: z.string(),
  delegate: z.string(),
  position: z.string(),
  legalRepPhone: z.string(),
  legalRepDob: z.string(),
  legalRepPassport: z.string(),
  identityCard: z.string(),
  useIdentityCard: z.boolean(),
  legalRepGender: z.number(),
  email: z.string(),
  tel: z.string(),
  webSite: z.string(),
  fax: z.string(),
  cityName: z.string(),
  taxAuthorityName: z.string(),
  agentInfo: z.string(),
})

const emptyDefaults: LegalEntityFormData = {
  taxCode: '',
  identityCode: '',
  name: '',
  address: '',
  province: '',
  bankAccount: '',
  bankName: '',
  paymentCode: '',
  delegate: '',
  position: '',
  legalRepPhone: '',
  legalRepDob: '',
  legalRepPassport: '',
  identityCard: '',
  useIdentityCard: false,
  legalRepGender: 1,
  email: '',
  tel: '',
  webSite: '',
  fax: '',
  cityName: '',
  taxAuthorityName: '',
  agentInfo: '',
}

export type LegalEntityFormProps = {
  formId?: string
  initialValues?: Partial<LegalEntityFormData>
  onSubmit?: (data: LegalEntityFormData) => Promise<boolean | void> | boolean | void
  onCancel?: () => void
  submitLabel?: string
  isEdit?: boolean
  readOnly?: boolean
  isSubmitting?: boolean
}

function FieldError({ message }: { message?: string }) {
  if (!message) return null
  return <span className="mt-1 block text-xs text-red-600">{message}</span>
}

/** Form hồ sơ pháp nhân dùng cho dialog thêm/sửa và dialog xem hồ sơ. */
export function LegalEntityForm({
  formId = 'legal-entity-form',
  initialValues,
  onSubmit,
  onCancel,
  submitLabel = 'Lưu',
  isEdit = false,
  readOnly = false,
  isSubmitting = false,
}: LegalEntityFormProps) {
  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors },
  } = useForm<LegalEntityFormData>({
    resolver: zodResolver(legalEntityFormSchema),
    defaultValues: { ...emptyDefaults, ...initialValues },
  })

  const useIdentityCard = watch('useIdentityCard')
  const submitForm = handleSubmit(async (data) => {
    if (!onSubmit) return
    const saved = await onSubmit(data)
    // Chỉ cập nhật mốc dữ liệu sau khi API thành công để lỗi không làm mất nội dung đang sửa.
    if (saved !== false) reset(data)
  })
  const inputCls = (invalid?: string) => `${fieldInputClass} mt-1 ${invalid ? fieldInputInvalidClass : ''}`

  return (
    <form id={formId} onSubmit={submitForm} className="text-sm text-slate-800">
      <fieldset disabled={readOnly} className="m-0 min-w-0 border-0 p-0">
        <div className="grid grid-cols-1 items-stretch gap-4 xl:grid-cols-12">
          <section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5 xl:col-span-7">
            <h3 className="mb-4 flex items-center gap-3 text-base font-semibold text-slate-900">
              <Building2 aria-hidden className="size-5 text-teal-700" />
              Thông tin chung
            </h3>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-12">
              <div className="xl:col-span-3">
                <label className={labelClass}>Mã số thuế <span className="text-red-600">*</span></label>
                <input type="text" disabled={isEdit} className={`${inputCls(errors.taxCode?.message)} ${isEdit ? 'cursor-not-allowed bg-slate-50 text-slate-500' : ''}`} {...register('taxCode')} />
                <FieldError message={errors.taxCode?.message} />
              </div>
              <div className="xl:col-span-3">
                <label className={labelClass}>Mã định danh</label>
                <input type="text" className={inputCls()} {...register('identityCode')} />
              </div>
              <div className="sm:col-span-2 xl:col-span-6">
                <label className={labelClass}>Tên đơn vị <span className="text-red-600">*</span></label>
                <input type="text" className={inputCls(errors.name?.message)} {...register('name')} />
                <FieldError message={errors.name?.message} />
              </div>
              <div className="sm:col-span-2 xl:col-span-9">
                <label className={labelClass}>Địa chỉ <span className="text-red-600">*</span></label>
                <input type="text" className={inputCls(errors.address?.message)} {...register('address')} />
                <FieldError message={errors.address?.message} />
              </div>
              <div className="sm:col-span-2 xl:col-span-3">
                <label className={labelClass}>Tỉnh/TP <span className="text-red-600">*</span></label>
                <input type="text" className={inputCls(errors.province?.message)} {...register('province')} />
                <FieldError message={errors.province?.message} />
              </div>
            </div>
          </section>

          <section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5 xl:col-span-5">
            <h3 className="mb-4 flex items-center gap-3 text-base font-semibold text-slate-900">
              <CreditCard aria-hidden className="size-5 text-teal-700" />
              Thanh toán
            </h3>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-12">
              <div className="xl:col-span-5">
                <label className={labelClass}>Số tài khoản</label>
                <input type="text" className={inputCls()} {...register('bankAccount')} />
              </div>
              <div className="sm:col-span-2 xl:col-span-7">
                <label className={labelClass}>Ngân hàng</label>
                <input type="text" className={inputCls()} {...register('bankName')} />
              </div>
              <div className="sm:col-span-2 xl:col-span-12">
                <label className={labelClass}>Mã tích hợp thanh toán</label>
                <input type="text" className={inputCls()} {...register('paymentCode')} />
              </div>
            </div>
          </section>

          <section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5 xl:col-span-7">
            <h3 className="mb-4 flex items-center gap-3 text-base font-semibold text-slate-900">
              <UserRound aria-hidden className="size-5 text-teal-700" />
              Người đại diện pháp luật
            </h3>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <div>
                <label className={labelClass}>Người đại diện</label>
                <input type="text" className={inputCls()} {...register('delegate')} />
              </div>
              <div>
                <label className={labelClass}>Chức vụ</label>
                <input type="text" className={inputCls()} {...register('position')} />
              </div>
              <div>
                <label className={labelClass}>Điện thoại</label>
                <input type="text" className={inputCls()} {...register('legalRepPhone')} />
              </div>
              <div>
                <label className={labelClass}>Ngày sinh</label>
                <input type="date" className={inputCls()} {...register('legalRepDob')} />
              </div>
              <div>
                <label className={labelClass}>Hộ chiếu</label>
                <input type="text" className={inputCls()} {...register('legalRepPassport')} />
              </div>
              <div>
                <label className={labelClass}>Giới tính</label>
                <select className={inputCls()} {...register('legalRepGender', { valueAsNumber: true })}>
                  <option value={1}>Nam</option>
                  <option value={0}>Nữ</option>
                </select>
              </div>
              <div>
                <label className={labelClass}>CCCD {useIdentityCard ? <span className="text-red-600">*</span> : null}</label>
                <input type="text" className={inputCls()} {...register('identityCard')} />
              </div>
              <div className="flex min-h-9 items-center">
                <label className="inline-flex items-center gap-2 text-sm text-slate-800">
                  <input type="checkbox" className="size-4 rounded border-slate-300 accent-teal-700" {...register('useIdentityCard')} />
                  Hộ kinh doanh sử dụng CCCD thay cho mã số thuế
                </label>
              </div>
            </div>
          </section>

          <section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5 xl:col-span-5">
            <h3 className="mb-4 flex items-center gap-3 text-base font-semibold text-slate-900">
              <Phone aria-hidden className="size-5 text-teal-700" />
              Thông tin liên hệ
            </h3>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-12">
              <div className="xl:col-span-6">
                <label className={labelClass}>Email</label>
                <input type="text" className={inputCls()} {...register('email')} />
              </div>
              <div className="xl:col-span-6">
                <label className={labelClass}>Điện thoại</label>
                <input type="text" className={inputCls()} {...register('tel')} />
              </div>
              <div className="xl:col-span-6">
                <label className={labelClass}>Website</label>
                <input type="text" className={inputCls()} {...register('webSite')} />
              </div>
              <div className="xl:col-span-6">
                <label className={labelClass}>Fax</label>
                <input type="text" className={inputCls()} {...register('fax')} />
              </div>
            </div>
            <div className="mt-5 border-t border-slate-200 pt-4">
              <h4 className="mb-3 flex items-center gap-2 text-sm font-semibold text-slate-900">
                <Landmark aria-hidden className="size-4 text-teal-700" />
                Cơ quan thuế
              </h4>
              <div className="grid grid-cols-1 gap-3">
                <div>
                  <label className={labelClass}>CQT tỉnh</label>
                  <input type="text" className={inputCls()} {...register('cityName')} />
                </div>
                <div>
                  <label className={labelClass}>CQT quản lý</label>
                  <input type="text" className={inputCls()} {...register('taxAuthorityName')} />
                </div>
              </div>
            </div>
            <div className="mt-5 border-t border-slate-200 pt-4">
              <h4 className="mb-3 flex items-center gap-2 text-sm font-semibold text-slate-900">
                <ClipboardList aria-hidden className="size-4 text-teal-700" />
                Thông tin tra cứu
              </h4>
              <label className={labelClass}>Thông tin đại lý / tra cứu</label>
              <textarea className={`${fieldInputClass} mt-1 h-auto min-h-16 py-2`} {...register('agentInfo')} />
            </div>
          </section>
        </div>
      </fieldset>
      {!readOnly ? (
        <footer className="mt-4 flex justify-end gap-2">
          {onCancel ? (
            <button type="button" className={btnSecondaryClass} onClick={onCancel} disabled={isSubmitting}>Hủy</button>
          ) : null}
          <button type="submit" className={btnPrimaryClass} disabled={isSubmitting}>
            {isSubmitting ? 'Đang lưu…' : submitLabel}
          </button>
        </footer>
      ) : null}
    </form>
  )
}
