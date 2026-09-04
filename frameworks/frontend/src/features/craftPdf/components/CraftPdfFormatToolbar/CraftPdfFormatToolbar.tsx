import { useEffect, useRef, useState, type ReactNode } from 'react'
import Quill from 'quill'
import {
  AlignCenter,
  AlignJustify,
  AlignLeft,
  AlignRight,
  Bold,
  Highlighter,
  Italic,
  Link2,
  List,
  ListOrdered,
  RemoveFormatting,
  Strikethrough,
  Type,
  Underline,
} from 'lucide-react'
import type { PdfRichTextElement, PdfTextElement } from '../../types'
import { PdfElementType } from '../../types'

const FONT_FAMILIES = [
  { label: 'Manrope', value: 'Manrope, system-ui, sans-serif' },
  { label: 'Inter', value: 'Inter, system-ui, sans-serif' },
  { label: 'Georgia', value: 'Georgia, serif' },
  { label: 'Courier New', value: 'Courier New, monospace' },
  { label: 'Times New Roman', value: 'Times New Roman, Times, serif' },
  { label: 'Arial', value: 'Arial, Helvetica, sans-serif' },
]

const FONT_SIZES = [8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 28, 32, 36, 48]

const QUILL_SIZES = [
  { label: 'Small', value: 'small' },
  { label: 'Normal', value: '' },
  { label: 'Large', value: 'large' },
  { label: 'Huge', value: 'huge' },
]

let quillCssLoaded = false

function ensureQuillCss() {
  if (quillCssLoaded || typeof document === 'undefined') return
  const href = 'https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.snow.css'
  if (!document.querySelector('link[data-craft-pdf-quill="1"]')) {
    const link = document.createElement('link')
    link.rel = 'stylesheet'
    link.href = href
    link.dataset.craftPdfQuill = '1'
    document.head.appendChild(link)
  }
  quillCssLoaded = true
}

type FormatState = {
  bold: boolean
  italic: boolean
  underline: boolean
  strike: boolean
  align: '' | 'center' | 'right' | 'justify'
  list: '' | 'ordered' | 'bullet'
  size: string
  color: string
  background: string
}

const DEFAULT_FORMAT: FormatState = {
  bold: false,
  italic: false,
  underline: false,
  strike: false,
  align: '',
  list: '',
  size: '',
  color: '#0f172a',
  background: '#ffffff',
}

function readFormat(quill: Quill): FormatState {
  const f = quill.getFormat() as Record<string, unknown>
  return {
    bold: Boolean(f.bold),
    italic: Boolean(f.italic),
    underline: Boolean(f.underline),
    strike: Boolean(f.strike),
    align: (f.align as FormatState['align']) || '',
    list: (f.list as FormatState['list']) || '',
    size: typeof f.size === 'string' ? f.size : '',
    color: typeof f.color === 'string' ? f.color : '#0f172a',
    background:
      typeof f.background === 'string' ? f.background : '#ffffff',
  }
}

export type CraftPdfFormatToolbarProps = {
  element: PdfTextElement | PdfRichTextElement | null
  onChange: (
    id: string,
    patch: Partial<PdfTextElement> | Partial<PdfRichTextElement>,
  ) => void
  className?: string
}

/**
 * Ribbon công cụ chữ kiểu Word / Excel.
 * Text: bind props element. RichText: Quill API + cùng UI ribbon.
 */
export function CraftPdfFormatToolbar({
  element,
  onChange,
  className = '',
}: CraftPdfFormatToolbarProps) {
  const editorHostRef = useRef<HTMLDivElement>(null)
  const quillRef = useRef<Quill | null>(null)
  const skipSyncRef = useRef(false)
  const [richFormat, setRichFormat] = useState<FormatState>(DEFAULT_FORMAT)

  const isRich = element?.type === PdfElementType.RichText
  const isText = element?.type === PdfElementType.Text
  const enabled = Boolean(element && (isRich || isText))

  useEffect(() => {
    ensureQuillCss()
  }, [])

  useEffect(() => {
    if (!isRich || !element || !editorHostRef.current) {
      quillRef.current = null
      setRichFormat(DEFAULT_FORMAT)
      return
    }

    ensureQuillCss()
    editorHostRef.current.innerHTML = ''
    const editorEl = document.createElement('div')
    editorHostRef.current.appendChild(editorEl)

    const quill = new Quill(editorEl, {
      theme: 'snow',
      modules: { toolbar: false },
      placeholder: 'Nhập nội dung rich text…',
    })

    quill.clipboard.dangerouslyPasteHTML(
      (element as PdfRichTextElement).html || '',
    )

    const syncFormat = () => setRichFormat(readFormat(quill))

    quill.on('text-change', () => {
      skipSyncRef.current = true
      onChange(element.id, {
        html: quill.root.innerHTML,
      } as Partial<PdfRichTextElement>)
      syncFormat()
    })
    quill.on('selection-change', syncFormat)

    quillRef.current = quill
    syncFormat()

    return () => {
      quillRef.current = null
      if (editorHostRef.current) editorHostRef.current.innerHTML = ''
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps -- remount per element
  }, [isRich, element?.id])

  useEffect(() => {
    if (!isRich || !element || !quillRef.current) return
    if (skipSyncRef.current) {
      skipSyncRef.current = false
      return
    }
    const html = (element as PdfRichTextElement).html || ''
    if (quillRef.current.root.innerHTML !== html) {
      const sel = quillRef.current.getSelection()
      quillRef.current.clipboard.dangerouslyPasteHTML(html)
      if (sel) quillRef.current.setSelection(sel)
      setRichFormat(readFormat(quillRef.current))
    }
  }, [isRich, element])

  const applyRich = (key: string, value: unknown) => {
    const quill = quillRef.current
    if (!quill) return
    quill.focus()
    quill.format(key, value)
    setRichFormat(readFormat(quill))
  }

  const toggleRich = (key: 'bold' | 'italic' | 'underline' | 'strike') => {
    const quill = quillRef.current
    if (!quill) return
    const current = Boolean(quill.getFormat()[key])
    applyRich(key, !current)
  }

  return (
    <div
      className={`craft-pdf-ribbon shrink-0 border-b border-slate-200 bg-[#f3f4f6] ${className}`}
    >
      <div
        className={`box-border flex h-[48px] min-h-[48px] max-h-[64px] items-stretch gap-0 overflow-x-auto overflow-y-hidden px-2 py-1 ${
          enabled ? '' : 'pointer-events-none opacity-45'
        }`}
      >
        {!enabled ? (
          <div className="flex h-full w-full items-center px-2 text-[11px] text-slate-400">
            Chọn Text hoặc Rich Text để dùng thanh công cụ định dạng.
          </div>
        ) : isText && element ? (
          <TextRibbon
            text={element as PdfTextElement}
            onChange={onChange}
          />
        ) : isRich ? (
          <RichRibbon
            format={richFormat}
            onToggleBold={() => toggleRich('bold')}
            onToggleItalic={() => toggleRich('italic')}
            onToggleUnderline={() => toggleRich('underline')}
            onToggleStrike={() => toggleRich('strike')}
            onAlign={(v) => applyRich('align', v || false)}
            onList={(v) => applyRich('list', v || false)}
            onSize={(v) => applyRich('size', v || false)}
            onColor={(v) => applyRich('color', v)}
            onBackground={(v) => applyRich('background', v === '#ffffff' ? false : v)}
            onLink={() => {
              const quill = quillRef.current
              if (!quill) return
              const range = quill.getSelection()
              if (!range) return
              const current = quill.getFormat(range).link as string | undefined
              const url = window.prompt('URL', current || 'https://')
              if (url === null) return
              quill.format('link', url ? url : false)
            }}
            onClean={() => {
              const quill = quillRef.current
              if (!quill) return
              const range = quill.getSelection()
              if (range) {
                quill.removeFormat(range.index, range.length)
              } else {
                quill.setContents([{ insert: quill.getText() }])
              }
              setRichFormat(readFormat(quill))
            }}
          />
        ) : null}
      </div>

      {isRich ? (
        <div
          ref={editorHostRef}
          className="craft-pdf-quill-editor h-[88px] min-h-[88px] max-h-[88px] shrink-0 overflow-hidden border-t border-slate-200/80 bg-white"
        />
      ) : (
        /* Giữ chỗ trống 0 — không đẩy layout khi không phải RichText */
        null
      )}

      <RibbonStyles />
    </div>
  )
}

function TextRibbon({
  text,
  onChange,
}: {
  text: PdfTextElement
  onChange: CraftPdfFormatToolbarProps['onChange']
}) {
  return (
    <>
      <RibbonGroup label="Font">
        <select
          className="ribbon-select min-w-[132px]"
          value={text.fontFamily}
          onChange={(e) =>
            onChange(text.id, {
              fontFamily: e.target.value,
            } as Partial<PdfTextElement>)
          }
          title="Font"
        >
          {FONT_FAMILIES.map((f) => (
            <option key={f.value} value={f.value} style={{ fontFamily: f.value }}>
              {f.label}
            </option>
          ))}
        </select>
        <select
          className="ribbon-select w-[64px]"
          value={text.fontSize}
          onChange={(e) =>
            onChange(text.id, {
              fontSize: Number(e.target.value) || 14,
            } as Partial<PdfTextElement>)
          }
          title="Cỡ chữ"
        >
          {FONT_SIZES.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
        <BtnRow>
          <IconBtn
            active={text.fontWeight === 'bold'}
            title="Đậm"
            onClick={() =>
              onChange(text.id, {
                fontWeight: text.fontWeight === 'bold' ? 'normal' : 'bold',
              } as Partial<PdfTextElement>)
            }
          >
            <Bold className="size-3.5" strokeWidth={2.25} />
          </IconBtn>
          <IconBtn
            active={text.fontStyle === 'italic'}
            title="Nghiêng"
            onClick={() =>
              onChange(text.id, {
                fontStyle: text.fontStyle === 'italic' ? 'normal' : 'italic',
              } as Partial<PdfTextElement>)
            }
          >
            <Italic className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn
            active={text.textDecoration === 'underline'}
            title="Gạch chân"
            onClick={() =>
              onChange(text.id, {
                textDecoration:
                  text.textDecoration === 'underline' ? 'none' : 'underline',
              } as Partial<PdfTextElement>)
            }
          >
            <Underline className="size-3.5" strokeWidth={2} />
          </IconBtn>
        </BtnRow>
        <BtnRow>
          <ColorChip
            title="Màu chữ"
            icon={<Type className="size-3.5" strokeWidth={2} />}
            value={text.color}
            onChange={(color) =>
              onChange(text.id, { color } as Partial<PdfTextElement>)
            }
          />
          <ColorChip
            title="Màu nền"
            icon={<Highlighter className="size-3.5" strokeWidth={2} />}
            value={
              !text.backgroundColor || text.backgroundColor === 'transparent'
                ? '#ffffff'
                : text.backgroundColor
            }
            onChange={(backgroundColor) =>
              onChange(text.id, {
                backgroundColor,
              } as Partial<PdfTextElement>)
            }
          />
        </BtnRow>
      </RibbonGroup>

      <RibbonGroup label="Paragraph">
        <BtnRow>
          <IconBtn
            active={text.align === 'left'}
            title="Căn trái"
            onClick={() =>
              onChange(text.id, { align: 'left' } as Partial<PdfTextElement>)
            }
          >
            <AlignLeft className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn
            active={text.align === 'center'}
            title="Căn giữa"
            onClick={() =>
              onChange(text.id, { align: 'center' } as Partial<PdfTextElement>)
            }
          >
            <AlignCenter className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn
            active={text.align === 'right'}
            title="Căn phải"
            onClick={() =>
              onChange(text.id, { align: 'right' } as Partial<PdfTextElement>)
            }
          >
            <AlignRight className="size-3.5" strokeWidth={2} />
          </IconBtn>
        </BtnRow>
      </RibbonGroup>
    </>
  )
}

function RichRibbon({
  format,
  onToggleBold,
  onToggleItalic,
  onToggleUnderline,
  onToggleStrike,
  onAlign,
  onList,
  onSize,
  onColor,
  onBackground,
  onLink,
  onClean,
}: {
  format: FormatState
  onToggleBold: () => void
  onToggleItalic: () => void
  onToggleUnderline: () => void
  onToggleStrike: () => void
  onAlign: (v: FormatState['align']) => void
  onList: (v: FormatState['list']) => void
  onSize: (v: string) => void
  onColor: (v: string) => void
  onBackground: (v: string) => void
  onLink: () => void
  onClean: () => void
}) {
  return (
    <>
      <RibbonGroup label="Font">
        <select
          className="ribbon-select w-[88px]"
          value={format.size}
          onChange={(e) => onSize(e.target.value)}
          title="Cỡ chữ"
        >
          {QUILL_SIZES.map((s) => (
            <option key={s.label} value={s.value}>
              {s.label}
            </option>
          ))}
        </select>
        <BtnRow>
          <IconBtn active={format.bold} title="Đậm" onClick={onToggleBold}>
            <Bold className="size-3.5" strokeWidth={2.25} />
          </IconBtn>
          <IconBtn active={format.italic} title="Nghiêng" onClick={onToggleItalic}>
            <Italic className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn
            active={format.underline}
            title="Gạch chân"
            onClick={onToggleUnderline}
          >
            <Underline className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn
            active={format.strike}
            title="Gạch ngang"
            onClick={onToggleStrike}
          >
            <Strikethrough className="size-3.5" strokeWidth={2} />
          </IconBtn>
        </BtnRow>
        <BtnRow>
          <ColorChip
            title="Màu chữ"
            icon={<Type className="size-3.5" strokeWidth={2} />}
            value={format.color}
            onChange={onColor}
          />
          <ColorChip
            title="Màu nền"
            icon={<Highlighter className="size-3.5" strokeWidth={2} />}
            value={format.background || '#ffffff'}
            onChange={onBackground}
          />
        </BtnRow>
      </RibbonGroup>

      <RibbonGroup label="Paragraph">
        <BtnRow>
          <IconBtn
            active={format.align === ''}
            title="Căn trái"
            onClick={() => onAlign('')}
          >
            <AlignLeft className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn
            active={format.align === 'center'}
            title="Căn giữa"
            onClick={() => onAlign('center')}
          >
            <AlignCenter className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn
            active={format.align === 'right'}
            title="Căn phải"
            onClick={() => onAlign('right')}
          >
            <AlignRight className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn
            active={format.align === 'justify'}
            title="Căn đều"
            onClick={() => onAlign('justify')}
          >
            <AlignJustify className="size-3.5" strokeWidth={2} />
          </IconBtn>
        </BtnRow>
        <BtnRow>
          <IconBtn
            active={format.list === 'ordered'}
            title="Danh sách số"
            onClick={() =>
              onList(format.list === 'ordered' ? '' : 'ordered')
            }
          >
            <ListOrdered className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn
            active={format.list === 'bullet'}
            title="Danh sách bulleted"
            onClick={() => onList(format.list === 'bullet' ? '' : 'bullet')}
          >
            <List className="size-3.5" strokeWidth={2} />
          </IconBtn>
        </BtnRow>
      </RibbonGroup>

      <RibbonGroup label="Insert">
        <BtnRow>
          <IconBtn title="Chèn link" onClick={onLink}>
            <Link2 className="size-3.5" strokeWidth={2} />
          </IconBtn>
          <IconBtn title="Xóa định dạng" onClick={onClean}>
            <RemoveFormatting className="size-3.5" strokeWidth={2} />
          </IconBtn>
        </BtnRow>
      </RibbonGroup>
    </>
  )
}

function RibbonGroup({
  label,
  children,
}: {
  label: string
  children: ReactNode
}) {
  return (
    <div className="flex h-full shrink-0 items-stretch">
      <div className="flex h-full flex-col items-center justify-between gap-0.5 px-2.5 py-0.5">
        <div className="flex flex-nowrap items-center justify-center gap-1">
          {children}
        </div>
        <span className="text-[9px] font-semibold uppercase tracking-[0.1em] text-slate-400">
          {label}
        </span>
      </div>
      <div className="my-1 w-px self-stretch bg-slate-300/70" />
    </div>
  )
}

function BtnRow({ children }: { children: ReactNode }) {
  return <div className="inline-flex items-center gap-0.5">{children}</div>
}

function IconBtn({
  active,
  title,
  onClick,
  children,
}: {
  active?: boolean
  title: string
  onClick: () => void
  children: ReactNode
}) {
  return (
    <button
      type="button"
      title={title}
      aria-label={title}
      aria-pressed={active}
      onClick={onClick}
      className={`ribbon-icon-btn ${active ? 'is-active' : ''}`}
    >
      {children}
    </button>
  )
}

function ColorChip({
  title,
  icon,
  value,
  onChange,
}: {
  title: string
  icon: ReactNode
  value: string
  onChange: (v: string) => void
}) {
  const hex = value.startsWith('#') ? value : '#0f172a'
  return (
    <label className="ribbon-color" title={title}>
      {icon}
      <span
        className="mt-0.5 h-0.5 w-3.5 rounded-sm"
        style={{ background: hex }}
        aria-hidden
      />
      <input
        type="color"
        value={hex}
        onChange={(e) => onChange(e.target.value)}
        className="craft-pdf-color-input absolute inset-0 m-0 h-full w-full cursor-pointer border-0 p-0 opacity-0"
        aria-label={title}
      />
    </label>
  )
}

function RibbonStyles() {
  return (
    <style>{`
      .craft-pdf-ribbon .ribbon-select {
        height: 26px;
        border: 1px solid rgb(203 213 225);
        border-radius: 4px;
        background: #fff;
        padding: 0 6px;
        font-size: 12px;
        color: rgb(51 65 85);
        outline: none;
      }
      .craft-pdf-ribbon .ribbon-select:focus {
        border-color: rgb(13 148 136);
        box-shadow: 0 0 0 2px rgba(13, 148, 136, 0.15);
      }
      .craft-pdf-ribbon .ribbon-icon-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 28px;
        height: 26px;
        border: 1px solid transparent;
        border-radius: 4px;
        background: transparent;
        color: rgb(51 65 85);
        padding: 0;
        cursor: pointer;
      }
      .craft-pdf-ribbon .ribbon-icon-btn:hover {
        background: rgb(226 232 240);
        border-color: rgb(203 213 225);
      }
      .craft-pdf-ribbon .ribbon-icon-btn.is-active {
        background: rgb(204 251 241);
        border-color: rgb(94 234 212);
        color: rgb(15 118 110);
      }
      .craft-pdf-ribbon .ribbon-color {
        position: relative;
        display: inline-flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        width: 28px;
        height: 26px;
        border: 1px solid transparent;
        border-radius: 4px;
        color: rgb(51 65 85);
        cursor: pointer;
      }
      .craft-pdf-ribbon .ribbon-color:hover {
        background: rgb(226 232 240);
        border-color: rgb(203 213 225);
      }
      .craft-pdf-quill-editor .ql-toolbar.ql-snow {
        display: none !important;
      }
      .craft-pdf-quill-editor .ql-container.ql-snow {
        border: 0;
        font-size: 13px;
      }
      .craft-pdf-quill-editor .ql-editor {
        min-height: 88px;
        max-height: 88px;
        height: 88px;
        overflow: auto;
        padding: 8px 12px;
        line-height: 1.45;
      }
      .craft-pdf-quill-editor .ql-editor.ql-blank::before {
        color: rgb(148 163 184);
        font-style: normal;
        left: 12px;
        right: 12px;
      }
    `}</style>
  )
}
