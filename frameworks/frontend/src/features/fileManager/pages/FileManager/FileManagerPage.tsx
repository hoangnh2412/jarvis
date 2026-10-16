import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ChangeEvent,
  type KeyboardEvent,
  type ReactNode,
} from 'react'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import { FeatureDialog } from '../../../../common/FeatureDialog'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import {
  FileManagerSidebar,
  FileManagerTable,
  FileManagerToolbar,
} from '../../components'
import { btnOutlinedClass, btnPrimaryClass, fieldInputClass, fieldInputInvalidClass } from '../../components/fieldStyles'
import { getFileManagerMessages, type FileManagerLocale } from '../../localization'
import type { FileEntry, ListFilesResult } from '../../types'
import {
  filterMovableEntries,
} from '../../utils/fileManagerDnD'
import {
  applyMovesToTreeEntries,
  getEntryPath,
  getParentPath,
  normalizePath,
  resolveFileManagerContent,
  type FileManagerSlotContent,
} from '../../utils'
import { getEntryNameError } from '../../validation'
import { InputText } from 'primereact/inputtext'
import { Button } from 'primereact/button'
import { Select } from 'primereact/select'

function missingCallback(action: string) {
  return () => {
    throw new Error(
      `FileManagerPage: thiếu callback.${action}.onSubmit — host cần wire API (vd. @platform/files FilesPage).`,
    )
  }
}

export type FileManagerPageContentContext = {
  items: FileEntry[]
  folders: FileEntry[]
  total: number
  loading: boolean
  currentPath: string
  navigate: (path: string) => void
  reload: () => Promise<void>
  openUpload: () => void
  openCreateFolder: () => void
  DefaultLayout: ReactNode
}

export type FileManagerPageProps = {
  items?: FileEntry[]
  folders?: FileEntry[]
  /** Toàn bộ file + folder cho cây sidebar */
  entries?: FileEntry[]
  loading?: boolean
  total?: number
  locale?: FileManagerLocale
  className?: string
  /** Provider root label (e.g. mydrive, D:, SharePoint). */
  rootLabel?: string
  /** Action callbacks — host wire API qua @platform/files hoặc custom services */
  callback?: {
    list?: ActionProps<
      { path: string; filter: string; sortField: 'name' | 'size'; sortOrder: 'asc' | 'desc' },
      { path: string; filter: string; sortField: 'name' | 'size'; sortOrder: 'asc' | 'desc' },
      ListFilesResult
    >
    entries?: ActionProps<Record<string, never>, Record<string, never>, FileEntry[]>
    /** Lazy-load immediate child folders for sidebar tree (one tree API level). */
    treeChildren?: ActionProps<
      { parentPath: string },
      { parentPath: string },
      FileEntry[]
    >
    upload?: ActionProps<
      { path: string; files: File[] },
      { path: string; files: File[] },
      FileEntry[]
    >
    createFolder?: ActionProps<
      { path: string; name: string },
      { path: string; name: string },
      FileEntry
    >
    delete?: ActionProps<{ entry: FileEntry }, FileEntry>
    rename?: ActionProps<
      { entry: FileEntry; name: string },
      { entry: FileEntry; name: string },
      FileEntry
    >
    move?: ActionProps<
      { entry: FileEntry; targetPath: string },
      { entry: FileEntry; targetPath: string },
      FileEntry
    >
    openFile?: ActionProps<{ entry: FileEntry }, FileEntry, void>
    getSharedLink?: ActionProps<{ entry: FileEntry }, FileEntry, string>
  }
  content?: FileManagerSlotContent<FileManagerPageContentContext>
}

export function FileManagerPage({
  items: itemsProp,
  folders: foldersProp,
  entries: entriesProp,
  loading: loadingProp,
  total: totalProp,
  locale = 'vi',
  className = '',
  rootLabel,
  callback,
  content,
}: FileManagerPageProps) {
  const messages = getFileManagerMessages(locale)
  const storageRootLabel = rootLabel?.trim() || messages.list.allFiles
  const controlled = itemsProp != null
  const controlledEntries = entriesProp != null

  const [internalItems, setInternalItems] = useState<FileEntry[]>([])
  const [internalTotal, setInternalTotal] = useState(0)
  const [internalLoading, setInternalLoading] = useState(!controlled)
  const [allEntries, setAllEntries] = useState<FileEntry[]>([])

  const [currentPath, setCurrentPath] = useState('')
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const [createFolderOpen, setCreateFolderOpen] = useState(false)
  const [folderName, setFolderName] = useState('')
  const [savingFolder, setSavingFolder] = useState(false)

  const [renameEntry, setRenameEntry] = useState<FileEntry | null>(null)
  const [renameValue, setRenameValue] = useState('')
  const [savingRename, setSavingRename] = useState(false)

  const [moveEntries, setMoveEntries] = useState<FileEntry[]>([])
  const [moveTargetPath, setMoveTargetPath] = useState('')
  const [moveSelectOpen, setMoveSelectOpen] = useState(false)
  const [savingMove, setSavingMove] = useState(false)

  const [pendingDelete, setPendingDelete] = useState<FileEntry | null>(null)
  const [deleting, setDeleting] = useState(false)
  const [uploading, setUploading] = useState(false)

  const fileInputRef = useRef<HTMLInputElement>(null)
  const loadedTreePathsRef = useRef(new Set<string>())
  const loadingTreePathsRef = useRef(new Set<string>())

  const items = controlled ? itemsProp : internalItems
  const total = controlled ? (totalProp ?? items.length) : internalTotal
  const loading = controlled ? Boolean(loadingProp) : internalLoading
  const folders = useMemo(
    () => (foldersProp ?? allEntries.filter((e) => e.type === 'folder')),
    [foldersProp, allEntries],
  )
  const treeEntries = entriesProp ?? allEntries
  const treeFolderEntries = useMemo(
    () => treeEntries.filter((entry) => entry.type === 'folder'),
    [treeEntries],
  )

  const mergeTreeEntries = useCallback((next: FileEntry[]) => {
    setAllEntries((previous) => {
      const byId = new Map(previous.map((entry) => [entry.id, entry]))
      for (const entry of next) {
        byId.set(entry.id, entry)
      }
      return [...byId.values()]
    })
  }, [])

  const loadFolderChildren = useCallback(
    async (parentPath: string, options?: { force?: boolean }) => {
      if (controlled || controlledEntries || !callback?.treeChildren?.onSubmit) return

      const normalized = normalizePath(parentPath)
      if (
        !options?.force &&
        (loadedTreePathsRef.current.has(normalized) ||
          loadingTreePathsRef.current.has(normalized))
      ) {
        return
      }

      loadingTreePathsRef.current.add(normalized)
      try {
        const outcome = await handleAction({
          ctx: { parentPath: normalized },
          callback: callback.treeChildren,
          defaultSubmit: missingCallback('treeChildren'),
          getPayload: ({ parentPath: path }) => ({ parentPath: path }),
        })
        if (outcome.status === 'success') {
          loadedTreePathsRef.current.add(normalized)
          mergeTreeEntries(outcome.result)
        }
      } catch (error) {
        notify.error(getErrorMessage(error, messages.toast.error))
      } finally {
        loadingTreePathsRef.current.delete(normalized)
      }
    },
    [
      controlled,
      controlledEntries,
      callback?.treeChildren,
      mergeTreeEntries,
      messages.toast.error,
    ],
  )

  const ensureTreePathLoaded = useCallback(
    async (path: string) => {
      const normalized = normalizePath(path)
      await loadFolderChildren('')

      if (!normalized) return

      let acc = ''
      for (const part of normalized.split('/')) {
        acc = acc ? `${acc}/${part}` : part
        await loadFolderChildren(acc)
      }
    },
    [loadFolderChildren],
  )

  const refreshEntries = useCallback(async () => {
    if (controlled || controlledEntries) return

    loadedTreePathsRef.current.clear()

    try {
      const outcome = await handleAction({
        ctx: {},
        callback: callback?.entries,
        defaultSubmit: missingCallback('entries'),
        getPayload: () => ({}),
      })
      if (outcome.status === 'success') {
        loadedTreePathsRef.current.add('')
        setAllEntries(outcome.result)
        await ensureTreePathLoaded(currentPath)
      }
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    }
  }, [
    controlled,
    controlledEntries,
    callback?.entries,
    currentPath,
    ensureTreePathLoaded,
    messages.toast.error,
  ])

  const refreshTreeAfterMove = useCallback(
    async (movable: FileEntry[], targetPath: string) => {
      if (controlled || controlledEntries) return

      setAllEntries((previous) =>
        applyMovesToTreeEntries(previous, movable, targetPath),
      )

      const parentsToRefresh = new Set<string>([normalizePath(targetPath)])
      for (const entry of movable) {
        parentsToRefresh.add(normalizePath(entry.parentPath))
      }

      for (const parent of parentsToRefresh) {
        loadedTreePathsRef.current.delete(parent)
      }

      await Promise.all(
        [...parentsToRefresh].map((parent) =>
          loadFolderChildren(parent, { force: true }),
        ),
      )
    },
    [controlled, controlledEntries, loadFolderChildren],
  )

  const reload = useCallback(async (options?: { refreshTree?: boolean }) => {
    if (controlled) return
    setInternalLoading(true)
    try {
      const outcome = await handleAction({
        ctx: {
          path: currentPath,
          filter: '',
          sortField: 'name' as const,
          sortOrder: 'asc' as const,
        },
        callback: callback?.list,
        defaultSubmit: missingCallback('list'),
        getPayload: ({ path, filter, sortField, sortOrder }) => ({
          path,
          filter,
          sortField,
          sortOrder,
        }),
      })
      if (outcome.status === 'success') {
        setInternalItems(outcome.result.items)
        setInternalTotal(outcome.result.total)
      }
      if (options?.refreshTree !== false) {
        await refreshEntries()
      }
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    } finally {
      setInternalLoading(false)
    }
  }, [
    controlled,
    currentPath,
    callback?.list,
    refreshEntries,
    messages.toast.error,
  ])

  useEffect(() => {
    void reload()
  }, [reload])

  useEffect(() => {
    setSelectedIds([])
  }, [currentPath])

  const navigate = (path: string) => {
    const normalized = normalizePath(path)
    setCurrentPath(normalized)
    void ensureTreePathLoaded(normalized)
  }

  const handleGoUp = () => {
    setCurrentPath(getParentPath(currentPath))
  }

  const openUpload = () => fileInputRef.current?.click()

  const onFileInputChange = async (e: ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files
    if (!files?.length) return
    setUploading(true)
    try {
      const outcome = await handleAction({
        ctx: { path: currentPath, files: [...files] },
        callback: callback?.upload,
        defaultSubmit: missingCallback('upload'),
        getPayload: ({ path, files: list }) => ({ path, files: list }),
        onSuccess: async () => {
          await reload()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(messages.toast.uploadSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    } finally {
      setUploading(false)
      e.target.value = ''
    }
  }

  const onCreateFolder = async () => {
    const name = folderName.trim()
    const nameError = getEntryNameError(name)
    if (nameError) {
      notify.error(nameError)
      return
    }
    setSavingFolder(true)
    try {
      const outcome = await handleAction({
        ctx: { path: currentPath, name },
        callback: callback?.createFolder,
        defaultSubmit: missingCallback('createFolder'),
        getPayload: ({ path, name: n }) => ({ path, name: n }),
        onSuccess: async () => {
          setCreateFolderOpen(false)
          setFolderName('')
          await reload()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(messages.toast.createFolderSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    } finally {
      setSavingFolder(false)
    }
  }

  const onConfirmRename = async () => {
    if (!renameEntry) return
    const name = renameValue.trim()
    const nameError = getEntryNameError(name)
    if (nameError) {
      notify.error(nameError)
      return
    }
    setSavingRename(true)
    try {
      const outcome = await handleAction({
        ctx: { entry: renameEntry, name },
        callback: callback?.rename,
        defaultSubmit: missingCallback('rename'),
        getPayload: ({ entry, name: n }) => ({ entry, name: n }),
        onSuccess: async () => {
          setRenameEntry(null)
          setRenameValue('')
          await reload()
        },
      })
      if (outcome.status === 'cancelled') return
      notify.success(messages.toast.renameSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    } finally {
      setSavingRename(false)
    }
  }

  const onConfirmMove = async () => {
    if (!moveEntries.length) return

    const movable = filterMovableEntries(moveEntries, moveTargetPath)
    if (!movable.length) {
      notify.error(messages.toast.moveInvalid)
      return
    }

    setSavingMove(true)
    try {
      for (const entry of movable) {
        const outcome = await handleAction({
          ctx: { entry, targetPath: moveTargetPath },
          callback: callback?.move,
          defaultSubmit: missingCallback('move'),
          getPayload: ({ entry: item, targetPath }) => ({
            entry: item,
            targetPath,
          }),
        })
        if (outcome.status === 'cancelled') return
      }
      await refreshTreeAfterMove(movable, moveTargetPath)
      setMoveEntries([])
      setMoveTargetPath('')
      setSelectedIds([])
      await reload({ refreshTree: false })
      notify.success(messages.toast.moveSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    } finally {
      setSavingMove(false)
    }
  }

  const handleDropMove = async (targetPath: string, entryIds: string[]) => {
    if (!entryIds.length || !callback?.move?.onSubmit) return

    const entries = entryIds
      .map((id) => items.find((entry) => entry.id === id))
      .filter((entry): entry is FileEntry => entry != null)

    const movable = filterMovableEntries(entries, targetPath)
    if (!movable.length) {
      notify.error(messages.toast.moveInvalid)
      return
    }

    setSavingMove(true)
    try {
      for (const entry of movable) {
        const outcome = await handleAction({
          ctx: { entry, targetPath },
          callback: callback.move,
          defaultSubmit: missingCallback('move'),
          getPayload: ({ entry: item, targetPath: dest }) => ({
            entry: item,
            targetPath: dest,
          }),
        })
        if (outcome.status === 'cancelled') return
      }
      await refreshTreeAfterMove(movable, targetPath)
      setSelectedIds([])
      await reload({ refreshTree: false })
      notify.success(messages.toast.moveSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    } finally {
      setSavingMove(false)
    }
  }

  const confirmDelete = async () => {
    if (!pendingDelete) return
    setDeleting(true)
    try {
      const outcome = await handleAction({
        ctx: { entry: pendingDelete },
        callback: callback?.delete,
        defaultSubmit: missingCallback('delete'),
        getPayload: ({ entry }) => entry,
        onSuccess: async () => {
          setPendingDelete(null)
          await reload()
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

  const handleOpen = async (entry: FileEntry) => {
    if (entry.type === 'folder') {
      navigate(getEntryPath(entry))
      return
    }

    if (!callback?.openFile?.onSubmit) {
      notify.error(messages.toast.error)
      return
    }

    try {
      await handleAction({
        ctx: { entry },
        callback: callback.openFile,
        defaultSubmit: missingCallback('openFile'),
        getPayload: ({ entry: item }) => item,
      })
    } catch (error) {
      notify.error(getErrorMessage(error, messages.toast.error))
    }
  }

  const handleCopySharedLink = async (entry: FileEntry, fallbackLink: string) => {
    if (!callback?.getSharedLink?.onSubmit) {
      await navigator.clipboard.writeText(fallbackLink)
      return
    }

    const outcome = await handleAction({
      ctx: { entry },
      callback: callback.getSharedLink,
      defaultSubmit: missingCallback('getSharedLink'),
      getPayload: ({ entry: item }) => item,
    })
    if (outcome.status !== 'success') return
    await navigator.clipboard.writeText(outcome.result)
  }

  const moveTargetOptions = useMemo(() => {
    const options: { label: string; value: string }[] = [
      { label: storageRootLabel, value: '' },
    ]
    if (!moveEntries.length) return options

    for (const folder of folders) {
      const path = getEntryPath(folder)
      if (moveEntries.some((entry) => entry.id === folder.id)) continue

      const isInvalidDestination = moveEntries.some((entry) => {
        if (entry.type !== 'folder') return false
        const sourcePath = getEntryPath(entry)
        return path === sourcePath || path.startsWith(`${sourcePath}/`)
      })
      if (isInvalidDestination) continue

      options.push({ label: path, value: path })
    }
    return options
  }, [folders, moveEntries, storageRootLabel])

  const busy =
    loading ||
    uploading ||
    savingFolder ||
    savingRename ||
    savingMove ||
    deleting

  const folderNameError = folderName.trim()
    ? getEntryNameError(folderName)
    : null
  const renameValueError = renameValue.trim()
    ? getEntryNameError(renameValue)
    : null
  const canSaveFolder = Boolean(folderName.trim()) && !folderNameError
  const canSaveRename = Boolean(renameValue.trim()) && !renameValueError

  const defaultLayout = (
    <div
      className={`file-manager-shell flex h-full min-h-[520px] overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm ${className} [&_button:focus]:outline-none [&_button:focus]:ring-0 [&_button:focus-visible]:outline-none [&_button:focus-visible]:ring-0 [&_[data-focused]]:outline-none [&_[data-focused]]:ring-0`}
    >
      <input
        ref={fileInputRef}
        type="file"
        multiple
        className="hidden"
        onChange={(e) => void onFileInputChange(e)}
      />

      <FileManagerSidebar
        entries={treeFolderEntries}
        currentPath={currentPath}
        allFilesLabel={storageRootLabel}
        uploadLabel={messages.list.upload}
        createFolderLabel={messages.list.createFolder}
        disabled={busy}
        onNavigate={navigate}
        onOpen={(entry) => void handleOpen(entry)}
        onUploadClick={openUpload}
        onCreateFolderClick={() => {
          setFolderName('')
          setCreateFolderOpen(true)
        }}
        onDropEntries={(targetPath, entryIds) => void handleDropMove(targetPath, entryIds)}
        dropDisabled={busy}
        onLoadFolderChildren={(parentPath) => void loadFolderChildren(parentPath)}
      />

      <div className="flex min-w-0 flex-1 flex-col">
        <FileManagerToolbar
          currentPath={currentPath}
          rootLabel={storageRootLabel}
          messages={messages}
          disabled={busy}
          onNavigate={navigate}
          onGoUp={handleGoUp}
        />

        <FileManagerTable
          items={items}
          loading={loading}
          total={total}
          messages={messages}
          disabled={busy}
          selectedIds={selectedIds}
          onSelectedIdsChange={setSelectedIds}
          onOpen={(entry) => void handleOpen(entry)}
          onRename={(entry) => {
            setRenameEntry(entry)
            setRenameValue(entry.name)
          }}
          onMove={(entry) => {
            const entriesToMove =
              selectedIds.includes(entry.id) && selectedIds.length > 1
                ? items.filter((item) => selectedIds.includes(item.id))
                : [entry]
            setMoveEntries(entriesToMove)
            setMoveTargetPath('')
          }}
          onDelete={setPendingDelete}
          onCopySharedLink={(entry, fallbackLink) =>
            handleCopySharedLink(entry, fallbackLink)
          }
        />

      </div>

      <FeatureDialog
        open={createFolderOpen}
        onClose={() => setCreateFolderOpen(false)}
        title={messages.dialog.createFolderTitle}
        size="sm"
        footer={
          <>
            <Button
              type="button"
              unstyled
              className={btnOutlinedClass}
              disabled={savingFolder}
              onClick={() => setCreateFolderOpen(false)}
            >
              {messages.dialog.cancel}
            </Button>
            <Button
              type="button"
              unstyled
              className={`${btnPrimaryClass} !w-auto`}
              disabled={savingFolder || !canSaveFolder}
              onClick={() => void onCreateFolder()}
            >
              {messages.dialog.save}
            </Button>
          </>
        }
      >
        <label className="block text-sm font-medium text-slate-700">
          {messages.dialog.createFolderLabel}
          <InputText
            value={folderName}
            unstyled
            autoFocus
            className={`${folderNameError ? fieldInputInvalidClass : fieldInputClass} mt-1.5`}
            placeholder="Documents"
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              setFolderName(e.target.value)
            }
            onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
              if (e.key === 'Enter' && canSaveFolder) void onCreateFolder()
            }}
          />
          {folderNameError ? (
            <span className="mt-1 block text-xs text-red-600">{folderNameError}</span>
          ) : null}
        </label>
      </FeatureDialog>

      <FeatureDialog
        open={renameEntry != null}
        onClose={() => setRenameEntry(null)}
        title={messages.dialog.renameTitle}
        size="sm"
        footer={
          <>
            <Button
              type="button"
              unstyled
              className={btnOutlinedClass}
              disabled={savingRename}
              onClick={() => setRenameEntry(null)}
            >
              {messages.dialog.cancel}
            </Button>
            <Button
              type="button"
              unstyled
              className={`${btnPrimaryClass} !w-auto`}
              disabled={savingRename || !canSaveRename}
              onClick={() => void onConfirmRename()}
            >
              {messages.dialog.save}
            </Button>
          </>
        }
      >
        <label className="block text-sm font-medium text-slate-700">
          {messages.dialog.renameLabel}
          <InputText
            value={renameValue}
            unstyled
            autoFocus
            className={`${renameValueError ? fieldInputInvalidClass : fieldInputClass} mt-1.5`}
            placeholder="Report.pdf"
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              setRenameValue(e.target.value)
            }
            onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
              if (e.key === 'Enter' && canSaveRename) void onConfirmRename()
            }}
          />
          {renameValueError ? (
            <span className="mt-1 block text-xs text-red-600">{renameValueError}</span>
          ) : null}
        </label>
      </FeatureDialog>

      <FeatureDialog
        open={moveEntries.length > 0}
        onClose={() => setMoveEntries([])}
        title={
          moveEntries.length > 1
            ? messages.dialog.moveTitleMultiple(moveEntries.length)
            : messages.dialog.moveTitle
        }
        size="sm"
        footer={
          <>
            <Button
              type="button"
              unstyled
              className={btnOutlinedClass}
              disabled={savingMove}
              onClick={() => setMoveEntries([])}
            >
              {messages.dialog.cancel}
            </Button>
            <Button
              type="button"
              unstyled
              className={`${btnPrimaryClass} !w-auto`}
              disabled={savingMove}
              onClick={() => void onConfirmMove()}
            >
              {messages.dialog.save}
            </Button>
          </>
        }
      >
        <label className="block text-sm font-medium text-slate-700">
          {messages.dialog.moveLabel}
          <Select.Root
            value={moveTargetPath}
            open={moveSelectOpen}
            options={moveTargetOptions}
            optionLabel="label"
            optionValue="value"
            onOpenChange={(e: { value: boolean }) => setMoveSelectOpen(e.value)}
            onValueChange={(e: { value?: unknown }) =>
              setMoveTargetPath(String(e.value ?? ''))
            }
          >
            <Select.Trigger
              type="button"
              className={`${fieldInputClass} mt-1.5 flex items-center justify-between gap-2 text-left`}
            >
              <Select.Value placeholder={storageRootLabel} />
              <Select.Indicator className="text-slate-400">▾</Select.Indicator>
            </Select.Trigger>
            <Select.Portal>
              <Select.Positioner className="z-[300]">
                <Select.Popup className="max-h-60 min-w-[var(--px-positioner-anchor-width)] overflow-auto rounded-xl border border-slate-200 bg-white py-1 shadow-lg">
                  <Select.List className="m-0 list-none p-0 outline-none">
                    {moveTargetOptions.map((opt, index) => (
                      <Select.Option
                        key={opt.value || 'root'}
                        index={index}
                        className="cursor-pointer px-3 py-2 text-sm text-slate-800 outline-none data-[focused]:bg-slate-50 data-[selected]:bg-teal-50 data-[selected]:font-medium data-[selected]:text-teal-800"
                      >
                        {opt.label}
                      </Select.Option>
                    ))}
                  </Select.List>
                </Select.Popup>
              </Select.Positioner>
            </Select.Portal>
          </Select.Root>
        </label>
      </FeatureDialog>

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onClose={() => setPendingDelete(null)}
        onConfirm={confirmDelete}
        loading={deleting}
        title={messages.dialog.deleteTitle}
        description={
          pendingDelete
            ? messages.dialog.deleteDescription(pendingDelete.name)
            : undefined
        }
        confirmText={messages.dialog.confirmDelete}
      />
    </div>
  )

  const contentCtx: FileManagerPageContentContext = {
    items,
    folders,
    total,
    loading,
    currentPath,
    navigate,
    reload,
    openUpload,
    openCreateFolder: () => {
      setFolderName('')
      setCreateFolderOpen(true)
    },
    DefaultLayout: defaultLayout,
  }

  return (
    <div className="flex h-full min-h-0 flex-col font-sans text-ink">
      {resolveFileManagerContent(content, contentCtx, defaultLayout)}
    </div>
  )
}

