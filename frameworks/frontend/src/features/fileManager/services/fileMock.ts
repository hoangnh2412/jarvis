import { FAKE_FILE_ENTRIES, FAKE_STORAGE } from '../constants'
import type {
  CreateFolderPayload,
  FileEntry,
  ListFilesParams,
  ListFilesResult,
  MoveEntryPayload,
  RenameEntryPayload,
  StorageInfo,
  UploadFilesPayload,
} from '../types'
import {
  filterEntries,
  getChildren,
  joinPath,
  normalizePath,
  sortEntries,
} from '../utils/fileTree'

let seq = 100
const store: FileEntry[] = FAKE_FILE_ENTRIES.map((e) => ({ ...e }))

const delay = (ms = 100) => new Promise((r) => setTimeout(r, ms))

function cloneEntry(entry: FileEntry): FileEntry {
  return { ...entry }
}

function computeStorage(entries: FileEntry[]): StorageInfo {
  const usedBytes = entries
    .filter((e) => e.type === 'file')
    .reduce((sum, e) => sum + (e.size ?? 0), 0)
  return {
    usedBytes,
    totalBytes: FAKE_STORAGE.totalBytes,
  }
}

export async function mockListFiles(
  params: ListFilesParams,
): Promise<ListFilesResult> {
  await delay()
  const path = normalizePath(params.path)
  let items = getChildren(store, path).map(cloneEntry)
  items = filterEntries(items, params.filter ?? '')
  items = sortEntries(items, params.sortField, params.sortOrder)
  return {
    items,
    storage: computeStorage(store),
    total: items.length,
  }
}

export async function mockUploadFiles(
  payload: UploadFilesPayload,
): Promise<FileEntry[]> {
  await delay()
  const parentPath = normalizePath(payload.path)
  const created: FileEntry[] = []

  for (const file of payload.files) {
    const name = file.name.trim()
    if (!name) continue
    if (
      store.some(
        (e) =>
          normalizePath(e.parentPath) === parentPath &&
          e.name.toLowerCase() === name.toLowerCase(),
      )
    ) {
      throw new Error(`"${name}" đã tồn tại trong thư mục này`)
    }
    const entry: FileEntry = {
      id: `file-${++seq}`,
      name,
      type: 'file',
      parentPath,
      size: file.size,
      sharedLink: null,
      updatedAt: new Date().toISOString(),
    }
    store.push(entry)
    created.push(cloneEntry(entry))
  }

  return created
}

export async function mockCreateFolder(
  payload: CreateFolderPayload,
): Promise<FileEntry> {
  await delay()
  const parentPath = normalizePath(payload.path)
  const name = payload.name.trim()
  if (!name) throw new Error('Tên thư mục không được để trống')
  if (name.includes('/') || name.includes('\\')) {
    throw new Error('Tên thư mục không hợp lệ')
  }
  if (
    store.some(
      (e) =>
        normalizePath(e.parentPath) === parentPath &&
        e.name.toLowerCase() === name.toLowerCase(),
    )
  ) {
    throw new Error(`Thư mục "${name}" đã tồn tại`)
  }

  const entry: FileEntry = {
    id: `folder-${++seq}`,
    name,
    type: 'folder',
    parentPath,
    updatedAt: new Date().toISOString(),
  }
  store.push(entry)
  return cloneEntry(entry)
}

export async function mockDeleteEntry(entry: FileEntry): Promise<void> {
  await delay()
  const index = store.findIndex((e) => e.id === entry.id)
  if (index < 0) throw new Error('Không tìm thấy mục')

  if (entry.type === 'folder') {
    const folderPath = joinPath(entry.parentPath, entry.name)
    const hasChildren = store.some(
      (e) => normalizePath(e.parentPath) === normalizePath(folderPath),
    )
    if (hasChildren) {
      throw new Error('Thư mục không rỗng — xoá nội dung trước')
    }
  }

  store.splice(index, 1)
}

export async function mockRenameEntry(
  payload: RenameEntryPayload,
): Promise<FileEntry> {
  await delay()
  const item = store.find((e) => e.id === payload.entry.id)
  if (!item) throw new Error('Không tìm thấy mục')

  const name = payload.name.trim()
  if (!name) throw new Error('Tên không được để trống')
  if (name.includes('/') || name.includes('\\')) {
    throw new Error('Tên không hợp lệ')
  }

  const parentPath = normalizePath(item.parentPath)
  if (
    store.some(
      (e) =>
        e.id !== item.id &&
        normalizePath(e.parentPath) === parentPath &&
        e.name.toLowerCase() === name.toLowerCase(),
    )
  ) {
    throw new Error(`"${name}" đã tồn tại`)
  }

  if (item.type === 'folder') {
    const oldPath = joinPath(item.parentPath, item.name)
    const newPath = joinPath(item.parentPath, name)
    for (const child of store) {
      if (normalizePath(child.parentPath) === normalizePath(oldPath)) {
        child.parentPath = newPath
      } else if (
        normalizePath(child.parentPath).startsWith(`${normalizePath(oldPath)}/`)
      ) {
        child.parentPath = child.parentPath.replace(
          normalizePath(oldPath),
          normalizePath(newPath),
        )
      }
    }
  }

  item.name = name
  item.updatedAt = new Date().toISOString()
  return cloneEntry(item)
}

export async function mockMoveEntry(
  payload: MoveEntryPayload,
): Promise<FileEntry> {
  await delay()
  const item = store.find((e) => e.id === payload.entry.id)
  if (!item) throw new Error('Không tìm thấy mục')

  const targetPath = normalizePath(payload.targetPath)
  const currentParent = normalizePath(item.parentPath)
  if (currentParent === targetPath) {
    throw new Error('Mục đã nằm trong thư mục đích')
  }

  if (item.type === 'folder') {
    const sourcePath = joinPath(item.parentPath, item.name)
    if (
      targetPath === normalizePath(sourcePath) ||
      targetPath.startsWith(`${normalizePath(sourcePath)}/`)
    ) {
      throw new Error('Không thể di chuyển thư mục vào chính nó hoặc thư mục con')
    }
  }

  if (
    store.some(
      (e) =>
        e.id !== item.id &&
        normalizePath(e.parentPath) === targetPath &&
        e.name.toLowerCase() === item.name.toLowerCase(),
    )
  ) {
    throw new Error(`"${item.name}" đã tồn tại trong thư mục đích`)
  }

  const oldFolderPath =
    item.type === 'folder' ? joinPath(item.parentPath, item.name) : null

  item.parentPath = targetPath
  item.updatedAt = new Date().toISOString()

  if (oldFolderPath) {
    const newFolderPath = joinPath(targetPath, item.name)
    for (const child of store) {
      if (child.id === item.id) continue
      const childParent = normalizePath(child.parentPath)
      const oldNorm = normalizePath(oldFolderPath)
      if (childParent === oldNorm) {
        child.parentPath = newFolderPath
      } else if (childParent.startsWith(`${oldNorm}/`)) {
        child.parentPath = childParent.replace(oldNorm, normalizePath(newFolderPath))
      }
    }
  }

  return cloneEntry(item)
}

/** Reset store — tiện cho demo / test */
export function mockResetFiles(): void {
  store.length = 0
  store.push(...FAKE_FILE_ENTRIES.map((e) => ({ ...e })))
}

export function mockGetAllEntries(): FileEntry[] {
  return store.map(cloneEntry)
}
