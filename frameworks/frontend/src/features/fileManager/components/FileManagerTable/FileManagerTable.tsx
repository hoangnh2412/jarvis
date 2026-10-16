import { memo, useCallback, useMemo, type DragEvent, type ReactNode } from 'react'
import {
  ArrowDown,
  ArrowUp,
  ArrowUpDown,
  FileText,
  Folder,
  Link2,
  X,
} from 'lucide-react'
import { DataTable } from 'primereact/datatable'
import { InputText } from 'primereact/inputtext'
import { ProgressSpinner } from 'primereact/progressspinner'
import { notify } from '../../../../common/Toaster'
import type { FileEntry } from '../../types'
import type { FileManagerMessages } from '../../localization'
import { formatFileSize, getShareLink } from '../../utils/fileTree'
import { setDraggedEntryIds, endDragEntryIds } from '../../utils/fileManagerDnD'
import { FileRowActions } from '../FileRowActions'
import { navBtnClass } from '../fieldStyles'

export type FileManagerTableProps = {
  items: FileEntry[]
  loading?: boolean
  total: number
  messages: FileManagerMessages
  disabled?: boolean
  selectedIds?: string[]
  onSelectedIdsChange?: (ids: string[]) => void
  onOpen?: (entry: FileEntry) => void
  onRename?: (entry: FileEntry) => void
  onMove?: (entry: FileEntry) => void
  onDelete?: (entry: FileEntry) => void
  onCopySharedLink?: (entry: FileEntry, link: string) => void | Promise<void>
}

type FileTableRow = FileEntry & {
  displaySize: string
}

type FilterSlot = {
  value?: unknown
  onChange: (event: unknown, value: unknown, matchMode?: string) => void
  onClear?: (event: unknown) => void
}

const filterInputClass =
  'box-border mt-1.5 h-9 w-full min-w-[7rem] rounded-lg border border-solid border-slate-200 bg-white px-2.5 text-sm font-normal normal-case tracking-normal text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20'

const rowCheckboxClass =
  'size-4 shrink-0 cursor-pointer rounded border-slate-300 text-teal-600 focus:ring-teal-600/30 disabled:cursor-not-allowed disabled:opacity-60'

function TextColumnFilter({ value, onChange, onClear }: FilterSlot) {
  const text = value == null ? '' : String(value)
  return (
    <div className="relative">
      <InputText
        unstyled
        value={text}
        placeholder="Lọc…"
        className={filterInputClass}
        onChange={(e: { target: { value: string } }) => {
          const next = e.target.value
          onChange(e, next === '' ? null : next, 'contains')
        }}
      />
      {text ? (
        <button
          type="button"
          aria-label="Xóa lọc"
          className="absolute right-1.5 top-[calc(50%+3px)] inline-flex size-5 -translate-y-1/2 items-center justify-center rounded text-slate-400 hover:bg-slate-100 hover:text-slate-600"
          onClick={(e) => onClear?.(e)}
        >
          <X className="size-3.5" aria-hidden />
        </button>
      ) : null}
    </div>
  )
}

function HeaderWithFilter({
  label,
  sortField,
  align = 'left',
  children,
}: {
  label: string
  sortField?: string
  align?: 'left' | 'right' | 'center'
  children?: ReactNode
}) {
  const title = sortField ? (
    <DataTable.Sort
      field={sortField}
      className="inline-flex w-full items-center gap-1 border-0 bg-transparent p-0 text-[11px] font-semibold uppercase tracking-[0.06em] text-slate-500 outline-none transition hover:text-slate-800"
    >
      <span>{label}</span>
      <span className="inline-flex size-3.5 shrink-0 items-center justify-center text-slate-400">
        <DataTable.SortIndicator match="asc" className="text-teal-700">
          <ArrowUp className="size-3.5" aria-hidden />
        </DataTable.SortIndicator>
        <DataTable.SortIndicator match="desc" className="text-teal-700">
          <ArrowDown className="size-3.5" aria-hidden />
        </DataTable.SortIndicator>
        <DataTable.SortIndicator match="unsorted" className="opacity-50">
          <ArrowUpDown className="size-3.5" aria-hidden />
        </DataTable.SortIndicator>
      </span>
    </DataTable.Sort>
  ) : (
    <span className="block text-[11px] font-semibold uppercase tracking-[0.06em] text-slate-500">
      {label}
    </span>
  )

  return (
    <DataTable.THeadCell
      className={[
        'px-4 py-3 align-top',
        align === 'right'
          ? 'text-right'
          : align === 'center'
            ? 'text-center'
            : 'text-left',
      ].join(' ')}
    >
      {title}
      {children}
    </DataTable.THeadCell>
  )
}

export const FileManagerTable = memo(function FileManagerTable({
  items,
  loading = false,
  total,
  messages,
  disabled = false,
  selectedIds = [],
  onSelectedIdsChange,
  onOpen,
  onRename,
  onMove,
  onDelete,
  onCopySharedLink,
}: FileManagerTableProps) {
  const rows = useMemo<FileTableRow[]>(
    () =>
      items.map((entry) => ({
        ...entry,
        displaySize:
          entry.type === 'file' ? formatFileSize(entry.size) : '—',
      })),
    [items],
  )

  const actionLabels = useMemo(
    () => ({
      actions: messages.list.actions,
      open: messages.actions.open,
      rename: messages.actions.rename,
      move: messages.actions.move,
      delete: messages.actions.delete,
    }),
    [messages],
  )

  const allSelected =
    rows.length > 0 && selectedIds.length > 0 && selectedIds.length === rows.length
  const someSelected = selectedIds.length > 0 && !allSelected

  const toggleAll = useCallback(() => {
    if (!onSelectedIdsChange) return
    onSelectedIdsChange(allSelected ? [] : rows.map((row) => row.id))
  }, [allSelected, onSelectedIdsChange, rows])

  const toggleOne = useCallback(
    (id: string) => {
      if (!onSelectedIdsChange) return
      onSelectedIdsChange(
        selectedIds.includes(id)
          ? selectedIds.filter((value) => value !== id)
          : [...selectedIds, id],
      )
    },
    [onSelectedIdsChange, selectedIds],
  )

  const handleRowDragStart = useCallback(
    (entry: FileEntry, event: DragEvent<HTMLElement>) => {
      if (disabled) return

      const dragIds =
        selectedIds.includes(entry.id) && selectedIds.length > 0
          ? selectedIds
          : [entry.id]
      setDraggedEntryIds(event.dataTransfer, dragIds)
    },
    [disabled, selectedIds],
  )

  const handleRowDragEnd = useCallback(() => {
    endDragEntryIds()
  }, [])

  const handleCopySharedLink = useCallback(
    async (entry: FileEntry) => {
      if (disabled) return

      const link = getShareLink(entry)
      try {
        if (onCopySharedLink) {
          await onCopySharedLink(entry, link)
        } else {
          await navigator.clipboard.writeText(link)
        }
        notify.success(messages.toast.copyLinkSuccess)
      } catch {
        notify.error(messages.toast.error)
      }
    },
    [disabled, messages.toast.copyLinkSuccess, messages.toast.error, onCopySharedLink],
  )

  return (
    <div className="flex min-h-0 flex-1 flex-col">
      <DataTable.Root
        data={rows}
        dataKey="id"
        loading={loading}
        removableSort
        className="relative flex min-h-0 flex-1 flex-col overflow-hidden"
      >
        <DataTable.TableContainer className="min-h-0 flex-1 overflow-auto">
          <DataTable.Table className="w-full min-w-[640px] border-collapse text-sm">
            <DataTable.THead>
              <DataTable.THeadRow className="sticky top-0 z-[1] border-b border-slate-200 bg-slate-50/95 backdrop-blur-sm">
                <DataTable.THeadCell className="w-10 px-2 py-3 align-top">
                  <input
                    type="checkbox"
                    aria-label="Chọn tất cả"
                    className={rowCheckboxClass}
                    checked={allSelected}
                    ref={(element) => {
                      if (element) element.indeterminate = someSelected
                    }}
                    disabled={disabled || rows.length === 0 || !onSelectedIdsChange}
                    onChange={toggleAll}
                  />
                </DataTable.THeadCell>

                <HeaderWithFilter label={messages.list.name} sortField="name">
                  <DataTable.Filter field="name" display="row" dataType="text">
                    {(ctx: FilterSlot) => <TextColumnFilter {...ctx} />}
                  </DataTable.Filter>
                </HeaderWithFilter>

                <HeaderWithFilter label={messages.list.size} sortField="size">
                  <DataTable.Filter field="displaySize" display="row" dataType="text">
                    {(ctx: FilterSlot) => <TextColumnFilter {...ctx} />}
                  </DataTable.Filter>
                </HeaderWithFilter>

                <HeaderWithFilter
                  label={messages.list.sharedLink}
                  align="center"
                />

                <HeaderWithFilter label={messages.list.actions} align="right" />
              </DataTable.THeadRow>
            </DataTable.THead>

            <DataTable.TBody>
              {({ item }) => {
                const entry = item as FileTableRow
                const hasLink = Boolean(entry.sharedLink)
                const isSelected = selectedIds.includes(entry.id)

                return (
                  <DataTable.Row
                    className={[
                      'border-b border-slate-100 transition hover:bg-slate-50/80',
                      isSelected ? 'bg-teal-50/40' : '',
                    ].join(' ')}
                  >
                    <DataTable.Cell className="w-10 px-2 py-2.5 align-middle">
                      <input
                        type="checkbox"
                        aria-label={`Chọn ${entry.name}`}
                        className={rowCheckboxClass}
                        checked={isSelected}
                        disabled={disabled || !onSelectedIdsChange}
                        draggable={false}
                        onChange={() => toggleOne(entry.id)}
                        onClick={(event) => event.stopPropagation()}
                      />
                    </DataTable.Cell>
                    <DataTable.Cell className="px-4 py-2.5 align-middle">
                      <div
                        draggable={!disabled}
                        onDragStart={(event: DragEvent<HTMLDivElement>) =>
                          handleRowDragStart(entry, event)
                        }
                        onDragEnd={handleRowDragEnd}
                        className="file-manager-row-drag inline-flex max-w-full items-center gap-2"
                      >
                        {entry.type === 'folder' ? (
                          <>
                            <Folder className="size-4 shrink-0 text-sky-500" aria-hidden />
                            <span
                              role="button"
                              tabIndex={disabled ? -1 : 0}
                              aria-disabled={disabled}
                              onClick={() => {
                                if (!disabled) onOpen?.(entry)
                              }}
                              onKeyDown={(event) => {
                                if (disabled) return
                                if (event.key === 'Enter' || event.key === ' ') {
                                  event.preventDefault()
                                  onOpen?.(entry)
                                }
                              }}
                              className={`file-manager-entry-link inline-flex min-w-0 max-w-full cursor-pointer items-center rounded-lg px-1 py-0.5 text-left text-slate-800 hover:text-teal-700 ${navBtnClass}`}
                            >
                              <span className="truncate">{entry.name}</span>
                            </span>
                          </>
                        ) : (
                          <span className="inline-flex max-w-full items-center gap-2 text-slate-800">
                            <FileText className="size-4 shrink-0 text-slate-400" />
                            <span className="truncate">{entry.name}</span>
                          </span>
                        )}
                      </div>
                    </DataTable.Cell>
                    <DataTable.Cell className="px-4 py-2.5 align-middle tabular-nums text-slate-600">
                      {entry.displaySize}
                    </DataTable.Cell>
                    <DataTable.Cell className="px-4 py-2.5 text-center align-middle">
                      <button
                        type="button"
                        disabled={disabled}
                        title={messages.actions.copyLink}
                        aria-label={messages.actions.copyLink}
                        onClick={() => void handleCopySharedLink(entry)}
                        className={`mx-auto inline-flex size-8 items-center justify-center rounded-lg transition hover:bg-teal-50 ${navBtnClass} ${
                          hasLink
                            ? 'text-teal-600 hover:text-teal-700'
                            : 'text-slate-400 hover:text-teal-600'
                        } disabled:cursor-not-allowed disabled:opacity-60`}
                      >
                        <Link2 className="size-4" aria-hidden />
                      </button>
                    </DataTable.Cell>
                    <DataTable.Cell className="px-4 py-2.5 text-right align-middle">
                      <div className="flex justify-end">
                        <FileRowActions
                          entry={entry}
                          labels={actionLabels}
                          disabled={disabled}
                          onOpen={onOpen}
                          onRename={onRename}
                          onMove={onMove}
                          onDelete={onDelete}
                        />
                      </div>
                    </DataTable.Cell>
                  </DataTable.Row>
                )
              }}
            </DataTable.TBody>

            <DataTable.EmptyTBody>
              <tr>
                <td colSpan={5} className="px-4 py-16 text-center">
                  {!loading ? (
                    <p className="m-0 text-sm text-slate-500">
                      {messages.list.empty}
                    </p>
                  ) : null}
                </td>
              </tr>
            </DataTable.EmptyTBody>
          </DataTable.Table>
        </DataTable.TableContainer>

        <DataTable.Loading className="absolute inset-0 flex flex-col items-center justify-center gap-3 bg-white/80">
          <ProgressSpinner.Root className="h-8 w-8">
            <ProgressSpinner.Track />
            <ProgressSpinner.Range />
          </ProgressSpinner.Root>
        </DataTable.Loading>
      </DataTable.Root>

      <p className="m-0 border-t border-slate-100 px-4 py-2 text-xs text-slate-500">
        {messages.list.total(total)}
      </p>
    </div>
  )
})
