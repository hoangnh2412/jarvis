import { useCallback, useEffect, useRef, useState, type UIEvent } from 'react'
import type {
  SearchableSelectLoadOptions,
  SearchableSelectLoadResult,
  SearchableSelectOption,
} from '../common/SearchableSelect'

export const DEFAULT_SELECT_PAGE_SIZE = 10

export function normalizeSelectLoadResult(
  result: SearchableSelectLoadResult,
  pageSize: number,
): { options: SearchableSelectOption[]; hasMore: boolean } {
  if (Array.isArray(result)) {
    return {
      options: result,
      hasMore: result.length >= pageSize,
    }
  }
  const options = Array.isArray(result.options) ? result.options : []
  return {
    options,
    hasMore:
      typeof result.hasMore === 'boolean'
        ? result.hasMore
        : options.length >= pageSize,
  }
}

function isAbortError(error: unknown): boolean {
  if (error === null || typeof error !== 'object') return false
  return 'name' in error && (error as { name?: string }).name === 'AbortError'
}

type UseSearchableAsyncOptionsArgs = {
  enabled: boolean
  open: boolean
  loadOptions?: SearchableSelectLoadOptions
  search: string
  context?: Record<string, unknown>
  pageSize?: number
}

export function useSearchableAsyncOptions({
  enabled,
  open,
  loadOptions,
  search,
  context,
  pageSize = DEFAULT_SELECT_PAGE_SIZE,
}: UseSearchableAsyncOptionsArgs) {
  const [options, setOptions] = useState<SearchableSelectOption[]>([])
  const [page, setPage] = useState(1)
  const [hasMore, setHasMore] = useState(true)
  const [loading, setLoading] = useState(false)
  const [loadingMore, setLoadingMore] = useState(false)
  const loadingMoreRef = useRef(false)
  const requestIdRef = useRef(0)

  useEffect(() => {
    if (!enabled || !open || !loadOptions) return

    const controller = new AbortController()
    const requestId = ++requestIdRef.current
    setLoading(true)
    setLoadingMore(false)
    loadingMoreRef.current = false
    setPage(1)
    setHasMore(true)

    void loadOptions({
      search,
      signal: controller.signal,
      context,
      page: 1,
      pageSize,
    })
      .then((result) => {
        if (requestId !== requestIdRef.current) return
        const normalized = normalizeSelectLoadResult(result, pageSize)
        setOptions(normalized.options)
        setHasMore(normalized.hasMore)
        setPage(1)
      })
      .catch((error: unknown) => {
        if (requestId !== requestIdRef.current) return
        if (isAbortError(error)) return
        setOptions([])
        setHasMore(false)
      })
      .finally(() => {
        if (requestId === requestIdRef.current) setLoading(false)
      })

    return () => {
      controller.abort()
    }
  }, [enabled, open, loadOptions, search, context, pageSize])

  const loadMore = useCallback(() => {
    if (!enabled || !open || !loadOptions) return
    if (!hasMore || loading || loadingMoreRef.current) return

    const nextPage = page + 1
    const requestId = requestIdRef.current
    loadingMoreRef.current = true
    setLoadingMore(true)

    void loadOptions({
      search,
      context,
      page: nextPage,
      pageSize,
    })
      .then((result) => {
        if (requestId !== requestIdRef.current) return
        const normalized = normalizeSelectLoadResult(result, pageSize)
        setOptions((prev) => {
          const seen = new Set(prev.map((option) => String(option.value)))
          const appended = normalized.options.filter(
            (option) => !seen.has(String(option.value)),
          )
          return [...prev, ...appended]
        })
        setHasMore(normalized.hasMore)
        setPage(nextPage)
      })
      .catch((error: unknown) => {
        if (requestId !== requestIdRef.current) return
        if (isAbortError(error)) return
      })
      .finally(() => {
        if (requestId === requestIdRef.current) {
          loadingMoreRef.current = false
          setLoadingMore(false)
        }
      })
  }, [
    enabled,
    open,
    loadOptions,
    hasMore,
    loading,
    page,
    search,
    context,
    pageSize,
  ])

  const onListScroll = useCallback(
    (event: UIEvent<HTMLElement>) => {
      const el = event.currentTarget
      if (el.scrollTop + el.clientHeight >= el.scrollHeight - 32) {
        loadMore()
      }
    },
    [loadMore],
  )

  return {
    options,
    loading,
    loadingMore,
    hasMore,
    onListScroll,
  }
}
