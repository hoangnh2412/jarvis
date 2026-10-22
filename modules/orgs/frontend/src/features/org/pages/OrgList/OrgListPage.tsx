import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type FormEventHandler,
  type ReactNode,
} from 'react'
import { Plus } from 'lucide-react'
import {
  Button,
  ConfirmDialog,
  FeatureDialog,
  InputText,
  ListPagination,
  getErrorMessage,
  handleAction,
  notify,
  useListPagination,
  type ActionProps,
} from '@platform/core'
import {
  OrgDetailView,
  OrgForm,
  OrgPageShell,
  OrgTable,
} from '../../components'
import { btnPrimaryClass, fieldInputClass } from '../../components/fieldStyles'
import { useOrgForm } from '../../hooks'
import { getOrgMessages, type OrgLocale } from '../../localization'
import {
  callCreateOrg,
  callGetOrg,
  callListOrgs,
  callSetOrgActive,
  callUpdateOrg,
  configureOrgRequestScope,
} from '../../services'
import type { CreateOrgPayload, Org } from '../../types'
import { resolveOrgContent, unwrapOrgBody, type OrgSlotContent } from '../../utils'
import {
  orgFormDefaultValues,
  toOrgFormValues,
  type OrgFormData,
} from '../../validation'

/* Bộ đếm request chỉ đọc trong callback async (reload / getById), không trong render. */
/* eslint-disable react-hooks/refs */

type FormDialogMode = 'create' | 'edit' | 'view' | null
type ActiveFilter = 'all' | 'active' | 'inactive'

type PendingActive = {
  org: Org
  next: boolean
}

export type OrgListQuery = {
  search: string
  isActive?: boolean
  page: number
  size: number
}

export type OrgListPageContentContext = {
  items: Org[]
  total: number
  loading: boolean
  searchInput: string
  setSearchInput: (value: string) => void
  activeFilter: ActiveFilter
  setActiveFilter: (value: ActiveFilter) => void
  reload: () => Promise<void>
  openCreate: () => void
  openEdit: (org: Org) => void
  openView: (org: Org) => void
  requestSetActive: (org: Org, next: boolean) => void
  pendingActive: PendingActive | null
  updatingActive: boolean
  cancelSetActive: () => void
  confirmSetActive: () => void | Promise<void>
  page: number
  size: number
  totalPages: number
  DefaultToolbar: ReactNode
  DefaultTable: ReactNode
  DefaultPagination: ReactNode
  DefaultDialogs: ReactNode
  DefaultContent: ReactNode
}

export type OrgListPageProps = {
  items?: Org[]
  loading?: boolean
  total?: number
  locale?: OrgLocale
  title?: string
  description?: string
  className?: string
  headerActions?: ReactNode
  /** Ẩn tạo / sửa / đổi trạng thái khi host không có `Org.Manage`. */
  canManage?: boolean
  tenantId?: string
  /** Organization làm việc gửi lên header `X-Org-Id`. */
  orgId?: string
  /**
   * Host giữ tham chiếu ổn định. Chỉ gọi khi page ở chế độ controlled (`items` được truyền).
   */
  onQueryChange?: (query: OrgListQuery) => void
  callback?: {
    create?: ActionProps<{ data: OrgFormData }, CreateOrgPayload, Org>
    update?: ActionProps<
      { org: Org; data: OrgFormData },
      { id: string; data: CreateOrgPayload },
      Org
    >
    setActive?: ActionProps<
      { org: Org; isActive: boolean },
      { id: string; isActive: boolean },
      Org
    >
  }
  content?: OrgSlotContent<OrgListPageContentContext>
  withShell?: boolean
  withDialogs?: boolean
}

function activeParam(filter: ActiveFilter): boolean | undefined {
  if (filter === 'active') return true
  if (filter === 'inactive') return false
  return undefined
}

export function OrgListPage({
  items: itemsProp,
  loading: loadingProp,
  total: totalProp,
  locale = 'vi',
  title,
  description,
  className,
  headerActions,
  canManage = true,
  tenantId,
  orgId,
  onQueryChange,
  callback,
  content,
  withShell = true,
  withDialogs = true,
}: OrgListPageProps) {
  const messages = getOrgMessages(locale)
  const controlled = itemsProp != null
  const requestSeq = useRef(0)
  const detailSeq = useRef(0)

  const [internalItems, setInternalItems] = useState<Org[]>([])
  const [internalLoading, setInternalLoading] = useState(!controlled)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [activeFilter, setActiveFilter] = useState<ActiveFilter>('all')
  const [formMode, setFormMode] = useState<FormDialogMode>(null)
  const [editingOrg, setEditingOrg] = useState<Org | null>(null)
  const [pendingActive, setPendingActive] = useState<PendingActive | null>(null)
  const [updatingActive, setUpdatingActive] = useState(false)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useOrgForm()

  const items = controlled ? itemsProp : internalItems
  const isActive = activeParam(activeFilter)
  const total = controlled ? (totalProp ?? items.length) : items.length
  const loading = controlled ? Boolean(loadingProp) : internalLoading

  const {
    page,
    size,
    pageInput,
    totalPages,
    setPage,
    setSize,
    setPageInput,
    commitPageInput,
    resetPage,
  } = useListPagination({ total })

  useEffect(() => {
    configureOrgRequestScope({ tenantId, orgId })
  }, [tenantId, orgId])

  useEffect(() => {
    const timer = window.setTimeout(() => {
      if (searchInput === search) return
      setSearch(searchInput)
      resetPage()
    }, 350)
    return () => window.clearTimeout(timer)
  }, [searchInput, search, resetPage])

  const reload = useCallback(async () => {
    if (controlled) return
    const seq = requestSeq.current + 1
    requestSeq.current = seq
    setInternalLoading(true)
    try {
      const response = await callListOrgs({ search, isActive })
      if (seq !== requestSeq.current) return
      const list = unwrapOrgBody<Org[]>(response.data, messages.form.loadError)
      setInternalItems(Array.isArray(list) ? list : [])
    } catch (error) {
      if (seq !== requestSeq.current) return
      setInternalItems([])
      notify.error(getErrorMessage(error, messages.form.loadError))
    } finally {
      if (seq === requestSeq.current) setInternalLoading(false)
    }
  }, [controlled, search, isActive, messages.form.loadError])

  useEffect(() => {
    // Tải lại khi bộ lọc đổi. setState nằm trong reload — đúng hợp đồng danh sách.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void reload()
  }, [reload])

  useEffect(() => {
    if (!controlled || !onQueryChange) return
    onQueryChange({ search, isActive, page, size })
  }, [controlled, onQueryChange, search, isActive, page, size])

  const pageItems = useMemo(() => {
    if (controlled) return items
    const start = (page - 1) * size
    return items.slice(start, start + size)
  }, [controlled, items, page, size])

  const closeFormDialog = () => {
    setFormMode(null)
    setEditingOrg(null)
  }

  const openCreate = () => {
    setEditingOrg(null)
    setFormMode('create')
    reset(orgFormDefaultValues)
  }

  const openLoaded = async (org: Org, mode: 'edit' | 'view') => {
    const seq = detailSeq.current + 1
    detailSeq.current = seq
    try {
      const response = await callGetOrg(org.id)
      if (seq !== detailSeq.current) return
      const fresh = unwrapOrgBody<Org>(response.data, messages.form.loadError)
      setEditingOrg(fresh)
      setFormMode(mode)
      if (mode === 'edit') reset(toOrgFormValues(fresh))
    } catch (error) {
      if (seq !== detailSeq.current) return
      notify.error(getErrorMessage(error, messages.form.loadError))
    }
  }

  const onSaveForm: FormEventHandler<HTMLFormElement> = handleSubmit(
    async (data) => {
      try {
        if (formMode === 'create') {
          const outcome = await handleAction({
            ctx: { data },
            callback: callback?.create,
            defaultSubmit: async (payload) => {
              const response = await callCreateOrg(payload)
              return unwrapOrgBody<Org>(response.data, messages.form.saveError)
            },
            getPayload: ({ data: form }) => ({
              code: form.code,
              name: form.name,
            }),
            onSuccess: async () => {
              closeFormDialog()
              await reload()
            },
          })
          if (outcome.status === 'cancelled') return
          notify.success(messages.form.saveSuccess)
          return
        }

        if (formMode === 'edit' && editingOrg) {
          const outcome = await handleAction({
            ctx: { org: editingOrg, data },
            callback: callback?.update,
            defaultSubmit: async ({ id, data: payload }) => {
              const response = await callUpdateOrg(id, payload)
              return unwrapOrgBody<Org>(response.data, messages.form.saveError)
            },
            getPayload: ({ org, data: form }) => ({
              id: org.id,
              data: { code: form.code, name: form.name },
            }),
            onSuccess: async () => {
              closeFormDialog()
              await reload()
            },
          })
          if (outcome.status === 'cancelled') return
          notify.success(messages.form.saveSuccess)
        }
      } catch (error) {
        notify.error(getErrorMessage(error, messages.form.saveError))
      }
    },
  )

  const confirmSetActive = async () => {
    if (!pendingActive) return
    setUpdatingActive(true)
    try {
      const outcome = await handleAction({
        ctx: { org: pendingActive.org, isActive: pendingActive.next },
        callback: callback?.setActive,
        defaultSubmit: async ({ id, isActive: next }) => {
          const response = await callSetOrgActive(id, { isActive: next })
          return unwrapOrgBody<Org>(response.data, messages.form.saveError)
        },
        getPayload: ({ org, isActive: next }) => ({ id: org.id, isActive: next }),
        onSuccess: async () => {
          setPendingActive(null)
          await reload()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(
        pendingActive.next
          ? messages.form.activateSuccess
          : messages.form.deactivateSuccess,
      )
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setUpdatingActive(false)
    }
  }

  const changeActiveFilter = (value: ActiveFilter) => {
    setActiveFilter(value)
    resetPage()
  }

  const hasOrgHeader = Boolean(orgId?.trim() || import.meta.env.VITE_ORG_ID?.trim())

  const toolbar = (
    <div className="mb-4 flex flex-col gap-3 sm:flex-row">
      <InputText
        unstyled
        value={searchInput}
        placeholder={messages.list.searchPlaceholder}
        className={fieldInputClass}
        onChange={(event: { target: { value: string } }) =>
          setSearchInput(event.target.value)
        }
      />
      <select
        className={`${fieldInputClass} sm:max-w-56`}
        value={activeFilter}
        onChange={(event) =>
          changeActiveFilter(event.target.value as ActiveFilter)
        }
      >
        <option value="all">{messages.list.statusAll}</option>
        <option value="active">{messages.list.statusActive}</option>
        <option value="inactive">{messages.list.statusInactive}</option>
      </select>
    </div>
  )

  const defaultTable = (
    <OrgTable
      items={pageItems}
      loading={loading}
      locale={locale}
      emptyMessage={messages.list.empty}
      onView={(org) => void openLoaded(org, 'view')}
      onEdit={canManage ? (org) => void openLoaded(org, 'edit') : undefined}
      onActivate={
        canManage ? (org) => setPendingActive({ org, next: true }) : undefined
      }
      onDeactivate={
        canManage ? (org) => setPendingActive({ org, next: false }) : undefined
      }
    />
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

  const formTitle =
    formMode === 'create'
      ? messages.form.createTitle
      : formMode === 'edit'
        ? messages.form.editTitle
        : messages.form.viewTitle

  const dialogs = (
    <>
      <FeatureDialog
        open={formMode === 'create' || formMode === 'edit'}
        onClose={closeFormDialog}
        title={formTitle}
        size="md"
        onSubmit={onSaveForm}
        submitting={isSubmitting}
        submitLabel={formMode === 'create' ? messages.list.create : 'Lưu'}
      >
        <OrgForm
          register={register}
          errors={errors}
          codeLabel={`${messages.form.code} *`}
          nameLabel={`${messages.form.name} *`}
          codePlaceholder={messages.form.codePlaceholder}
          namePlaceholder={messages.form.namePlaceholder}
        />
      </FeatureDialog>

      <FeatureDialog
        open={formMode === 'view' && editingOrg != null}
        onClose={closeFormDialog}
        title={formTitle}
        size="md"
      >
        {editingOrg ? (
          <OrgDetailView org={editingOrg} labels={messages.detail} />
        ) : null}
      </FeatureDialog>

      <ConfirmDialog
        open={pendingActive != null}
        onClose={() => setPendingActive(null)}
        onConfirm={() => void confirmSetActive()}
        loading={updatingActive}
        variant={pendingActive?.next === false ? 'danger' : 'primary'}
        title={
          pendingActive?.next === false
            ? messages.confirm.deactivateTitle
            : messages.confirm.activateTitle
        }
        description={
          pendingActive
            ? `${pendingActive.org.code} — ${pendingActive.org.name}`
            : undefined
        }
        confirmText={
          pendingActive?.next === false
            ? messages.confirm.deactivate
            : messages.confirm.activate
        }
      />
    </>
  )

  const defaultContent = (
    <>
      {toolbar}
      {!hasOrgHeader ? (
        <p className="mb-3 mt-0 text-sm text-amber-800">{messages.list.scopeHint}</p>
      ) : null}
      {defaultTable}
      {paginationBar}
    </>
  )

  const contentCtx: OrgListPageContentContext = {
    items: pageItems,
    total,
    loading,
    searchInput,
    setSearchInput,
    activeFilter,
    setActiveFilter: changeActiveFilter,
    reload,
    openCreate,
    openEdit: (org) => void openLoaded(org, 'edit'),
    openView: (org) => void openLoaded(org, 'view'),
    requestSetActive: (org, next) => setPendingActive({ org, next }),
    pendingActive,
    updatingActive,
    cancelSetActive: () => setPendingActive(null),
    confirmSetActive,
    page,
    size,
    totalPages,
    DefaultToolbar: toolbar,
    DefaultTable: defaultTable,
    DefaultPagination: paginationBar,
    DefaultDialogs: dialogs,
    DefaultContent: defaultContent,
  }

  const resolved = resolveOrgContent(content, contentCtx, defaultContent)

  const actions = (
    <>
      {headerActions}
      {canManage ? (
        <Button type="button" unstyled className={btnPrimaryClass} onClick={openCreate}>
          <Plus className="size-4" />
          {messages.list.create}
        </Button>
      ) : null}
    </>
  )

  const shellDescription = description ?? messages.list.description

  if (!withShell) {
    return (
      <>
        {resolved}
        {withDialogs ? dialogs : null}
      </>
    )
  }

  return (
    <>
      <OrgPageShell
        title={title ?? messages.routes.list}
        description={shellDescription}
        headerActions={actions}
        className={className}
      >
        {resolved}
      </OrgPageShell>
      {withDialogs ? dialogs : null}
    </>
  )
}
