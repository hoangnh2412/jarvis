import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ChangeEvent,
  type KeyboardEvent,
  type ReactNode,
} from 'react'
import {
  ChevronsLeft,
  ChevronsRight,
  ChevronLeft,
  ChevronRight,
  ChevronDown,
} from 'lucide-react'
import { InputText } from 'primereact/inputtext'
import { Select } from 'primereact/select'

export const DEFAULT_LIST_PAGE_SIZE_OPTIONS = [
  { label: '10/trang', value: 10 },
  { label: '20/trang', value: 20 },
  { label: '25/trang', value: 25 },
  { label: '50/trang', value: 50 },
] as const

export type ListPageSizeOption = {
  label: string
  value: number
}

export type UseListPaginationOptions = {
  total: number
  initialPage?: number
  initialSize?: number
}

export type ListPaginationState = {
  page: number
  size: number
  pageInput: string
  totalPages: number
  setPage: (page: number) => void
  setSize: (size: number) => void
  setPageInput: (value: string) => void
  commitPageInput: () => void
  resetPage: () => void
}

export function useListPagination({
  total,
  initialPage = 1,
  initialSize = 10,
}: UseListPaginationOptions): ListPaginationState {
  const [page, setPageState] = useState(initialPage)
  const [size, setSizeState] = useState(initialSize)
  const [pageInput, setPageInput] = useState(String(initialPage))

  const totalPages = useMemo(
    () => Math.max(1, Math.ceil(total / size) || 1),
    [total, size],
  )

  useEffect(() => {
    setPageInput(String(page))
  }, [page])

  useEffect(() => {
    if (page > totalPages) {
      setPageState(totalPages)
    }
  }, [page, totalPages])

  const setPage = useCallback(
    (next: number) => {
      setPageState(Math.min(totalPages, Math.max(1, next)))
    },
    [totalPages],
  )

  const setSize = useCallback((next: number) => {
    setSizeState(next)
    setPageState(1)
  }, [])

  const commitPageInput = useCallback(() => {
    const parsed = Number.parseInt(pageInput, 10)
    if (Number.isNaN(parsed)) {
      setPageInput(String(page))
      return
    }
    const next = Math.min(totalPages, Math.max(1, parsed))
    setPageState(next)
    setPageInput(String(next))
  }, [pageInput, page, totalPages])

  const resetPage = useCallback(() => {
    setPageState(1)
  }, [])

  return {
    page,
    size,
    pageInput,
    totalPages,
    setPage,
    setSize,
    setPageInput,
    commitPageInput,
    resetPage,
  }
}

export type ListPaginationVariant = 'default' | 'simple'

export type ListPaginationProps = {
  total: number
  page: number
  size: number
  pageInput: string
  totalPages: number
  loading?: boolean
  variant?: ListPaginationVariant
  pageSizeOptions?: readonly ListPageSizeOption[]
  className?: string
  formatSummary?: (range: { from: number; to: number; total: number }) => ReactNode
  pageOfLabel?: string
  pageSizeLabel?: string
  onPageChange: (page: number) => void
  onSizeChange: (size: number) => void
  onPageInputChange: (value: string) => void
  onCommitPageInput: () => void
}

function defaultFormatSummary({
  from,
  to,
  total,
}: {
  from: number
  to: number
  total: number
}) {
  if (total <= 0) return 'Không có bản ghi'
  return (
    <>
      Hiển thị{' '}
      <span className="kit-list-pagination-stat">{from}–{to}</span> trên{' '}
      <span className="kit-list-pagination-stat">{total}</span> bản ghi
    </>
  )
}

function simpleFormatSummary({ total }: { from: number; to: number; total: number }) {
  return <>Tổng số bản ghi: {total}</>
}

function PageSizeSelect({
  value,
  options,
  disabled,
  displayValueOnly = false,
  onChange,
}: {
  value: number
  options: readonly ListPageSizeOption[]
  disabled?: boolean
  displayValueOnly?: boolean
  onChange: (value: number) => void
}) {
  const [open, setOpen] = useState(false)
  const list = options as ListPageSizeOption[]

  return (
    <Select.Root
      value={value}
      open={open}
      options={list}
      optionLabel="label"
      optionValue="value"
      disabled={disabled}
      onOpenChange={(e: { value: boolean }) => setOpen(e.value)}
      onValueChange={(e: { value?: unknown }) => {
        if (e.value != null) onChange(Number(e.value))
      }}
    >
      <Select.Trigger type="button" className="kit-list-pagination-size-trigger">
        {displayValueOnly ? (
          <span className="kit-list-pagination-size-value">{value}</span>
        ) : (
          <Select.Value placeholder="Chọn…" />
        )}
        <Select.Indicator
          className={[
            'kit-list-pagination-size-chevron',
            open ? 'is-open' : '',
          ].join(' ')}
        >
          <ChevronDown className="size-3.5" aria-hidden />
        </Select.Indicator>
      </Select.Trigger>
      <Select.Portal>
        <Select.Positioner className="z-[120]">
          <Select.Popup className="kit-list-pagination-size-popup">
            <Select.List className="m-0 list-none p-0 outline-none">
              {list.map((option, index) => (
                <Select.Option
                  key={String(option.value)}
                  index={index}
                  className="kit-list-pagination-size-option"
                >
                  {option.label}
                </Select.Option>
              ))}
            </Select.List>
          </Select.Popup>
        </Select.Positioner>
      </Select.Portal>
    </Select.Root>
  )
}

function NavIconButton({
  label,
  disabled,
  onClick,
  children,
}: {
  label: string
  disabled?: boolean
  onClick: () => void
  children: ReactNode
}) {
  return (
    <button
      type="button"
      className="kit-list-pagination-nav-btn"
      aria-label={label}
      disabled={disabled}
      onClick={onClick}
    >
      {children}
    </button>
  )
}

function PageJumpInput({
  pageInput,
  totalPages,
  pageOfLabel,
  disabled,
  compact = false,
  onPageInputChange,
  onCommitPageInput,
}: {
  pageInput: string
  totalPages: number
  pageOfLabel: string
  disabled?: boolean
  compact?: boolean
  onPageInputChange: (value: string) => void
  onCommitPageInput: () => void
}) {
  return (
    <div className={['kit-list-pagination-jump', compact ? 'is-compact' : ''].filter(Boolean).join(' ')}>
      {!compact ? <span className="kit-list-pagination-jump-label">Trang</span> : null}
      <InputText
        value={pageInput}
        onChange={(e: ChangeEvent<HTMLInputElement>) =>
          onPageInputChange(e.target.value)
        }
        onBlur={onCommitPageInput}
        onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
          if (e.key === 'Enter') {
            e.preventDefault()
            onCommitPageInput()
          }
        }}
        disabled={disabled}
        unstyled
        className="kit-list-pagination-page-input"
        aria-label="Trang hiện tại"
        inputMode="numeric"
      />
      <span className="kit-list-pagination-page-sep">{pageOfLabel}</span>
      <span className="kit-list-pagination-page-total">{totalPages}</span>
    </div>
  )
}

/** Pagination bar dùng chung cho các trang danh sách (tenant, role, …). */
export function ListPagination({
  total,
  page,
  size,
  pageInput,
  totalPages,
  loading = false,
  variant = 'default',
  pageSizeOptions = DEFAULT_LIST_PAGE_SIZE_OPTIONS,
  className = '',
  formatSummary,
  pageOfLabel = '/',
  pageSizeLabel = 'Hiển thị',
  onPageChange,
  onSizeChange,
  onPageInputChange,
  onCommitPageInput,
}: ListPaginationProps) {
  const disabled = loading || total === 0
  const from = total > 0 ? (page - 1) * size + 1 : 0
  const to = total > 0 ? Math.min(page * size, total) : 0
  const isSimple = variant === 'simple'
  const summaryFormatter = formatSummary ?? (isSimple ? simpleFormatSummary : defaultFormatSummary)

  return (
    <nav
      className={[
        'kit-list-pagination',
        isSimple ? 'is-simple' : '',
        loading ? 'is-loading' : '',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
      aria-label="Phân trang"
    >
      <p className="kit-list-pagination-summary">
        {summaryFormatter({ from, to, total })}
      </p>

      <div className="kit-list-pagination-toolbar">
        <div className="kit-list-pagination-size">
          {!isSimple ? (
            <span className="kit-list-pagination-size-label">{pageSizeLabel}</span>
          ) : null}
          <PageSizeSelect
            value={size}
            options={pageSizeOptions}
            disabled={disabled}
            displayValueOnly={isSimple}
            onChange={onSizeChange}
          />
        </div>

        {!isSimple ? <div className="kit-list-pagination-divider" aria-hidden /> : null}

        <div className="kit-list-pagination-nav">
          {!isSimple ? (
            <NavIconButton
              label="Trang đầu"
              disabled={disabled || page <= 1}
              onClick={() => onPageChange(1)}
            >
              <ChevronsLeft className="size-3.5" />
            </NavIconButton>
          ) : null}
          <NavIconButton
            label="Trang trước"
            disabled={disabled || page <= 1}
            onClick={() => onPageChange(page - 1)}
          >
            <ChevronLeft className="size-3.5" />
          </NavIconButton>

          {isSimple ? (
            <>
              <span className="kit-list-pagination-jump-label">Trang</span>
              <PageJumpInput
                pageInput={pageInput}
                totalPages={totalPages}
                pageOfLabel={pageOfLabel}
                disabled={disabled}
                compact
                onPageInputChange={onPageInputChange}
                onCommitPageInput={onCommitPageInput}
              />
            </>
          ) : (
            <PageJumpInput
              pageInput={pageInput}
              totalPages={totalPages}
              pageOfLabel={pageOfLabel}
              disabled={disabled}
              onPageInputChange={onPageInputChange}
              onCommitPageInput={onCommitPageInput}
            />
          )}

          <NavIconButton
            label="Trang sau"
            disabled={disabled || page >= totalPages}
            onClick={() => onPageChange(page + 1)}
          >
            <ChevronRight className="size-3.5" />
          </NavIconButton>
          {!isSimple ? (
            <NavIconButton
              label="Trang cuối"
              disabled={disabled || page >= totalPages}
              onClick={() => onPageChange(totalPages)}
            >
              <ChevronsRight className="size-3.5" />
            </NavIconButton>
          ) : null}
        </div>
      </div>
    </nav>
  )
}
