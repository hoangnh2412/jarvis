import { memo, useCallback, useMemo, type ReactNode } from 'react'
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
import { FileRowActions } from '../FileRowActions'
import { navBtnClass } from '../fieldStyles'

export type FileManagerTableProps = {
  items: FileEntry[]
  loading?: boolean
  total: number
  messages: FileManagerMessages
  disabled?: boolean
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

                return (
                  <DataTable.Row className="border-b border-slate-100 transition hover:bg-slate-50/80">
                    <DataTable.Cell className="px-4 py-2.5 align-middle">
                      {entry.type === 'folder' ? (
                        <button
                          type="button"
                          disabled={disabled}
                          onClick={() => onOpen?.(entry)}
                          className={`inline-flex max-w-full items-center gap-2 rounded-lg px-1 py-0.5 text-left font-medium text-slate-800 hover:text-teal-700 ${navBtnClass}`}
                        >
                          <Folder className="size-4 shrink-0 text-sky-500" />
                          <span className="truncate">{entry.name}</span>
                        </button>
                      ) : (
                        <span className="inline-flex max-w-full items-center gap-2 text-slate-800">
                          <FileText className="size-4 shrink-0 text-slate-400" />
                          <span className="truncate">{entry.name}</span>
                        </span>
                      )}
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
                <td colSpan={4} className="px-4 py-16 text-center">
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
