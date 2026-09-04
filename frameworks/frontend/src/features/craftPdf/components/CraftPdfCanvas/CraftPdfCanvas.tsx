import { useCallback, type DragEvent } from 'react'
import { Rnd } from 'react-rnd'
import { ensureBrowserProcess } from '../../utils/ensureBrowserProcess'
import { defaultCraftPdfTheme } from '../../theme'
import {
  PdfElementType,
  type CraftPdfDropPayload,
  type PdfElement,
  type PdfElementTypeValue,
  type PdfPageSize,
} from '../../types'
import { createDefaultElement } from '../../utils'
import { CRAFT_PDF_DROP_MIME } from '../CraftPdfPalette'
import { renderElementPreview } from './renderElementPreview'

// react-draggable đọc process.env.DRAGGABLE_DEBUG — polyfill trước khi render Rnd
ensureBrowserProcess()

export type CraftPdfCanvasProps = {
  pageSize: PdfPageSize
  elements: PdfElement[]
  selectedId: string | null
  zoom?: number
  /** Sample map để resolve `{{data.*}}` trên canvas. */
  sampleData?: Record<string, string>
  onSelect: (id: string | null) => void
  onChangeElement: (id: string, patch: Partial<PdfElement>) => void
  onAddElement?: (
    type: PdfElementTypeValue,
    at?: { x: number; y: number },
    defaults?: Partial<PdfElement>,
  ) => void
  className?: string
}

function parseDropPayload(raw: string): CraftPdfDropPayload | null {
  if (!raw) return null
  try {
    return JSON.parse(raw) as CraftPdfDropPayload
  } catch {
    return null
  }
}

export function CraftPdfCanvas({
  pageSize,
  elements,
  selectedId,
  zoom = 1,
  sampleData,
  onSelect,
  onChangeElement,
  onAddElement,
  className = '',
}: CraftPdfCanvasProps) {
  const theme = defaultCraftPdfTheme

  const handleDrop = useCallback(
    (e: DragEvent<HTMLDivElement>) => {
      e.preventDefault()
      const rect = e.currentTarget.getBoundingClientRect()
      const x = Math.max(0, (e.clientX - rect.left) / zoom - 40)
      const y = Math.max(0, (e.clientY - rect.top) / zoom - 20)
      const at = { x, y }

      const payload: CraftPdfDropPayload | null =
        parseDropPayload(e.dataTransfer.getData(CRAFT_PDF_DROP_MIME)) ??
        (() => {
          const type = e.dataTransfer.getData(
            'application/x-craft-pdf-type',
          ) as PdfElementTypeValue
          if (!type || !Object.values(PdfElementType).includes(type)) return null
          return {
            kind: 'component' as const,
            type,
          } satisfies CraftPdfDropPayload
        })()

      if (!payload || !onAddElement) return
      // Data fields: click-only từ palette — bỏ qua drop dataField
      if (payload.kind !== 'component') return
      onAddElement(payload.type, at, payload.defaults)
    },
    [onAddElement, zoom],
  )

  return (
    <div
      className={`relative flex min-h-0 flex-1 items-start justify-center overflow-auto p-5 ${className}`}
      style={{ backgroundColor: theme.canvasBg }}
      onClick={() => onSelect(null)}
    >
      <div
        className="relative shrink-0 bg-white shadow-[0_8px_30px_rgba(15,23,42,0.1)] ring-1 ring-slate-300/60"
        style={{
          width: pageSize.width * zoom,
          height: pageSize.height * zoom,
        }}
        onDragOver={(e) => {
          e.preventDefault()
          e.dataTransfer.dropEffect = 'copy'
        }}
        onDrop={handleDrop}
        onClick={(e) => e.stopPropagation()}
      >
        <div
          className="absolute left-0 top-0 origin-top-left"
          style={{
            width: pageSize.width,
            height: pageSize.height,
            transform: `scale(${zoom})`,
          }}
        >
          {elements.map((el) => {
            if (el.visible === false) return null
            const selected = el.id === selectedId
            return (
              <Rnd
                key={el.id}
                size={{ width: el.width, height: el.height }}
                position={{ x: el.x, y: el.y }}
                scale={zoom}
                bounds="parent"
                disableDragging={el.locked}
                enableResizing={!el.locked}
                style={{
                  zIndex: el.zIndex ?? 1,
                  transform: el.rotation
                    ? `rotate(${el.rotation}deg)`
                    : undefined,
                }}
                onDrag={(_e, d) => {
                  onChangeElement(el.id, { x: d.x, y: d.y })
                }}
                onDragStop={(_e, d) => {
                  onChangeElement(el.id, { x: d.x, y: d.y })
                }}
                onResize={(_e, _dir, ref, _delta, position) => {
                  onChangeElement(el.id, {
                    width: ref.offsetWidth,
                    height: ref.offsetHeight,
                    x: position.x,
                    y: position.y,
                  })
                }}
                onResizeStop={(_e, _dir, ref, _delta, position) => {
                  onChangeElement(el.id, {
                    width: ref.offsetWidth,
                    height: ref.offsetHeight,
                    x: position.x,
                    y: position.y,
                  })
                }}
                onMouseDown={(e) => {
                  e.stopPropagation()
                  onSelect(el.id)
                }}
                className={
                  selected
                    ? 'ring-2 ring-teal-600 ring-offset-1'
                    : 'hover:ring-1 hover:ring-slate-300'
                }
              >
                <div className="box-border h-full w-full select-none">
                  {renderElementPreview(el, {
                    data: sampleData,
                    resolveBindings: false,
                  })}
                </div>
              </Rnd>
            )
          })}
        </div>
      </div>
    </div>
  )
}

/** Helper: tạo element tại vị trí drop */
export function createElementAt(
  type: PdfElementTypeValue,
  at?: { x: number; y: number },
  zIndex = 1,
  defaults?: Partial<PdfElement>,
): PdfElement {
  return createDefaultElement(type, {
    x: at?.x ?? 40,
    y: at?.y ?? 40,
    zIndex,
    ...defaults,
  })
}
