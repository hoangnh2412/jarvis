import { useCallback, useEffect, useState, type FormEventHandler, type ReactNode } from 'react'
import type {
  Control,
  FieldErrors,
  UseFormHandleSubmit,
  UseFormRegister,
} from 'react-hook-form'
import { Plus } from 'lucide-react'
import { Button } from 'primereact/button'
import { Dialog } from 'primereact/dialog'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { DomainForm } from '../../components/DomainForm'
import { DomainTable } from '../../components/DomainTable'
import { TenantPageShell } from '../../components/TenantPageShell'
import {
  btnOutlinedClass,
  btnPrimaryClass,
} from '../../components/fieldStyles'
import { useTenantDomainForm } from '../../hooks'
import { getTenantMessages, type TenantLocale } from '../../localization'
import {
  callAddTenantDomain,
  callDeleteTenantDomain,
  callGetTenantDomainList,
  callUpdateTenantDomain,
} from '../../services'
import type { TenantDomain } from '../../types'
import { unwrapDomainList } from '../../utils/apiData'
import {
  resolveTenantContent,
  type TenantSlotContent,
} from '../../utils'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import {
  tenantDomainFormDefaultValues,
  type TenantDomainFormData,
} from '../../validation'

export type TenantDomainsPageContentContext = {
  items: TenantDomain[]
  loading: boolean
  reload: () => Promise<void>

  dialogOpen: boolean
  editing: TenantDomain | null
  pendingDelete: TenantDomain | null
  deleting: boolean

  openCreate: () => void
  openEdit: (item: TenantDomain) => void
  closeDialog: () => void
  requestDelete: (item: TenantDomain) => void
  cancelDelete: () => void
  confirmDelete: () => void | Promise<void>

  register: UseFormRegister<TenantDomainFormData>
  control: Control<TenantDomainFormData>
  errors: FieldErrors<TenantDomainFormData>
  isSubmitting: boolean
  handleSubmit: UseFormHandleSubmit<TenantDomainFormData>
  /** Gắn vào form dialog lưu domain */
  onSave: FormEventHandler<HTMLFormElement>

  DefaultTable: ReactNode
  DefaultDialogs: ReactNode
  DefaultContent: ReactNode
}

export type TenantDomainsPageProps = {
  tenantId: string
  tenantName?: string
  /** Nếu không truyền, page tự fetch qua API */
  items?: TenantDomain[]
  loading?: boolean
  locale?: TenantLocale
  title?: string
  description?: string
  className?: string
  /** Action callback kiểu jQuery ajax cho add / update / delete */
  callback?: {
    add?: ActionProps<{ data: TenantDomainFormData }, TenantDomainFormData>
    update?: ActionProps<
      { domainId: string; data: TenantDomainFormData },
      { domainId: string; data: TenantDomainFormData }
    >
    delete?: ActionProps<{ item: TenantDomain }, TenantDomain>
  }
  headerActions?: ReactNode
  content?: TenantSlotContent<TenantDomainsPageContentContext>
  withShell?: boolean
  /**
   * `false` = không render dialog mặc định (form + confirm).
   * Dùng khi custom toàn bộ UI dialog qua `content`.
   * @default true
   */
  withDialogs?: boolean
}

export function TenantDomainsPage({
  tenantId,
  tenantName,
  items: itemsProp,
  loading: loadingProp,
  locale = 'vi',
  title,
  description,
  className,
  callback,
  headerActions,
  content,
  withShell = true,
  withDialogs = true,
}: TenantDomainsPageProps) {
  const messages = getTenantMessages(locale)
  const controlled = itemsProp != null

  const [internalItems, setInternalItems] = useState<TenantDomain[]>([])
  const [internalLoading, setInternalLoading] = useState(!controlled)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editing, setEditing] = useState<TenantDomain | null>(null)
  const [pendingDelete, setPendingDelete] = useState<TenantDomain | null>(null)
  const [busy, setBusy] = useState(false)

  const items = controlled ? itemsProp : internalItems
  const loading = controlled ? Boolean(loadingProp) : internalLoading

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useTenantDomainForm()

  const reload = useCallback(async () => {
    if (controlled) return
    setInternalLoading(true)
    try {
      const res = await callGetTenantDomainList(tenantId)
      setInternalItems(unwrapDomainList(res))
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setInternalLoading(false)
    }
  }, [controlled, tenantId, messages.form.saveError])

  useEffect(() => {
    if (!controlled) {
      void reload()
    }
  }, [controlled, reload])

  useEffect(() => {
    if (!dialogOpen) return
    if (editing) {
      reset({
        domain: editing.domain,
        isPrimary: editing.isPrimary,
      })
    } else {
      reset(tenantDomainFormDefaultValues)
    }
  }, [dialogOpen, editing, reset])

  const openCreate = () => {
    setEditing(null)
    setDialogOpen(true)
  }

  const openEdit = (item: TenantDomain) => {
    setEditing(item)
    setDialogOpen(true)
  }

  const closeDialog = () => {
    setDialogOpen(false)
    setEditing(null)
  }

  const defaultAdd = async (data: TenantDomainFormData) => {
    await callAddTenantDomain(tenantId, data)
  }

  const defaultUpdate = async (
    domainId: string,
    data: TenantDomainFormData,
  ) => {
    await callUpdateTenantDomain(tenantId, domainId, data)
  }

  const defaultDelete = async (item: TenantDomain) => {
    await callDeleteTenantDomain(tenantId, item.id)
  }

  const onSave = handleSubmit(async (data) => {
    try {
      if (editing) {
        const outcome = await handleAction({
          ctx: { domainId: editing.id, data },
          callback: callback?.update,
          defaultSubmit: ({ domainId, data: payload }) =>
            defaultUpdate(domainId, payload),
          getPayload: (ctx) => ({
            domainId: ctx.domainId,
            data: ctx.data,
          }),
          onSuccess: async () => {
            await reload()
          },
        })
        if (outcome.status === 'cancelled') return
      } else {
        const outcome = await handleAction({
          ctx: { data },
          callback: callback?.add,
          defaultSubmit: defaultAdd,
          getPayload: ({ data: payload }) => payload,
          onSuccess: async () => {
            await reload()
          },
        })
        if (outcome.status === 'cancelled') return
      }
      notify.success(messages.domain.saveSuccess)
      closeDialog()
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    }
  })

  const confirmDelete = async () => {
    if (!pendingDelete) return
    setBusy(true)
    try {
      const outcome = await handleAction({
        ctx: { item: pendingDelete },
        callback: callback?.delete,
        defaultSubmit: defaultDelete,
        getPayload: ({ item }) => item,
        onSuccess: async () => {
          await reload()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(messages.domain.deleteSuccess)
      setPendingDelete(null)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setBusy(false)
    }
  }

  const defaultTable = (
    <DomainTable
      items={items}
      loading={loading}
      onEdit={openEdit}
      onDelete={(item) => setPendingDelete(item)}
    />
  )

  const dialogs = (
    <>
      <Dialog.Root
        open={dialogOpen}
        onOpenChange={(e: { value?: boolean }) => {
          if (!e.value) closeDialog()
        }}
      >
        <Dialog.Portal>
          <Dialog.Backdrop className="fixed inset-0 z-[100] bg-slate-900/40 backdrop-blur-[2px]" />
          <Dialog.Positioner className="fixed inset-0 z-[110] flex items-center justify-center p-4">
            <Dialog.Popup className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-0 shadow-xl">
              <Dialog.Header className="flex items-start justify-between gap-3 border-b border-slate-100 px-5 py-4">
                <Dialog.Title className="text-lg font-semibold text-slate-900">
                  {editing ? 'Cập nhật domain' : messages.domain.add}
                </Dialog.Title>
                <Dialog.Close
                  className="rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                  aria-label="Đóng"
                />
              </Dialog.Header>
              <form onSubmit={onSave} noValidate>
                <Dialog.Content className="px-5 py-4">
                  <DomainForm
                    register={register}
                    control={control}
                    errors={errors}
                  />
                </Dialog.Content>
                <Dialog.Footer className="flex justify-end gap-2 border-t border-slate-100 px-5 py-4">
                  <Button
                    type="button"
                    unstyled
                    className={btnOutlinedClass}
                    onClick={closeDialog}
                  >
                    Hủy
                  </Button>
                  <Button
                    type="submit"
                    unstyled
                    className={btnPrimaryClass}
                    disabled={isSubmitting}
                  >
                    {isSubmitting ? messages.form.submitting : 'Lưu'}
                  </Button>
                </Dialog.Footer>
              </form>
            </Dialog.Popup>
          </Dialog.Positioner>
        </Dialog.Portal>
      </Dialog.Root>

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onClose={() => setPendingDelete(null)}
        onConfirm={confirmDelete}
        loading={busy}
        title="Xoá domain?"
        description="Domain sẽ bị xoá mềm và không còn dùng để resolve tenant."
        confirmText="Xoá"
      />
    </>
  )

  const contentCtx: TenantDomainsPageContentContext = {
    items,
    loading,
    reload,
    dialogOpen,
    editing,
    pendingDelete,
    deleting: busy,
    openCreate,
    openEdit,
    closeDialog,
    requestDelete: (item) => setPendingDelete(item),
    cancelDelete: () => setPendingDelete(null),
    confirmDelete,
    register,
    control,
    errors,
    isSubmitting,
    handleSubmit,
    onSave,
    DefaultTable: defaultTable,
    DefaultDialogs: dialogs,
    DefaultContent: defaultTable,
  }

  const resolved = resolveTenantContent(content, contentCtx, defaultTable)

  const actions = (
    <>
      {headerActions}
      <Button type="button" unstyled className={btnPrimaryClass} onClick={openCreate}>
        <Plus className="size-4" />
        {messages.domain.add}
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
      <TenantPageShell
        title={title ?? messages.domain.title}
        description={
          description ??
          (tenantName
            ? `Domain của 「${tenantName}」`
            : messages.routes.domains)
        }
        headerActions={actions}
        className={className}
      >
        {resolved}
      </TenantPageShell>
      {withDialogs ? dialogs : null}
    </>
  )
}
