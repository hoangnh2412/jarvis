import { useState, type KeyboardEvent } from 'react'
import { X } from 'lucide-react'

function normalizeTags(value: unknown): string[] {
  if (Array.isArray(value)) {
    return value.map((v) => String(v ?? '').trim()).filter(Boolean)
  }
  if (typeof value === 'string' && value.trim()) {
    return value
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean)
  }
  return []
}

export function InTagsValueEditor({
  value,
  handleOnChange,
  title,
  disabled,
  className,
}: {
  value: unknown
  handleOnChange: (v: string[]) => void
  title?: string
  disabled?: boolean
  className?: string
}) {
  const tags = normalizeTags(value)
  const [draft, setDraft] = useState('')

  const commit = (next: string[]) => {
    handleOnChange(next)
  }

  const addFromDraft = () => {
    const trimmed = draft.trim()
    if (!trimmed) return
    const parts = trimmed
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean)
    if (parts.length === 0) return
    const merged = [...tags]
    for (const part of parts) {
      if (!merged.includes(part)) merged.push(part)
    }
    commit(merged)
    setDraft('')
  }

  const removeTag = (index: number) => {
    commit(tags.filter((_, i) => i !== index))
  }

  const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' || e.key === ',') {
      e.preventDefault()
      addFromDraft()
      return
    }
    if (e.key === 'Backspace' && draft === '' && tags.length > 0) {
      commit(tags.slice(0, -1))
    }
  }

  return (
    <div
      title={title}
      className={[
        'box-border inline-flex h-10 min-w-[17rem] w-[17rem] max-w-full items-center gap-1.5 overflow-hidden rounded-xl border border-solid !border-[#e2e8f0] bg-white px-2.5 shadow-[0_1px_2px_rgba(15,23,42,0.04)] transition-[border-color,box-shadow] focus-within:!border-[#0d9488] focus-within:ring-2 focus-within:ring-teal-600/20',
        disabled ? 'cursor-not-allowed opacity-50' : '',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <div className="flex h-full min-w-0 flex-1 items-center gap-1 overflow-x-auto overflow-y-hidden">
        {tags.map((tag, index) => (
          <span
            key={`${tag}-${index}`}
            className="inline-flex max-w-[calc(100%-0.5rem)] shrink-0 items-center gap-0.5 rounded-md bg-teal-50 py-0.5 pl-2 pr-1 text-xs font-medium text-teal-900"
          >
            <span className="max-w-[8rem] truncate">{tag}</span>
            {!disabled ? (
              <button
                type="button"
                aria-label={`Xóa ${tag}`}
                className="inline-flex size-4 shrink-0 appearance-none items-center justify-center rounded border-0 bg-transparent text-teal-700/70 outline-none transition hover:bg-teal-100 hover:text-teal-900"
                onClick={() => removeTag(index)}
              >
                <X className="size-3" />
              </button>
            ) : null}
          </span>
        ))}
        <input
          type="text"
          disabled={disabled}
          value={draft}
          placeholder={tags.length === 0 ? 'Nhập rồi Enter…' : 'Thêm…'}
          className="h-7 min-w-[3.5rem] flex-1 border-0 bg-transparent p-0 text-sm text-slate-800 outline-none placeholder:text-slate-400 disabled:cursor-not-allowed"
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={onKeyDown}
          onBlur={addFromDraft}
        />
      </div>
    </div>
  )
}
