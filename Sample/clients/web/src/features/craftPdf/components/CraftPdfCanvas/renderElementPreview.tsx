import type { ReactNode } from 'react'
import {
  PdfElementType,
  type PdfBarcodeElement,
  type PdfChartElement,
  type PdfElement,
  type PdfImageElement,
  type PdfQrCodeElement,
  type PdfRichTextElement,
  type PdfShapeElement,
  type PdfTableElement,
  type PdfTextElement,
} from '../../types'
import {
  buildCraftPdfSampleMap,
  resolvePdfElementBindings,
} from '../../utils/resolveBindings'

export type RenderElementPreviewOptions = {
  /** path/key → giá trị. Mặc định dùng sample từ fake data fields. */
  data?: Record<string, string>
  /** false = hiện nguyên `{{...}}`. Mặc định true. */
  resolveBindings?: boolean
}

function TextPreview({ el }: { el: PdfTextElement }) {
  return (
    <div
      className="box-border h-full w-full overflow-hidden px-1 py-0.5"
      style={{
        fontFamily: el.fontFamily,
        fontSize: el.fontSize,
        fontWeight: el.fontWeight,
        fontStyle: el.fontStyle,
        textDecoration: el.textDecoration,
        color: el.color,
        backgroundColor:
          el.backgroundColor && el.backgroundColor !== 'transparent'
            ? el.backgroundColor
            : undefined,
        textAlign: el.align,
        lineHeight: 1.3,
      }}
    >
      {el.content || 'Text'}
    </div>
  )
}

function RichTextPreview({ el }: { el: PdfRichTextElement }) {
  return (
    <div
      className="box-border h-full w-full overflow-hidden px-1 py-0.5 text-slate-800 [&_p]:m-0"
      style={{ fontSize: el.fontSize, color: el.color }}
      dangerouslySetInnerHTML={{ __html: el.html || '<p>Rich text</p>' }}
    />
  )
}

function ImagePreview({ el }: { el: PdfImageElement }) {
  if (el.src) {
    return (
      <img
        src={el.src}
        alt={el.alt || 'Image'}
        className="h-full w-full"
        style={{ objectFit: el.objectFit }}
        draggable={false}
      />
    )
  }
  return (
    <div className="flex h-full w-full items-center justify-center bg-slate-100 text-xs text-slate-400">
      Image
    </div>
  )
}

function TablePreview({ el }: { el: PdfTableElement }) {
  const cells = Array.from({ length: el.rows * el.columns })
  return (
    <div className="box-border flex h-full w-full flex-col overflow-hidden border border-slate-300 bg-white text-[10px]">
      <div className="border-b border-slate-300 bg-slate-50 px-1 py-0.5 font-semibold text-slate-600">
        {el.headerLabel}
      </div>
      <div
        className="grid min-h-0 flex-1"
        style={{
          gridTemplateColumns: `repeat(${el.columns}, 1fr)`,
          gridTemplateRows: `repeat(${Math.max(el.rows - 1, 1)}, 1fr)`,
        }}
      >
        {cells.slice(el.columns).map((_, i) => (
          <div
            key={i}
            className="border-b border-r border-slate-200 px-0.5 text-slate-400"
          >
            ·
          </div>
        ))}
      </div>
    </div>
  )
}

function QrPreview({ el }: { el: PdfQrCodeElement }) {
  return (
    <div className="flex h-full w-full flex-col items-center justify-center gap-1 border border-dashed border-slate-300 bg-white text-[10px] text-slate-500">
      <div
        className="grid gap-0.5"
        style={{ gridTemplateColumns: 'repeat(5, 1fr)' }}
      >
        {Array.from({ length: 25 }).map((_, i) => (
          <span
            key={i}
            className={`size-1.5 ${i % 3 === 0 ? 'bg-slate-800' : 'bg-slate-200'}`}
          />
        ))}
      </div>
      <span className="max-w-full truncate px-1">{el.value}</span>
    </div>
  )
}

function BarcodePreview({ el }: { el: PdfBarcodeElement }) {
  return (
    <div className="flex h-full w-full flex-col items-center justify-center gap-1 bg-white px-2">
      <div className="flex h-8 w-full items-end gap-px">
        {Array.from({ length: 40 }).map((_, i) => (
          <span
            key={i}
            className="flex-1 bg-slate-900"
            style={{ height: `${40 + (i % 5) * 12}%` }}
          />
        ))}
      </div>
      <span className="text-[10px] tracking-widest text-slate-600">
        {el.value}
      </span>
    </div>
  )
}

function ShapePreview({ el }: { el: PdfShapeElement }) {
  if (el.shape === 'ellipse') {
    return (
      <div
        className="h-full w-full rounded-full"
        style={{
          background: el.fill,
          border: `${el.strokeWidth}px solid ${el.stroke}`,
        }}
      />
    )
  }
  if (el.shape === 'line') {
    return (
      <div className="flex h-full w-full items-center">
        <div
          className="w-full"
          style={{
            borderTop: `${Math.max(el.strokeWidth, 1)}px solid ${el.stroke}`,
          }}
        />
      </div>
    )
  }
  return (
    <div
      className="h-full w-full"
      style={{
        background: el.fill,
        border: `${el.strokeWidth}px solid ${el.stroke}`,
      }}
    />
  )
}

function ChartPreview({ el }: { el: PdfChartElement }) {
  const bars = [40, 70, 55, 85, 45]
  return (
    <div className="box-border flex h-full w-full flex-col gap-1 border border-slate-200 bg-white p-2">
      <p className="m-0 text-[10px] font-semibold text-slate-600">{el.title}</p>
      <div className="flex min-h-0 flex-1 items-end gap-1 px-1">
        {bars.map((h, i) => (
          <div
            key={i}
            className="flex-1 rounded-t-sm bg-teal-600/80"
            style={{ height: `${h}%` }}
          />
        ))}
      </div>
      <p className="m-0 text-[9px] uppercase tracking-wide text-slate-400">
        {el.chartType}
      </p>
    </div>
  )
}

function renderResolved(el: PdfElement): ReactNode {
  if (el.visible === false) return null
  switch (el.type) {
    case PdfElementType.Text:
      return <TextPreview el={el} />
    case PdfElementType.RichText:
      return <RichTextPreview el={el} />
    case PdfElementType.Image:
      return <ImagePreview el={el} />
    case PdfElementType.Table:
      return <TablePreview el={el} />
    case PdfElementType.QrCode:
      return <QrPreview el={el} />
    case PdfElementType.Barcode:
      return <BarcodePreview el={el} />
    case PdfElementType.Shape:
      return <ShapePreview el={el} />
    case PdfElementType.Chart:
      return <ChartPreview el={el} />
    default:
      return null
  }
}

/**
 * Render element trên canvas / HTML preview.
 * Mặc định thay `{{data.*}}` bằng sample fake (hoặc `options.data`).
 */
export function renderElementPreview(
  el: PdfElement,
  options?: RenderElementPreviewOptions,
): ReactNode {
  const resolve = options?.resolveBindings !== false
  const data = options?.data ?? buildCraftPdfSampleMap()
  const view = resolve ? resolvePdfElementBindings(el, data) : el
  return renderResolved(view)
}
