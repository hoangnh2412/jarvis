import { useMemo, useState, type DragEvent } from 'react'
import { Braces, Component, Plus } from 'lucide-react'
import type {
  CraftPdfDataField,
  CraftPdfDropPayload,
  PdfElement,
  PdfElementTypeValue,
} from '../../types'
import { toBindingExpression } from '../../constants/dataFields'
import {
  CRAFT_PDF_DROP_MIME,
  CRAFT_PDF_PALETTE_ITEMS,
  groupDataFields,
  type CraftPdfPaletteItem,
} from './paletteItems'

export type CraftPdfPaletteProps = {
  title?: string
  dataFieldsTitle?: string
  dataFields?: CraftPdfDataField[]
  onAdd: (
    type: PdfElementTypeValue,
    at?: { x: number; y: number },
    defaults?: Partial<PdfElement>,
  ) => void
  onInsertDataField?: (field: CraftPdfDataField) => void
  className?: string
}

type SideTab = 'components' | 'data'

function setDropPayload(e: DragEvent, payload: CraftPdfDropPayload) {
  e.dataTransfer.setData(CRAFT_PDF_DROP_MIME, JSON.stringify(payload))
  e.dataTransfer.effectAllowed = 'copy'
}

export function CraftPdfPalette({
  title = 'Components',
  dataFieldsTitle = 'Data Fields',
  dataFields = [],
  onAdd,
  onInsertDataField,
  className = '',
}: CraftPdfPaletteProps) {
  const [tab, setTab] = useState<SideTab>('components')
  const groupedFields = useMemo(() => groupDataFields(dataFields), [dataFields])

  const handleAddItem = (item: CraftPdfPaletteItem) => {
    if (item.soon) return
    onAdd(item.type, undefined, item.defaults)
  }

  return (
    <aside
      className={`craft-pdf-palette box-border flex h-full px-3 py-2 w-[280px] min-w-[280px] max-w-[280px] shrink-0 grow-0 basis-[280px] flex-col overflow-hidden border-r border-slate-200 bg-white ${className}`}
      style={{ flex: '0 0 280px' }}
    >
      <div className="craft-pdf-palette-tabs grid shrink-0 grid-cols-2 border-b border-slate-200">
        <button
          type="button"
          className={`craft-pdf-palette-tab inline-flex h-9 items-center justify-center gap-1.5 border-0 border-b-2 bg-transparent text-[11px] font-semibold shadow-none transition-colors ${
            tab === 'components'
              ? 'is-active border-teal-600 text-teal-700'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
          onClick={() => setTab('components')}
        >
          <Component className="size-3.5" aria-hidden />
          {title}
        </button>
        <button
          type="button"
          className={`craft-pdf-palette-tab inline-flex h-9 items-center justify-center gap-1.5 border-0 border-b-2 bg-transparent text-[11px] font-semibold shadow-none transition-colors ${
            tab === 'data'
              ? 'is-active border-teal-600 text-teal-700'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
          onClick={() => setTab('data')}
        >
          <Braces className="size-3.5" aria-hidden />
          {dataFieldsTitle}
        </button>
      </div>

      {tab === 'components' ? (
        <div className="min-h-0 flex-1 overflow-y-auto p-2.5">
          <p className="m-0 mb-2 px-0.5 text-[10px] font-semibold uppercase tracking-[0.12em] text-slate-400">
            Standard
          </p>
          <div className="grid grid-cols-3 gap-1.5">
            {CRAFT_PDF_PALETTE_ITEMS.filter((i) => i.category === 'standard').map(
              (item) => {
                const Icon = item.icon
                return (
                  <button
                    key={item.id}
                    type="button"
                    title={`${item.label} — click hoặc kéo vào trang`}
                    disabled={item.soon}
                    draggable={!item.soon}
                    onDragStart={(e) => {
                      setDropPayload(e, {
                        kind: 'component',
                        type: item.type,
                        defaults: item.defaults,
                      })
                    }}
                    onClick={() => handleAddItem(item)}
                    className="group flex aspect-square flex-col items-center justify-center gap-1 rounded-lg border border-slate-200 bg-slate-50/80 px-1 text-center transition-all hover:border-teal-400 hover:bg-teal-50 active:scale-[0.97] disabled:cursor-not-allowed disabled:opacity-45"
                  >
                    <Icon
                      className="size-4 text-slate-600 transition-colors group-hover:text-teal-700"
                      strokeWidth={1.75}
                      aria-hidden
                    />
                    <span className="line-clamp-2 text-[10px] font-medium leading-tight text-slate-600 group-hover:text-slate-800">
                      {item.label}
                    </span>
                  </button>
                )
              },
            )}
          </div>
          <p className="m-0 mt-3 px-0.5 text-[10px] leading-relaxed text-slate-400">
            Click hoặc kéo component vào trang.
          </p>
        </div>
      ) : (
        <div className="min-h-0 flex-1 overflow-y-auto p-2.5">
          <p className="m-0 mb-2 px-0.5 text-[10px] leading-relaxed text-slate-400">
            Click để chèn field vào element đang chọn, hoặc tạo text mới.
          </p>
          {groupedFields.length === 0 ? (
            <p className="m-0 px-0.5 py-3 text-xs leading-relaxed text-slate-400">
              Chưa có data fields. Sẽ nối API schema sau.
            </p>
          ) : (
            <div className="flex flex-col gap-3">
              {groupedFields.map(([group, fields]) => (
                <div key={group}>
                  <p className="m-0 mb-1.5 px-0.5 text-[10px] font-semibold uppercase tracking-[0.12em] text-slate-400">
                    {group}
                  </p>
                  <ul className="m-0 flex list-none flex-col gap-1 p-0">
                    {fields.map((field) => (
                      <li key={field.key}>
                        <button
                          type="button"
                          onClick={() => onInsertDataField?.(field)}
                          className="group flex w-full items-center gap-2 rounded-lg border border-slate-200 bg-slate-50/80 px-2 py-1.5 text-left transition-colors hover:border-teal-400 hover:bg-teal-50"
                        >
                          <span className="min-w-0 flex-1">
                            <span className="block truncate text-[12px] font-medium text-slate-800">
                              {field.label}
                            </span>
                            <span className="block truncate font-mono text-[10px] text-slate-500 group-hover:text-teal-700">
                              {toBindingExpression(field)}
                            </span>
                          </span>
                          <Plus
                            className="size-3.5 shrink-0 text-slate-300 group-hover:text-teal-600"
                            aria-hidden
                          />
                        </button>
                      </li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </aside>
  )
}
