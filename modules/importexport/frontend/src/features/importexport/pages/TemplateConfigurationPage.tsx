import { useEffect, useMemo, useState } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Button,
  Card,
  ConfirmDialog,
  FieldSelect,
  InputText,
  Label,
  Loading,
  Message,
  getErrorMessage,
  notify,
} from '@platform/core'
import { MappingTable, normalizeMappingRowErrors } from '../components/MappingTable'
import { ImportExportPageShell } from '../components/PageShell'
import {
  btnOutlinedClass,
  fieldInputClass,
  fieldInputInvalidClass,
} from '../components/fieldStyles'
import {
  getImportExportEntityFields,
  getTemplate,
  listImportExportEntities,
  updateTemplateConfiguration,
} from '../services'
import type { ImportExportEntitySummary, ImportExportFieldSchema } from '../types'
import {
  getImportExportTemplateListPath,
  navigateImportExport,
} from '../routes'
import {
  templateConfigurationDefaultValues,
  templateConfigurationSchema,
  type TemplateConfigurationValues,
} from '../validation'

export type TemplateConfigurationPageProps = {
  code: string
}

function hasExistingMappings(mappings: TemplateConfigurationValues['mappings']) {
  return mappings.some(
    (row) =>
      row.columnName.trim() !== '' ||
      row.fieldName.trim() !== '' ||
      row.required ||
      row.unique ||
      row.regex ||
      row.minLength != null ||
      row.maxLength != null ||
      row.min != null ||
      row.max != null,
  )
}

export function TemplateConfigurationPage({ code }: TemplateConfigurationPageProps) {
  const [file, setFile] = useState<File | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [entities, setEntities] = useState<ImportExportEntitySummary[]>([])
  const [selectedEntityKey, setSelectedEntityKey] = useState('')
  const [fieldSchemas, setFieldSchemas] = useState<
    Record<string, ImportExportFieldSchema>
  >({})
  const [loadingFields, setLoadingFields] = useState(false)
  const [pendingEntityKey, setPendingEntityKey] = useState<string | null>(null)
  const [existingFileName, setExistingFileName] = useState('')

  const {
    register,
    control,
    handleSubmit,
    reset,
    setValue,
    getValues,
    watch,
    clearErrors,
    formState: { errors },
  } = useForm<TemplateConfigurationValues>({
    resolver: zodResolver(templateConfigurationSchema),
    defaultValues: templateConfigurationDefaultValues,
  })

  const fileName = watch('fileName')

  const isExcelFile = useMemo(() => {
    const name = (file?.name ?? fileName ?? existingFileName).toLowerCase()
    return name.endsWith('.xlsx')
  }, [file, fileName, existingFileName])

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      try {
        const list = await listImportExportEntities()
        if (!cancelled) setEntities(list)
      } catch (err) {
        notify.error(getErrorMessage(err, 'Không tải được danh sách đối tượng'))
      }
    })()
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      setLoading(true)
      try {
        const t = await getTemplate(code)
        if (cancelled) return
        setExistingFileName(t.fileName)
        const mappings =
          t.mappings?.length > 0
            ? t.mappings
            : (t.sheets?.[0]?.mappings ?? templateConfigurationDefaultValues.mappings)
        reset({
          sheetName: t.sheetName ?? t.sheets?.[0]?.sheetName ?? '',
          headerRow: t.headerRow,
          startRow: t.startRow,
          rowCount: t.rowCount ?? null,
          mappings,
          fileName: t.fileName,
        })
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

  const entityOptions = entities.map((entity) => ({
    label: entity.displayName,
    value: entity.key,
  }))

  const loadEntitySchemas = async (entityKey: string) => {
    if (!entityKey) return

    setLoadingFields(true)
    try {
      const fields = await getImportExportEntityFields(entityKey)
      if (fields.length === 0) {
        notify.error('Đối tượng không có field nào khả dụng')
        setSelectedEntityKey('')
        setFieldSchemas({})
        return
      }

      setFieldSchemas(Object.fromEntries(fields.map((f) => [f.name, f])))
      setSelectedEntityKey(entityKey)
    } catch (err) {
      notify.error(getErrorMessage(err, 'Không tải được fields của đối tượng'))
      setSelectedEntityKey('')
      setFieldSchemas({})
    } finally {
      setLoadingFields(false)
    }
  }

  const autoFillMappingsFromEntity = () => {
    if (!selectedEntityKey) return

    const fields = Object.values(fieldSchemas)
    if (fields.length === 0) {
      notify.error('Chưa có schema field — chọn đối tượng trước')
      return
    }

    if (hasExistingMappings(getValues('mappings'))) {
      setPendingEntityKey(selectedEntityKey)
      return
    }

    applyMappingsFromSchemas(fields)
  }

  const applyMappingsFromSchemas = (fields: ImportExportFieldSchema[]) => {
    setValue(
      'mappings',
      fields.map((field) => ({
        columnName: '',
        fieldName: field.name,
        required: field.required,
        unique: false,
        regex: field.regex ?? null,
        minLength: field.minLength ?? null,
        maxLength: field.maxLength ?? null,
        min: field.isNumeric ? (field.min ?? null) : null,
        max: field.isNumeric ? (field.max ?? null) : null,
      })),
      { shouldValidate: false },
    )
    clearErrors('mappings')
    setPendingEntityKey(null)
  }

  const onEntityChange = (entityKey: string) => {
    if (!entityKey) {
      setSelectedEntityKey('')
      setFieldSchemas({})
      return
    }

    void loadEntitySchemas(entityKey)
  }

  const onSubmit = handleSubmit(async (values) => {
    setSaving(true)
    try {
      await updateTemplateConfiguration(code, {
        file,
        mappings: values.mappings,
        sheetName: values.sheetName || null,
        headerRow: values.headerRow,
        startRow: values.startRow,
        rowCount: values.rowCount ?? null,
      })
      notify.success('Đã cập nhật cấu hình template')
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
    <>
      <ImportExportPageShell
        asForm
        title={`Cấu hình: ${code}`}
        description="File mẫu, rows và column mappings."
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
        <Card.Root className="mx-auto w-full max-w-8xl overflow-hidden rounded-xl border border-line bg-white shadow-sm">
          <Card.Body className="space-y-4 p-5 sm:p-6">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="sm:col-span-2">
                <Label
                  htmlFor="template-entity"
                  className="mb-1.5 block text-sm font-medium text-slate-700"
                >
                  Đối tượng (nạp schema field)
                </Label>
                <div className="flex flex-wrap items-start gap-2">
                  <div className="min-w-0 flex-1">
                    <FieldSelect
                      id="template-entity"
                      value={selectedEntityKey || null}
                      options={entityOptions}
                      onChange={onEntityChange}
                      placeholder="— Chọn đối tượng —"
                      disabled={loadingFields || entities.length === 0}
                    />
                  </div>
                  <Button
                    type="button"
                    unstyled
                    className={btnOutlinedClass}
                    disabled={
                      loadingFields ||
                      !selectedEntityKey ||
                      Object.keys(fieldSchemas).length === 0
                    }
                    onClick={autoFillMappingsFromEntity}
                  >
                    Điền mappings
                  </Button>
                </div>
                {loadingFields ? (
                  <p className="mt-1.5 m-0 text-xs text-mute">Đang tải schema…</p>
                ) : selectedEntityKey ? (
                  <p className="mt-1.5 m-0 text-xs text-mute">
                    Schema đã nạp — Min/Max/MinLen/MaxLen theo kiểu field. Dùng
                    “Điền mappings” nếu muốn thay toàn bộ mapping bằng fields của
                    đối tượng.
                  </p>
                ) : null}
              </div>

              <div className="sm:col-span-2">
                <Label
                  htmlFor="template-file"
                  className="mb-1.5 block text-sm font-medium text-slate-700"
                >
                  File mẫu (.xlsx / .csv) — hiện tại: {existingFileName}
                </Label>
                <input
                  id="template-file"
                  type="file"
                  accept=".xlsx,.csv,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,text/csv"
                  onChange={(e) => {
                    const next = e.target.files?.[0] ?? null
                    setFile(next)
                    setValue('fileName', next?.name ?? existingFileName, {
                      shouldValidate: true,
                    })
                  }}
                  className="block w-full text-sm text-slate-600 file:mr-3 file:rounded-lg file:border-0 file:bg-teal-50 file:px-3 file:py-2 file:text-sm file:font-medium file:text-teal-800 hover:file:bg-teal-100"
                />
              </div>

              <div>
                <Label
                  htmlFor="template-sheet"
                  className="mb-1.5 block text-sm font-medium text-slate-700"
                >
                  Sheet name {isExcelFile ? '(bắt buộc với .xlsx)' : '(optional với .csv)'}
                </Label>
                <InputText
                  id="template-sheet"
                  unstyled
                  className={
                    errors.sheetName ? fieldInputInvalidClass : fieldInputClass
                  }
                  placeholder={isExcelFile ? 'VD: Sheet1' : 'Để trống nếu CSV'}
                  {...register('sheetName')}
                />
                {errors.sheetName?.message ? (
                  <Message.Root
                    severity="error"
                    className="mt-1.5 border-0 bg-transparent p-0"
                  >
                    <Message.Content>
                      <Message.Text className="text-sm text-red-600">
                        {errors.sheetName.message}
                      </Message.Text>
                    </Message.Content>
                  </Message.Root>
                ) : null}
              </div>

              <div className="grid grid-cols-3 gap-3 sm:col-span-2">
                <div>
                  <Label
                    htmlFor="template-header-row"
                    className="mb-1.5 block text-sm font-medium text-slate-700"
                  >
                    Header row
                  </Label>
                  <InputText
                    id="template-header-row"
                    unstyled
                    type="number"
                    className={fieldInputClass}
                    {...register('headerRow', { valueAsNumber: true })}
                  />
                </div>
                <div>
                  <Label
                    htmlFor="template-start-row"
                    className="mb-1.5 block text-sm font-medium text-slate-700"
                  >
                    Start row
                  </Label>
                  <InputText
                    id="template-start-row"
                    unstyled
                    type="number"
                    className={fieldInputClass}
                    {...register('startRow', { valueAsNumber: true })}
                  />
                </div>
                <div>
                  <Label
                    htmlFor="template-row-count"
                    className="mb-1.5 block text-sm font-medium text-slate-700"
                  >
                    Row count
                  </Label>
                  <InputText
                    id="template-row-count"
                    unstyled
                    type="number"
                    className={fieldInputClass}
                    placeholder="optional"
                    {...register('rowCount', {
                      setValueAs: (v) =>
                        v === '' || v == null || Number.isNaN(Number(v))
                          ? null
                          : Number(v),
                    })}
                  />
                </div>
              </div>
            </div>

            <Controller
              name="mappings"
              control={control}
              render={({ field }) => (
                <MappingTable
                  value={field.value}
                  onChange={field.onChange}
                  fieldSchemas={fieldSchemas}
                  rowErrors={normalizeMappingRowErrors(errors.mappings)}
                />
              )}
            />
          </Card.Body>
        </Card.Root>
      </ImportExportPageShell>

      <ConfirmDialog
        open={pendingEntityKey != null}
        onClose={() => setPendingEntityKey(null)}
        onConfirm={() => {
          const fields = Object.values(fieldSchemas)
          if (fields.length > 0) applyMappingsFromSchemas(fields)
        }}
        title="Thay thế mappings?"
        description="Thay thế mappings hiện tại bằng fields của đối tượng đã chọn? Column name sẽ bị xóa."
        confirmText="Thay thế"
      />
    </>
  )
}
