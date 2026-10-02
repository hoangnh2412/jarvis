import { useCallback, type KeyboardEvent, type MouseEvent } from 'react'
import { Plus, Upload, X } from 'lucide-react'
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
  /** `advanced` = kéo thả + click cả khung; `basic` = một hàng click chọn; `cccd`/`passport` = document modes với preview fit theo khung */
  mode?: 'basic' | 'advanced' | 'cccd' | 'passport'
  dropzoneTitle?: string
  dropzoneHint?: string
  showPreview?: boolean
  showUploadButton?: boolean
  showClearButton?: boolean
  uploadLabel?: string
  clearLabel?: string
  /** `single` = chỉ upload 1 file; `multiple` = upload nhiều file (mặc định) */
  uploadMode?: 'single' | 'multiple'
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

function usesDropzonePreview(mode?: KitFileUploadProps['mode']): boolean {
  return mode === 'cccd' || mode === 'passport' || mode === 'advanced'
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

function FilePreviewItem({
  file,
  index,
  uploadMode = 'multiple',
}: {
  file: File
  index: number
  uploadMode?: 'single' | 'multiple'
}) {
  const isImage = file.type.startsWith('image/')
  const sizeVariant = uploadMode === 'single' ? 'single' : 'multiple'

  return (
    <FileUploadItem file={file} index={index} className="kit-file-upload__preview-item">
      <div
        className={[
          'kit-file-upload__preview-content',
          `kit-file-upload__preview-content--${sizeVariant}`,
        ].join(' ')}
      >
        {isImage ? (
          <div
            className={[
              'kit-file-upload__preview-image-wrapper',
              `kit-file-upload__preview-image-wrapper--${sizeVariant}`,
            ].join(' ')}
          >
            <FileUploadItemPreview className="kit-file-upload__preview-image" />
            <FileUploadItemRemove
              type="button"
              className="kit-file-upload__preview-remove-button"
              aria-label="Xóa file"
              onClick={(event: MouseEvent<HTMLButtonElement>) => event.stopPropagation()}
            >
              <X className="size-3.5" />
            </FileUploadItemRemove>
          </div>
        ) : (
          <div
            className={[
              'kit-file-upload__preview-icon-wrapper',
              `kit-file-upload__preview-image-wrapper--${sizeVariant}`,
            ].join(' ')}
          >
            <span className="kit-file-upload__file-icon" aria-hidden>
              <Upload className="size-4" />
            </span>
            <FileUploadItemRemove
              type="button"
              className="kit-file-upload__preview-remove-button"
              aria-label="Xóa file"
              onClick={(event: MouseEvent<HTMLButtonElement>) => event.stopPropagation()}
            >
              <X className="size-3.5" />
            </FileUploadItemRemove>
          </div>
        )}
        <FileUploadItemName
          className={[
            'kit-file-upload__preview-name',
            uploadMode === 'multiple' ? 'kit-file-upload__preview-name--visible' : '',
          ]
            .filter(Boolean)
            .join(' ')}
        />
      </div>
    </FileUploadItem>
  )
}

function PreviewAddButton({
  onAdd,
  disabled,
}: {
  onAdd: () => void
  disabled?: boolean
}) {
  return (
    <button
      type="button"
      className="kit-file-upload__preview-add-button"
      aria-label="Thêm file"
      disabled={disabled}
      onClick={(event: MouseEvent<HTMLButtonElement>) => {
        event.stopPropagation()
        onAdd()
      }}
    >
      <Plus className="size-5" aria-hidden />
    </button>
  )
}

function KitFileUploadDropzoneWrapper({
  mode,
  dropzoneTitle,
  dropzoneHint,
  limitsText,
  disabled,
  uploadMode,
  fileLimit,
}: {
  mode?: 'basic' | 'advanced' | 'cccd' | 'passport'
  dropzoneTitle: string
  dropzoneHint: string
  limitsText: string | null
  disabled?: boolean
  uploadMode?: 'single' | 'multiple'
  fileLimit?: number
}) {
  const ctx = useFileUploadContext()
  const files = ctx?.state.files ?? []

  return (
    <KitFileUploadDropzone
      mode={mode}
      dropzoneTitle={dropzoneTitle}
      dropzoneHint={dropzoneHint}
      limitsText={limitsText}
      disabled={disabled}
      files={files}
      uploadMode={uploadMode}
      fileLimit={fileLimit}
    />
  )
}

function KitFileUploadDropzone({
  mode,
  dropzoneTitle,
  dropzoneHint,
  limitsText,
  disabled,
  files,
  uploadMode,
  fileLimit,
}: {
  mode?: 'basic' | 'advanced' | 'cccd' | 'passport'
  dropzoneTitle: string
  dropzoneHint: string
  limitsText: string | null
  disabled?: boolean
  files: File[]
  uploadMode?: 'single' | 'multiple'
  fileLimit?: number
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

  const hasFiles = files.length > 0
  const canAddMore =
    uploadMode === 'multiple' && (!fileLimit || fileLimit <= 0 || files.length < fileLimit)
  const dropzoneClass = [
    'kit-file-upload__dropzone',
    mode === 'basic' ? 'kit-file-upload__dropzone--basic' : '',
    hasFiles ? 'kit-file-upload__dropzone--filled' : '',
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <FileUploadContent
      className={dropzoneClass}
      role="button"
      tabIndex={disabled ? -1 : 0}
      aria-disabled={disabled || undefined}
      onClick={onClick}
      onKeyDown={onKeyDown}
    >
      {!hasFiles ? (
        <>
          <span className="kit-file-upload__dropzone-icon" aria-hidden>
            <Upload className={mode === 'basic' ? 'size-5' : 'size-6'} />
          </span>
          <p className="kit-file-upload__dropzone-title">{dropzoneTitle}</p>
          {mode !== 'basic' ? (
            <p className="kit-file-upload__dropzone-hint">{dropzoneHint}</p>
          ) : null}
        </>
      ) : (
        <div
          className="kit-file-upload__preview-container"
          onClick={(event: MouseEvent<HTMLDivElement>) => event.stopPropagation()}
        >
          {uploadMode === 'multiple' ? (
            <div className="kit-file-upload__preview-scroll">
              {files.map((file, index) => (
                <FilePreviewItem
                  key={`${file.name}-${file.size}-${index}`}
                  file={file}
                  index={index}
                  uploadMode={uploadMode}
                />
              ))}
              {canAddMore ? (
                <PreviewAddButton onAdd={openPicker} disabled={disabled} />
              ) : null}
            </div>
          ) : (
            <FilePreviewItem file={files[0]} index={0} uploadMode={uploadMode} />
          )}
        </div>
      )}

      {!hasFiles && limitsText ? (
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
  uploadMode = 'multiple',
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
  // Override multiple setting based on uploadMode
  const isMultipleMode = uploadMode === 'multiple'
  const fileUploadMultiple = isMultipleMode ? multiple : false

  const rootClass = [
    'kit-file-upload',
    mode === 'basic' ? 'kit-file-upload--basic' : mode === 'advanced' ? 'kit-file-upload--advanced' : `kit-file-upload--${mode}`,
    uploadMode === 'single' ? 'kit-file-upload--single' : 'kit-file-upload--multiple',
    invalid ? 'is-invalid' : '',
    disabled ? 'is-disabled' : '',
    className ?? '',
  ]
    .filter(Boolean)
    .join(' ')

  const resolvedLimitsText = buildLimitsText(fileLimit, maxFileSize, limitsText)
  const hideDropzoneExtras = usesDropzonePreview(mode)
  const showActions =
    !auto && !hideDropzoneExtras && (showUploadButton || showClearButton)
  const hideFileList = hideDropzoneExtras

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
        multiple={fileUploadMultiple}
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
        <KitFileUploadDropzoneWrapper
          mode={mode}
          dropzoneTitle={dropzoneTitle}
          dropzoneHint={dropzoneHint}
          limitsText={resolvedLimitsText}
          disabled={disabled}
          uploadMode={uploadMode}
          fileLimit={fileLimit}
        />

        {!hideFileList ? <KitFileUploadList showPreview={showPreview} /> : null}
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

/** Alias ngắn khi import từ @platform/core */
export { KitFileUpload as FileUploadField }
