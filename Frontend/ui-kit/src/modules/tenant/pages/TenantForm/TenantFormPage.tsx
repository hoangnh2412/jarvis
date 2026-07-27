import { useEffect, type FormEventHandler, type ReactNode } from 'react'
import type {
  Control,
  FieldErrors,
  UseFormHandleSubmit,
  UseFormRegister,
} from 'react-hook-form'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { TenantForm } from '../../components/TenantForm'
import { TenantPageShell } from '../../components/TenantPageShell'
import { useTenantForm } from '../../hooks'
import { getTenantMessages, type TenantLocale } from '../../localization'
import { getTenantListPath, navigateTenant } from '../../routes'
import { callCreateTenant, callUpdateTenant } from '../../services'
import { Card } from 'primereact/card'
import {
  resolveTenantContent,
  type TenantSlotContent,
} from '../../utils'
import {
  tenantFormDefaultValues,
  type TenantFormData,
} from '../../validation'

export type TenantFormPageContentContext = {
  mode: 'create' | 'edit'
  tenantId?: string
  register: UseFormRegister<TenantFormData>
  control: Control<TenantFormData>
  errors: FieldErrors<TenantFormData>
  isSubmitting: boolean
  handleSubmit: UseFormHandleSubmit<TenantFormData>
  /** Handler gắn vào <form onSubmit> — đã validate + toast */
  submit: FormEventHandler<HTMLFormElement>
  onCancel?: () => void
  hideStatus: boolean
  DefaultContent: ReactNode
}

export type TenantFormPageProps = {
  mode?: 'create' | 'edit'
  /** Bắt buộc khi `mode="edit"` và dùng API mặc định */
  tenantId?: string
  defaultValues?: Partial<TenantFormData>
  onCancel?: () => void
  /**
   * `false` = không navigate về list khi cancel / sau save (nếu không truyền onCancel).
   * @default true
   */
  useRoutes?: boolean
  title?: string
  description?: string
  submitLabel?: string
  cancelLabel?: string
  locale?: TenantLocale
  className?: string
  hideStatus?: boolean
  /** Action callback kiểu jQuery ajax. `callback.onSubmit` thay create/update API. */
  callback?: ActionProps<
    { data: TenantFormData; mode: 'create' | 'edit' },
    TenantFormData
  >
  content?: TenantSlotContent<TenantFormPageContentContext>
  withShell?: boolean
}

export function TenantFormPage({
  mode = 'create',
  tenantId,
  defaultValues,
  onCancel,
  useRoutes = true,
  title,
  description,
  submitLabel,
  cancelLabel,
  locale = 'vi',
  className,
  hideStatus = false,
  callback,
  content,
  withShell = true,
}: TenantFormPageProps) {
  const messages = getTenantMessages(locale).form
  const isEdit = mode === 'edit'

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useTenantForm({ defaultValues })

  useEffect(() => {
    reset({
      ...tenantFormDefaultValues,
      ...defaultValues,
    })
  }, [defaultValues, reset])

  const defaultSubmit = async (data: TenantFormData) => {
    if (isEdit) {
      if (!tenantId) {
        throw new Error('Thiếu tenantId khi cập nhật tenant')
      }
      await callUpdateTenant(tenantId, {
        code: data.code,
        name: data.name,
        parentId: data.parentId ?? null,
      })
    } else {
      await callCreateTenant({
        code: data.code,
        name: data.name,
        status: data.status,
        parentId: data.parentId ?? null,
      })
    }
  }

  const submit = handleSubmit(async (data) => {
    try {
      const outcome = await handleAction({
        ctx: { data, mode },
        callback,
        defaultSubmit,
        getPayload: ({ data: payload }) => payload,
        onSuccess: async () => {
          if (useRoutes) navigateTenant(getTenantListPath())
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(isEdit ? messages.updateSuccess : messages.saveSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.saveError))
    }
  })

  const handleCancel =
    onCancel ??
    (useRoutes ? () => navigateTenant(getTenantListPath()) : undefined)

  const defaultFields = (
    <Card.Root className="mx-auto w-full max-w-8xl overflow-hidden rounded-xl border border-line bg-white shadow-sm">
      <Card.Body className="p-5 sm:p-6">
        <TenantForm
          register={register}
          control={control}
          errors={errors}
          hideStatus={hideStatus}
        />
      </Card.Body>
    </Card.Root>
  )

  const contentCtx: TenantFormPageContentContext = {
    mode,
    tenantId,
    register,
    control,
    errors,
    isSubmitting,
    handleSubmit,
    submit,
    onCancel: handleCancel,
    hideStatus,
    DefaultContent: defaultFields,
  }

  const resolved = resolveTenantContent(content, contentCtx, defaultFields)

  if (!withShell) {
    return <>{resolved}</>
  }

  return (
    <TenantPageShell
      asForm
      title={title ?? (isEdit ? messages.editTitle : messages.createTitle)}
      description={
        description ??
        (isEdit ? messages.editDescription : messages.createDescription)
      }
      submitLabel={
        submitLabel ?? (isEdit ? messages.editSubmit : messages.createSubmit)
      }
      cancelLabel={cancelLabel}
      submittingLabel={messages.submitting}
      isSubmitting={isSubmitting}
      onSubmit={submit}
      onCancel={handleCancel}
      className={className}
    >
      {resolved}
    </TenantPageShell>
  )
}
