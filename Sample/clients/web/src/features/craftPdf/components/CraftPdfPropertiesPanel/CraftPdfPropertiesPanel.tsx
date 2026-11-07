import type { ChangeEvent, ReactNode } from 'react'
import { Trash2 } from 'lucide-react'
import { Button } from 'primereact/button'
import { InputText } from 'primereact/inputtext'
import { Textarea } from 'primereact/textarea'
import {
  PdfElementType,
  type PdfElement,
  type PdfShapeElement,
  type PdfTextElement,
} from '../../types'
import { editorBtnGhostClass, editorFieldClass } from '../fieldStyles'

export type CraftPdfPropertiesPanelProps = {
  title?: string
  emptyLabel?: string
  bindingHint?: string
  element: PdfElement | null
  onChange: (id: string, patch: Partial<PdfElement>) => void
  onRemove?: (id: string) => void
  className?: string
}

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="flex min-w-0 flex-col gap-1">
      <span className="text-[10px] font-semibold uppercase tracking-[0.08em] text-slate-400">
        {label}
      </span>
      {children}
    </label>
  )
}

/** Color swatch — input chỉ gắn vào ô màu nhỏ, tránh native picker đè cả hàng */
function ColorSwatch({
  value,
  onChange,
  ariaLabel,
}: {
  value: string
  onChange: (hex: string) => void
  ariaLabel: string
}) {
  const hex = value?.startsWith('#') ? value : '#0f172a'
  return (
    <div className="box-border flex h-8 w-full min-w-0 items-center gap-2 overflow-hidden rounded-md border border-slate-200 bg-white px-2 shadow-none">
      <span className="relative size-5 shrink-0 overflow-hidden rounded border border-slate-300">
        <span
          className="pointer-events-none absolute inset-0 block"
          style={{ backgroundColor: hex }}
          aria-hidden
        />
        <input
          type="color"
          value={hex}
          aria-label={ariaLabel}
          onChange={(e) => onChange(e.target.value)}
          className="craft-pdf-color-input absolute inset-0 m-0 h-full w-full cursor-pointer border-0 p-0 opacity-0"
        />
      </span>
      <span className="min-w-0 truncate font-mono text-[11px] uppercase text-slate-600">
        {hex}
      </span>
    </div>
  )
}

type InputChange = ChangeEvent<HTMLInputElement>
type AreaChange = ChangeEvent<HTMLTextAreaElement>

export function CraftPdfPropertiesPanel({
  title = 'Properties',
  emptyLabel = 'Chọn một element trên canvas để chỉnh.',
  bindingHint = 'Binding key',
  element,
  onChange,
  onRemove,
  className = '',
}: CraftPdfPropertiesPanelProps) {
  return (
    <aside
      className={`craft-pdf-properties box-border flex h-full w-[300px] min-w-[300px] max-w-[300px] shrink-0 grow-0 basis-[300px] flex-col overflow-hidden border-l border-slate-200 bg-white px-0 py-0 ${className}`}
      style={{ flex: '0 0 300px' }}
    >
      <div className="flex h-11 shrink-0 items-center justify-between border-b border-slate-200 px-3">
        <p className="m-0 text-[11px] font-semibold tracking-wide text-slate-700">
          {title}
        </p>
        <div className="flex size-8 shrink-0 items-center justify-center">
          {element && onRemove ? (
            <Button
              type="button"
              unstyled
              className={`${editorBtnGhostClass} !text-red-600 hover:!bg-red-50`}
              onClick={() => onRemove(element.id)}
              aria-label="Remove"
            >
              <Trash2 className="size-3.5" />
            </Button>
          ) : null}
        </div>
      </div>

      <div
        className="box-border min-h-0 w-full flex-1 overflow-x-hidden overflow-y-scroll p-3"
        style={{ scrollbarGutter: 'stable' }}
      >
        {!element ? (
          <div className="flex h-full min-h-[240px] flex-col items-center justify-center px-1 text-center">
            <p className="m-0 text-[12px] leading-relaxed text-slate-400">
              {emptyLabel}
            </p>
          </div>
        ) : (
          <div className="box-border flex w-full min-w-0 flex-col gap-3">
            <div className="flex min-w-0 items-center justify-between gap-2 rounded-md bg-slate-50 px-2.5 py-2">
              <span className="text-[10px] font-semibold uppercase tracking-wide text-slate-400">
                Type
              </span>
              <span className="max-w-[140px] truncate rounded bg-white px-2 py-0.5 font-mono text-[11px] text-slate-700 ring-1 ring-slate-200">
                {element.type}
              </span>
            </div>

            <div className="grid min-w-0 grid-cols-2 gap-2">
              <Field label="X">
                <InputText
                  unstyled
                  className={editorFieldClass}
                  type="number"
                  value={String(Math.round(element.x))}
                  onChange={(e: InputChange) =>
                    onChange(element.id, { x: Number(e.target.value) || 0 })
                  }
                />
              </Field>
              <Field label="Y">
                <InputText
                  unstyled
                  className={editorFieldClass}
                  type="number"
                  value={String(Math.round(element.y))}
                  onChange={(e: InputChange) =>
                    onChange(element.id, { y: Number(e.target.value) || 0 })
                  }
                />
              </Field>
              <Field label="W">
                <InputText
                  unstyled
                  className={editorFieldClass}
                  type="number"
                  value={String(Math.round(element.width))}
                  onChange={(e: InputChange) =>
                    onChange(element.id, {
                      width: Math.max(8, Number(e.target.value) || 8),
                    })
                  }
                />
              </Field>
              <Field label="H">
                <InputText
                  unstyled
                  className={editorFieldClass}
                  type="number"
                  value={String(Math.round(element.height))}
                  onChange={(e: InputChange) =>
                    onChange(element.id, {
                      height: Math.max(8, Number(e.target.value) || 8),
                    })
                  }
                />
              </Field>
            </div>

            {'bindingKey' in element && (
              <Field label={bindingHint}>
                <InputText
                  unstyled
                  className={editorFieldClass}
                  value={element.bindingKey ?? ''}
                  placeholder="e.g. invoice_number"
                  onChange={(e: InputChange) =>
                    onChange(element.id, { bindingKey: e.target.value })
                  }
                />
              </Field>
            )}

            {element.type === PdfElementType.Text && (
              <Field label="Content">
                <Textarea
                  unstyled
                  className={`${editorFieldClass} !h-auto min-h-[72px] resize-none py-2`}
                  value={(element as PdfTextElement).content}
                  onChange={(e: AreaChange) =>
                    onChange(element.id, {
                      content: e.target.value,
                    } as Partial<PdfTextElement>)
                  }
                />
              </Field>
            )}

            {element.type === PdfElementType.RichText && (
              <Field label="HTML">
                <Textarea
                  unstyled
                  className={`${editorFieldClass} !h-auto min-h-[96px] resize-none py-2 font-mono text-[11px]`}
                  value={element.html}
                  onChange={(e: AreaChange) =>
                    onChange(element.id, { html: e.target.value })
                  }
                />
              </Field>
            )}

            {element.type === PdfElementType.Chart && (
              <>
                <Field label="Title">
                  <InputText
                    unstyled
                    className={editorFieldClass}
                    value={element.title}
                    onChange={(e: InputChange) =>
                      onChange(element.id, { title: e.target.value })
                    }
                  />
                </Field>
                <Field label="Chart type">
                  <InputText
                    unstyled
                    className={editorFieldClass}
                    value={element.chartType}
                    onChange={(e: InputChange) =>
                      onChange(element.id, {
                        chartType: e.target.value as 'bar' | 'line' | 'pie',
                      })
                    }
                  />
                </Field>
              </>
            )}

            {(element.type === PdfElementType.QrCode ||
              element.type === PdfElementType.Barcode) && (
              <Field label="Value">
                <InputText
                  unstyled
                  className={editorFieldClass}
                  value={element.value}
                  onChange={(e: InputChange) =>
                    onChange(element.id, { value: e.target.value })
                  }
                />
              </Field>
            )}

            {element.type === PdfElementType.Image && (
              <Field label="Image URL">
                <InputText
                  unstyled
                  className={editorFieldClass}
                  value={element.src ?? ''}
                  placeholder="https://…"
                  onChange={(e: InputChange) =>
                    onChange(element.id, { src: e.target.value })
                  }
                />
              </Field>
            )}

            {element.type === PdfElementType.Shape && (
              <div className="grid min-w-0 grid-cols-2 gap-2">
                <Field label="Fill">
                  <ColorSwatch
                    ariaLabel="Fill color"
                    value={(element as PdfShapeElement).fill}
                    onChange={(fill) =>
                      onChange(element.id, {
                        fill,
                      } as Partial<PdfShapeElement>)
                    }
                  />
                </Field>
                <Field label="Stroke">
                  <ColorSwatch
                    ariaLabel="Stroke color"
                    value={(element as PdfShapeElement).stroke}
                    onChange={(stroke) =>
                      onChange(element.id, {
                        stroke,
                      } as Partial<PdfShapeElement>)
                    }
                  />
                </Field>
              </div>
            )}
          </div>
        )}
      </div>
    </aside>
  )
}
