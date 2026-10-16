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

export function getBreadcrumbSegments(
  path: string,
  rootLabel = 'All files',
): { label: string; path: string }[] {
  const normalized = normalizePath(path)
  const segments: { label: string; path: string }[] = [
    { label: rootLabel, path: '' },
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
          leaf: entry.type === 'file',
          children: entry.type === 'folder' ? childNodes : undefined,
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
      children: rootChildren,
    },
  ]
}

export function getShareLink(entry: FileEntry): string {
  return entry.sharedLink?.trim() ?? ''
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

/** Cập nhật cây sidebar sau move: gỡ khỏi parent cũ, thêm vào target (kèm remap subtree). */
export function applyMovesToTreeEntries(
  entries: FileEntry[],
  moved: FileEntry[],
  targetPath: string,
): FileEntry[] {
  let next = [...entries]
  const target = normalizePath(targetPath)

  for (const entry of moved) {
    const oldPath = getEntryPath(entry)
    const newPath = joinPath(target, entry.name)
    const newId =
      entry.type === 'folder' ? `folder:${newPath}` : `file:${newPath}`

    next = next.filter((item) => {
      if (item.id === entry.id) return false
      if (entry.type !== 'folder') return true
      const itemPath = getEntryPath(item)
      return itemPath !== oldPath && !itemPath.startsWith(`${oldPath}/`)
    })

    next.push({
      ...entry,
      parentPath: target,
      id: newId,
    })

    if (entry.type === 'folder') {
      const oldPrefix = `${oldPath}/`
      const descendants = entries.filter((item) => {
        const path = getEntryPath(item)
        return path.startsWith(oldPrefix)
      })

      for (const item of descendants) {
        const relative = getEntryPath(item).slice(oldPath.length + 1)
        const remappedPath = joinPath(newPath, relative)
        const slash = remappedPath.lastIndexOf('/')
        const name = slash >= 0 ? remappedPath.slice(slash + 1) : remappedPath
        const parentPath = slash >= 0 ? remappedPath.slice(0, slash) : ''

        next.push({
          ...item,
          id:
            item.type === 'folder'
              ? `folder:${remappedPath}`
              : `file:${remappedPath}`,
          name,
          parentPath,
        })
      }
    }
  }

  const byId = new Map<string, FileEntry>()
  for (const entry of next) {
    byId.set(entry.id, entry)
  }
  return [...byId.values()]
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
