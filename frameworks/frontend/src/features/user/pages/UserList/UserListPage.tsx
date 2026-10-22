import {
  useCallback,
  useEffect,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import { Plus, Search } from 'lucide-react'
import { Button } from 'primereact/button'
import { InputText } from 'primereact/inputtext'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import {
  ListPagination,
  useListPagination,
} from '../../../../common/ListPagination'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { UserPageShell } from '../../components/UserPageShell'
import { UserTable } from '../../components/UserTable'
import { btnPrimaryClass } from '../../components/fieldStyles'
import { getUserMessages, type UserLocale } from '../../localization'
import {
  getUserCreatePath,
  getUserDetailPath,
  getUserEditPath,
  navigateUser,
} from '../../routes'
import { mockDeleteUser, mockGetUserList } from '../../services'
import type { AdminUser } from '../../types'
import { resolveUserContent, type UserSlotContent } from '../../utils'

export type UserListPageContentContext = {
  items: AdminUser[]
  total: number
  loading: boolean
  searchInput: string
  setSearchInput: (value: string) => void
  reload: () => Promise<void>
  openCreate: () => void
  openEdit: (user: AdminUser) => void
  openView: (user: AdminUser) => void
  requestDelete: (user: AdminUser) => void
  DefaultTable: ReactNode
  DefaultPagination: ReactNode
  DefaultContent: ReactNode
}

export type UserListPageProps = {
  items?: AdminUser[]
  loading?: boolean
  total?: number
  locale?: UserLocale
  title?: string
  description?: string
  className?: string
  headerActions?: ReactNode
  useRoutes?: boolean
  onCreate?: () => void
  onView?: (user: AdminUser) => void
  onEdit?: (user: AdminUser) => void
  callback?: {
    delete?: ActionProps<{ user: AdminUser }, AdminUser>
  }
  content?: UserSlotContent<UserListPageContentContext>
  withShell?: boolean
}

export function UserListPage({
  items: itemsProp,
  loading: loadingProp,
  total: totalProp,
  locale = 'vi',
  title,
  description,
  className,
  headerActions,
  useRoutes = true,
  onCreate,
  onView,
  onEdit,
  callback,
  content,
  withShell = true,
}: UserListPageProps) {
  const messages = getUserMessages(locale)
  const controlled = itemsProp != null
  const requestId = useRef(0)
  const [internalItems, setInternalItems] = useState<AdminUser[]>([])
  const [internalTotal, setInternalTotal] = useState(0)
  const [internalLoading, setInternalLoading] = useState(!controlled)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [pendingDelete, setPendingDelete] = useState<AdminUser | null>(null)
  const [deleting, setDeleting] = useState(false)

  const items = controlled ? itemsProp : internalItems
  const total = controlled ? (totalProp ?? items.length) : internalTotal
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
    const timer = window.setTimeout(() => {
      if (searchInput === search) return
      setSearch(searchInput)
      resetPage()
    }, 350)
    return () => window.clearTimeout(timer)
  }, [resetPage, search, searchInput])

  const reload = useCallback(async () => {
    if (controlled) return
    const current = ++requestId.current
    setInternalLoading(true)
    try {
      const result = await mockGetUserList({ search, page, size })
      if (current !== requestId.current) return
      setInternalItems(result.items)
      setInternalTotal(result.total)
    } catch (error) {
      if (current !== requestId.current) return
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      if (current === requestId.current) setInternalLoading(false)
    }
  }, [controlled, messages.form.saveError, page, search, size])

  useEffect(() => {
    void reload()
  }, [reload])

  const openCreate = () => {
    if (onCreate) {
      onCreate()
      return
    }
    if (useRoutes) navigateUser(getUserCreatePath())
  }

  const openView = (user: AdminUser) => {
    if (onView) {
      onView(user)
      return
    }
    if (useRoutes) navigateUser(getUserDetailPath(user.id))
  }

  const openEdit = (user: AdminUser) => {
    if (onEdit) {
      onEdit(user)
      return
    }
    if (useRoutes) navigateUser(getUserEditPath(user.id))
  }

  const confirmDelete = async () => {
    if (!pendingDelete) return
    setDeleting(true)
    try {
      const outcome = await handleAction({
        ctx: { user: pendingDelete },
        callback: callback?.delete,
        defaultSubmit: (user) => mockDeleteUser(user.id),
        getPayload: ({ user }) => user,
        onSuccess: async () => {
          setPendingDelete(null)
          await reload()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(messages.form.deleteSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setDeleting(false)
    }
  }

  const defaultTable = (
    <UserTable
      items={items}
      loading={loading}
      messages={messages}
      onView={openView}
      onEdit={openEdit}
      onDelete={(user) => setPendingDelete(user)}
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

  const toolbar = (
    <div className="mb-4">
      <div className="relative max-w-sm">
        <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
        <InputText
          unstyled
          value={searchInput}
          placeholder={messages.list.searchPlaceholder}
          className="box-border h-11 w-full rounded-xl border border-slate-200 bg-white pl-10 pr-3 text-sm text-slate-800 shadow-sm outline-none focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20"
          onChange={(event: { target: { value: string } }) =>
            setSearchInput(event.target.value)
          }
        />
      </div>
    </div>
  )

  const dialogs = (
    <ConfirmDialog
      open={Boolean(pendingDelete)}
      onClose={() => setPendingDelete(null)}
      onConfirm={() => void confirmDelete()}
      loading={deleting}
      title={messages.confirm.deleteTitle}
      description={
        pendingDelete
          ? messages.confirm.deleteDescription(pendingDelete.username)
          : undefined
      }
      confirmText={messages.confirm.confirm}
      cancelText={messages.confirm.cancel}
    />
  )

  const defaultContent = (
    <>
      {toolbar}
      {defaultTable}
      {paginationBar}
      {dialogs}
    </>
  )

  const contentCtx: UserListPageContentContext = {
    items,
    total,
    loading,
    searchInput,
    setSearchInput,
    reload,
    openCreate,
    openEdit,
    openView,
    requestDelete: (user) => setPendingDelete(user),
    DefaultTable: defaultTable,
    DefaultPagination: paginationBar,
    DefaultContent: defaultContent,
  }

  const resolved = resolveUserContent(content, contentCtx, defaultContent)
  const actions = (
    <>
      {headerActions}
      <Button type="button" unstyled className={btnPrimaryClass} onClick={openCreate}>
        <Plus className="size-4" />
        {messages.list.create}
      </Button>
    </>
  )

  if (!withShell) return resolved

  return (
    <UserPageShell
      title={title ?? messages.routes.list}
      description={description ?? messages.list.description}
      headerActions={actions}
      className={className}
    >
      {resolved}
    </UserPageShell>
  )
}
