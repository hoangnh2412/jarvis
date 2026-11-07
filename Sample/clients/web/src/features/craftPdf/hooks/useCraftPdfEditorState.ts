import { useCallback, useMemo, useState } from 'react'
import {
  PDF_ZOOM_DEFAULT,
  PDF_ZOOM_MAX,
  PDF_ZOOM_MIN,
  PDF_ZOOM_STEP,
} from '../constants'
import type { PdfElement, PdfElementTypeValue, PdfPageSize, PdfTemplate } from '../types'
import { createDefaultElement } from '../utils'

export type UseCraftPdfEditorStateOptions = {
  initialTemplate?: PdfTemplate | null
}

export function useCraftPdfEditorState(
  options: UseCraftPdfEditorStateOptions = {},
) {
  const [template, setTemplate] = useState<PdfTemplate | null>(
    options.initialTemplate ?? null,
  )
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [zoom, setZoom] = useState(PDF_ZOOM_DEFAULT)
  const [dirty, setDirty] = useState(false)

  const elements = template?.elements ?? []
  const pageSize: PdfPageSize | undefined = template?.pageSize

  const selectedElement = useMemo(
    () => elements.find((e) => e.id === selectedId) ?? null,
    [elements, selectedId],
  )

  const loadTemplate = useCallback((next: PdfTemplate) => {
    setTemplate(structuredClone(next))
    setSelectedId(null)
    setDirty(false)
  }, [])

  const updateElements = useCallback((next: PdfElement[]) => {
    setTemplate((prev) => {
      if (!prev) return prev
      return { ...prev, elements: next }
    })
    setDirty(true)
  }, [])

  const addElement = useCallback(
    (
      type: PdfElementTypeValue,
      at?: { x: number; y: number },
      defaults?: Partial<PdfElement>,
    ) => {
      setTemplate((prev) => {
        if (!prev) return prev
        const el = createDefaultElement(type, {
          zIndex: prev.elements.length + 1,
          ...(at ? { x: at.x, y: at.y } : {}),
          ...defaults,
        })
        setSelectedId(el.id)
        setDirty(true)
        return { ...prev, elements: [...prev.elements, el] }
      })
    },
    [],
  )

  const updateElement = useCallback(
    (id: string, patch: Partial<PdfElement>) => {
      setTemplate((prev) => {
        if (!prev) return prev
        return {
          ...prev,
          elements: prev.elements.map((el) =>
            el.id === id ? ({ ...el, ...patch } as PdfElement) : el,
          ),
        }
      })
      setDirty(true)
    },
    [],
  )

  const removeElement = useCallback((id: string) => {
    setTemplate((prev) => {
      if (!prev) return prev
      return {
        ...prev,
        elements: prev.elements.filter((el) => el.id !== id),
      }
    })
    setSelectedId((cur) => (cur === id ? null : cur))
    setDirty(true)
  }, [])

  const zoomIn = useCallback(() => {
    setZoom((z) => Math.min(PDF_ZOOM_MAX, +(z + PDF_ZOOM_STEP).toFixed(2)))
  }, [])

  const zoomOut = useCallback(() => {
    setZoom((z) => Math.max(PDF_ZOOM_MIN, +(z - PDF_ZOOM_STEP).toFixed(2)))
  }, [])

  const resetZoom = useCallback(() => setZoom(PDF_ZOOM_DEFAULT), [])

  return {
    template,
    setTemplate,
    loadTemplate,
    elements,
    pageSize,
    selectedId,
    setSelectedId,
    selectedElement,
    zoom,
    setZoom,
    zoomIn,
    zoomOut,
    resetZoom,
    dirty,
    setDirty,
    addElement,
    updateElement,
    updateElements,
    removeElement,
  }
}

export type CraftPdfEditorState = ReturnType<typeof useCraftPdfEditorState>
