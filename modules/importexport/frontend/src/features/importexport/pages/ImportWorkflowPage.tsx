import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react'
import type { FileUploadChangeEvent } from '@primereact/types/primitive/fileupload'
import { ArrowLeft } from 'lucide-react'
import {
  Button,
  Card,
  getErrorMessage,
  getImportMessages,
  ImportPageShell,
  KitFileUpload,
  Label,
  notify,
  ProgressSpinner,
  importBtnSuccessClass,
  type ImportLocale,
} from '@platform/core'
import { btnOutlinedClass } from '../components/fieldStyles'
import { ImportResultPanel } from '../components/ImportResultPanel'
import type { ImportResult } from '../types'

const MAX_FILE_SIZE = 100 * 1024 * 1024

export type ImportWorkflowOutcome = {
  result: ImportResult
  message?: string
  status?: number
}

export type ImportWorkflowCallbacks = {
  validate: (file: File) => Promise<ImportWorkflowOutcome>
  commit: (file: File) => Promise<ImportWorkflowOutcome>
}

export type ImportWorkflowPageProps = {
  locale?: ImportLocale
  title?: string
  description?: string
  className?: string
  backLabel?: string
  accept?: string
  maxFileSize?: number
  callbacks: ImportWorkflowCallbacks
  onConfirmed?: (result: ImportResult) => void
  onCancelled?: () => void
  headerActions?: ReactNode
  showBackButton?: boolean
  onBack?: () => void
}

export function ImportWorkflowPage({
  locale = 'vi',
  title,
  description,
  className,
  backLabel,
  accept = '.csv,.xlsx,.xls,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,text/csv',
  maxFileSize = MAX_FILE_SIZE,
  callbacks,
  onConfirmed,
  onCancelled,
  headerActions,
  showBackButton = true,
  onBack,
}: ImportWorkflowPageProps) {
  const messages = getImportMessages(locale).page
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [validation, setValidation] = useState<ImportResult | null>(null)
  const [validationMessage, setValidationMessage] = useState<string>()
  const [validating, setValidating] = useState(false)
  const [committing, setCommitting] = useState(false)
  const [uploadKey, setUploadKey] = useState(0)
  const validateSeq = useRef(0)

  const validCount = validation
    ? validation.sheets.reduce((total, sheet) => total + sheet.validRowCount, 0)
    : 0

  const canConfirm = Boolean(validation && validCount > 0)

  const handleBack = useCallback(() => {
    onBack?.()
  }, [onBack])

  const resetSelection = useCallback(() => {
    validateSeq.current += 1
    setSelectedFile(null)
    setValidation(null)
    setValidationMessage(undefined)
    setValidating(false)
    setUploadKey((key) => key + 1)
    onCancelled?.()
  }, [onCancelled])

  const runValidate = useCallback(
    async (file: File) => {
      const seq = ++validateSeq.current
      setValidating(true)
      setValidation(null)
      setValidationMessage(undefined)

      try {
        const outcome = await callbacks.validate(file)
        if (seq !== validateSeq.current) return
        setValidation(outcome.result)
        setValidationMessage(outcome.message)
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
    [callbacks, messages.validateError, resetSelection],
  )

  const onConfirm = useCallback(async () => {
    if (!selectedFile || !canConfirm) return

    setCommitting(true)
    try {
      const outcome = await callbacks.commit(selectedFile)
      const { result, message, status } = outcome
      const allRowsFailed =
        status === 422 || (result.failedRowCount > 0 && result.savedCount === 0)

      if (allRowsFailed) {
        setValidation(result)
        setValidationMessage(message)
        notify.warning(message || messages.commitError)
        return
      }

      if (result.failedRowCount > 0) {
        notify.warning(message || messages.commitSuccess)
      } else {
        notify.success(message || messages.commitSuccess)
      }
      onConfirmed?.(result)
      resetSelection()
    } catch (error) {
      notify.error(getErrorMessage(error, messages.commitError))
    } finally {
      setCommitting(false)
    }
  }, [
    callbacks,
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
      setValidationMessage(undefined)
      if (next) {
        void runValidate(next)
      }
    },
    [runValidate],
  )

  useEffect(() => {
    if (!selectedFile && validation) {
      setValidation(null)
      setValidationMessage(undefined)
    }
  }, [selectedFile, validation])

  const resolvedHeaderActions =
    headerActions ??
    (showBackButton && onBack ? (
      <Button type="button" unstyled className={btnOutlinedClass} onClick={handleBack}>
        <ArrowLeft className="size-4" aria-hidden />
        {backLabel ?? messages.backLabel}
      </Button>
    ) : undefined)

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
        className={importBtnSuccessClass}
        disabled={!canConfirm || committing || validating}
        onClick={onConfirm}
      >
        {committing ? messages.committingLabel : messages.confirmLabel}
      </Button>
    </>
  )

  return (
    <ImportPageShell
      title={title ?? messages.title}
      description={description ?? messages.description}
      className={className}
      headerActions={resolvedHeaderActions}
    >
      <div className="kit-import-page__content mx-auto flex w-full max-w-8xl flex-col gap-5">
        <Card.Root className="overflow-hidden rounded-xl border border-line bg-white shadow-sm">
          <Card.Body className="p-5 sm:p-6">
            <Label htmlFor="import-export-file" className="mb-1.5 block text-sm font-medium text-slate-700">
              {messages.fileLabel}
            </Label>
            <KitFileUpload
              key={uploadKey}
              id="import-export-file"
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
          <div className="space-y-4">
            <ImportResultPanel
              result={validation}
              message={validationMessage}
              mode="validate"
            />
            <div className="flex flex-wrap items-center justify-end gap-2.5">{actionFooter}</div>
          </div>
        ) : null}
      </div>
    </ImportPageShell>
  )
}
