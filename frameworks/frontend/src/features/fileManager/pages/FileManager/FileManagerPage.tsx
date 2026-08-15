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
  FileManagerStorageBar,
  FileManagerTable,
  FileManagerToolbar,
} from '../../components'
import { btnOutlinedClass, btnPrimaryClass, fieldInputClass } from '../../components/fieldStyles'
import { getFileManagerMessages, type FileManagerLocale } from '../../localization'
import {
  mockCreateFolder,
  mockDeleteEntry,
  mockGetAllEntries,
  mockListFiles,
  mockMoveEntry,
  mockRenameEntry,
  mockUploadFiles,
} from '../../services'
import type { FileEntry, ListFilesResult, StorageInfo } from '../../types'
import {
  getEntryPath,
  getParentPath,
  normalizePath,
  resolveFileManagerContent,
  type FileManagerSlotContent,
} from '../../utils'
import { InputText } from 'primereact/inputtext'
import { Button } from 'primereact/button'
import { Select } from 'primereact/select'
import { FAKE_STORAGE } from '../../constants'

export type FileManagerPageContentContext = {
  items: FileEntry[]
  folders: FileEntry[]
  total: number
  loading: boolean
  currentPath: string
  storage: StorageInfo
  navigate: (path: string) => void
  reload: () => Promise<void>
  openUpload: () => void
  openCreateFolder: () => void
  DefaultLayout: ReactNode
}

export type FileManagerPageProps = {
  items?: FileEntry[]
  folders?: FileEntry[]
  /** Toàn bộ file + folder cho cây sidebar; mặc định lấy từ mock khi không controlled */
  entries?: FileEntry[]
  loading?: boolean
  total?: number
  storage?: StorageInfo
  locale?: FileManagerLocale
  className?: string
  /**
   * Action callback kiểu jQuery ajax.
   * Mặc định dùng mock; host override `onSubmit` khi gắn API từ `modules/files`.
   */
  callback?: {
    list?: ActionProps<
      { path: string; filter: string; sortField: 'name' | 'size'; sortOrder: 'asc' | 'desc' },
      { path: string; filter: string; sortField: 'name' | 'size'; sortOrder: 'asc' | 'desc' },
      ListFilesResult
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
  }
  content?: FileManagerSlotContent<FileManagerPageContentContext>
}

export function FileManagerPage({
  items: itemsProp,
  folders: foldersProp,
  entries: entriesProp,
  loading: loadingProp,
  total: totalProp,
  storage: storageProp,
  locale = 'vi',
  className = '',
  callback,
  content,
}: FileManagerPageProps) {
  const messages = getFileManagerMessages(locale)
  const controlled = itemsProp != null

  const [internalItems, setInternalItems] = useState<FileEntry[]>([])
  const [internalTotal, setInternalTotal] = useState(0)
  const [internalLoading, setInternalLoading] = useState(!controlled)
  const [internalStorage, setInternalStorage] = useState<StorageInfo>(FAKE_STORAGE)
  const [allEntries, setAllEntries] = useState<FileEntry[]>([])

  const [currentPath, setCurrentPath] = useState('')

  const [createFolderOpen, setCreateFolderOpen] = useState(false)
  const [folderName, setFolderName] = useState('')
  const [savingFolder, setSavingFolder] = useState(false)

  const [renameEntry, setRenameEntry] = useState<FileEntry | null>(null)
  const [renameValue, setRenameValue] = useState('')
  const [savingRename, setSavingRename] = useState(false)

  const [moveEntry, setMoveEntry] = useState<FileEntry | null>(null)
  const [moveTargetPath, setMoveTargetPath] = useState('')
  const [moveSelectOpen, setMoveSelectOpen] = useState(false)
  const [savingMove, setSavingMove] = useState(false)

  const [pendingDelete, setPendingDelete] = useState<FileEntry | null>(null)
  const [deleting, setDeleting] = useState(false)
  const [uploading, setUploading] = useState(false)

  const fileInputRef = useRef<HTMLInputElement>(null)

  const items = controlled ? itemsProp : internalItems
  const total = controlled ? (totalProp ?? items.length) : internalTotal
  const loading = controlled ? Boolean(loadingProp) : internalLoading
  const storage = storageProp ?? internalStorage
  const folders = useMemo(
    () => (foldersProp ?? allEntries.filter((e) => e.type === 'folder')),
    [foldersProp, allEntries],
  )
  const treeEntries = entriesProp ?? allEntries

  const refreshEntries = useCallback(async () => {
    if (controlled) return
    setAllEntries(mockGetAllEntries())
  }, [controlled])

  const reload = useCallback(async () => {
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
        defaultSubmit: mockListFiles,
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
        setInternalStorage(outcome.result.storage)
      }
      await refreshEntries()
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

  const navigate = (path: string) => {
    setCurrentPath(normalizePath(path))
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
        defaultSubmit: mockUploadFiles,
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
    if (!name) return
    setSavingFolder(true)
    try {
      const outcome = await handleAction({
        ctx: { path: currentPath, name },
        callback: callback?.createFolder,
        defaultSubmit: mockCreateFolder,
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
    if (!name) return
    setSavingRename(true)
    try {
      const outcome = await handleAction({
        ctx: { entry: renameEntry, name },
        callback: callback?.rename,
        defaultSubmit: mockRenameEntry,
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
    if (!moveEntry) return
    setSavingMove(true)
    try {
      const outcome = await handleAction({
        ctx: { entry: moveEntry, targetPath: moveTargetPath },
        callback: callback?.move,
        defaultSubmit: mockMoveEntry,
        getPayload: ({ entry, targetPath }) => ({ entry, targetPath }),
        onSuccess: async () => {
          setMoveEntry(null)
          setMoveTargetPath('')
          await reload()
        },
      })
      if (outcome.status === 'cancelled') return
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
        defaultSubmit: mockDeleteEntry,
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

  const handleOpen = (entry: FileEntry) => {
    if (entry.type === 'folder') {
      navigate(getEntryPath(entry))
      return
    }
    notify.success(`${messages.toast.openFile}: ${entry.name}`)
  }

  const moveTargetOptions = useMemo(() => {
    const options: { label: string; value: string }[] = [
      { label: messages.list.allFiles, value: '' },
    ]
    if (!moveEntry) return options

    const sourcePath =
      moveEntry.type === 'folder' ? getEntryPath(moveEntry) : null

    for (const folder of folders) {
      const path = getEntryPath(folder)
      if (moveEntry.id === folder.id) continue
      if (
        sourcePath &&
        (path === sourcePath || path.startsWith(`${sourcePath}/`))
      ) {
        continue
      }
      options.push({ label: path, value: path })
    }
    return options
  }, [folders, moveEntry, messages.list.allFiles])

  const busy =
    loading ||
    uploading ||
    savingFolder ||
    savingRename ||
    savingMove ||
    deleting

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
        entries={treeEntries}
        currentPath={currentPath}
        allFilesLabel={messages.list.allFiles}
        uploadLabel={messages.list.upload}
        createFolderLabel={messages.list.createFolder}
        disabled={busy}
        onNavigate={navigate}
        onOpen={handleOpen}
        onUploadClick={openUpload}
        onCreateFolderClick={() => {
          setFolderName('')
          setCreateFolderOpen(true)
        }}
      />

      <div className="flex min-w-0 flex-1 flex-col">
        <FileManagerToolbar
          currentPath={currentPath}
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
          onOpen={handleOpen}
          onRename={(entry) => {
            setRenameEntry(entry)
            setRenameValue(entry.name)
          }}
          onMove={(entry) => {
            setMoveEntry(entry)
            setMoveTargetPath('')
          }}
          onDelete={setPendingDelete}
        />

        <FileManagerStorageBar storage={storage} messages={messages} />
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
              disabled={savingFolder || !folderName.trim()}
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
            className={`${fieldInputClass} mt-1.5`}
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              setFolderName(e.target.value)
            }
            onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
              if (e.key === 'Enter') void onCreateFolder()
            }}
          />
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
              disabled={savingRename || !renameValue.trim()}
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
            className={`${fieldInputClass} mt-1.5`}
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              setRenameValue(e.target.value)
            }
            onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
              if (e.key === 'Enter') void onConfirmRename()
            }}
          />
        </label>
      </FeatureDialog>

      <FeatureDialog
        open={moveEntry != null}
        onClose={() => setMoveEntry(null)}
        title={messages.dialog.moveTitle}
        size="sm"
        footer={
          <>
            <Button
              type="button"
              unstyled
              className={btnOutlinedClass}
              disabled={savingMove}
              onClick={() => setMoveEntry(null)}
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
              <Select.Value placeholder={messages.list.allFiles} />
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
    storage,
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
