import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Button,
  Card,
  InputText,
  Label,
  Loading,
  Message,
  getErrorMessage,
  notify,
} from '@platform/core'
import { ImportExportPageShell } from '../components/PageShell'
import {
  btnOutlinedClass,
  fieldInputClass,
  fieldInputInvalidClass,
} from '../components/fieldStyles'
import { getTemplate, updateTemplateMetadata } from '../services'
import {
  getImportExportTemplateListPath,
  navigateImportExport,
} from '../routes'
import {
  templateMetadataSchema,
  type TemplateMetadataValues,
} from '../validation'

export type TemplateMetadataPageProps = {
  code: string
}

export function TemplateMetadataPage({ code }: TemplateMetadataPageProps) {
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<TemplateMetadataValues>({
    resolver: zodResolver(templateMetadataSchema),
  })

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      setLoading(true)
      try {
        const t = await getTemplate(code)
        if (cancelled) return
        reset({ code: t.code, name: t.name })
      } catch (err) {
        notify.error(getErrorMessage(err, 'Không tải được template'))
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [code, reset])

  const onSubmit = handleSubmit(async (values) => {
    setSaving(true)
    try {
      await updateTemplateMetadata(code, { name: values.name })
      notify.success('Đã cập nhật tên template')
      navigateImportExport(getImportExportTemplateListPath())
    } catch (err) {
      notify.error(getErrorMessage(err, 'Lưu template thất bại'))
    } finally {
      setSaving(false)
    }
  })

  if (loading) {
    return (
      <ImportExportPageShell title="Đang tải…">
        <Loading label="Vui lòng chờ." />
      </ImportExportPageShell>
    )
  }

  return (
    <ImportExportPageShell
      asForm
      title={`Sửa tên: ${code}`}
      description="Template code không thể đổi sau khi tạo."
      headerActions={
        <Button
          type="button"
          unstyled
          className={btnOutlinedClass}
          onClick={() => navigateImportExport(getImportExportTemplateListPath())}
        >
          Quay lại
        </Button>
      }
      submitLabel="Lưu"
      cancelLabel="Hủy"
      isSubmitting={saving}
      onSubmit={(e) => void onSubmit(e)}
      onCancel={() => navigateImportExport(getImportExportTemplateListPath())}
    >
      <Card.Root className="mx-auto w-full max-w-lg overflow-hidden rounded-xl border border-line bg-white shadow-sm">
        <Card.Body className="space-y-4 p-5 sm:p-6">
          <div>
            <Label
              htmlFor="template-code"
              className="mb-1.5 block text-sm font-medium text-slate-700"
            >
              Code
            </Label>
            <InputText
              id="template-code"
              unstyled
              readOnly
              disabled
              className={`${fieldInputClass} bg-slate-50 text-mute`}
              {...register('code')}
            />
          </div>

          <div>
            <Label
              htmlFor="template-name"
              className="mb-1.5 block text-sm font-medium text-slate-700"
            >
              Tên
            </Label>
            <InputText
              id="template-name"
              unstyled
              className={errors.name ? fieldInputInvalidClass : fieldInputClass}
              {...register('name')}
            />
            {errors.name?.message ? (
              <Message.Root
                severity="error"
                className="mt-1.5 border-0 bg-transparent p-0"
              >
                <Message.Content>
                  <Message.Text className="text-sm text-red-600">
                    {errors.name.message}
                  </Message.Text>
                </Message.Content>
              </Message.Root>
            ) : null}
          </div>
        </Card.Body>
      </Card.Root>
    </ImportExportPageShell>
  )
}
