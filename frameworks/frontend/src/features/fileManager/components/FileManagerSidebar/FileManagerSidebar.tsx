import { useEffect, useMemo, useState, type CSSProperties, type DragEvent } from 'react'
import {
  ChevronDown,
  ChevronRight,
  FileText,
  Folder,
  FolderOpen,
  FolderPlus,
  Upload,
} from 'lucide-react'
import { Button } from 'primereact/button'
import { Tree } from 'primereact/tree'
import type { FileEntry } from '../../types'
import {
  buildFileTreeNodes,
  expandedKeysForFolderPath,
  getEntryPath,
  normalizePath,
  type FileTreeNode,
} from '../../utils/fileTree'
import { parseDraggedEntryIds, endDragEntryIds, isFileManagerDragActive } from '../../utils/fileManagerDnD'
import { btnOutlinedClass, btnPrimaryClass, navBtnClass } from '../fieldStyles'

const TREE_INDENT_REM = 1.4

function treeNodeIndentStyle(level = 1): CSSProperties {
  const depth = Math.max(0, level - 1)
  return {
    paddingLeft: `${depth * TREE_INDENT_REM}rem`,
  }
}

export type FileManagerSidebarProps = {
  entries: FileEntry[]
  currentPath: string
  allFilesLabel: string
  uploadLabel: string
  createFolderLabel: string
  disabled?: boolean
  onNavigate: (path: string) => void
  onOpen?: (entry: FileEntry) => void
  onUploadClick: () => void
  onCreateFolderClick: () => void
  onDropEntries?: (targetPath: string, entryIds: string[]) => void
  dropDisabled?: boolean
  onLoadFolderChildren?: (parentPath: string) => void | Promise<void>
}

function isNodeActive(treeNode: FileTreeNode, currentPath: string): boolean {
  if (treeNode.entryType === 'root') {
    return normalizePath(currentPath) === ''
  }
  if (treeNode.entryType === 'folder') {
    return normalizePath(currentPath) === normalizePath(treeNode.path)
  }
  return false
}

function TreeNodeIcon({
  entryType,
  active,
}: {
  entryType: FileTreeNode['entryType']
  active: boolean
}) {
  if (entryType === 'file') {
    return <FileText className="size-4 shrink-0 text-slate-500" aria-hidden />
  }
  if (active) {
    return <FolderOpen className="size-4 shrink-0 text-sky-600" aria-hidden />
  }
  return <Folder className="size-4 shrink-0 text-sky-500" aria-hidden />
}

function getNodeTargetPath(treeNode: FileTreeNode): string {
  if (treeNode.entryType === 'root') return ''
  return normalizePath(treeNode.path)
}

function renderTreeNode({
  node,
  leaf,
  level = 1,
  expanded = false,
  currentPath,
  disabled,
  dropDisabled = false,
  dropTargetPath,
  onSelect,
  onToggleExpand,
  onDropEntries,
  onDropTargetChange,
}: {
  node: FileTreeNode
  leaf?: boolean
  level?: number
  expanded?: boolean
  currentPath: string
  disabled: boolean
  dropDisabled?: boolean
  dropTargetPath?: string | null
  onSelect: (key: string) => void
  onToggleExpand: (key: string, folderPath?: string) => void
  onDropEntries?: (targetPath: string, entryIds: string[]) => void
  onDropTargetChange?: (targetPath: string | null) => void
}) {
  const treeNode = node
  const entryType = treeNode.entryType ?? (leaf ? 'file' : 'folder')
  const isFolderNode = entryType === 'folder' || entryType === 'root'
  const active = isNodeActive(treeNode, currentPath)
  const nodeKey = String(node.key)
  const targetPath = isFolderNode ? getNodeTargetPath(treeNode) : null
  const isDropTarget =
    isFolderNode &&
    targetPath != null &&
    dropTargetPath != null &&
    normalizePath(dropTargetPath) === normalizePath(targetPath)

  const handleDragOver = (event: DragEvent<HTMLDivElement>) => {
    if (dropDisabled || !isFolderNode || targetPath == null || !onDropEntries) return

    event.preventDefault()
    event.stopPropagation()
    event.dataTransfer.dropEffect = 'move'
    onDropTargetChange?.(targetPath)
  }

  const handleDrop = (event: DragEvent<HTMLDivElement>) => {
    if (dropDisabled || !isFolderNode || targetPath == null || !onDropEntries) return

    event.preventDefault()
    event.stopPropagation()
    onDropTargetChange?.(null)

    const entryIds = parseDraggedEntryIds(event.dataTransfer)
    endDragEntryIds()
    if (entryIds.length > 0) {
      onDropEntries(targetPath, entryIds)
    }
  }

  const rowDropHandlers = isFolderNode && onDropEntries && !dropDisabled
    ? {
        onDragOver: handleDragOver,
        onDragLeave: (event: DragEvent<HTMLDivElement>) => {
          const next = event.relatedTarget
          if (next instanceof Node && event.currentTarget.contains(next)) return
          onDropTargetChange?.(null)
        },
        onDrop: handleDrop,
      }
    : undefined

  return (
    <Tree.Node uKey={nodeKey}>
      <Tree.Content
        className={[
          'file-manager-tree-row',
          active ? 'file-manager-tree-row--active' : '',
          isDropTarget ? 'file-manager-tree-row--drop-target' : '',
        ].join(' ')}
        style={treeNodeIndentStyle(level)}
      >
        <div
          className="file-manager-tree-drop-row flex min-w-0 flex-1 items-center gap-1.5"
          {...rowDropHandlers}
        >
          {isFolderNode ? (
            <button
              type="button"
              className="file-manager-tree-toggle"
              aria-expanded={expanded}
              aria-label={expanded ? 'Thu gọn' : 'Mở rộng'}
              disabled={disabled}
              onClick={(e) => {
                e.stopPropagation()
                onToggleExpand(
                  nodeKey,
                  entryType === 'root' ? '' : normalizePath(treeNode.path),
                )
              }}
            >
              {expanded ? (
                <ChevronDown className="size-3.5" strokeWidth={2.5} />
              ) : (
                <ChevronRight className="size-3.5" strokeWidth={2.5} />
              )}
            </button>
          ) : (
            <span className="file-manager-tree-toggle-spacer" aria-hidden />
          )}

          <span
            role="button"
            tabIndex={disabled ? -1 : 0}
            aria-disabled={disabled}
            onClick={(e) => {
              e.stopPropagation()
              onSelect(nodeKey)
            }}
            onKeyDown={(e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault()
                e.stopPropagation()
                onSelect(nodeKey)
              }
            }}
            className={`file-manager-tree-label ${navBtnClass} ${disabled ? 'pointer-events-none opacity-60' : ''}`}
          >
            <TreeNodeIcon entryType={entryType} active={active} />
            <Tree.Label className="truncate">{node.label}</Tree.Label>
          </span>
        </div>
      </Tree.Content>
    </Tree.Node>
  )
}

export function FileManagerSidebar({
  entries,
  currentPath,
  allFilesLabel,
  uploadLabel,
  createFolderLabel,
  disabled = false,
  onNavigate,
  onOpen,
  onUploadClick,
  onCreateFolderClick,
  onDropEntries,
  dropDisabled = false,
  onLoadFolderChildren,
}: FileManagerSidebarProps) {
  const treeNodes = useMemo(
    () => buildFileTreeNodes(entries, allFilesLabel),
    [entries, allFilesLabel],
  )

  const [expandedKeys, setExpandedKeys] = useState<Record<string, boolean>>(() => ({
    __root__: true,
    ...expandedKeysForFolderPath(entries, currentPath),
  }))

  useEffect(() => {
    setExpandedKeys((prev) => ({
      __root__: true,
      ...prev,
      ...expandedKeysForFolderPath(entries, currentPath),
    }))
  }, [entries, currentPath])

  const [dropTargetPath, setDropTargetPath] = useState<string | null>(null)

  const handleToggleExpand = (key: string, folderPath?: string) => {
    const willExpand = !expandedKeys[key]
    setExpandedKeys((prev) => ({
      __root__: true,
      ...prev,
      [key]: !prev[key],
    }))
    if (willExpand && onLoadFolderChildren) {
      void onLoadFolderChildren(folderPath ?? '')
    }
  }

  const handleTreeSelect = (key: string) => {
    if (disabled) return

    if (key === '__root__') {
      onNavigate('')
      return
    }

    const entry = entries.find((e) => e.id === key)
    if (!entry) return

    if (entry.type === 'folder') {
      const folderPath = getEntryPath(entry)
      void onLoadFolderChildren?.(folderPath)
      onNavigate(folderPath)
      return
    }

    onNavigate(normalizePath(entry.parentPath))
    onOpen?.(entry)
  }

  return (
    <aside className="file-manager-sidebar flex w-[300px] shrink-0 flex-col gap-3 border-r border-slate-200 bg-white p-3">
      <div className="flex flex-col gap-2">
        <Button
          type="button"
          unstyled
          disabled={disabled}
          onClick={onUploadClick}
          className={btnPrimaryClass}
        >
          <Upload className="size-4" />
          {uploadLabel}
        </Button>
        <Button
          type="button"
          unstyled
          disabled={disabled}
          onClick={onCreateFolderClick}
          className={btnOutlinedClass}
        >
          <FolderPlus className="size-4" />
          {createFolderLabel}
        </Button>
      </div>

      <div
        className="file-manager-tree min-h-0 flex-1 overflow-y-auto"
        onDragOver={(event: DragEvent<HTMLDivElement>) => {
          if (dropDisabled || !onDropEntries || !isFileManagerDragActive()) return
          event.preventDefault()
          event.dataTransfer.dropEffect = 'move'
        }}
      >
        <Tree.Root
          value={treeNodes}
          expandedKeys={expandedKeys}
          onExpandedChange={(e: { value: Record<string, boolean> }) =>
            setExpandedKeys({ __root__: true, ...e.value })
          }
          className="w-full border-0 bg-transparent p-0 text-sm"
        >
          <Tree.Nodes>
            {(slot) =>
              renderTreeNode({
                node: slot.node as FileTreeNode,
                leaf: slot.leaf,
                level: slot.level ?? 1,
                expanded: slot.expanded,
                currentPath,
                disabled,
                dropDisabled,
                dropTargetPath,
                onSelect: handleTreeSelect,
                onToggleExpand: handleToggleExpand,
                onDropEntries,
                onDropTargetChange: setDropTargetPath,
              })
            }
          </Tree.Nodes>
          <Tree.Empty />
        </Tree.Root>
      </div>
    </aside>
  )
}
