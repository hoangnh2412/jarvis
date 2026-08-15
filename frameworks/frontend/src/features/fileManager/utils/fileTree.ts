import type { FileEntry, ListFilesParams } from '../types'

export function normalizePath(path: string): string {
  return path.replace(/^\/+|\/+$/g, '').replace(/\\/g, '/')
}

export function joinPath(parent: string, name: string): string {
  const base = normalizePath(parent)
  const segment = name.trim().replace(/[/\\]/g, '')
  if (!segment) return base
  return base ? `${base}/${segment}` : segment
}

export function getParentPath(path: string): string {
  const normalized = normalizePath(path)
  if (!normalized) return ''
  const parts = normalized.split('/')
  parts.pop()
  return parts.join('/')
}

export function getEntryPath(entry: FileEntry): string {
  return joinPath(entry.parentPath, entry.name)
}

export function getBreadcrumbSegments(path: string): { label: string; path: string }[] {
  const normalized = normalizePath(path)
  const segments: { label: string; path: string }[] = [
    { label: 'All files', path: '' },
  ]
  if (!normalized) return segments

  let acc = ''
  for (const part of normalized.split('/')) {
    acc = acc ? `${acc}/${part}` : part
    segments.push({ label: part, path: acc })
  }
  return segments
}

export function listFolders(entries: FileEntry[]): FileEntry[] {
  return entries.filter((e) => e.type === 'folder')
}

export function getChildren(
  entries: FileEntry[],
  parentPath: string,
): FileEntry[] {
  const parent = normalizePath(parentPath)
  return entries.filter((e) => normalizePath(e.parentPath) === parent)
}

export function filterEntries(
  entries: FileEntry[],
  filter: string,
): FileEntry[] {
  const q = filter.trim().toLowerCase()
  if (!q) return entries
  return entries.filter((e) => e.name.toLowerCase().includes(q))
}

export function sortEntries(
  entries: FileEntry[],
  sortField: ListFilesParams['sortField'] = 'name',
  sortOrder: ListFilesParams['sortOrder'] = 'asc',
): FileEntry[] {
  const dir = sortOrder === 'desc' ? -1 : 1
  return [...entries].sort((a, b) => {
    if (a.type !== b.type) {
      return a.type === 'folder' ? -1 : 1
    }
    if (sortField === 'size') {
      const sa = a.size ?? 0
      const sb = b.size ?? 0
      return (sa - sb) * dir
    }
    return a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }) * dir
  })
}

export function formatFileSize(bytes?: number): string {
  if (bytes == null || bytes <= 0) return '—'
  const units = ['B', 'KB', 'MB', 'GB', 'TB']
  let value = bytes
  let unit = 0
  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024
    unit += 1
  }
  return `${value.toFixed(unit === 0 ? 0 : 2)} ${units[unit]}`
}

export function formatStoragePercent(used: number, total: number): string {
  if (total <= 0) return '0.0%'
  return `${((used / total) * 100).toFixed(1)}%`
}

export function formatAvailableBytes(used: number, total: number): string {
  const available = Math.max(0, total - used)
  return formatFileSize(available)
}

export type FileTreeNode = {
  key: string
  label: string
  path: string
  entryType: 'root' | 'folder' | 'file'
  leaf?: boolean
  children?: FileTreeNode[]
}

/** @deprecated Dùng {@link FileTreeNode} */
export type FolderTreeNode = FileTreeNode

export function buildFileTreeNodes(
  entries: FileEntry[],
  rootLabel: string,
): FileTreeNode[] {
  const buildChildren = (parentPath: string): FileTreeNode[] =>
    entries
      .filter((e) => normalizePath(e.parentPath) === normalizePath(parentPath))
      .sort((a, b) => {
        if (a.type !== b.type) return a.type === 'folder' ? -1 : 1
        return a.name.localeCompare(b.name, undefined, { sensitivity: 'base' })
      })
      .map((entry) => {
        const folderPath = getEntryPath(entry)
        const childNodes =
          entry.type === 'folder' ? buildChildren(folderPath) : undefined

        return {
          key: entry.id,
          label: entry.name,
          path:
            entry.type === 'folder'
              ? folderPath
              : normalizePath(entry.parentPath),
          entryType: entry.type,
          leaf: entry.type === 'file' || !childNodes?.length,
          children: childNodes?.length ? childNodes : undefined,
        }
      })

  const rootChildren = buildChildren('')

  return [
    {
      key: '__root__',
      label: rootLabel,
      path: '',
      entryType: 'root',
      leaf: false,
      children: rootChildren.length ? rootChildren : undefined,
    },
  ]
}

export function getShareLink(entry: FileEntry): string {
  if (entry.sharedLink) return entry.sharedLink

  const path =
    entry.type === 'folder'
      ? getEntryPath(entry)
      : joinPath(entry.parentPath, entry.name)
  const segment = path ? encodeURIComponent(path) : entry.id

  if (typeof window !== 'undefined' && window.location?.origin) {
    return `${window.location.origin}/files/shared/${segment}`
  }

  return `/files/shared/${segment}`
}

export function buildFolderTreeNodes(
  folders: FileEntry[],
  rootLabel: string,
): FileTreeNode[] {
  return buildFileTreeNodes(folders, rootLabel)
}

export function folderSelectionKeyForPath(
  entries: FileEntry[],
  currentPath: string,
): string {
  const folders = entries.filter((e) => e.type === 'folder')
  const normalized = normalizePath(currentPath)
  if (!normalized) return '__root__'
  const folder = folders.find((f) => getEntryPath(f) === normalized)
  return folder?.id ?? '__root__'
}

export function expandedKeysForFolderPath(
  entries: FileEntry[],
  currentPath: string,
): Record<string, boolean> {
  const folders = entries.filter((e) => e.type === 'folder')
  const keys: Record<string, boolean> = { __root__: true }
  const normalized = normalizePath(currentPath)
  if (!normalized) return keys

  let acc = ''
  for (const part of normalized.split('/')) {
    acc = acc ? `${acc}/${part}` : part
    const folder = folders.find((f) => getEntryPath(f) === acc)
    if (folder) keys[folder.id] = true
  }
  return keys
}
