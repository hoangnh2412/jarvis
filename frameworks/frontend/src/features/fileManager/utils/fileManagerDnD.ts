import type { FileEntry } from '../types'
import { getEntryPath, normalizePath } from './fileTree'

export const FILE_MANAGER_DRAG_MIME =
  'application/x-platform-file-manager-entries'

let activeDragEntryIds: string[] | null = null

export function beginDragEntryIds(ids: string[]): void {
  activeDragEntryIds = ids.length > 0 ? ids : null
}

export function endDragEntryIds(): void {
  activeDragEntryIds = null
}

export function isFileManagerDragActive(): boolean {
  return activeDragEntryIds != null && activeDragEntryIds.length > 0
}

export function parseDraggedEntryIds(dataTransfer: DataTransfer): string[] {
  const raw = dataTransfer.getData(FILE_MANAGER_DRAG_MIME)
  if (raw) {
    try {
      const parsed = JSON.parse(raw) as unknown
      if (Array.isArray(parsed)) {
        const ids = parsed.filter(
          (id): id is string => typeof id === 'string' && id.length > 0,
        )
        if (ids.length > 0) return ids
      }
    } catch {
      // fall through to session fallback
    }
  }

  return activeDragEntryIds ?? []
}

export function setDraggedEntryIds(
  dataTransfer: DataTransfer,
  ids: string[],
): void {
  dataTransfer.setData(FILE_MANAGER_DRAG_MIME, JSON.stringify(ids))
  dataTransfer.setData('text/plain', ids.join(','))
  dataTransfer.effectAllowed = 'move'
  beginDragEntryIds(ids)
}

export function canMoveEntryToTarget(
  entry: FileEntry,
  targetPath: string,
): boolean {
  const target = normalizePath(targetPath)
  const parent = normalizePath(entry.parentPath)
  if (parent === target) return false

  if (entry.type === 'folder') {
    const entryPath = normalizePath(getEntryPath(entry))
    if (
      entryPath &&
      (target === entryPath || target.startsWith(`${entryPath}/`))
    ) {
      return false
    }
  }

  return true
}

export function filterMovableEntries(
  entries: FileEntry[],
  targetPath: string,
): FileEntry[] {
  return entries.filter((entry) => canMoveEntryToTarget(entry, targetPath))
}
