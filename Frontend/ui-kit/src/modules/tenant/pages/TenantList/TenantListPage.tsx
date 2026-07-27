import {
  useCallback,
  useEffect,
  useState,
  type ChangeEvent,
  type KeyboardEvent,
  type ReactNode,
} from 'react'
import { ChevronsLeft, ChevronsRight, ChevronLeft, ChevronRight, Plus, Search } from 'lucide-react'
import { Button } from 'primereact/button'
import { IconField } from 'primereact/iconfield'
import { InputText } from 'primereact/inputtext'
import { Paginator } from 'primereact/paginator'
import type { PaginatorRootChangeEvent } from 'primereact/paginator'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { TenantPageShell } from '../../components/TenantPageShell'
import { TenantTable } from '../../components/TenantTable'
import { FieldSelect } from '../../components/FieldSelect'
import {
  btnOutlinedClass,
  btnPrimaryClass,
  fieldInputClass,
} from '../../components/fieldStyles'
import { getTenantMessages, type TenantLocale } from '../../localization'
import {
  callDeleteTenant,
  callGetTenantList,
  callSetTenantStatus,
} from '../../services'
import {
  getTenantCreatePath,
  getTenantDetailPath,
  getTenantEditPath,
  navigateTenant,
} from '../../routes'
import {
  TENANT_STATUS_OPTIONS,
  TenantStatus,
  type GetTenantListParams,
  type Tenant,
  type TenantStatusValue,
} from '../../types'
import { unwrapTenantListResult } from '../../utils/apiData'
import {
  resolveTenantContent,
  type TenantSlotContent,
} from '../../utils'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'

const PAGE_SIZE_OPTIONS = [
  { label: '10 / trang', value: 10 },
  { label: '20 / trang', value: 20 },
  { label: '50 / trang', value: 50 },
] as const

export type TenantListPageContentContext = {
  /** Data */
  items: Tenant[]
  total: number
  loading: boolean
  query: GetTenantListParams
  totalPages: number

  /** Filter / pagination state */
  searchInput: string
  search: string
  statusFilter: TenantStatusValue | 'all'
  page: number
  size: number
  pageInput: string
  statusOptions: { label: string; value: TenantStatusValue | 'all' }[]
  pageSizeOptions: { label: string; value: number }[]

  /** Filter / pagination setters */
  setSearchInput: (value: string) => void
  setStatusFilter: (value: TenantStatusValue | 'all') => void
  setPage: (page: number) => void
  setSize: (size: number) => void
  setPageInput: (value: string) => void
  commitPageInput: () => void

  /** Data */
  fetchTenants: () => Promise<void>
  reload: () => Promise<void>

  /** Actions — gắn vào UI custom */
  onCreate?: () => void
  onView?: (tenant: Tenant) => void
  onEdit?: (tenant: Tenant) => void
  /** Mở confirm xoá (dùng ConfirmDialog mặc định của kit) */
  requestDelete: (tenant: Tenant) => void
  onToggleStatus: (tenant: Tenant) => void | Promise<void>
  pendingDelete: Tenant | null
  deleting: boolean
  cancelDelete: () => void
  confirmDelete: () => void | Promise<void>

  /** UI mặc định (có thể tái sử dụng từng phần) */
  DefaultToolbar: ReactNode
  DefaultTable: ReactNode
  DefaultPagination: ReactNode
  DefaultContent: ReactNode
}

export type TenantListPageProps = {
  /** Nếu không truyền, page tự fetch qua `callGetTenantList` */
  items?: Tenant[]
  loading?: boolean
  /** Tổng số bản ghi (khi controlled + pagination UI) */
  total?: number
  /** Prefill / override query ban đầu */
  initialQuery?: Partial<GetTenantListParams>
  /** Page size mặc định — query `?size=` */
  defaultPageSize?: number
  locale?: TenantLocale
  title?: string
  description?: string
  className?: string
  onCreate?: () => void
  onView?: (tenant: Tenant) => void
  onEdit?: (tenant: Tenant) => void
  /**
   * `false` = ẩn nút tạo / không gắn navigate mặc định cho view/edit.
   * @default true
   */
  useRoutes?: boolean
  /**
   * Action callback kiểu jQuery ajax: `before` / `onSubmit` / `success` / `error` / `complete`.
   */
  callback?: {
    delete?: ActionProps<{ tenant: Tenant }, Tenant>
    toggleStatus?: ActionProps<{ tenant: Tenant }, Tenant>
  }
  /** Gọi khi query (search/page/size/status) đổi — dùng khi controlled */
  onQueryChange?: (query: GetTenantListParams) => void
  headerActions?: ReactNode
  content?: TenantSlotContent<TenantListPageContentContext>
  withShell?: boolean
  /**
   * `false` = không render ConfirmDialog xoá mặc định.
   * Dùng khi custom confirm qua `content` (`pendingDelete` / `confirmDelete`).
   * @default true
   */
  withDialogs?: boolean
}

function buildQuery(input: {
  search: string
  statusFilter: TenantStatusValue | 'all'
  page: number
  size: number
}): GetTenantListParams {
  const query: GetTenantListParams = {
    page: input.page,
    size: input.size,
  }
  const search = input.search.trim()
  if (search) query.search = search
  if (input.statusFilter !== 'all') query.status = input.statusFilter
  return query
}

export function TenantListPage({
  items: itemsProp,
  loading: loadingProp,
  total: totalProp,
  initialQuery,
  defaultPageSize = 10,
  locale = 'vi',
  title,
  description,
  className,
  onCreate,
  onView,
  onEdit,
  useRoutes = true,
  callback,
  onQueryChange,
  headerActions,
  content,
  withShell = true,
  withDialogs = true,
}: TenantListPageProps) {
  const messages = getTenantMessages(locale)
  const controlled = itemsProp != null

  const [internalItems, setInternalItems] = useState<Tenant[]>([])
  const [internalTotal, setInternalTotal] = useState(0)
  const [internalLoading, setInternalLoading] = useState(!controlled)

  const [searchInput, setSearchInput] = useState(initialQuery?.search ?? '')
  const [search, setSearch] = useState(initialQuery?.search ?? '')
  const [statusFilter, setStatusFilter] = useState<TenantStatusValue | 'all'>(
    initialQuery?.status ?? 'all',
  )
  const [page, setPage] = useState(initialQuery?.page ?? 1)
  const [size, setSize] = useState(initialQuery?.size ?? defaultPageSize)
  const [pageInput, setPageInput] = useState(String(initialQuery?.page ?? 1))

  const [pendingDelete, setPendingDelete] = useState<Tenant | null>(null)
  const [deleting, setDeleting] = useState(false)

  const items = controlled ? itemsProp : internalItems
  const total = controlled ? (totalProp ?? items.length) : internalTotal
  const loading = controlled ? Boolean(loadingProp) : internalLoading

  const query = buildQuery({ search, statusFilter, page, size })
  const totalPages = Math.max(1, Math.ceil(total / size) || 1)

  useEffect(() => {
    setPageInput(String(page))
  }, [page])

  // Debounce search → backend
  useEffect(() => {
    const timer = window.setTimeout(() => {
      if (searchInput === search) return
      setSearch(searchInput)
      setPage(1)
    }, 350)
    return () => window.clearTimeout(timer)
  }, [searchInput, search])

  const fetchTenants = useCallback(async () => {
    const nextQuery = buildQuery({ search, statusFilter, page, size })
    if (controlled) {
      onQueryChange?.(nextQuery)
      return
    }
    setInternalLoading(true)
    try {
      const res = await callGetTenantList(nextQuery)
      const result = unwrapTenantListResult(res)
      setInternalItems(result.items)
      setInternalTotal(result.total)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setInternalLoading(false)
    }
  }, [
    controlled,
    search,
    statusFilter,
    page,
    size,
    onQueryChange,
    messages.form.saveError,
  ])

  useEffect(() => {
    fetchTenants()
  }, [fetchTenants])

  const defaultDelete = async (tenant: Tenant) => {
    await callDeleteTenant(tenant.id)
  }

  const defaultToggleStatus = async (tenant: Tenant) => {
    const next =
      tenant.status === TenantStatus.Active
        ? TenantStatus.Inactive
        : TenantStatus.Active
    await callSetTenantStatus(tenant.id, { status: next })
  }

  const handleConfirmDelete = async () => {
    if (!pendingDelete) return
    setDeleting(true)
    try {
      const outcome = await handleAction({
        ctx: { tenant: pendingDelete },
        callback: callback?.delete,
        defaultSubmit: defaultDelete,
        getPayload: ({ tenant }) => tenant,
        onSuccess: async () => {
          await fetchTenants()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(messages.form.deleteSuccess)
      setPendingDelete(null)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setDeleting(false)
    }
  }

  const handleToggleStatus = async (tenant: Tenant) => {
    try {
      const outcome = await handleAction({
        ctx: { tenant },
        callback: callback?.toggleStatus,
        defaultSubmit: defaultToggleStatus,
        getPayload: ({ tenant: t }) => t,
        onSuccess: async () => {
          await fetchTenants()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(
        tenant.status === TenantStatus.Active
          ? 'Đã ngừng hoạt động tenant'
          : 'Đã kích hoạt tenant',
      )
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    }
  }

  const statusOptions = [
    { label: 'Tất cả trạng thái', value: 'all' as const },
    ...TENANT_STATUS_OPTIONS.map((o) => ({
      label: o.label,
      value: o.value,
    })),
  ]

  const handleCreate =
    onCreate ??
    (useRoutes ? () => navigateTenant(getTenantCreatePath()) : undefined)

  const handleView =
    onView ??
    (useRoutes
      ? (tenant: Tenant) => navigateTenant(getTenantDetailPath(tenant.id))
      : undefined)

  const handleEdit =
    onEdit ??
    (useRoutes
      ? (tenant: Tenant) => navigateTenant(getTenantEditPath(tenant.id))
      : undefined)

  const defaultTable = (
    <TenantTable
      items={items}
      loading={loading}
      emptyMessage={messages.list.empty}
      onView={handleView}
      onEdit={handleEdit}
      onDelete={(t) => setPendingDelete(t)}
      onToggleStatus={handleToggleStatus}
    />
  )

  const toolbar = (
    <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center">
      <IconField.Root className="relative min-w-0 flex-1">
        <IconField.Inset className="pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2 text-slate-400">
          <Search className="size-4" />
        </IconField.Inset>
        <InputText
          value={searchInput}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setSearchInput(e.target.value)
          }
          placeholder={messages.list.searchPlaceholder}
          unstyled
          className={`${fieldInputClass} !pl-10`}
        />
      </IconField.Root>
      <div className="w-full sm:w-[200px]">
        <FieldSelect
          value={statusFilter}
          options={statusOptions}
          onChange={(value) => {
            setStatusFilter(value)
            setPage(1)
          }}
          placeholder="Trạng thái"
        />
      </div>
    </div>
  )

  const paginatorNavClass = `${btnOutlinedClass} !h-9 !min-w-9 !px-2`

  const commitPageInput = () => {
    const parsed = Number.parseInt(pageInput, 10)
    if (Number.isNaN(parsed)) {
      setPageInput(String(page))
      return
    }
    const next = Math.min(totalPages, Math.max(1, parsed))
    setPage(next)
    setPageInput(String(next))
  }

  const pagination = (
    <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      {total > 0 ? (
        <p className="m-0 text-sm text-mute">
          {`Hiển thị ${(page - 1) * size + 1}–${Math.min(page * size, total)} / ${total}`}
        </p>
      ) : (
        <span />
      )}
      <div className="flex flex-wrap items-center gap-3 sm:ml-auto">
        <div className="w-[140px]">
          <FieldSelect
            value={size}
            options={[...PAGE_SIZE_OPTIONS]}
            onChange={(value) => {
              setSize(value)
              setPage(1)
            }}
          />
        </div>
        <Paginator.Root
          page={page}
          total={total}
          itemsPerPage={size}
          disabled={loading || total === 0}
          onPageChange={(e: PaginatorRootChangeEvent) => setPage(e.value)}
          className="shrink-0"
        >
          <Paginator.Content className="flex flex-wrap items-center gap-1">
            <Paginator.First className={paginatorNavClass}>
              <ChevronsLeft className="size-4" />
            </Paginator.First>
            <Paginator.Prev className={paginatorNavClass}>
              <ChevronLeft className="size-4" />
            </Paginator.Prev>
            <div className="mx-1 flex items-center gap-2">
              <InputText
                value={pageInput}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  setPageInput(e.target.value)
                }
                onBlur={commitPageInput}
                onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
                  if (e.key === 'Enter') {
                    e.preventDefault()
                    commitPageInput()
                  }
                }}
                disabled={loading || total === 0}
                unstyled
                className="box-border h-9 w-12 rounded-lg border border-slate-200 bg-white px-2 text-center text-sm text-ink shadow-sm outline-none transition-[border-color,box-shadow] focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:opacity-50"
                aria-label="Trang hiện tại"
              />
              <span className="whitespace-nowrap text-sm text-mute">
                of {totalPages}
              </span>
            </div>
            <Paginator.Next className={paginatorNavClass}>
              <ChevronRight className="size-4" />
            </Paginator.Next>
            <Paginator.Last className={paginatorNavClass}>
              <ChevronsRight className="size-4" />
            </Paginator.Last>
          </Paginator.Content>
        </Paginator.Root>
      </div>
    </div>
  )

  const defaultContent = (
    <>
      {toolbar}
      {defaultTable}
      {pagination}
    </>
  )

  const contentCtx: TenantListPageContentContext = {
    items,
    total,
    loading,
    query,
    totalPages,
    searchInput,
    search,
    statusFilter,
    page,
    size,
    pageInput,
    statusOptions,
    pageSizeOptions: [...PAGE_SIZE_OPTIONS],
    setSearchInput,
    setStatusFilter: (value) => {
      setStatusFilter(value)
      setPage(1)
    },
    setPage,
    setSize: (value) => {
      setSize(value)
      setPage(1)
    },
    setPageInput,
    commitPageInput,
    fetchTenants,
    reload: fetchTenants,
    onCreate: handleCreate,
    onView: handleView,
    onEdit: handleEdit,
    requestDelete: (tenant) => setPendingDelete(tenant),
    onToggleStatus: handleToggleStatus,
    pendingDelete,
    deleting,
    cancelDelete: () => setPendingDelete(null),
    confirmDelete: handleConfirmDelete,
    DefaultToolbar: toolbar,
    DefaultTable: defaultTable,
    DefaultPagination: pagination,
    DefaultContent: defaultContent,
  }

  const resolved = resolveTenantContent(content, contentCtx, defaultContent)

  const actions = (
    <>
      {headerActions}
      {handleCreate && (
        <Button
          type="button"
          unstyled
          className={btnPrimaryClass}
          onClick={handleCreate}
        >
          <Plus className="size-4" />
          {messages.list.create}
        </Button>
      )}
    </>
  )

  const dialog = (
    <ConfirmDialog
      open={Boolean(pendingDelete)}
      onClose={() => setPendingDelete(null)}
      onConfirm={handleConfirmDelete}
      loading={deleting}
      title="Xoá tenant?"
      description={
        pendingDelete
          ? `Tenant 「${pendingDelete.code}」 sẽ bị xoá mềm và không còn phục vụ request.`
          : undefined
      }
      confirmText="Xoá"
    />
  )

  if (!withShell) {
    return (
      <>
        {resolved}
        {withDialogs ? dialog : null}
      </>
    )
  }

  return (
    <>
      <TenantPageShell
        title={title ?? messages.routes.list}
        description={description ?? messages.list.description}
        headerActions={actions}
        className={className}
      >
        {resolved}
      </TenantPageShell>
      {withDialogs ? dialog : null}
    </>
  )
}
