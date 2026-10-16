import { normalizePath } from '@platform/core'

const fileSizeByPath = new Map<string, number>()

export function rememberFileSize(filePath: string, size?: number): void {
  if (size == null || size <= 0) return
  fileSizeByPath.set(normalizePath(filePath), size)
}

export function forgetFilePath(filePath: string): void {
  fileSizeByPath.delete(normalizePath(filePath))
}

export function forgetFilePathTree(prefix: string): void {
  const root = normalizePath(prefix)
  for (const path of [...fileSizeByPath.keys()]) {
    if (path === root || path.startsWith(`${root}/`)) {
      fileSizeByPath.delete(path)
    }
  }
}

export function renameFileSizeRegistry(oldPath: string, newPath: string): void {
  const oldNorm = normalizePath(oldPath)
  const newNorm = normalizePath(newPath)
  const size = fileSizeByPath.get(oldNorm)
  if (size == null) return

  fileSizeByPath.delete(oldNorm)
  fileSizeByPath.set(newNorm, size)
}

export function getFileSize(filePath: string): number | undefined {
  return fileSizeByPath.get(normalizePath(filePath))
}

export function applyKnownSizes<T extends { id: string; type: string; parentPath: string; name: string; size?: number }>(
  entries: T[],
): T[] {
  return entries.map((entry) => {
    if (entry.type !== 'file' || entry.size != null) return entry
    const fullPath = entry.parentPath
      ? `${normalizePath(entry.parentPath)}/${entry.name}`
      : entry.name
    const size = getFileSize(fullPath)
    return size == null ? entry : { ...entry, size }
  })
}

export function computeUsedBytes(paths: string[]): number {
  let total = 0
  for (const path of paths) {
    total += fileSizeByPath.get(normalizePath(path)) ?? 0
  }
  return total
}

export function resetFileMetadata(): void {
  fileSizeByPath.clear()
}
