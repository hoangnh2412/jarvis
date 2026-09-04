import {
  useCallback,
  useEffect,
  useState,
  type FormEventHandler,
  type ReactNode,
} from 'react'
import { Plus } from 'lucide-react'
import { Button } from 'primereact/button'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import { FeatureDialog } from '../../../../common/FeatureDialog'
import {
  ListPagination,
  useListPagination,
} from '../../../../common/ListPagination'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import {
  PermissionEditor,
  RoleDetailView,
  RoleForm,
  RolePageShell,
  RoleTable,
} from '../../components'
import { btnOutlinedClass, btnPrimaryClass } from '../../components/fieldStyles'
import { useRoleForm } from '../../hooks'
import { getRoleMessages, type RoleLocale } from '../../localization'
import {
  mockCreateRole,
  mockDeleteRole,
  mockGetPermissionCatalog,
  mockGetRoleList,
  mockUpdateRole,
  mockUpdateRolePermissions,
} from '../../services'
import type { PermissionGroup, Role } from '../../types'
import { resolveRoleContent, type RoleSlotContent } from '../../utils'
import { roleFormDefaultValues, type RoleFormData } from '../../validation'

type FormDialogMode = 'create' | 'edit' | 'view' | null

export type RoleListPageContentContext = {
  items: Role[]
  total: number
  loading: boolean
  searchInput: string
  setSearchInput: (value: string) => void
  reload: () => Promise<void>
  openCreate: () => void
  openEdit: (role: Role) => void
  openView: (role: Role) => void
  openPermissions: (role: Role) => void
  requestDelete: (role: Role) => void
  pendingDelete: Role | null
  deleting: boolean
  cancelDelete: () => void
  confirmDelete: () => void | Promise<void>
  DefaultTable: ReactNode
  DefaultPagination: ReactNode
  DefaultDialogs: ReactNode
  DefaultContent: ReactNode
  page: number
  size: number
  totalPages: number
}

export type RoleListPageProps = {
  items?: Role[]
  loading?: boolean
  total?: number
  locale?: RoleLocale
  title?: string
  description?: string
  className?: string
  headerActions?: ReactNode
  /**
   * Action callback kiểu jQuery ajax: `before` / `onSubmit` / `success` / `error` / `complete`.
   * Mặc định dùng mock (fake data); host override `onSubmit` bằng `callCreateRole`, …
   */
  callback?: {
    create?: ActionProps<{ data: RoleFormData }, RoleFormData, Role>
    update?: ActionProps<
      { role: Role; data: RoleFormData },
      { id: string; data: RoleFormData },
      Role
    >
    delete?: ActionProps<{ role: Role }, Role>
    updatePermissions?: ActionProps<
      { role: Role; permissions: string[] },
      { id: string; permissions: string[] },
      Role
    >
  }
  content?: RoleSlotContent<RoleListPageContentContext>
  withShell?: boolean
  withDialogs?: boolean
}

export function RoleListPage({
  items: itemsProp,
  loading: loadingProp,
  total: totalProp,
  locale = 'vi',
  title,
  description,
  className,
  headerActions,
  callback,
  content,
  withShell = true,
  withDialogs = true,
}: RoleListPageProps) {
  const messages = getRoleMessages(locale)
  const controlled = itemsProp != null

  const [internalItems, setInternalItems] = useState<Role[]>([])
  const [internalTotal, setInternalTotal] = useState(0)
  const [internalLoading, setInternalLoading] = useState(!controlled)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')

  const [formMode, setFormMode] = useState<FormDialogMode>(null)
  const [editingRole, setEditingRole] = useState<Role | null>(null)
  const [permissionRole, setPermissionRole] = useState<Role | null>(null)
  const [permissionDraft, setPermissionDraft] = useState<string[]>([])
  const [catalog, setCatalog] = useState<PermissionGroup[]>([])
  const [pendingDelete, setPendingDelete] = useState<Role | null>(null)
  const [deleting, setDeleting] = useState(false)
  const [savingForm, setSavingForm] = useState(false)
  const [savingPermissions, setSavingPermissions] = useState(false)

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useRoleForm()

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
  }, [searchInput, search, resetPage])

  const reload = useCallback(async () => {
    if (controlled) return
    setInternalLoading(true)
    try {
      const result = await mockGetRoleList({ search, page, size })
      setInternalItems(result.items)
      setInternalTotal(result.total)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setInternalLoading(false)
    }
  }, [controlled, search, page, size, messages.form.saveError])

  useEffect(() => {
    void reload()
  }, [reload])

  useEffect(() => {
    void mockGetPermissionCatalog().then(setCatalog)
  }, [])

  const closeFormDialog = () => {
    setFormMode(null)
    setEditingRole(null)
  }

  const openCreate = () => {
    setEditingRole(null)
    setFormMode('create')
    reset(roleFormDefaultValues)
  }

  const openEdit = (role: Role) => {
    setEditingRole(role)
    setFormMode('edit')
    reset({
      name: role.name,
      displayName: role.displayName,
      description: role.description ?? '',
      isDefault: role.isDefault,
      isPublic: role.isPublic,
    })
  }

  const openView = (role: Role) => {
    setEditingRole(role)
    setFormMode('view')
  }

  const openPermissions = (role: Role) => {
    setPermissionRole(role)
    setPermissionDraft([...role.permissions])
  }

  const onSaveForm: FormEventHandler<HTMLFormElement> = handleSubmit(
    async (data: RoleFormData) => {
      setSavingForm(true)
      try {
        if (formMode === 'create') {
          const outcome = await handleAction({
            ctx: { data },
            callback: callback?.create,
            defaultSubmit: mockCreateRole,
            getPayload: ({ data: payload }) => payload,
            onSuccess: async () => {
              closeFormDialog()
              await reload()
            },
          })
          if (outcome.status === 'cancelled') return
          notify.success(messages.form.saveSuccess)
        } else if (formMode === 'edit' && editingRole) {
          const outcome = await handleAction({
            ctx: { role: editingRole, data },
            callback: callback?.update,
            defaultSubmit: ({ id, data: payload }) =>
              mockUpdateRole(id, payload),
            getPayload: ({ role, data: payload }) => ({
              id: role.id,
              data: payload,
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
      } finally {
        setSavingForm(false)
      }
    },
  )

  const onSavePermissions = async () => {
    if (!permissionRole) return
    setSavingPermissions(true)
    try {
      const outcome = await handleAction({
        ctx: { role: permissionRole, permissions: permissionDraft },
        callback: callback?.updatePermissions,
        defaultSubmit: ({ id, permissions }) =>
          mockUpdateRolePermissions(id, permissions),
        getPayload: ({ role, permissions }) => ({
          id: role.id,
          permissions,
        }),
        onSuccess: async () => {
          setPermissionRole(null)
          await reload()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(messages.form.permissionsSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setSavingPermissions(false)
    }
  }

  const confirmDelete = async () => {
    if (!pendingDelete) return
    setDeleting(true)
    try {
      const outcome = await handleAction({
        ctx: { role: pendingDelete },
        callback: callback?.delete,
        defaultSubmit: (role) => mockDeleteRole(role.id),
        getPayload: ({ role }) => role,
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

  const formTitle =
    formMode === 'create'
      ? messages.form.createTitle
      : formMode === 'edit'
        ? messages.form.editTitle
        : messages.form.viewTitle

  const defaultTable = (
    <RoleTable
      items={items}
      loading={loading}
      emptyMessage={messages.list.empty}
      onView={openView}
      onEdit={openEdit}
      onPermissions={openPermissions}
      onDelete={(role) => setPendingDelete(role)}
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
  const dialogs = (
    <>
      <FeatureDialog
        open={formMode === 'create' || formMode === 'edit'}
        onClose={closeFormDialog}
        title={formTitle}
        size="md"
        onSubmit={onSaveForm}
        submitting={savingForm || isSubmitting}
        submitLabel={formMode === 'create' ? messages.list.create : 'Lưu'}
      >
        <RoleForm register={register} control={control} errors={errors} />
      </FeatureDialog>

      <FeatureDialog
        open={formMode === 'view' && editingRole != null}
        onClose={closeFormDialog}
        title={formTitle}
        size="md"
      >
        {editingRole ? <RoleDetailView role={editingRole} /> : null}
      </FeatureDialog>

      <FeatureDialog
        open={permissionRole != null}
        onClose={() => setPermissionRole(null)}
        title={
          permissionRole
            ? `${messages.form.permissionsTitle} — ${permissionRole.displayName}`
            : messages.form.permissionsTitle
        }
        size="2xl"
        footer={
          <>
            <Button
              type="button"
              unstyled
              className={btnOutlinedClass}
              disabled={savingPermissions}
              onClick={() => setPermissionRole(null)}
            >
              Hủy
            </Button>
            <Button
              type="button"
              unstyled
              className={btnPrimaryClass}
              disabled={savingPermissions}
              onClick={() => void onSavePermissions()}
            >
              {savingPermissions ? messages.form.submitting : 'Save'}
            </Button>
          </>
        }
      >
        <PermissionEditor
          groups={catalog}
          value={permissionDraft}
          onChange={setPermissionDraft}
        />
      </FeatureDialog>

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onClose={() => setPendingDelete(null)}
        onConfirm={confirmDelete}
        loading={deleting}
        title="Xoá vai trò?"
        description={
          pendingDelete
            ? `Vai trò ${pendingDelete.displayName} sẽ bị xoá.`
            : undefined
        }
        confirmText="Xoá"
      />
    </>
  )

  const defaultContent = (
    <>
      {defaultTable}
      {paginationBar}
    </>
  )

  const contentCtx: RoleListPageContentContext = {
    items,
    total,
    loading,
    searchInput,
    setSearchInput,
    reload,
    openCreate,
    openEdit,
    openView,
    openPermissions,
    requestDelete: (role) => setPendingDelete(role),
    pendingDelete,
    deleting,
    cancelDelete: () => setPendingDelete(null),
    confirmDelete,
    page,
    size,
    totalPages,
    DefaultTable: defaultTable,
    DefaultPagination: paginationBar,
    DefaultDialogs: dialogs,
    DefaultContent: defaultContent,
  }

  const resolved = resolveRoleContent(content, contentCtx, defaultContent)

  const actions = (
    <>
      {headerActions}
      <Button type="button" unstyled className={btnPrimaryClass} onClick={openCreate}>
        <Plus className="size-4" />
        {messages.list.create}
      </Button>
    </>
  )

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
      <RolePageShell
        title={title ?? messages.routes.list}
        description={description ?? messages.list.description}
        headerActions={actions}
        className={className}
      >
        {resolved}
      </RolePageShell>
      {withDialogs ? dialogs : null}
    </>
  )
}
