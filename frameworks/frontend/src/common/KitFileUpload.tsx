import { useCallback, type KeyboardEvent, type MouseEvent } from 'react'
import { Upload, X } from 'lucide-react'
import {
  FileUploadClear,
  FileUploadContent,
  FileUploadItem,
  FileUploadItemGroup,
  FileUploadItemInfo,
  FileUploadItemName,
  FileUploadItemPreview,
  FileUploadItemRemove,
  FileUploadItemSize,
  FileUploadRoot,
  FileUploadUpload,
  useFileUploadContext,
} from 'primereact/fileupload'
import type { FileUploadRootProps } from '@primereact/types/primitive/fileupload'
import { Label } from 'primereact/label'
import { Message } from 'primereact/message'

const btnPrimaryClass =
  'pr-btn-primary inline-flex h-10 items-center justify-center gap-2 rounded-xl border-0 px-4 text-sm font-semibold shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30 disabled:cursor-not-allowed disabled:opacity-60'

const btnOutlinedClass =
  'pr-btn-outlined inline-flex h-10 items-center justify-center gap-2 rounded-xl border px-4 text-sm font-medium shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60'

export type KitFileUploadProps = Omit<FileUploadRootProps, 'children'> & {
  id?: string
  label?: string
  /** Ghi đè text giới hạn trong khung; mặc định tự sinh từ fileLimit + maxFileSize */
  limitsText?: string
  errorMessage?: string
  invalid?: boolean
  className?: string
  /** `advanced` = kéo thả + click cả khung; `basic` = một hàng click chọn */
  mode?: 'basic' | 'advanced'
  dropzoneTitle?: string
  dropzoneHint?: string
  showPreview?: boolean
  showUploadButton?: boolean
  showClearButton?: boolean
  uploadLabel?: string
  clearLabel?: string
}

function formatMaxFileSize(bytes?: number): string {
  if (!bytes || bytes <= 0) return ''
  const mb = bytes / (1024 * 1024)
  if (mb >= 1) {
    const rounded = Number.isInteger(mb) ? mb : Math.round(mb * 10) / 10
    return `${rounded}MB`
  }
  const kb = bytes / 1024
  return `${Math.round(kb)}KB`
}

function buildLimitsText(
  fileLimit?: number,
  maxFileSize?: number,
  override?: string,
): string | null {
  if (override?.trim()) return override.trim()
  const parts: string[] = []
  if (fileLimit && fileLimit > 0) parts.push(`Tối đa ${fileLimit} file`)
  const sizeLabel = formatMaxFileSize(maxFileSize)
  if (sizeLabel) parts.push(`mỗi file không quá ${sizeLabel}`)
  return parts.length > 0 ? parts.join(', ') : null
}

function KitFileUploadDropzone({
  mode,
  dropzoneTitle,
  dropzoneHint,
  limitsText,
  disabled,
}: {
  mode: 'basic' | 'advanced'
  dropzoneTitle: string
  dropzoneHint: string
  limitsText: string | null
  disabled?: boolean
}) {
  const ctx = useFileUploadContext()

  const openPicker = useCallback(() => {
    if (disabled) return
    ctx?.choose()
  }, [ctx, disabled])

  const onClick = useCallback(
    (event: MouseEvent<HTMLDivElement>) => {
      event.preventDefault()
      openPicker()
    },
    [openPicker],
  )

  const onKeyDown = useCallback(
    (event: KeyboardEvent<HTMLDivElement>) => {
      if (disabled) return
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault()
        openPicker()
      }
    },
    [disabled, openPicker],
  )

  if (mode === 'basic') {
    return (
      <FileUploadContent
        className="kit-file-upload__dropzone kit-file-upload__dropzone--basic"
        role="button"
        tabIndex={disabled ? -1 : 0}
        aria-disabled={disabled || undefined}
        onClick={onClick}
        onKeyDown={onKeyDown}
      >
        <span className="kit-file-upload__dropzone-icon" aria-hidden>
          <Upload className="size-5" />
        </span>
        <p className="kit-file-upload__dropzone-title">{dropzoneTitle}</p>
        {limitsText ? (
          <p className="kit-file-upload__dropzone-limits">{limitsText}</p>
        ) : null}
      </FileUploadContent>
    )
  }

  return (
    <FileUploadContent
      className="kit-file-upload__dropzone"
      role="button"
      tabIndex={disabled ? -1 : 0}
      aria-disabled={disabled || undefined}
      onClick={onClick}
      onKeyDown={onKeyDown}
    >
      <span className="kit-file-upload__dropzone-icon" aria-hidden>
        <Upload className="size-6" />
      </span>
      <p className="kit-file-upload__dropzone-title">{dropzoneTitle}</p>
      <p className="kit-file-upload__dropzone-hint">{dropzoneHint}</p>
      {limitsText ? (
        <p className="kit-file-upload__dropzone-limits">{limitsText}</p>
      ) : null}
    </FileUploadContent>
  )
}

function KitFileUploadList({
  showPreview = true,
}: {
  showPreview?: boolean
}) {
  const ctx = useFileUploadContext()
  const files = ctx?.state.files ?? []

  if (files.length === 0) return null

  return (
    <FileUploadItemGroup className="kit-file-upload__list">
      {files.map((file, index) => (
        <FileUploadItem
          key={`${file.name}-${file.size}-${index}`}
          file={file}
          index={index}
          className="kit-file-upload__item"
        >
          {showPreview && file.type.startsWith('image/') ? (
            <FileUploadItemPreview className="kit-file-upload__preview" />
          ) : (
            <span className="kit-file-upload__file-icon" aria-hidden>
              <Upload className="size-4" />
            </span>
          )}
          <FileUploadItemInfo className="kit-file-upload__info">
            <FileUploadItemName className="kit-file-upload__name" />
            <FileUploadItemSize className="kit-file-upload__size" />
          </FileUploadItemInfo>
          <FileUploadItemRemove
            type="button"
            className="kit-file-upload__remove"
            aria-label="Xóa file"
            onClick={(event: MouseEvent<HTMLButtonElement>) => event.stopPropagation()}
          >
            <X className="size-4" />
          </FileUploadItemRemove>
        </FileUploadItem>
      ))}
    </FileUploadItemGroup>
  )
}

function KitFileUploadProgress() {
  const ctx = useFileUploadContext()
  const progress = ctx?.state.progress ?? 0

  if (progress <= 0 || progress >= 100) return null

  return (
    <div className="kit-file-upload__progress" role="progressbar" aria-valuenow={progress}>
      <div className="kit-file-upload__progress-bar" style={{ width: `${progress}%` }} />
      <span className="kit-file-upload__progress-label">{Math.round(progress)}%</span>
    </div>
  )
}

function KitFileUploadActions({
  showUploadButton = true,
  showClearButton = true,
  uploadLabel = 'Tải lên',
  clearLabel = 'Xóa tất cả',
}: {
  showUploadButton?: boolean
  showClearButton?: boolean
  uploadLabel?: string
  clearLabel?: string
}) {
  const ctx = useFileUploadContext()
  const hasFiles = (ctx?.state.files.length ?? 0) > 0

  if (!showUploadButton && !showClearButton) return null
  if (!hasFiles) return null

  return (
    <div className="kit-file-upload__actions">
      {showUploadButton && (
        <FileUploadUpload
          type="button"
          className={`${btnPrimaryClass} kit-file-upload__btn-upload`}
        >
          {uploadLabel}
        </FileUploadUpload>
      )}
      {showClearButton && (
        <FileUploadClear
          type="button"
          className={`${btnOutlinedClass} kit-file-upload__btn-clear`}
        >
          {clearLabel}
        </FileUploadClear>
      )}
    </div>
  )
}

export function KitFileUpload({
  id,
  label,
  limitsText,
  errorMessage,
  invalid = false,
  className,
  mode = 'advanced',
  dropzoneTitle = 'Kéo thả ảnh vào đây',
  dropzoneHint = 'hoặc bấm để chọn từ máy',
  showPreview = true,
  showUploadButton = true,
  showClearButton = true,
  uploadLabel = 'Tải lên',
  clearLabel = 'Xóa tất cả',
  multiple = true,
  accept = 'image/*',
  customUpload = true,
  auto = false,
  disabled = false,
  fileLimit,
  maxFileSize,
  invalidFileLimitMessage = 'Vượt quá số file cho phép (tối đa {0}).',
  invalidFileSizeMessage = '{0}: Dung lượng vượt quá {1}.',
  invalidFileTypeMessage = '{0}: Loại file không hợp lệ. Cho phép: {1}.',
  ...rootProps
}: KitFileUploadProps) {
  const rootClass = [
    'kit-file-upload',
    mode === 'basic' ? 'kit-file-upload--basic' : 'kit-file-upload--advanced',
    invalid ? 'is-invalid' : '',
    disabled ? 'is-disabled' : '',
    className ?? '',
  ]
    .filter(Boolean)
    .join(' ')

  const resolvedLimitsText = buildLimitsText(fileLimit, maxFileSize, limitsText)
  const showActions = !auto && (showUploadButton || showClearButton)

  return (
    <div className={rootClass}>
      {label ? (
        <Label htmlFor={id} className="kit-file-upload__label">
          {label}
        </Label>
      ) : null}

      <FileUploadRoot
        id={id}
        className="kit-file-upload__root"
        multiple={multiple}
        accept={accept}
        fileLimit={fileLimit}
        maxFileSize={maxFileSize}
        customUpload={customUpload}
        auto={auto}
        disabled={disabled}
        invalidFileLimitMessage={invalidFileLimitMessage}
        invalidFileSizeMessage={invalidFileSizeMessage}
        invalidFileTypeMessage={invalidFileTypeMessage}
        {...rootProps}
      >
        <KitFileUploadDropzone
          mode={mode}
          dropzoneTitle={dropzoneTitle}
          dropzoneHint={dropzoneHint}
          limitsText={resolvedLimitsText}
          disabled={disabled}
        />

        <KitFileUploadList showPreview={showPreview} />
        <KitFileUploadProgress />

        {showActions ? (
          <KitFileUploadActions
            showUploadButton={showUploadButton}
            showClearButton={showClearButton}
            uploadLabel={uploadLabel}
            clearLabel={clearLabel}
          />
        ) : null}
      </FileUploadRoot>

      {errorMessage ? (
        <Message.Root severity="error" className="kit-file-upload__error">
          <Message.Content>
            <Message.Text>{errorMessage}</Message.Text>
          </Message.Content>
        </Message.Root>
      ) : null}
    </div>
  )
}

/** Alias ngắn khi import từ @jarvis/core */
export { KitFileUpload as FileUploadField }
