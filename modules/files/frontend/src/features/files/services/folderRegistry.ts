import type { FileEntry } from '@platform/core'
import { getParentPath, normalizePath } from '@platform/core'
import { buildFolderPath } from './mapping'

const emptyFolderPaths = new Set<string>()

export function registerEmptyFolder(folderPath: string): void {
  const normalized = normalizePath(folderPath)
  if (normalized) emptyFolderPaths.add(normalized)
}

export function unregisterEmptyFolder(folderPath: string): void {
  emptyFolderPaths.delete(normalizePath(folderPath))
}

export function unregisterEmptyFolderTree(folderPath: string): void {
  const root = normalizePath(folderPath)
  for (const path of [...emptyFolderPaths]) {
    if (path === root || path.startsWith(`${root}/`)) {
      emptyFolderPaths.delete(path)
    }
  }
}

export function renameEmptyFolderRegistry(oldPath: string, newPath: string): void {
  const oldNorm = normalizePath(oldPath)
  const newNorm = normalizePath(newPath)
  if (!emptyFolderPaths.has(oldNorm)) return

  emptyFolderPaths.delete(oldNorm)
  emptyFolderPaths.add(newNorm)

  for (const path of [...emptyFolderPaths]) {
    if (path.startsWith(`${oldNorm}/`)) {
      emptyFolderPaths.delete(path)
      emptyFolderPaths.add(path.replace(oldNorm, newNorm))
    }
  }
}

export function moveEmptyFolderRegistry(sourcePath: string, destinationFolder: string): void {
  const source = normalizePath(sourcePath)
  const destination = normalizePath(destinationFolder)
  const fileName = source.includes('/') ? source.slice(source.lastIndexOf('/') + 1) : source
  const newPath = buildFolderPath(destination, fileName)

  if (emptyFolderPaths.has(source)) {
    emptyFolderPaths.delete(source)
    emptyFolderPaths.add(newPath)
  }

  for (const path of [...emptyFolderPaths]) {
    if (path.startsWith(`${source}/`)) {
      emptyFolderPaths.delete(path)
      emptyFolderPaths.add(path.replace(source, newPath))
    }
  }
}

export function clearEmptyFolderIfContainsFiles(filePath: string): void {
  const normalized = normalizePath(filePath)
  let parent = getParentPath(normalized)
  while (parent) {
    emptyFolderPaths.delete(parent)
    parent = getParentPath(parent)
  }
}

export function getEmptyFolderEntries(parentPath = ''): FileEntry[] {
  const parent = normalizePath(parentPath)
  const entries: FileEntry[] = []

  for (const folderPath of emptyFolderPaths) {
    if (getParentPath(folderPath) !== parent) continue
    const name = folderPath.includes('/')
      ? folderPath.slice(folderPath.lastIndexOf('/') + 1)
      : folderPath
    entries.push({
      id: `folder:${folderPath}`,
      name,
      type: 'folder',
      parentPath: parent,
    })
  }

  return entries
}

export function getAllEmptyFolderEntries(): FileEntry[] {
  return [...emptyFolderPaths].map((folderPath) => {
    const parentPath = getParentPath(folderPath)
    const name = folderPath.includes('/')
      ? folderPath.slice(folderPath.lastIndexOf('/') + 1)
      : folderPath
    return {
      id: `folder:${folderPath}`,
      name,
      type: 'folder',
      parentPath,
    }
  })
}

export function resetEmptyFolderRegistry(): void {
  emptyFolderPaths.clear()
}
