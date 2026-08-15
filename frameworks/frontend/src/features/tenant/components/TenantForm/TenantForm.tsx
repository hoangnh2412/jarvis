import {
  Controller,
  type Control,
  type FieldErrors,
  type UseFormRegister,
} from 'react-hook-form'
import { InputText } from 'primereact/inputtext'
import { Label } from 'primereact/label'
import { Message } from 'primereact/message'
import { KitFileUpload } from '../../../../common/KitFileUpload'
import { FieldSelect } from '../FieldSelect'
import { fieldInputClass, fieldInputInvalidClass } from '../fieldStyles'
import { TENANT_STATUS_OPTIONS } from '../../types'
import type { TenantFormData } from '../../validation'

export type TenantFormProps = {
  register: UseFormRegister<TenantFormData>
  control: Control<TenantFormData>
  errors: FieldErrors<TenantFormData>
  /** Ẩn trường status (vd. khi edit status riêng) */
  hideStatus?: boolean
  /** Hiển thị khối upload tài liệu (UI — chưa gửi API tenant) */
  showAttachments?: boolean
}

export function TenantForm({
  register,
  control,
  errors,
  hideStatus = false,
  showAttachments = true,
}: TenantFormProps) {
  return (
    <div className="flex flex-col gap-4">
      <div>
        <Label
          htmlFor="tenant-code"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Mã tenant <span className="text-red-500">*</span>
        </Label>
        <InputText
          id="tenant-code"
          placeholder="vd. acme"
          unstyled
          className={errors.code ? fieldInputInvalidClass : fieldInputClass}
          {...register('code')}
        />
        {errors.code?.message && (
          <Message.Root severity="error" className="mt-1.5 border-0 bg-transparent p-0">
            <Message.Content>
              <Message.Text className="text-sm text-red-600">
                {errors.code.message}
              </Message.Text>
            </Message.Content>
          </Message.Root>
        )}
      </div>

      <div>
        <Label
          htmlFor="tenant-name"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Tên tenant <span className="text-red-500">*</span>
        </Label>
        <InputText
          id="tenant-name"
          placeholder="vd. Acme Corporation"
          unstyled
          className={errors.name ? fieldInputInvalidClass : fieldInputClass}
          {...register('name')}
        />
        {errors.name?.message && (
          <Message.Root severity="error" className="mt-1.5 border-0 bg-transparent p-0">
            <Message.Content>
              <Message.Text className="text-sm text-red-600">
                {errors.name.message}
              </Message.Text>
            </Message.Content>
          </Message.Root>
        )}
      </div>

      {!hideStatus && (
        <div>
          <Label
            htmlFor="tenant-status"
            className="mb-1.5 block text-sm font-medium text-slate-700"
          >
            Trạng thái
          </Label>
          <Controller
            name="status"
            control={control}
            render={({ field }) => (
              <FieldSelect
                id="tenant-status"
                value={field.value}
                options={TENANT_STATUS_OPTIONS}
                onChange={field.onChange}
                placeholder="Chọn trạng thái"
                invalid={Boolean(errors.status)}
              />
            )}
          />
          {errors.status?.message && (
            <Message.Root severity="error" className="mt-1.5 border-0 bg-transparent p-0">
              <Message.Content>
                <Message.Text className="text-sm text-red-600">
                  {errors.status.message}
                </Message.Text>
              </Message.Content>
            </Message.Root>
          )}
        </div>
      )}

      <div>
        <Label
          htmlFor="tenant-parent"
          className="mb-1.5 block text-sm font-medium text-slate-700"
        >
          Parent ID
        </Label>
        <InputText
          id="tenant-parent"
          placeholder="UUID tenant cha (reseller) — để trống nếu không có"
          unstyled
          className={
            errors.parentId ? fieldInputInvalidClass : fieldInputClass
          }
          {...register('parentId', {
            setValueAs: (v: string) => (v?.trim() ? v.trim() : null),
          })}
        />
        {errors.parentId?.message && (
          <Message.Root severity="error" className="mt-1.5 border-0 bg-transparent p-0">
            <Message.Content>
              <Message.Text className="text-sm text-red-600">
                {errors.parentId.message}
              </Message.Text>
            </Message.Content>
          </Message.Root>
        )}
      </div>

      {showAttachments ? (
        <KitFileUpload
          id="tenant-attachments"
          label="Tài liệu đính kèm"
          accept="image/*"
          multiple
          fileLimit={5}
          maxFileSize={10 * 1024 * 1024}
          customUpload
          showUploadButton={false}
          uploadHandler={() => {
            /* UI demo — nối API tenant sau */
          }}
        />
      ) : null}
    </div>
  )
}
