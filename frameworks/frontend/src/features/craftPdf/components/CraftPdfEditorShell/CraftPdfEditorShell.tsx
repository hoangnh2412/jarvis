import type { ReactNode } from 'react'
import type {
  CraftPdfDataField,
  PdfElement,
  PdfElementTypeValue,
  PdfPageSize,
  PdfRichTextElement,
  PdfTextElement,
} from '../../types'
import { PdfElementType } from '../../types'
import { CraftPdfCanvas } from '../CraftPdfCanvas'
import { CraftPdfFormatToolbar } from '../CraftPdfFormatToolbar'
import { CraftPdfPalette } from '../CraftPdfPalette'
import { CraftPdfPropertiesPanel } from '../CraftPdfPropertiesPanel'
import { CraftPdfToolbar } from '../CraftPdfToolbar'

export type CraftPdfEditorShellProps = {
  title?: string
  pageSize: PdfPageSize
  elements: PdfElement[]
  selectedId: string | null
  selectedElement: PdfElement | null
  zoom: number
  dirty?: boolean
  saving?: boolean
  paletteTitle?: string
  dataFieldsTitle?: string
  dataFields?: CraftPdfDataField[]
  /** Sample map resolve bindings trên canvas. */
  sampleData?: Record<string, string>
  propertiesTitle?: string
  emptyPropertiesLabel?: string
  bindingHint?: string
  onSelect: (id: string | null) => void
  onChangeElement: (id: string, patch: Partial<PdfElement>) => void
  onAddElement: (
    type: PdfElementTypeValue,
    at?: { x: number; y: number },
    defaults?: Partial<PdfElement>,
  ) => void
  onInsertDataField?: (field: CraftPdfDataField) => void
  onRemoveElement?: (id: string) => void
  onZoomIn?: () => void
  onZoomOut?: () => void
  onBack?: () => void
  onSave?: () => void
  onPreview?: () => void
  onGenerate?: () => void
  toolbar?: ReactNode
  className?: string
}

export function CraftPdfEditorShell({
  title,
  pageSize,
  elements,
  selectedId,
  selectedElement,
  zoom,
  dirty,
  saving,
  paletteTitle,
  dataFieldsTitle,
  dataFields,
  sampleData,
  propertiesTitle,
  emptyPropertiesLabel,
  bindingHint,
  onSelect,
  onChangeElement,
  onAddElement,
  onInsertDataField,
  onRemoveElement,
  onZoomIn,
  onZoomOut,
  onBack,
  onSave,
  onPreview,
  onGenerate,
  toolbar,
  className = '',
}: CraftPdfEditorShellProps) {
  const formatTarget =
    selectedElement?.type === PdfElementType.Text ||
    selectedElement?.type === PdfElementType.RichText
      ? (selectedElement as PdfTextElement | PdfRichTextElement)
      : null

  return (
    <div
      className={`flex h-[min(920px,calc(100vh-3.5rem))] min-h-[640px] w-full flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm ${className}`}
    >
      {toolbar ?? (
        <CraftPdfToolbar
          title={title}
          zoom={zoom}
          dirty={dirty}
          saving={saving}
          onBack={onBack}
          onZoomIn={onZoomIn}
          onZoomOut={onZoomOut}
          onSave={onSave}
          onPreview={onPreview}
          onGenerate={onGenerate}
        />
      )}

      <CraftPdfFormatToolbar
        element={formatTarget}
        onChange={(id, patch) => onChangeElement(id, patch as Partial<PdfElement>)}
      />

      <div className="flex min-h-0 flex-1 overflow-hidden">
        <CraftPdfPalette
          title={paletteTitle}
          dataFieldsTitle={dataFieldsTitle}
          dataFields={dataFields}
          onAdd={onAddElement}
          onInsertDataField={onInsertDataField}
        />
        <CraftPdfCanvas
          pageSize={pageSize}
          elements={elements}
          selectedId={selectedId}
          zoom={zoom}
          sampleData={sampleData}
          onSelect={onSelect}
          onChangeElement={onChangeElement}
          onAddElement={onAddElement}
        />
        <CraftPdfPropertiesPanel
          title={propertiesTitle}
          emptyLabel={emptyPropertiesLabel}
          bindingHint={bindingHint}
          element={selectedElement}
          onChange={onChangeElement}
          onRemove={onRemoveElement}
        />
      </div>
    </div>
  )
}
