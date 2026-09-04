import { useRef } from 'react'
import { Button } from '@jarvis/core'
import { ImagePlus, Trash2 } from 'lucide-react'
import {
  getImageAccept,
  getImageDataUrlDecodedBytes,
  isAllowedImageMimeType,
} from '../utils/imageConstraints'
import { getImageMaxBytes, getImageMimeTypes } from '../utils/typeOptions'

type SettingImageFieldProps = {
  value: string
  options?: string | null
  disabled?: boolean
  invalid?: boolean
  onChange: (value: string) => void
  onError?: (message: string) => void
}

export function SettingImageField({
  value,
  options,
  disabled = false,
  invalid = false,
  onChange,
  onError,
}: SettingImageFieldProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const maxBytes = getImageMaxBytes(options)
  const mimeLabel = getImageMimeTypes(options).join(', ')
  const maxMb = maxBytes / (1024 * 1024)
  const maxMbLabel = maxMb % 1 === 0 ? String(maxMb) : maxMb.toFixed(2)
  const preview =
    getImageDataUrlDecodedBytes(value, options) !== null ? value.trim() : ''

  const handleFile = (file: File | undefined) => {
    if (!file) return
    if (!isAllowedImageMimeType(file.type, options)) {
      onError?.(`File phải là ảnh (${mimeLabel}).`)
      return
    }
    if (file.size > maxBytes) {
      onError?.(`Ảnh vượt quá ${maxMbLabel}MB.`)
      return
    }

    const reader = new FileReader()
    reader.onload = () => {
      const result = typeof reader.result === 'string' ? reader.result : ''
      if (getImageDataUrlDecodedBytes(result, options) === null) {
        onError?.(`Ảnh phải là data URL (${mimeLabel}).`)
        return
      }
      onChange(result)
    }
    reader.onerror = () => onError?.('Không đọc được file ảnh.')
    reader.readAsDataURL(file)
  }

  return (
    <div className="flex flex-col gap-3">
      <div
        className={[
          'flex min-h-28 items-center justify-center overflow-hidden rounded-xl border border-dashed bg-slate-50',
          invalid ? 'border-red-400' : 'border-slate-300',
        ].join(' ')}
      >
        {preview ? (
          <img
            src={preview}
            alt="Preview"
            className="max-h-40 max-w-full object-contain"
          />
        ) : (
          <span className="px-4 text-center text-xs text-slate-500">
            Chưa có ảnh. Chọn file để upload (tối đa {maxMbLabel}MB).
          </span>
        )}
      </div>

      <div className="flex flex-wrap gap-2">
        <input
          ref={inputRef}
          type="file"
          accept={getImageAccept(options)}
          className="hidden"
          disabled={disabled}
          onChange={(event) => {
            handleFile(event.target.files?.[0])
            event.target.value = ''
          }}
        />
        <Button
          type="button"
          unstyled
          disabled={disabled}
          className="pr-btn-outlined inline-flex h-10 items-center gap-2 rounded-xl border px-3 text-sm font-medium shadow-sm"
          onClick={() => inputRef.current?.click()}
        >
          <ImagePlus className="size-4" aria-hidden />
          Chọn ảnh
        </Button>
        {value && (
          <Button
            type="button"
            unstyled
            disabled={disabled}
            className="pr-btn-outlined inline-flex h-10 items-center gap-2 rounded-xl border border-red-200 px-3 text-sm font-medium text-red-700 shadow-sm"
            onClick={() => onChange('')}
          >
            <Trash2 className="size-4" aria-hidden />
            Xóa
          </Button>
        )}
      </div>
    </div>
  )
}
