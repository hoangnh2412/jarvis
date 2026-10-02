import { useEffect, useRef, useState, type ReactNode } from 'react'
import { getCraftDocMessages, type CraftDocLocale } from '../../localization'

const SIDEBAR_RATIO_KEY = 'craft-doc-sidebar-ratio'
const DEFAULT_SIDEBAR_RATIO = 0.3
const MIN_SIDEBAR_RATIO = 0.18
const MAX_SIDEBAR_RATIO = 0.62
const MIN_SIDEBAR_PX = 260

function clampSidebarRatio(ratio: number, containerWidth: number) {
  const minRatio = Math.min(MIN_SIDEBAR_RATIO, MIN_SIDEBAR_PX / Math.max(containerWidth, 1))
  return Math.min(MAX_SIDEBAR_RATIO, Math.max(minRatio, ratio))
}

function readStoredSidebarRatio() {
  if (typeof window === 'undefined') return DEFAULT_SIDEBAR_RATIO
  const raw = window.localStorage.getItem(SIDEBAR_RATIO_KEY)
  const parsed = raw ? Number(raw) : NaN
  if (!Number.isFinite(parsed)) return DEFAULT_SIDEBAR_RATIO
  return clampSidebarRatio(parsed, window.innerWidth)
}

export type CraftDocSplitLayoutProps = {
  locale?: CraftDocLocale
  form: ReactNode
  preview: ReactNode
  className?: string
}

export function CraftDocSplitLayout({
  locale = 'vi',
  form,
  preview,
  className = '',
}: CraftDocSplitLayoutProps) {
  const messages = getCraftDocMessages(locale)
  const containerRef = useRef<HTMLDivElement>(null)
  const [sidebarRatio, setSidebarRatio] = useState(DEFAULT_SIDEBAR_RATIO)
  const [resizing, setResizing] = useState(false)
  const dragRef = useRef<{ startX: number; startRatio: number; containerWidth: number } | null>(
    null,
  )

  useEffect(() => {
    setSidebarRatio(readStoredSidebarRatio())
  }, [])

  useEffect(() => {
    const onResize = () => {
      const width = containerRef.current?.clientWidth ?? window.innerWidth
      setSidebarRatio((current) => clampSidebarRatio(current, width))
    }
    window.addEventListener('resize', onResize)
    return () => window.removeEventListener('resize', onResize)
  }, [])

  useEffect(() => {
    if (!resizing) return

    const onPointerMove = (event: PointerEvent) => {
      const drag = dragRef.current
      if (!drag || drag.containerWidth <= 0) return
      const delta = (event.clientX - drag.startX) / drag.containerWidth
      setSidebarRatio(clampSidebarRatio(drag.startRatio + delta, drag.containerWidth))
    }

    const onPointerUp = () => {
      dragRef.current = null
      setResizing(false)
      document.body.classList.remove('craft-doc-split-resizing')
      setSidebarRatio((current) => {
        const width = containerRef.current?.clientWidth ?? window.innerWidth
        const next = clampSidebarRatio(current, width)
        window.localStorage.setItem(SIDEBAR_RATIO_KEY, String(next))
        return next
      })
    }

    window.addEventListener('pointermove', onPointerMove)
    window.addEventListener('pointerup', onPointerUp)
    window.addEventListener('pointercancel', onPointerUp)
    return () => {
      window.removeEventListener('pointermove', onPointerMove)
      window.removeEventListener('pointerup', onPointerUp)
      window.removeEventListener('pointercancel', onPointerUp)
      document.body.classList.remove('craft-doc-split-resizing')
    }
  }, [resizing])

  const sidebarPercent = Math.round(sidebarRatio * 100)
  const containerWidth = containerRef.current?.clientWidth ?? 0
  const sidebarPx =
    containerWidth > 0
      ? Math.round(containerWidth * sidebarRatio)
      : Math.round(DEFAULT_SIDEBAR_RATIO * 1200)

  const nudgeRatio = (delta: number) => {
    const width = containerRef.current?.clientWidth ?? window.innerWidth
    setSidebarRatio((current) => {
      const next = clampSidebarRatio(current + delta, width)
      window.localStorage.setItem(SIDEBAR_RATIO_KEY, String(next))
      return next
    })
  }

  return (
    <div
      ref={containerRef}
      className={[
        'craft-doc-split',
        resizing ? 'is-resizing' : '',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <div
        className="craft-doc-split__form"
        style={{ flexBasis: `${sidebarPercent}%`, width: `${sidebarPercent}%` }}
      >
        {form}
      </div>

      <div
        className="craft-doc-split__resizer"
        role="separator"
        aria-orientation="vertical"
        aria-label={messages.layout.resizePanel}
        aria-valuemin={Math.round(MIN_SIDEBAR_RATIO * 100)}
        aria-valuemax={Math.round(MAX_SIDEBAR_RATIO * 100)}
        aria-valuenow={sidebarPercent}
        tabIndex={0}
        onPointerDown={(event) => {
          if (event.button !== 0) return
          event.preventDefault()
          const width = containerRef.current?.clientWidth ?? window.innerWidth
          dragRef.current = {
            startX: event.clientX,
            startRatio: sidebarRatio,
            containerWidth: width,
          }
          setResizing(true)
          document.body.classList.add('craft-doc-split-resizing')
          ;(event.currentTarget as HTMLElement).setPointerCapture?.(event.pointerId)
        }}
        onKeyDown={(event) => {
          const step = event.shiftKey ? 0.08 : 0.04
          if (event.key === 'ArrowLeft') {
            event.preventDefault()
            nudgeRatio(step)
          }
          if (event.key === 'ArrowRight') {
            event.preventDefault()
            nudgeRatio(-step)
          }
          if (event.key === 'Home') {
            event.preventDefault()
            const width = containerRef.current?.clientWidth ?? window.innerWidth
            const next = clampSidebarRatio(MIN_SIDEBAR_RATIO, width)
            window.localStorage.setItem(SIDEBAR_RATIO_KEY, String(next))
            setSidebarRatio(next)
          }
          if (event.key === 'End') {
            event.preventDefault()
            const width = containerRef.current?.clientWidth ?? window.innerWidth
            const next = clampSidebarRatio(MAX_SIDEBAR_RATIO, width)
            window.localStorage.setItem(SIDEBAR_RATIO_KEY, String(next))
            setSidebarRatio(next)
          }
        }}
      />

      <div className="craft-doc-split__preview">{preview}</div>

      <span className="sr-only" aria-live="polite">
        {messages.layout.sidebarWidth.replace('{width}', String(sidebarPx))}
      </span>
    </div>
  )
}
