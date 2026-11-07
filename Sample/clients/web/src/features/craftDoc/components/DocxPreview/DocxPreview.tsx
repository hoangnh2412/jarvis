import { useCallback, useEffect, useRef, useState } from 'react'
import { renderAsync } from 'docx-preview'
import type { PreviewMode } from '../../types'

export type DocxPreviewProps = {
  blob: Blob | null
  previewMode: PreviewMode
  selectedFieldName?: string | null
  zoom: number
  fitMode: 'none' | 'width' | 'page'
  fullscreen: boolean
  currentPage: number
  onCurrentPageChange: (page: number) => void
  onTotalPagesChange: (total: number) => void
  onFieldClick?: (fieldName: string) => void
  /** Chỉ hiện overlay khi đổi mẫu — cập nhật dữ liệu form dùng soft swap. */
  showLoadingOverlay?: boolean
  className?: string
}

const PLACEHOLDER_PATTERN = /\{\{([^}]+)\}\}/g

const DOCX_RENDER_OPTIONS = {
  className: 'docx',
  inWrapper: true,
  ignoreWidth: false,
  ignoreHeight: false,
  ignoreFonts: false,
  useBase64URL: true,
  breakPages: true,
  // Word thường lưu ngắt trang bằng lastRenderedPageBreak — phải false để tách đúng số trang.
  ignoreLastRenderedPageBreak: false,
  experimental: true,
  trimXmlDeclaration: true,
} as const

const TAB_STOP_LAYOUT_REFRESH_MS = 750

function markPlaceholders(
  root: HTMLElement,
  previewMode: PreviewMode,
  selectedFieldName?: string | null,
  onFieldClick?: (fieldName: string) => void,
) {
  // Filled preview — giữ nguyên DOM/font từ docx-preview, không bọc span.
  if (previewMode === 'filled') return

  const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT)
  const textNodes: Text[] = []

  while (walker.nextNode()) {
    textNodes.push(walker.currentNode as Text)
  }

  for (const node of textNodes) {
    const value = node.nodeValue ?? ''
    if (!value.includes('{{') && previewMode === 'template') continue

    const parent = node.parentElement
    if (!parent || parent.closest('.craft-doc-field-mark')) continue

    const fragments: Array<string | HTMLElement> = []
    let lastIndex = 0
    let match: RegExpExecArray | null
    const regex = new RegExp(PLACEHOLDER_PATTERN.source, 'g')

    while ((match = regex.exec(value)) !== null) {
      if (match.index > lastIndex) {
        fragments.push(value.slice(lastIndex, match.index))
      }

      const fieldName = match[1].trim()
      const mark = document.createElement('span')
      mark.className = 'craft-doc-field-mark'
      mark.dataset.fieldName = fieldName
      mark.textContent = match[0]
      if (selectedFieldName === fieldName) {
        mark.classList.add('is-selected')
      }
      mark.addEventListener('click', (event) => {
        event.stopPropagation()
        onFieldClick?.(fieldName)
      })
      fragments.push(mark)
      lastIndex = match.index + match[0].length
    }

    if (fragments.length === 0) continue

    if (lastIndex < value.length) {
      fragments.push(value.slice(lastIndex))
    }

    const wrapper = document.createElement('span')
    for (const fragment of fragments) {
      if (typeof fragment === 'string') {
        wrapper.appendChild(document.createTextNode(fragment))
      } else {
        wrapper.appendChild(fragment)
      }
    }
    parent.replaceChild(wrapper, node)
  }

  if (selectedFieldName) {
    root
      .querySelectorAll<HTMLElement>('.craft-doc-field-mark')
      .forEach((mark) => {
        mark.classList.toggle(
          'is-selected',
          mark.dataset.fieldName === selectedFieldName,
        )
      })
  }
}

export function DocxPreview({
  blob,
  previewMode,
  selectedFieldName,
  zoom,
  fitMode,
  fullscreen,
  currentPage,
  onCurrentPageChange,
  onTotalPagesChange,
  onFieldClick,
  showLoadingOverlay = true,
  className = '',
}: DocxPreviewProps) {
  const viewportRef = useRef<HTMLDivElement>(null)
  const containerRef = useRef<HTMLDivElement>(null)
  const styleContainerRef = useRef<HTMLDivElement>(null)
  const bodyContainerRef = useRef<HTMLDivElement>(null)
  const currentPageRef = useRef(currentPage)
  const scrollSourceRef = useRef<'pager' | 'scroll'>('pager')
  const [rendering, setRendering] = useState(false)
  const [renderError, setRenderError] = useState<string | null>(null)
  const [computedZoom, setComputedZoom] = useState(zoom)

  const getPageElements = useCallback(() => {
    const body = bodyContainerRef.current
    if (!body) return [] as HTMLElement[]
    return Array.from(
      body.querySelectorAll<HTMLElement>('.docx-wrapper > section.docx'),
    )
  }, [])

  const syncPages = useCallback(() => {
    const pages = getPageElements()
    pages.forEach((page, index) => {
      page.dataset.pageNumber = String(index + 1)
    })
    onTotalPagesChange(Math.max(pages.length, 1))
  }, [getPageElements, onTotalPagesChange])

  useEffect(() => {
    const bodyContainer = bodyContainerRef.current
    const styleContainer = styleContainerRef.current
    if (!bodyContainer || !styleContainer || !blob) {
      if (bodyContainer) bodyContainer.innerHTML = ''
      if (styleContainer) styleContainer.innerHTML = ''
      return
    }

    let cancelled = false
    let layoutRefreshTimer: number | undefined

    if (showLoadingOverlay) {
      setRendering(true)
      setRenderError(null)
    }

    const finalizeRender = () => {
      if (!bodyContainerRef.current) return
      markPlaceholders(
        bodyContainerRef.current,
        previewMode,
        selectedFieldName,
        onFieldClick,
      )
      syncPages()
      setRendering(false)
    }

    const runRender = () => renderAsync(blob, bodyContainer, styleContainer, DOCX_RENDER_OPTIONS)

    void runRender()
      .then(() => {
        if (cancelled || !bodyContainerRef.current) return
        finalizeRender()

        // docx-preview tính tab stop sau ~500ms — render lại khi panel preview đã có kích thước.
        layoutRefreshTimer = window.setTimeout(() => {
          if (cancelled || !bodyContainerRef.current || !viewportRef.current) return
          if (viewportRef.current.clientWidth < 20) return

          void runRender()
            .then(() => {
              if (cancelled || !bodyContainerRef.current) return
              finalizeRender()
            })
            .catch(() => {
              // Giữ bản render đầu nếu refresh tab stop thất bại.
            })
        }, TAB_STOP_LAYOUT_REFRESH_MS)
      })
      .catch((error) => {
        if (cancelled) return
        setRendering(false)
        setRenderError(
          error instanceof Error ? error.message : 'Unable to preview this document.',
        )
      })

    return () => {
      cancelled = true
      if (layoutRefreshTimer) window.clearTimeout(layoutRefreshTimer)
      if (showLoadingOverlay) {
        bodyContainer.innerHTML = ''
        styleContainer.innerHTML = ''
      }
    }
  }, [blob, previewMode, selectedFieldName, onFieldClick, showLoadingOverlay, syncPages])

  useEffect(() => {
    const body = bodyContainerRef.current
    if (!body) return
    body.querySelectorAll<HTMLElement>('.craft-doc-field-mark').forEach((mark) => {
      mark.classList.toggle('is-selected', mark.dataset.fieldName === selectedFieldName)
    })
  }, [selectedFieldName])

  useEffect(() => {
    const viewport = viewportRef.current
    const body = bodyContainerRef.current
    if (!viewport || !body) {
      setComputedZoom(zoom)
      return
    }

    const page = body.querySelector('.docx-wrapper > section.docx') as HTMLElement | null
    if (!page || fitMode === 'none') {
      setComputedZoom(zoom)
      return
    }

    const viewportWidth = viewport.clientWidth - 48
    const pageWidth = page.offsetWidth || 816
    const pageHeight = page.offsetHeight || 1056

    if (fitMode === 'width') {
      setComputedZoom(Math.min(200, Math.max(50, (viewportWidth / pageWidth) * 100)))
      return
    }

    const viewportHeight = viewport.clientHeight - 48
    setComputedZoom(
      Math.min(
        200,
        Math.max(
          50,
          Math.min(viewportWidth / pageWidth, viewportHeight / pageHeight) * 100,
        ),
      ),
    )
  }, [zoom, fitMode, blob, rendering])

  currentPageRef.current = currentPage

  useEffect(() => {
    const viewport = viewportRef.current
    if (!viewport || rendering) return

    const pages = getPageElements()
    if (pages.length === 0) return

    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((entry) => entry.isIntersecting)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio)
        const top = visible[0]?.target as HTMLElement | undefined
        const pageNumber = Number(top?.dataset.pageNumber)
        if (pageNumber >= 1 && pageNumber !== currentPageRef.current) {
          scrollSourceRef.current = 'scroll'
          onCurrentPageChange(pageNumber)
        }
      },
      { root: viewport, threshold: [0.35, 0.55, 0.75] },
    )

    pages.forEach((page) => observer.observe(page))
    return () => observer.disconnect()
  }, [blob, rendering, getPageElements, onCurrentPageChange])

  useEffect(() => {
    const viewport = viewportRef.current
    if (!viewport || rendering) return
    if (scrollSourceRef.current === 'scroll') {
      scrollSourceRef.current = 'pager'
      return
    }

    const page = getPageElements()[currentPage - 1]
    if (!page) return

    page.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }, [currentPage, rendering, getPageElements])

  return (
    <div
      ref={viewportRef}
      className={`craft-doc-preview-viewport h-full overflow-x-hidden overflow-y-auto ${fullscreen ? 'is-fullscreen p-6' : 'p-4'} ${className}`}
    >
      <div
        className="craft-doc-preview-stage mx-auto"
        style={{ transform: `scale(${computedZoom / 100})`, transformOrigin: 'top center' }}
      >
        <div ref={containerRef} className="craft-doc-preview-container">
          <div ref={styleContainerRef} className="craft-doc-preview-styles" />
          <div ref={bodyContainerRef} className="craft-doc-preview-body" />
        </div>
      </div>

      {rendering && showLoadingOverlay ? (
        <div className="pointer-events-none absolute inset-0 flex items-center justify-center bg-white/70">
          <div className="rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-600 shadow-sm">
            Loading document...
          </div>
        </div>
      ) : null}

      {renderError ? (
        <div className="absolute inset-0 flex items-center justify-center p-6">
          <div className="max-w-md rounded-2xl border border-red-200 bg-white p-6 text-center shadow-sm">
            <h3 className="text-base font-semibold text-slate-900">
              Unable to preview this document.
            </h3>
            <p className="mt-2 text-sm text-slate-600">
              Please check that the uploaded file is a valid DOCX file.
            </p>
            <p className="mt-2 text-xs text-red-600">{renderError}</p>
          </div>
        </div>
      ) : null}
    </div>
  )
}
