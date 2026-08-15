import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { Filter, Plus } from 'lucide-react'
import { Button } from 'primereact/button'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import {
  DEFAULT_LIST_PAGE_SIZE_OPTIONS,
  ListPagination,
  useListPagination,
} from '../../../../common/ListPagination'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import {
  QueryBuilder,
  callGetCompanyEmployees,
  parseFilterInput,
  prepareEmployeeFilter,
  toFilterJson,
  toFilterParams,
  toPagedListParams,
  useQueryBuilderState,
  type FilterAst,
  type FilterParams,
} from '../../../queryBuilder'
import { TenantPageShell } from '../../components/TenantPageShell'
import { TenantTable } from '../../components/TenantTable'
import {
  btnOutlinedClass,
  btnPrimaryClass,
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
  /** Applied QueryBuilder AST (UI); wire `query.filter` is JSON string */
  filterAst: FilterAst | null
  filterParams: FilterParams
  applyFilter: () => void
  clearFilter: () => void

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
  /**
   * Hiện QueryBuilder phía trên toolbar (fake fields / API fields).
   * @default true
   */
  withQueryBuilder?: boolean
  /** Gọi khi query (search/page/size/status/filter) đổi — dùng khi controlled */
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
  filter?: FilterAst | string | null
  sort?: string | null
  columns?: string | null
}): GetTenantListParams {
  const query: GetTenantListParams = {
    page: input.page,
    size: input.size,
  }
  const search = input.search.trim()
  if (search) query.search = search
  if (input.statusFilter !== 'all') query.status = input.statusFilter

  const ast =
    typeof input.filter === 'string'
      ? parseFilterInput(input.filter)
      : input.filter ?? null
  const filterJson = toFilterJson(ast)
  if (filterJson) query.filter = filterJson

  const sort = input.sort?.trim()
  if (sort) query.sort = sort
  const columns = input.columns?.trim()
  if (columns) query.columns = columns

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
  withQueryBuilder = true,
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
  const [appliedFilter, setAppliedFilter] = useState<FilterAst | null>(() =>
    parseFilterInput(initialQuery?.filter),
  )
  const [testingEmployees, setTestingEmployees] = useState(false)

  const items = controlled ? itemsProp : internalItems
  const total = controlled ? (totalProp ?? items.length) : internalTotal
  const loading = controlled ? Boolean(loadingProp) : internalLoading

  const pagination = useListPagination({
    total,
    initialPage: initialQuery?.page ?? 1,
    initialSize: initialQuery?.size ?? defaultPageSize,
  })
  const {
    page,
    size,
    pageInput,
    totalPages,
    setPage,
    setSize,
    setPageInput,
    commitPageInput,
  } = pagination

  const qb = useQueryBuilderState({
    loadFields: withQueryBuilder,
    fieldsMode: 'employees',
    initialAst: initialQuery?.filter ?? null,
    initialSort: initialQuery?.sort ?? '',
    initialColumns: initialQuery?.columns ?? '',
  })

  const [pendingDelete, setPendingDelete] = useState<Tenant | null>(null)
  const [deleting, setDeleting] = useState(false)

  // QueryBuilder filter dùng test company/employees APIs — không gửi vào tenant list
  const query = buildQuery({
    search,
    statusFilter,
    page,
    size,
    filter: null,
  })

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
    const nextQuery = buildQuery({
      search,
      statusFilter,
      page,
      size,
      filter: null,
    })
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

  const applyFilter = useCallback(async () => {
    const { ast, validation } = qb.deriveFilter()
    if (!validation.ok) {
      notify.error(validation.message)
      return
    }
    setAppliedFilter(ast)
    setPage(1)

    const prepared = prepareEmployeeFilter(ast)
    const params = toPagedListParams({
      page: 1,
      size,
      ast: prepared.requestAst,
      sort: qb.sort || null,
      columns: qb.columns || null,
    })

    setTestingEmployees(true)
    try {
      const res = await callGetCompanyEmployees(params)

      const raw = (res.data ?? {}) as Record<string, unknown>
      const total = Number(raw.total ?? raw.Total ?? 0)
      const items = Array.isArray(raw.items)
        ? raw.items
        : Array.isArray(raw.Items)
          ? raw.Items
          : []

      console.log('[TenantList] Company employees API', {
        api: 'GET /api/v1/company/employees',
        params,
        filterAst: prepared.filterAst,
        requestAst: prepared.requestAst,
        apiAst: prepared.serverAst,
        total,
        items,
        response: res.data,
      })
      notify.success(`employees: ${total} bản ghi`)
    } catch (error) {
      notify.error(getErrorMessage(error, 'Gọi API company/employees thất bại'))
      console.error('[TenantList] Company employees API error', error)
    } finally {
      setTestingEmployees(false)
    }
  }, [qb.deriveFilter, qb.sort, qb.columns, size])

  const clearFilter = useCallback(() => {
    qb.reset()
    setAppliedFilter(null)
    setPage(1)
    console.log('[TenantList] QueryBuilder cleared')
  }, [qb.reset])

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

  const appliedFilterParams = useMemo((): FilterParams => {
    if (!appliedFilter) return toFilterParams(null)
    try {
      return { filter: toFilterJson(appliedFilter) }
    } catch {
      return toFilterParams(null)
    }
  }, [appliedFilter])

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
    <div className="mb-4 flex flex-col gap-3">
      {withQueryBuilder ? (
        <div className="overflow-hidden rounded-2xl border border-slate-200/90 bg-white shadow-[0_1px_3px_rgba(15,23,42,0.06)]">
          <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 bg-gradient-to-r from-slate-50 to-white px-4 py-3">
            <div className="min-w-0">
              <p className="m-0 text-sm font-semibold text-slate-900">
                Bộ lọc nâng cao
              </p>
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <Button
                type="button"
                unstyled
                className={btnOutlinedClass}
                onClick={clearFilter}
                disabled={qb.loadingFields || testingEmployees}
              >
                Xóa
              </Button>
              <Button
                type="button"
                unstyled
                className={btnPrimaryClass}
                onClick={() => void applyFilter()}
                disabled={qb.loadingFields || testingEmployees}
              >
                <Filter className="size-4" />
                {testingEmployees ? 'Đang gọi API…' : 'Áp dụng'}
              </Button>
            </div>
          </div>
          <div className="p-3 sm:p-4">
            {qb.loadingFields ? (
              <div className="flex h-24 items-center justify-center rounded-xl border border-dashed border-slate-200 bg-slate-50/70 text-sm text-slate-500">
                Đang tải…
              </div>
            ) : (
              <QueryBuilder
                fields={qb.fields}
                query={qb.query}
                onQueryChange={qb.setQuery}
              />
            )}
          </div>
        </div>
      ) : null}

      
    </div>
  )

  const paginationBar = (
    <ListPagination
      total={total}
      page={page}
      size={size}
      pageInput={pageInput}
      totalPages={totalPages}
      loading={loading}
      onPageChange={setPage}
      onSizeChange={setSize}
      onPageInputChange={setPageInput}
      onCommitPageInput={commitPageInput}
    />
  )

  const defaultContent = (
    <>
      {toolbar}
      {defaultTable}
      {paginationBar}
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
    pageSizeOptions: [...DEFAULT_LIST_PAGE_SIZE_OPTIONS],
    filterAst: appliedFilter,
    filterParams: appliedFilterParams,
    applyFilter,
    clearFilter,
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
    DefaultPagination: paginationBar,
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
