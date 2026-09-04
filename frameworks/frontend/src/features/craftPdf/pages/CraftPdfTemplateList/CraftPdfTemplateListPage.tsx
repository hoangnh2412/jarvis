import {
  useCallback,
  useEffect,
  useState,
  type ChangeEvent,
  type KeyboardEvent,
  type ReactNode,
} from 'react'
import { Plus, Search } from 'lucide-react'
import { Button } from 'primereact/button'
import { IconField } from 'primereact/iconfield'
import { InputText } from 'primereact/inputtext'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import { notify } from '../../../../common/Toaster'
import {
  getErrorMessage,
  handleAction,
  type ActionProps,
} from '../../../../lib'
import { CraftPdfPageShell } from '../../components/CraftPdfPageShell'
import { CraftPdfTemplateGrid } from '../../components/CraftPdfTemplateGrid'
import {
  btnPrimaryClass,
  fieldInputClass,
} from '../../components/fieldStyles'
import {
  getCraftPdfMessages,
  type CraftPdfLocale,
} from '../../localization'
import {
  getCraftPdfEditorPath,
  navigateCraftPdf,
} from '../../routes'
import {
  callCreatePdfTemplate,
  callDeletePdfTemplate,
  callGetPdfTemplateList,
} from '../../services'
import type { PdfTemplate } from '../../types'
import {
  resolveCraftPdfContent,
  type CraftPdfSlotContent,
} from '../../utils'

export type CraftPdfTemplateListPageContentContext = {
  items: PdfTemplate[]
  loading: boolean
  searchInput: string
  setSearchInput: (v: string) => void
  search: string
  fetchTemplates: () => Promise<void>
  reload: () => Promise<void>
  onCreate?: () => void
  onEdit?: (template: PdfTemplate) => void
  requestDelete: (template: PdfTemplate) => void
  pendingDelete: PdfTemplate | null
  deleting: boolean
  cancelDelete: () => void
  confirmDelete: () => void | Promise<void>
  DefaultToolbar: ReactNode
  DefaultGrid: ReactNode
  DefaultContent: ReactNode
}

export type CraftPdfTemplateListPageProps = {
  items?: PdfTemplate[]
  loading?: boolean
  locale?: CraftPdfLocale
  title?: string
  description?: string
  className?: string
  /** Override điều hướng / API */
  callback?: {
    create?: ActionProps<
      { payload: { name: string } },
      { name: string },
      PdfTemplate
    >
    edit?: ActionProps<{ template: PdfTemplate }, PdfTemplate, void>
    delete?: ActionProps<{ template: PdfTemplate }, PdfTemplate, void>
  }
  content?: CraftPdfSlotContent<CraftPdfTemplateListPageContentContext>
}

export function CraftPdfTemplateListPage({
  items: controlledItems,
  loading: controlledLoading,
  locale = 'vi',
  title,
  description,
  className,
  callback,
  content,
}: CraftPdfTemplateListPageProps) {
  const msg = getCraftPdfMessages(locale)
  const [items, setItems] = useState<PdfTemplate[]>(controlledItems ?? [])
  const [loading, setLoading] = useState(false)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [pendingDelete, setPendingDelete] = useState<PdfTemplate | null>(null)
  const [deleting, setDeleting] = useState(false)

  const isControlled = controlledItems != null

  const fetchTemplates = useCallback(async () => {
    if (isControlled) return
    setLoading(true)
    try {
      const result = await callGetPdfTemplateList({ search })
      setItems(result.items)
    } catch (err) {
      notify.error(getErrorMessage(err, 'Không tải được danh sách template'))
    } finally {
      setLoading(false)
    }
  }, [isControlled, search])

  useEffect(() => {
    if (controlledItems) setItems(controlledItems)
  }, [controlledItems])

  useEffect(() => {
    void fetchTemplates()
  }, [fetchTemplates])

  const createAndOpen = async () => {
    try {
      const payload = {
        name: `Template ${new Date().toLocaleString()}`,
      }
      const outcome = await handleAction<
        { payload: { name: string } },
        { name: string },
        PdfTemplate
      >({
        ctx: { payload },
        callback: callback?.create,
        defaultSubmit: callCreatePdfTemplate,
        getPayload: ({ payload: submitPayload }) => submitPayload,
        onSuccess: (_ctx, created) => {
          notify.success('Đã tạo template')
          navigateCraftPdf(getCraftPdfEditorPath(created.id))
        },
      })
      if (outcome.status === 'cancelled') return
    } catch (err) {
      notify.error(getErrorMessage(err, 'Không tạo được template'))
    }
  }

  const onCreate = () => {
    void createAndOpen()
  }

  const onEdit = (template: PdfTemplate) => {
    void (async () => {
      try {
        const outcome = await handleAction<
          { template: PdfTemplate },
          PdfTemplate,
          void
        >({
          ctx: { template },
          callback: callback?.edit,
          defaultSubmit: async () => undefined,
          getPayload: ({ template: currentTemplate }) => currentTemplate,
          onSuccess: ({ template: currentTemplate }) => {
            navigateCraftPdf(getCraftPdfEditorPath(currentTemplate.id))
          },
        })
        if (outcome.status === 'cancelled') return
      } catch (err) {
        notify.error(getErrorMessage(err, 'Không mở được template'))
      }
    })()
  }

  const confirmDelete = async () => {
    if (!pendingDelete) return
    setDeleting(true)
    try {
      const outcome = await handleAction<
        { template: PdfTemplate },
        PdfTemplate,
        void
      >({
        ctx: { template: pendingDelete },
        callback: callback?.delete,
        defaultSubmit: async (template) => {
          await callDeletePdfTemplate(template.id)
        },
        getPayload: ({ template }) => template,
        onSuccess: async () => {
          await fetchTemplates()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success('Đã xoá template')
      setPendingDelete(null)
    } catch (err) {
      notify.error(getErrorMessage(err, 'Không xoá được template'))
    } finally {
      setDeleting(false)
    }
  }

  const commitSearch = () => setSearch(searchInput.trim())

  const DefaultToolbar = (
    <div className="flex w-full max-w-xl items-center gap-2 sm:w-auto">
      <IconField.Root className="relative min-w-0 flex-1 sm:w-64">
        <IconField.Inset className="pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2 text-slate-400">
          <Search className="size-4" />
        </IconField.Inset>
        <InputText
          unstyled
          className={`${fieldInputClass} !h-11 !pl-10`}
          placeholder={msg.searchPlaceholder}
          value={searchInput}
          onChange={(e: ChangeEvent<HTMLInputElement>) =>
            setSearchInput(e.target.value)
          }
          onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
            if (e.key === 'Enter') commitSearch()
          }}
        />
      </IconField.Root>
      <Button
        type="button"
        unstyled
        className={btnPrimaryClass}
        onClick={onCreate}
      >
        <Plus className="size-4" />
        {msg.createTemplate}
      </Button>
    </div>
  )

  const DefaultGrid = (
    <CraftPdfTemplateGrid
      items={items}
      emptyLabel={msg.emptyList}
      onEdit={onEdit}
      onDelete={(t) => setPendingDelete(t)}
    />
  )

  const DefaultContent = (
    <>
      {(controlledLoading ?? loading) ? (
        <p className="m-0 py-12 text-center text-sm text-slate-400">
          Đang tải…
        </p>
      ) : (
        DefaultGrid
      )}
    </>
  )

  const ctx: CraftPdfTemplateListPageContentContext = {
    items,
    loading: controlledLoading ?? loading,
    searchInput,
    setSearchInput,
    search,
    fetchTemplates,
    reload: fetchTemplates,
    onCreate,
    onEdit,
    requestDelete: setPendingDelete,
    pendingDelete,
    deleting,
    cancelDelete: () => setPendingDelete(null),
    confirmDelete,
    DefaultToolbar,
    DefaultGrid,
    DefaultContent,
  }

  return (
    <CraftPdfPageShell
      title={title ?? msg.listTitle}
      description={description ?? msg.listDescription}
      className={`box-border px-6 py-5 sm:px-8 sm:py-6 ${className ?? ''}`}
      headerActions={DefaultToolbar}
    >
      {resolveCraftPdfContent(content, ctx, DefaultContent)}

      <ConfirmDialog
        open={!!pendingDelete}
        title={msg.deleteConfirmTitle}
        description={msg.deleteConfirmMessage}
        confirmText={msg.delete}
        cancelText={msg.cancel}
        loading={deleting}
        onConfirm={() => void confirmDelete()}
        onClose={() => setPendingDelete(null)}
      />
    </CraftPdfPageShell>
  )
}
