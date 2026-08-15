import { useEffect, useMemo, useState } from 'react'
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
import { btnOutlinedClass, btnPrimaryClass, navBtnClass } from '../fieldStyles'

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

  const handleTreeSelect = (key: string) => {
    if (disabled) return

    if (key === '__root__') {
      onNavigate('')
      return
    }

    const entry = entries.find((e) => e.id === key)
    if (!entry) return

    if (entry.type === 'folder') {
      onNavigate(getEntryPath(entry))
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

      <div className="file-manager-tree min-h-0 flex-1 overflow-y-auto">
        <Tree.Root
          value={treeNodes}
          expandedKeys={expandedKeys}
          onExpandedChange={(e: { value: Record<string, boolean> }) =>
            setExpandedKeys({ __root__: true, ...e.value })
          }
          className="w-full border-0 bg-transparent p-0 text-sm"
        >
          <Tree.Nodes>
            {({ node, leaf }) => {
              const treeNode = node as FileTreeNode
              const entryType =
                treeNode.entryType ?? (leaf ? 'file' : 'folder')
              const active = isNodeActive(treeNode, currentPath)
              const nodeKey = String(node.key)

              return (
                <Tree.Node uKey={nodeKey}>
                  <Tree.Content
                    className={[
                      'file-manager-tree-row',
                      active ? 'file-manager-tree-row--active' : '',
                    ].join(' ')}
                  >
                    <Tree.Toggle className="file-manager-tree-toggle">
                      <Tree.ToggleIndicator match="expanded">
                        <ChevronDown className="size-3.5" strokeWidth={2.5} />
                      </Tree.ToggleIndicator>
                      <Tree.ToggleIndicator match="collapsed">
                        <ChevronRight className="size-3.5" strokeWidth={2.5} />
                      </Tree.ToggleIndicator>
                    </Tree.Toggle>

                    <span
                      role="button"
                      tabIndex={disabled ? -1 : 0}
                      aria-disabled={disabled}
                      onClick={(e) => {
                        e.stopPropagation()
                        handleTreeSelect(nodeKey)
                      }}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' || e.key === ' ') {
                          e.preventDefault()
                          e.stopPropagation()
                          handleTreeSelect(nodeKey)
                        }
                      }}
                      className={`file-manager-tree-label ${navBtnClass} ${disabled ? 'pointer-events-none opacity-60' : ''}`}
                    >
                      <TreeNodeIcon entryType={entryType} active={active} />
                      <Tree.Label className="truncate">{node.label}</Tree.Label>
                    </span>
                  </Tree.Content>
                </Tree.Node>
              )
            }}
          </Tree.Nodes>
          <Tree.Empty />
        </Tree.Root>
      </div>
    </aside>
  )
}
