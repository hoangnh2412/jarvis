/**
 * API DTO types aligned with `Platform.Modules.Files.API` controllers.
 * JSON uses camelCase (ASP.NET default).
 */

export type FilesConnection = {
  /** API version segment, e.g. `v2` → `/api/v2/files` */
  apiVersion?: string
  /**
   * Blob bucket — route param `{bucket}` on `FileController`.
   * Empty string = provider storage root (Synology RootPath, FileSystem drive, …).
   */
  bucket: string
  /** Optional root label for breadcrumbs and folder tree (e.g. mydrive, D:). */
  rootLabel?: string
}

function resolveDefaultBucket(): string {
  const fromEnv =
    typeof import.meta !== 'undefined'
      ? import.meta.env?.VITE_FILES_BUCKET
      : undefined
  if (fromEnv === undefined || fromEnv === null) return ''
  return String(fromEnv).trim()
}

export const DEFAULT_FILES_CONNECTION: FilesConnection = {
  apiVersion: 'v2',
  bucket: resolveDefaultBucket(),
}

/** `GET /files/tree` → `FileTreeNode` */
export type ApiFileTreeNode = {
  name: string
  path: string
  isFolder: boolean
  children?: ApiFileTreeNode[]
}

/** `GET /files/{bucket}` → `string[]` (flat blob keys) */
export type ApiFileListResponse = string[]

/** `POST /files/{bucket}/upload` → `{ bucket, fileName, length }` */
export type ApiUploadFileResponse = {
  bucket: string
  fileName: string
  length: number
}

/** `GET /files/{bucket}/view` → `{ url }` */
export type ApiViewFileResponse = {
  url: string
}

/** `PATCH /files/{bucket}/rename` body */
export type ApiRenameFileRequest = {
  path: string
  newName: string
}

/** `POST /files/{bucket}/move` body */
export type ApiMoveFileRequest = {
  sourcePath: string
  destinationFolder: string
}

/** Sample `BaseResponse` envelope */
export type ApiEnvelope<T> = {
  data?: T
  code?: string
  traceId?: string
  error?: {
    message?: string
    systemMessage?: string
  }
}

/** @deprecated Use ApiUploadFileResponse */
export type UploadFileResponse = ApiUploadFileResponse

/** @deprecated Use ApiRenameFileRequest */
export type RenameFileBody = ApiRenameFileRequest

/** @deprecated Use ApiMoveFileRequest */
export type MoveFileBody = ApiMoveFileRequest
