import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import type { AxiosResponse } from 'axios'
import { FilterX, Plus } from 'lucide-react'
import { Button } from 'primereact/button'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import {
  CalendarView,
  KanbanBoard,
  PlannerBoardSkeleton,
  itemToFormState,
  PlannerDetailPanel,
  PlannerPageShell,
  PlannerToolbar,
  TimelineView,
  type PlannerItemFormState,
  type PlannerPriorityFilter,
} from '../../components'
import { btnOutlinedClass, btnPrimaryClass } from '../../components/fieldStyles'
import { getPlannerMessages, type PlannerLocale } from '../../localization'
import {
  callCreatePlannerItem,
  callDeletePlannerItem,
  callGetPlannerBoard,
  callMovePlannerItem,
  callReschedulePlannerItem,
  callUpdatePlannerItem,
} from '../../services'
import type {
  CreatePlannerItemPayload,
  MovePlannerItemPayload,
  PlannerBoardQuery,
  PlannerBoardResult,
  PlannerColumn,
  PlannerItem,
  PlannerViewMode,
  ReschedulePlannerItemPayload,
  UpdatePlannerItemPayload,
} from '../../types'
import {
  defaultItemEnd,
  defaultItemStart,
  resolvePlannerContent,
  type PlannerSlotContent,
} from '../../utils'

const SEARCH_DEBOUNCE_MS = 300

export type PlannerPageContentContext = {
  columns: PlannerColumn[]
  items: PlannerItem[]
  /** Kết quả đã lọc từ BE (cùng `items`). */
  visibleItems: PlannerItem[]
  view: PlannerViewMode
  loading: boolean
  setView: (view: PlannerViewMode) => void
  openCreate: () => void
  reload: () => Promise<void>
  DefaultLayout: ReactNode
}

export type PlannerPageProps = {
  locale?: PlannerLocale
  className?: string
  title?: string
  description?: string
  columns?: PlannerColumn[]
  items?: PlannerItem[]
  loading?: boolean
  initialView?: PlannerViewMode
  withShell?: boolean
  callback?: {
    list?: ActionProps<
      PlannerBoardQuery,
      PlannerBoardQuery | undefined,
      AxiosResponse<PlannerBoardResult>
    >
    create?: ActionProps<
      CreatePlannerItemPayload,
      CreatePlannerItemPayload,
      AxiosResponse<PlannerItem>
    >
    update?: ActionProps<
      UpdatePlannerItemPayload,
      UpdatePlannerItemPayload,
      AxiosResponse<PlannerItem>
    >
    move?: ActionProps<
      MovePlannerItemPayload,
      MovePlannerItemPayload,
      AxiosResponse<PlannerItem>
    >
    reschedule?: ActionProps<
      ReschedulePlannerItemPayload,
      ReschedulePlannerItemPayload,
      AxiosResponse<PlannerItem>
    >
    delete?: ActionProps<{ id: string }, string, AxiosResponse<unknown>>
  }
  content?: PlannerSlotContent<PlannerPageContentContext>
}

function emptyForm(columns: PlannerColumn[]): PlannerItemFormState {
  const start = defaultItemStart()
  return {
    title: '',
    description: '',
    statusId: columns[0]?.id ?? 'todo',
    start,
    end: defaultItemEnd(start),
    allDay: false,
    priority: 'medium',
    assignee: '',
  }
}

export function PlannerPage({
  locale = 'vi',
  className,
  title,
  description,
  columns: columnsProp,
  items: itemsProp,
  loading: loadingProp,
  initialView = 'kanban',
  withShell = true,
  callback,
  content,
}: PlannerPageProps) {
  const messages = getPlannerMessages(locale)
  const controlled = columnsProp != null && itemsProp != null

  const [columns, setColumns] = useState<PlannerColumn[]>(columnsProp ?? [])
  const [items, setItems] = useState<PlannerItem[]>(itemsProp ?? [])
  const [loading, setLoading] = useState(!controlled)
  const [view, setView] = useState<PlannerViewMode>(initialView)

  const [panelOpen, setPanelOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<PlannerItem | null>(null)
  const [form, setForm] = useState<PlannerItemFormState>(() =>
    emptyForm(columnsProp ?? []),
  )
  const [titleError, setTitleError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)
  const [pendingDelete, setPendingDelete] = useState<PlannerItem | null>(null)
  const [deleting, setDeleting] = useState(false)

  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [priorityFilter, setPriorityFilter] =
    useState<PlannerPriorityFilter>('all')

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setDebouncedSearch(search)
    }, SEARCH_DEBOUNCE_MS)
    return () => window.clearTimeout(timer)
  }, [search])

  useEffect(() => {
    if (!controlled) return
    setColumns(columnsProp)
    setItems(itemsProp)
  }, [controlled, columnsProp, itemsProp])

  const boardQuery = useMemo((): PlannerBoardQuery => {
    const trimmed = debouncedSearch.trim()
    return {
      search: trimmed || undefined,
      priority: priorityFilter,
    }
  }, [debouncedSearch, priorityFilter])

  const reload = useCallback(async () => {
    if (controlled) return
    setLoading(true)
    try {
      const outcome = await handleAction({
        ctx: boardQuery,
        callback: callback?.list,
        defaultSubmit: callGetPlannerBoard,
        getPayload: (ctx) => ctx,
        onSuccess: async (_ctx, res) => {
          setColumns(res.data.columns)
          setItems(res.data.items)
        },
      })
      if (outcome.status === 'cancelled') return
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    } finally {
      setLoading(false)
    }
  }, [boardQuery, callback?.list, controlled, messages.toast.error])

  useEffect(() => {
    if (controlled) {
      setLoading(Boolean(loadingProp))
      return
    }
    void reload()
  }, [controlled, loadingProp, reload])

  const closePanel = useCallback(() => {
    if (saving) return
    setPanelOpen(false)
    setEditingItem(null)
    setTitleError(null)
  }, [saving])

  const openCreate = useCallback(
    (preset?: Partial<PlannerItemFormState>) => {
      setEditingItem(null)
      setTitleError(null)
      setForm({ ...emptyForm(columns), ...preset })
      setPanelOpen(true)
    },
    [columns],
  )

  const openEdit = useCallback((item: PlannerItem) => {
    setEditingItem(item)
    setTitleError(null)
    setForm(itemToFormState(item))
    setPanelOpen(true)
  }, [])

  const applyItem = useCallback((item: PlannerItem) => {
    setItems((prev) => {
      const index = prev.findIndex((entry) => entry.id === item.id)
      if (index < 0) return [item, ...prev]
      const next = prev.slice()
      next[index] = item
      return next
    })
  }, [])

  const onSave = async () => {
    const titleValue = form.title.trim()
    if (!titleValue) {
      setTitleError(messages.form.titleRequired)
      return
    }
    setTitleError(null)
    setSaving(true)

    try {
      if (editingItem) {
        const payload: UpdatePlannerItemPayload = {
          id: editingItem.id,
          title: titleValue,
          description: form.description,
          statusId: form.statusId,
          start: form.start,
          end: form.end,
          allDay: form.allDay,
          priority: form.priority,
          assignee: form.assignee,
        }
        const outcome = await handleAction({
          ctx: payload,
          callback: callback?.update,
          defaultSubmit: callUpdatePlannerItem,
          getPayload: (ctx) => ctx,
          onSuccess: async (_ctx, res) => {
            applyItem(res.data)
            setPanelOpen(false)
            setEditingItem(null)
          },
        })
        if (outcome.status === 'cancelled') return
        notify.success(messages.toast.updateSuccess)
      } else {
        const payload: CreatePlannerItemPayload = {
          title: titleValue,
          description: form.description,
          statusId: form.statusId,
          start: form.start,
          end: form.end,
          allDay: form.allDay,
          priority: form.priority,
          assignee: form.assignee,
        }
        const outcome = await handleAction({
          ctx: payload,
          callback: callback?.create,
          defaultSubmit: callCreatePlannerItem,
          getPayload: (ctx) => ctx,
          onSuccess: async (_ctx, res) => {
            applyItem(res.data)
            setPanelOpen(false)
            setEditingItem(null)
          },
        })
        if (outcome.status === 'cancelled') return
        notify.success(messages.toast.createSuccess)
      }
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    } finally {
      setSaving(false)
    }
  }

  const onMoveItem = useCallback(
    async (itemId: string, statusId: string) => {
      const payload: MovePlannerItemPayload = { id: itemId, statusId }
      try {
        const outcome = await handleAction({
          ctx: payload,
          callback: callback?.move,
          defaultSubmit: callMovePlannerItem,
          getPayload: (ctx) => ctx,
          onSuccess: async (_ctx, res) => {
            applyItem(res.data)
          },
        })
        if (outcome.status === 'cancelled') return
        notify.success(messages.toast.moveSuccess)
      } catch (error) {
        notify.error(getErrorMessage(error, messages.toast.error))
        if (!controlled) void reload()
      }
    },
    [
      applyItem,
      callback?.move,
      controlled,
      messages.toast.error,
      messages.toast.moveSuccess,
      reload,
    ],
  )

  const onReschedule = useCallback(
    async (payload: ReschedulePlannerItemPayload) => {
      try {
        const outcome = await handleAction({
          ctx: payload,
          callback: callback?.reschedule,
          defaultSubmit: callReschedulePlannerItem,
          getPayload: (ctx) => ctx,
          onSuccess: async (_ctx, res) => {
            const item = res.data
            applyItem(item)
            setEditingItem((current) => {
              if (current?.id !== item.id) return current
              setForm(itemToFormState(item))
              return item
            })
          },
        })
        if (outcome.status === 'cancelled') return
        notify.success(messages.toast.rescheduleSuccess)
      } catch (error) {
        notify.error(getErrorMessage(error, messages.toast.error))
        if (!controlled) void reload()
      }
    },
    [
      applyItem,
      callback?.reschedule,
      controlled,
      messages.toast.error,
      messages.toast.rescheduleSuccess,
      reload,
    ],
  )

  const onTimelineUpdate = useCallback(
    async (payload: UpdatePlannerItemPayload) => {
      try {
        const outcome = await handleAction({
          ctx: payload,
          callback: callback?.update,
          defaultSubmit: callUpdatePlannerItem,
          getPayload: (ctx) => ctx,
          onSuccess: async (_ctx, res) => {
            applyItem(res.data)
          },
        })
        if (outcome.status === 'cancelled') return
        notify.success(
          payload.assignee !== undefined &&
            payload.start == null &&
            payload.end == null
            ? messages.toast.updateSuccess
            : messages.toast.rescheduleSuccess,
        )
      } catch (error) {
        notify.error(getErrorMessage(error, messages.toast.error))
        if (!controlled) void reload()
      }
    },
    [
      applyItem,
      callback?.update,
      controlled,
      messages.toast.error,
      messages.toast.rescheduleSuccess,
      messages.toast.updateSuccess,
      reload,
    ],
  )

  const onConfirmDelete = async () => {
    if (!pendingDelete) return
    setDeleting(true)
    try {
      const outcome = await handleAction({
        ctx: { id: pendingDelete.id },
        callback: callback?.delete,
        defaultSubmit: callDeletePlannerItem,
        getPayload: ({ id }) => id,
        onSuccess: async () => {
          setItems((prev) =>
            prev.filter((entry) => entry.id !== pendingDelete.id),
          )
          setPendingDelete(null)
          setPanelOpen(false)
          setEditingItem(null)
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(messages.toast.deleteSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    } finally {
      setDeleting(false)
    }
  }

  const isLoading = controlled ? Boolean(loadingProp) : loading
  const filtersActive =
    debouncedSearch.trim() !== '' || priorityFilter !== 'all'
  /** Items đã được BE lọc theo query; không filter lại trên FE. */
  const visibleItems = items

  const clearFilters = () => {
    setSearch('')
    setDebouncedSearch('')
    setPriorityFilter('all')
  }

  const emptyResults = (
    <div className="flex flex-1 flex-col items-center justify-center gap-3 rounded-2xl border border-dashed border-slate-200 bg-slate-50/60 p-10 text-center">
      <div>
        <p className="m-0 text-sm font-semibold text-slate-700">
          {messages.page.noResults}
        </p>
        <p className="m-0 mt-1 text-sm text-slate-500">
          {messages.page.noResultsHint}
        </p>
      </div>
      <Button
        type="button"
        unstyled
        className={btnOutlinedClass}
        onClick={clearFilters}
      >
        <FilterX className="h-4 w-4" />
        {messages.page.clearFilters}
      </Button>
    </div>
  )

  const openItemById = (itemId: string) => {
    const item = items.find((entry) => entry.id === itemId)
    if (item) openEdit(item)
  }

  const handleViewChange = (next: PlannerViewMode) => {
    setView(next)
  }

  const boardOrCalendar =
    view === 'kanban' ? (
      <KanbanBoard
        columns={columns}
        items={visibleItems}
        locale={locale}
        onMoveItem={(itemId: string, statusId: string) => {
          void onMoveItem(itemId, statusId)
        }}
        onOpenItem={openEdit}
        onAddItem={(statusId: string) => openCreate({ statusId })}
      />
    ) : view === 'timeline' ? (
      <TimelineView
        items={visibleItems}
        columns={columns}
        locale={locale}
        onOpenItem={openItemById}
        onSelectSlot={(
          start: string,
          end: string,
          allDay: boolean,
          statusId?: string,
        ) => {
          openCreate({
            start,
            end,
            allDay,
            statusId: statusId ?? columns[0]?.id ?? 'todo',
          })
        }}
        onUpdateItem={(payload: UpdatePlannerItemPayload) => {
          void onTimelineUpdate(payload)
        }}
      />
    ) : (
      <CalendarView
        items={visibleItems}
        columns={columns}
        locale={locale}
        onOpenItem={openItemById}
        onSelectSlot={(start: string, end: string, allDay: boolean) => {
          openCreate({
            start,
            end,
            allDay,
            statusId: columns[0]?.id ?? 'todo',
          })
        }}
        onReschedule={(payload: ReschedulePlannerItemPayload) => {
          void onReschedule(payload)
        }}
      />
    )

  const defaultLayout = (
    <>
      <PlannerToolbar
        view={view}
        onViewChange={handleViewChange}
        locale={locale}
        search={search}
        onSearchChange={setSearch}
        priority={priorityFilter}
        onPriorityChange={setPriorityFilter}
        total={isLoading ? undefined : visibleItems.length}
      />

      <div className="kit-planner-workspace">
        <div className="kit-planner-workspace__main">
          {isLoading ? (
            <PlannerBoardSkeleton />
          ) : visibleItems.length === 0 && filtersActive ? (
            emptyResults
          ) : (
            boardOrCalendar
          )}
        </div>
        <PlannerDetailPanel
          open={panelOpen}
          mode={editingItem ? 'edit' : 'create'}
          value={form}
          columns={columns}
          locale={locale}
          titleError={titleError}
          saving={saving}
          onChange={setForm}
          onSave={() => void onSave()}
          onClose={closePanel}
          onDelete={
            editingItem ? () => setPendingDelete(editingItem) : undefined
          }
        />
      </div>
    </>
  )

  const pageContent = resolvePlannerContent(
    content,
    {
      columns,
      items,
      visibleItems,
      view,
      loading: isLoading,
      setView: handleViewChange,
      openCreate: () => openCreate(),
      reload,
      DefaultLayout: defaultLayout,
    },
    defaultLayout,
  )

  const body = (
    <>
      {pageContent}

      <ConfirmDialog
        open={pendingDelete != null}
        onClose={() => {
          if (!deleting) setPendingDelete(null)
        }}
        onConfirm={() => void onConfirmDelete()}
        title={messages.dialog.deleteTitle}
        description={
          pendingDelete
            ? messages.dialog.deleteConfirm(pendingDelete.title)
            : undefined
        }
        confirmText={messages.form.delete}
        cancelText={messages.form.cancel}
        loading={deleting}
      />
    </>
  )

  if (!withShell) {
    return <div className={className}>{body}</div>
  }

  return (
    <PlannerPageShell
      title={title ?? messages.page.title}
      description={description ?? messages.page.description}
      className={className}
      headerActions={
        <Button
          type="button"
          unstyled
          className={`${btnPrimaryClass} !w-auto`}
          onClick={() => openCreate()}
        >
          <Plus className="h-4 w-4" />
          {messages.page.create}
        </Button>
      }
    >
      {body}
    </PlannerPageShell>
  )
}
