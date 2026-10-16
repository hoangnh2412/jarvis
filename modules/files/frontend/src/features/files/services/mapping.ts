import type {
  CreateFolderPayload,
  FileEntry,
  ListFilesParams,
  ListFilesResult,
  MoveEntryPayload,
  RenameEntryPayload,
  UploadFilesPayload,
} from '@platform/core'
import { joinPath, normalizePath } from '@platform/core'
import type { ApiFileTreeNode } from '../types'
import { applyKnownSizes } from './fileMetadata'
import { getAllEmptyFolderEntries, getEmptyFolderEntries } from './folderRegistry'

export function mergeFileEntries(...groups: FileEntry[][]): FileEntry[] {
  const byId = new Map<string, FileEntry>()
  for (const group of groups) {
    for (const entry of group) {
      byId.set(entry.id, entry)
    }
  }
  return [...byId.values()]
}

function filterEntries(entries: FileEntry[], filter: string): FileEntry[] {
  const query = filter.trim().toLowerCase()
  if (!query) return entries
  return entries.filter((entry) => entry.name.toLowerCase().includes(query))
}

function sortEntries(
  entries: FileEntry[],
  sortField: ListFilesParams['sortField'] = 'name',
  sortOrder: ListFilesParams['sortOrder'] = 'asc',
): FileEntry[] {
  const direction = sortOrder === 'desc' ? -1 : 1
  return [...entries].sort((a, b) => {
    if (a.type !== b.type) {
      return a.type === 'folder' ? -1 : 1
    }
    if (sortField === 'size') {
      const sizeA = a.size ?? 0
      const sizeB = b.size ?? 0
      return (sizeA - sizeB) * direction
    }
    return (
      a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }) *
      direction
    )
  })
}

export function toFilePath(entry: FileEntry): string {
  return joinPath(entry.parentPath, entry.name)
}

export function buildFolderPath(parentPath: string, name: string): string {
  return joinPath(parentPath, name)
}

export function parseBlobPath(fullPath: string): {
  parentPath: string
  name: string
} {
  const normalized = normalizePath(fullPath)
  if (!normalized) {
    return { parentPath: '', name: '' }
  }
  const parts = normalized.split('/')
  const name = parts.pop() ?? normalized
  return { parentPath: parts.join('/'), name }
}

export function fileEntryFromBlobPath(
  fullPath: string,
  size?: number,
): FileEntry {
  const normalized = normalizePath(fullPath)
  const { parentPath, name } = parseBlobPath(normalized)
  return {
    id: `file:${normalized}`,
    name,
    type: 'file',
    parentPath,
    size,
  }
}

export function folderEntryFromPath(folderPath: string): FileEntry {
  const normalized = normalizePath(folderPath)
  const { parentPath, name } = parseBlobPath(normalized)
  return {
    id: `folder:${normalized}`,
    name,
    type: 'folder',
    parentPath,
  }
}

export function findTreeNodeAtPath(
  node: ApiFileTreeNode,
  targetPath: string,
): ApiFileTreeNode | null {
  const target = normalizePath(targetPath)
  const nodePath = normalizePath(node.path)

  if (nodePath === target) return node

  for (const child of node.children ?? []) {
    const found = findTreeNodeAtPath(child, target)
    if (found) return found
  }

  return null
}

/** Immediate child folders from a tree node at `parentPath`. */
export function mapTreeChildrenToFolderEntries(
  node: ApiFileTreeNode | null | undefined,
  parentPath: string,
): FileEntry[] {
  if (!node?.children?.length) return []

  const parent = normalizePath(parentPath)
  return node.children
    .filter((child) => child.isFolder)
    .map((child) => folderEntryFromPath(normalizePath(child.path) || buildFolderPath(parent, child.name)))
}

/** Map flat blob keys under a prefix to immediate child entries. */
export function mapPathsToChildren(
  paths: string[],
  parentPath: string,
): FileEntry[] {
  const parent = normalizePath(parentPath)
  const prefix = parent ? `${parent}/` : ''
  const childFolders = new Set<string>()
  const entries: FileEntry[] = []

  for (const rawPath of paths) {
    const fullPath = normalizePath(rawPath)
    if (prefix && !fullPath.startsWith(prefix)) continue

    const remainder = prefix ? fullPath.slice(prefix.length) : fullPath
    if (!remainder) continue

    const slashIndex = remainder.indexOf('/')
    if (slashIndex >= 0) {
      const folderName = remainder.slice(0, slashIndex)
      if (childFolders.has(folderName)) continue
      childFolders.add(folderName)
      const folderPath = buildFolderPath(parent, folderName)
    entries.push({
      id: `folder:${folderPath}`,
      name: folderName,
      type: 'folder',
      parentPath: parent,
    })
      continue
    }

    entries.push(fileEntryFromBlobPath(fullPath))
  }

  return entries
}

/** Build full flat entry list (files + inferred folders) for sidebar tree. */
export function mapPathsToAllEntries(paths: string[]): FileEntry[] {
  const folderPaths = new Set<string>()
  const fileEntries: FileEntry[] = []

  for (const rawPath of paths) {
    const fullPath = normalizePath(rawPath)
    if (!fullPath) continue

    const parts = fullPath.split('/')
    const fileName = parts.pop()!
    let parent = ''

    for (const segment of parts) {
      const folderPath = joinPath(parent, segment)
      folderPaths.add(folderPath)
      parent = folderPath
    }

    fileEntries.push(fileEntryFromBlobPath(fullPath))
  }

  const folderEntries: FileEntry[] = [...folderPaths].map((folderPath) =>
    folderEntryFromPath(folderPath),
  )

  return [...folderEntries, ...fileEntries]
}

export function mapTreeNodeToEntries(
  node: ApiFileTreeNode,
  parentPath = '',
  isRoot = true,
): FileEntry[] {
  if (isRoot && node.isFolder && !normalizePath(node.path)) {
    return mergeFileEntries(
      ...(node.children ?? []).map((child) =>
        mapTreeNodeToEntries(child, '', false),
      ),
    )
  }

  const entries: FileEntry[] = []
  const path = normalizePath(node.path)

  if (node.isFolder) {
    entries.push(folderEntryFromPath(path))
  } else {
    entries.push(fileEntryFromBlobPath(path))
  }

  for (const child of node.children ?? []) {
    entries.push(...mapTreeNodeToEntries(child, path, false))
  }

  return entries
}

export function buildListResult(
  params: ListFilesParams,
  paths: string[],
  treeFolderEntries: FileEntry[] = [],
): ListFilesResult {
  let items = mergeFileEntries(
    mapPathsToChildren(paths, params.path),
    treeFolderEntries,
    getEmptyFolderEntries(params.path),
  )
  items = applyKnownSizes(items)
  items = filterEntries(items, params.filter ?? '')
  items = sortEntries(items, params.sortField, params.sortOrder)

  return {
    items,
    total: items.length,
  }
}

export function buildAllEntries(paths: string[]): FileEntry[] {
  return applyKnownSizes(
    mergeFileEntries(mapPathsToAllEntries(paths), getAllEmptyFolderEntries()),
  )
}

export function mapUploadResponseToEntries(
  payload: UploadFilesPayload,
  responses: { fileName: string; length?: number }[],
): FileEntry[] {
  return responses.map((response) =>
    fileEntryFromBlobPath(response.fileName, response.length),
  )
}

export function mapCreateFolderToEntry(
  payload: CreateFolderPayload,
): FileEntry {
  const folderPath = buildFolderPath(payload.path, payload.name)
  return {
    ...folderEntryFromPath(folderPath),
    updatedAt: new Date().toISOString(),
  }
}

export function mapRenameToEntry(payload: RenameEntryPayload): FileEntry {
  return {
    ...payload.entry,
    name: payload.name.trim(),
    id:
      payload.entry.type === 'folder'
        ? `folder:${buildFolderPath(payload.entry.parentPath, payload.name.trim())}`
        : `file:${buildFolderPath(payload.entry.parentPath, payload.name.trim())}`,
    updatedAt: new Date().toISOString(),
  }
}

export function mapMoveToEntry(payload: MoveEntryPayload): FileEntry {
  const targetPath = normalizePath(payload.targetPath)
  return {
    ...payload.entry,
    parentPath: targetPath,
    id:
      payload.entry.type === 'folder'
        ? `folder:${buildFolderPath(targetPath, payload.entry.name)}`
        : `file:${buildFolderPath(targetPath, payload.entry.name)}`,
    updatedAt: new Date().toISOString(),
  }
}

export function readApiErrorMessage(error: unknown): string {
  if (error && typeof error === 'object') {
    const record = error as Record<string, unknown>
    const nestedError = record.error
    if (nestedError && typeof nestedError === 'object') {
      const errObj = nestedError as Record<string, unknown>
      if (typeof errObj.message === 'string' && errObj.message) return errObj.message
      if (typeof errObj.systemMessage === 'string' && errObj.systemMessage) {
        return errObj.systemMessage
      }
    }
    if (typeof record.message === 'string' && record.message) return record.message
    if (typeof record.Message === 'string' && record.Message) return record.Message
  }
  return 'Không thể thực hiện thao tác file'
}
