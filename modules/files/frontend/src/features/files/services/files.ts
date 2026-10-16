import type {
  CreateFolderPayload,
  FileEntry,
  ListFilesParams,
  ListFilesResult,
  MoveEntryPayload,
  RenameEntryPayload,
  UploadFilesPayload,
} from '@platform/core'
import { BASE_URL } from '@platform/core'
import type {
  ApiFileTreeNode,
  ApiMoveFileRequest,
  ApiRenameFileRequest,
  ApiUploadFileResponse,
  FilesConnection,
} from '../types'
import { DEFAULT_FILES_CONNECTION } from '../types'
import { unwrapAxiosData } from './apiEnvelope'
import {
  rememberFileSize,
  forgetFilePath,
  forgetFilePathTree,
  renameFileSizeRegistry,
} from './fileMetadata'
import {
  registerEmptyFolder,
  unregisterEmptyFolderTree,
  renameEmptyFolderRegistry,
  moveEmptyFolderRegistry,
  clearEmptyFolderIfContainsFiles,
  getEmptyFolderEntries,
} from './folderRegistry'
import {
  buildAllEntries,
  buildFolderPath,
  buildListResult,
  mapCreateFolderToEntry,
  mapMoveToEntry,
  mapRenameToEntry,
  mapTreeChildrenToFolderEntries,
  mapUploadResponseToEntries,
  mergeFileEntries,
  readApiErrorMessage,
  toFilePath,
} from './mapping'
import instance from './req'

function resolveConnection(connection?: FilesConnection): FilesConnection {
  return connection ?? DEFAULT_FILES_CONNECTION
}

function filesBase(connection: FilesConnection): string {
  const version = connection.apiVersion?.trim() || 'v2'
  const bucket = connection.bucket.trim()
  return bucket
    ? `${version}/files/${encodeURIComponent(bucket)}`
    : `${version}/files`
}

function apiRoot(): string {
  const base = BASE_URL.trim().replace(/\/+$/, '')
  return base || '/api'
}

function fileBaseName(fileName: string): string {
  const normalized = fileName.replace(/\\/g, '/')
  const parts = normalized.split('/')
  return parts[parts.length - 1]?.trim() || fileName.trim()
}

async function listRawPaths(
  connection: FilesConnection,
  prefix?: string,
): Promise<string[]> {
  const params = prefix?.trim() ? { prefix: prefix.trim() } : undefined
  const response = await instance.get<string[]>(filesBase(connection), { params })
  const names = unwrapAxiosData<string[] | null>(response)
  return Array.isArray(names) ? names : []
}

async function fetchTreeNode(
  connection: FilesConnection,
  rootPath: string,
): Promise<ApiFileTreeNode | null> {
  const treeResponse = await instance.get<ApiFileTreeNode>(
    `${connection.apiVersion?.trim() || 'v2'}/files/tree`,
    {
      params: {
        bucket: connection.bucket,
        rootPath: rootPath.trim(),
      },
    },
  )
  return unwrapAxiosData<ApiFileTreeNode | null>(treeResponse)
}

async function fetchTreeFolderChildren(
  connection: FilesConnection,
  parentPath: string,
): Promise<FileEntry[]> {
  try {
    const node = await fetchTreeNode(connection, parentPath)
    return mapTreeChildrenToFolderEntries(node, parentPath)
  } catch {
    return []
  }
}

export const callGetTreeFolderChildren = async (
  parentPath: string,
  connection?: FilesConnection,
): Promise<FileEntry[]> => {
  const conn = resolveConnection(connection)
  try {
    const folders = await fetchTreeFolderChildren(conn, parentPath)
    return mergeFileEntries(folders, getEmptyFolderEntries(parentPath))
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callGetFileList = async (
  params: ListFilesParams,
  connection?: FilesConnection,
): Promise<ListFilesResult> => {
  const conn = resolveConnection(connection)
  try {
    const prefix = params.path?.trim() || undefined
    const [paths, treeFolders] = await Promise.all([
      listRawPaths(conn, prefix),
      fetchTreeFolderChildren(conn, params.path?.trim() ?? ''),
    ])
    return buildListResult(params, paths, treeFolders)
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callGetAllEntries = async (
  connection?: FilesConnection,
): Promise<FileEntry[]> => {
  const conn = resolveConnection(connection)

  try {
    return await callGetTreeFolderChildren('', conn)
  } catch {
    // Tree API may return 501 when IFileTreeProvider is not registered — fall back to flat list.
  }

  try {
    const paths = await listRawPaths(conn)
    return buildAllEntries(paths)
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callUploadFiles = async (
  payload: UploadFilesPayload,
  connection?: FilesConnection,
): Promise<FileEntry[]> => {
  const conn = resolveConnection(connection)
  const parentPath = payload.path?.trim() ?? ''
  const uploaded: ApiUploadFileResponse[] = []

  try {
    for (const file of payload.files) {
      const fileName = buildFolderPath(parentPath, fileBaseName(file.name))
      const formData = new FormData()
      formData.append('file', file, fileBaseName(file.name))

      const response = await instance.post<ApiUploadFileResponse>(
        `${filesBase(conn)}/upload`,
        formData,
        {
          params: { fileName },
          headers: { 'Content-Type': 'multipart/form-data' },
        },
      )
      const body = unwrapAxiosData<ApiUploadFileResponse>(response)
      uploaded.push(body)

      rememberFileSize(fileName, body.length ?? file.size)
      clearEmptyFolderIfContainsFiles(fileName)
    }

    return mapUploadResponseToEntries(payload, uploaded)
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callCreateFolder = async (
  payload: CreateFolderPayload,
  connection?: FilesConnection,
): Promise<FileEntry> => {
  const conn = resolveConnection(connection)
  const folderPath = buildFolderPath(payload.path, payload.name)

  try {
    await instance.post(filesBase(conn), null, {
      params: { path: folderPath },
    })
    registerEmptyFolder(folderPath)
    return mapCreateFolderToEntry(payload)
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callDeleteFile = async (
  entry: FileEntry,
  connection?: FilesConnection,
): Promise<void> => {
  const conn = resolveConnection(connection)
  const fullPath = toFilePath(entry)

  try {
    if (entry.type === 'folder') {
      await instance.delete(filesBase(conn), { params: { prefix: fullPath } })
      unregisterEmptyFolderTree(fullPath)
      forgetFilePathTree(fullPath)
      return
    }

    await instance.delete(filesBase(conn), { params: { fileName: fullPath } })
    forgetFilePath(fullPath)
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callRenameFile = async (
  payload: RenameEntryPayload,
  connection?: FilesConnection,
): Promise<FileEntry> => {
  const conn = resolveConnection(connection)
  const sourcePath = toFilePath(payload.entry)
  const targetPath = buildFolderPath(payload.entry.parentPath, payload.name.trim())

  try {
    await instance.patch(`${filesBase(conn)}/rename`, {
      path: sourcePath,
      newName: payload.name.trim(),
    } satisfies ApiRenameFileRequest)

    if (payload.entry.type === 'folder') {
      renameEmptyFolderRegistry(sourcePath, targetPath)
    } else {
      renameFileSizeRegistry(sourcePath, targetPath)
    }

    return mapRenameToEntry(payload)
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callMoveFile = async (
  payload: MoveEntryPayload,
  connection?: FilesConnection,
): Promise<FileEntry> => {
  const conn = resolveConnection(connection)
  const sourcePath = toFilePath(payload.entry)
  const destinationFolder = payload.targetPath?.trim() ?? ''

  try {
    await instance.post(`${filesBase(conn)}/move`, {
      sourcePath,
      destinationFolder,
    } satisfies ApiMoveFileRequest)

    if (payload.entry.type === 'folder') {
      moveEmptyFolderRegistry(sourcePath, destinationFolder)
    } else {
      const destinationPath = buildFolderPath(destinationFolder, payload.entry.name)
      renameFileSizeRegistry(sourcePath, destinationPath)
    }

    return mapMoveToEntry(payload)
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callGetViewUrl = async (
  entry: FileEntry,
  connection?: FilesConnection,
): Promise<string> => {
  const conn = resolveConnection(connection)
  const fileName = toFilePath(entry)

  try {
    const response = await instance.get<{ url: string }>(
      `${filesBase(conn)}/view`,
      { params: { fileName } },
    )
    const body = unwrapAxiosData<{ url?: string } | null>(response)
    const url = body?.url?.trim()
    if (!url) {
      throw new Error('API không trả về URL xem file.')
    }
    return url
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callDownloadFile = async (
  entry: FileEntry,
  connection?: FilesConnection,
): Promise<void> => {
  const conn = resolveConnection(connection)
  const fileName = toFilePath(entry)

  try {
    const response = await instance.get<Blob>(`${filesBase(conn)}/download`, {
      params: { fileName },
      responseType: 'blob',
    })

    const blob =
      response.data instanceof Blob
        ? response.data
        : new Blob([response.data as BlobPart])

    const blobUrl = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = blobUrl
    anchor.download = entry.name
    anchor.rel = 'noopener'
    document.body.appendChild(anchor)
    anchor.click()
    anchor.remove()
    window.setTimeout(() => URL.revokeObjectURL(blobUrl), 60_000)
  } catch (error) {
    throw new Error(readApiErrorMessage(error))
  }
}

export const callOpenFile = async (
  entry: FileEntry,
  connection?: FilesConnection,
): Promise<void> => {
  if (entry.type === 'folder') return

  try {
    const viewUrl = await callGetViewUrl(entry, connection)
    if (viewUrl.startsWith('http://') || viewUrl.startsWith('https://')) {
      window.open(viewUrl, '_blank', 'noopener,noreferrer')
      return
    }
  } catch {
    // FileSystem provider thường trả URL rỗng — fallback download.
  }

  await callDownloadFile(entry, connection)
}

export function buildDownloadUrl(
  entry: FileEntry,
  connection?: FilesConnection,
): string {
  const conn = resolveConnection(connection)
  const fileName = encodeURIComponent(toFilePath(entry))
  const relative = `${apiRoot()}/${filesBase(conn)}/download?fileName=${fileName}`

  if (typeof window !== 'undefined' && window.location?.origin) {
    return `${window.location.origin}${relative}`
  }

  return relative
}
