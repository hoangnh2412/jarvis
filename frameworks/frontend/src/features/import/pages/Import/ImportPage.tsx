import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react'
import type { FileUploadChangeEvent } from '@primereact/types/primitive/fileupload'
import { ArrowLeft } from 'lucide-react'
import { Button } from 'primereact/button'
import { Card } from 'primereact/card'
import { Label } from 'primereact/label'
import { ProgressSpinner } from 'primereact/progressspinner'
import { KitFileUpload } from '../../../../common/KitFileUpload'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { ImportPageShell, ImportValidationPanel } from '../../components'
import { btnOutlinedClass, btnSuccessClass } from '../../components/fieldStyles'
import { getImportMessages, type ImportLocale } from '../../localization'
import { getImportListFallbackPath, navigateImport } from '../../routes'
import { mockCommitImport, mockValidateImport } from '../../services'
import type { ImportCommitResult, ImportValidationResult } from '../../types'
import { resolveImportContent, type ImportSlotContent } from '../../utils'

const MAX_FILE_SIZE = 100 * 1024 * 1024

export type ImportPageContentContext = {
  selectedFile: File | null
  validation: ImportValidationResult | null
  validating: boolean
  committing: boolean
  canConfirm: boolean
  onConfirm: () => Promise<void>
  onCancel: () => void
  onBack: () => void
  DefaultContent: ReactNode
}

export type ImportPageProps = {
  locale?: ImportLocale
  title?: string
  description?: string
  className?: string
  backPath?: string
  backLabel?: string
  accept?: string
  maxFileSize?: number
  onConfirmed?: (result: ImportCommitResult) => void
  onCancelled?: () => void
  callback?: {
    validate?: ActionProps<{ file: File }, { file: File }, ImportValidationResult>
    commit?: ActionProps<{ file: File }, { file: File }, ImportCommitResult>
  }
  content?: ImportSlotContent<ImportPageContentContext>
  withShell?: boolean
  showBackButton?: boolean
}

export function ImportPage({
  locale = 'vi',
  title,
  description,
  className,
  backPath,
  backLabel,
  accept = '.csv,.xlsx,.xls,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,text/csv',
  maxFileSize = MAX_FILE_SIZE,
  onConfirmed,
  onCancelled,
  callback,
  content,
  withShell = true,
  showBackButton = true,
}: ImportPageProps) {
  const messages = getImportMessages(locale).page
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [validation, setValidation] = useState<ImportValidationResult | null>(null)
  const [validating, setValidating] = useState(false)
  const [committing, setCommitting] = useState(false)
  const [uploadKey, setUploadKey] = useState(0)
  const validateSeq = useRef(0)

  const canConfirm = Boolean(
    validation && validation.invalidCount === 0 && validation.validCount > 0,
  )

  const handleBack = useCallback(() => {
    navigateImport(backPath ?? getImportListFallbackPath())
  }, [backPath])

  const resetSelection = useCallback(() => {
    validateSeq.current += 1
    setSelectedFile(null)
    setValidation(null)
    setValidating(false)
    setUploadKey((key) => key + 1)
    onCancelled?.()
  }, [onCancelled])

  const runValidate = useCallback(
    async (file: File) => {
      const seq = ++validateSeq.current
      setValidating(true)
      setValidation(null)

      try {
        const outcome = await handleAction({
          ctx: { file },
          callback: callback?.validate,
          defaultSubmit: mockValidateImport,
          getPayload: ({ file: payloadFile }) => ({ file: payloadFile }),
        })
        if (seq !== validateSeq.current) return
        if (outcome.status === 'cancelled') {
          resetSelection()
          return
        }
        const result = outcome.result as ImportValidationResult
        setValidation(result)
      } catch (error) {
        if (seq !== validateSeq.current) return
        notify.error(getErrorMessage(error, messages.validateError))
        resetSelection()
      } finally {
        if (seq === validateSeq.current) {
          setValidating(false)
        }
      }
    },
    [callback?.validate, messages.validateError, resetSelection],
  )

  const onConfirm = useCallback(async () => {
    if (!selectedFile || !canConfirm) return

    setCommitting(true)
    try {
      const outcome = await handleAction({
        ctx: { file: selectedFile },
        callback: callback?.commit,
        defaultSubmit: mockCommitImport,
        getPayload: ({ file }) => ({ file }),
      })
      if (outcome.status === 'cancelled') return
      const result = outcome.result as ImportCommitResult
      if (result.success) {
        notify.success(result.message || messages.commitSuccess)
        onConfirmed?.(result)
        resetSelection()
      } else {
        notify.error(result.message || messages.commitError)
      }
    } catch (error) {
      notify.error(getErrorMessage(error, messages.commitError))
    } finally {
      setCommitting(false)
    }
  }, [
    callback?.commit,
    canConfirm,
    messages.commitError,
    messages.commitSuccess,
    onConfirmed,
    resetSelection,
    selectedFile,
  ])

  const onFileChange = useCallback(
    ({ acceptedFiles }: FileUploadChangeEvent) => {
      const next = acceptedFiles[0] ?? null
      setSelectedFile(next)
      setValidation(null)
      if (next) {
        void runValidate(next)
      }
    },
    [runValidate],
  )

  useEffect(() => {
    if (!selectedFile && validation) {
      setValidation(null)
    }
  }, [selectedFile, validation])

  const actionFooter = (
    <>
      <Button
        type="button"
        unstyled
        className={btnOutlinedClass}
        disabled={committing || validating}
        onClick={resetSelection}
      >
        {messages.cancelLabel}
      </Button>
      <Button
        type="button"
        unstyled
        className={btnSuccessClass}
        disabled={!canConfirm || committing || validating}
        onClick={onConfirm}
      >
        {committing ? messages.committingLabel : messages.confirmLabel}
      </Button>
    </>
  )

  const defaultContent = (
    <div className="kit-import-page__content mx-auto flex w-full max-w-8xl flex-col gap-5">
      <Card.Root className="overflow-hidden rounded-xl border border-line bg-white shadow-sm">
        <Card.Body className="p-5 sm:p-6">
          <Label htmlFor="import-file" className="mb-1.5 block text-sm font-medium text-slate-700">
            {messages.fileLabel}
          </Label>
          <KitFileUpload
            key={uploadKey}
            id="import-file"
            accept={accept}
            multiple={false}
            fileLimit={1}
            maxFileSize={maxFileSize}
            customUpload
            showUploadButton={false}
            showClearButton={false}
            disabled={validating || committing}
            dropzoneTitle={messages.dropzoneTitle}
            dropzoneHint={messages.dropzoneHint}
            onChange={onFileChange}
          />

          {validating ? (
            <div className="mt-4 flex items-center gap-2.5 rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-600">
              <ProgressSpinner.Root className="size-5">
                <ProgressSpinner.Track />
                <ProgressSpinner.Range />
              </ProgressSpinner.Root>
              <span>{messages.validatingLabel}</span>
            </div>
          ) : null}
        </Card.Body>
      </Card.Root>

      {validation && !validating ? (
        <ImportValidationPanel
          result={validation}
          messages={messages}
          footer={actionFooter}
        />
      ) : null}
    </div>
  )

  const contentCtx: ImportPageContentContext = {
    selectedFile,
    validation,
    validating,
    committing,
    canConfirm,
    onConfirm,
    onCancel: resetSelection,
    onBack: handleBack,
    DefaultContent: defaultContent,
  }

  const resolved = resolveImportContent(content, contentCtx, defaultContent)

  if (!withShell) {
    return <>{resolved}</>
  }

  return (
    <ImportPageShell
      title={title ?? messages.title}
      description={description ?? messages.description}
      className={className}
      headerActions={
        showBackButton ? (
          <Button type="button" unstyled className={btnOutlinedClass} onClick={handleBack}>
            <ArrowLeft className="size-4" aria-hidden />
            {backLabel ?? messages.backLabel}
          </Button>
        ) : undefined
      }
    >
      {resolved}
    </ImportPageShell>
  )
}

/** @deprecated Dùng `ImportPage` */
export const ImportStudentPage = ImportPage

export type ImportStudentPageProps = ImportPageProps
export type ImportStudentPageContentContext = ImportPageContentContext
