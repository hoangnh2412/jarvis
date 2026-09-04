export type FileEntryType = 'folder' | 'file'

/** Một mục file hoặc thư mục trong file manager */
export type FileEntry = {
  id: string
  name: string
  type: FileEntryType
  /** Đường dẫn thư mục cha — `''` = All files (root) */
  parentPath: string
  size?: number
  sharedLink?: string | null
  updatedAt?: string
}

export type StorageInfo = {
  usedBytes: number
  totalBytes: number
}

export type ListFilesParams = {
  path: string
  filter?: string
  sortField?: 'name' | 'size'
  sortOrder?: 'asc' | 'desc'
}

export type ListFilesResult = {
  items: FileEntry[]
  storage: StorageInfo
  total: number
}

export type UploadFilesPayload = {
  path: string
  files: File[]
}

export type CreateFolderPayload = {
  path: string
  name: string
}

export type RenameEntryPayload = {
  entry: FileEntry
  name: string
}

export type MoveEntryPayload = {
  entry: FileEntry
  targetPath: string
}

export type { ActionProps } from './mutation'
